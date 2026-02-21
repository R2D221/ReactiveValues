using ReactiveValues;
using ReactiveValues.Diagnostics;

namespace Signals.Tests.Misc;

[TestClass]
public sealed class WatcherTests
{
	[TestMethod(DisplayName = "It should work")]
	public void Test1()
	{
		using var syncContext = TestSynchronizationContext.Scope();

		var notifySpy = ProxyAction.For(() => { });
		var watcher = new TestWatcher(notifySpy.Invoke);

		Action effect(Action action)
		{
			var e = new Effect(action);
			watcher.Watch(e);
			return () => watcher.Unwatch(e);
		}

		var watchedSpy = ProxyAction.For(() => { });
		var unwatchedSpy = ProxyAction.For(() => { });
		var stateSignal = new ReactiveValue<int>(1);
		stateSignal.Watched += (_, _) => watchedSpy.Invoke();
		stateSignal.Unwatched += (_, _) => unwatchedSpy.Invoke();

		stateSignal.Value = (100);
		stateSignal.Value = (5);

		var computedSignal = new ReactiveFunc<int>(() => stateSignal.Value * 2);

		var calls = 0;
		var output = 0;
		var computedOutput = 0;

		// Ensure the call backs are not called yet
		syncContext.RunQueue();
		Assert(That(watchedSpy.TimesCalled) == 0);
		Assert(That(unwatchedSpy.TimesCalled) == 0);

		// Expect the watcher to not have any sources as nothing has been connected yet
		Assert(That(ReactiveDiagnostics.GetSources(watcher).Length) == 0);
		Assert(That(ReactiveDiagnostics.GetReceivers(computedSignal).Length) == 0);
		Assert(That(ReactiveDiagnostics.GetReceivers(stateSignal).Length) == 0);

		Assert(That(Reactive.IsWatched(stateSignal)) == false);

		var destructor = effect(() =>
		{
			output = stateSignal.Value;
			computedOutput = computedSignal.Value;
			calls++;
		});

		// The signal is now watched
		Assert(That(Reactive.IsWatched(stateSignal)) == true);

		// Now that the effect is created, there will be a source
		Assert(That(ReactiveDiagnostics.GetSources(watcher).Length) == 1);
		Assert(That(ReactiveDiagnostics.GetReceivers(computedSignal).Length) == 1);

		// Note: stateSignal has more sinks because one is for the computed signal and one is the effect.
		Assert(That(ReactiveDiagnostics.GetReceivers(stateSignal).Length) == 2);

		// Now the watched callback should be called
		syncContext.RunQueue();
		Assert(That(watchedSpy.TimesCalled) > 0);
		Assert(That(unwatchedSpy.TimesCalled) == 0);

		// It should not have notified yet
		Assert(That(notifySpy.TimesCalled) == 0);

		stateSignal.Value = (10);

		// After a signal has been set, it should notify
		Assert(That(notifySpy.TimesCalled) > 0);

		// Initially, the effect should not have run
		Assert(That(calls) == 1);
		Assert(That(output) == 5);
		Assert(That(computedOutput) == 10);

		watcher.RunPending();

		// The effect should run, and thus increment the value
		Assert(That(calls) == 2);
		Assert(That(output) == 10);
		Assert(That(computedOutput) == 20);

		// Kicking it off again, the effect should run again
		watcher.Watch();
		stateSignal.Value = (20);
		Assert(That(watcher.RunPending().Length) == 1);

		// After a signal has been set, it should notify again
		Assert(That(notifySpy.TimesCalled) == 2);

		Assert(That(calls) == 3);
		Assert(That(output) == 20);
		Assert(That(computedOutput) == 40);

		using (Reactive.Untrack())
		{
			// Untrack doesn't affect set, only get
			stateSignal.Value = (999);
			Assert(That(calls) == 3);
			watcher.RunPending();
			Assert(That(calls) == 4);
		}

		// Destroy and un-subscribe
		destructor.Invoke();

		// Since now it is un-subscribed, it should now be called
		syncContext.RunQueue();
		Assert(That(unwatchedSpy.TimesCalled) > 0);
		// We can confirm that it is un-watched by checking it
		Assert(That(Reactive.IsWatched(stateSignal)) == false);

		// Since now it is un-subscribed, this should have no effect now
		stateSignal.Value = (200);
		watcher.RunPending();

		// Make sure that effect is no longer running
		// Everything should stay the same
		Assert(That(calls) == 4);
		Assert(That(output) == 999);
		Assert(That(computedOutput) == 1998);

		Assert(That(watcher.RunPending().Length) == 0);

		// Adding any other effect after an unwatch should work as expected
		var destructor2 = effect(() =>
		{
			output = stateSignal.Value;
		});

		stateSignal.Value = (300);
		watcher.RunPending();
	}
}
