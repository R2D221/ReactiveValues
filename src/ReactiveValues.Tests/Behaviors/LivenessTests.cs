using ReactiveValues;

namespace Signals.Tests.Behaviors;

[TestClass]
public sealed class LivenessTests
{
	[TestMethod(DisplayName = "It only changes on first and last descendant")]
	public void Test1()
	{
		using var syncContext = TestSynchronizationContext.Scope();

		var watchedSpy = ProxyAction.For(() => { });
		var unwatchedSpy = ProxyAction.For(() => { });

		var state = new ReactiveValue<int>(1);
		state.Watched += (_, _) => watchedSpy.Invoke();
		state.Unwatched += (_, _) => unwatchedSpy.Invoke();
		var computed = new ReactiveFunc<int>(() => state.Value);
		_ = computed.Value;
		syncContext.RunQueue();
		Assert(That(watchedSpy.TimesCalled) == 0);
		Assert(That(unwatchedSpy.TimesCalled) == 0);

		var w = new TestWatcher(() => { });
		var w2 = new TestWatcher(() => { });
		var e = new Effect(() => _ = computed.Value);

		w.Watch(e);
		syncContext.RunQueue();
		Assert(That(watchedSpy.TimesCalled) == 1);
		Assert(That(unwatchedSpy.TimesCalled) == 0);

		w2.Watch(e);
		syncContext.RunQueue();
		Assert(That(watchedSpy.TimesCalled) == 1);
		Assert(That(unwatchedSpy.TimesCalled) == 0);

		w2.Unwatch(e);
		syncContext.RunQueue();
		Assert(That(watchedSpy.TimesCalled) == 1);
		Assert(That(unwatchedSpy.TimesCalled) == 0);

		w.Unwatch(e);
		syncContext.RunQueue();
		Assert(That(watchedSpy.TimesCalled) == 1);
		Assert(That(unwatchedSpy.TimesCalled) == 1);
	}

	[TestMethod(DisplayName = "It is tracked well on computed signals")]
	public void Test2()
	{
		using var syncContext = TestSynchronizationContext.Scope();

		var watchedSpy = ProxyAction.For(() => { });
		var unwatchedSpy = ProxyAction.For(() => { });

		var s = new ReactiveValue<int>(1);
		var c = new ReactiveFunc<int>(() => s.Value);
		c.Watched += (_, _) => watchedSpy.Invoke();
		c.Unwatched += (_, _) => unwatchedSpy.Invoke();

		_ = c.Value;
		syncContext.RunQueue();
		Assert(That(watchedSpy.TimesCalled) == 0);
		Assert(That(unwatchedSpy.TimesCalled) == 0);

		var w = new TestWatcher(() => { });
		var e = new Effect(() => _ = c.Value);
		w.Watch(e);
		syncContext.RunQueue();
		Assert(That(watchedSpy.TimesCalled) == 1);
		Assert(That(unwatchedSpy.TimesCalled) == 0);

		w.Unwatch(e);
		syncContext.RunQueue();
		Assert(That(watchedSpy.TimesCalled) == 1);
		Assert(That(unwatchedSpy.TimesCalled) == 1);
	}
}
