using ReactiveValues;
using ReactiveValues.Diagnostics;
using System.Collections.Immutable;

namespace Signals.Tests.Misc;

[TestClass]
public sealed class WatchUnwatchTests
{
	[TestMethod(DisplayName = "It handles multiple Watchers well")]
	public void Test1()
	{
		var s = new ReactiveValue<int>(1);
		var s2 = new ReactiveValue<int>(2);
		var n = 0;
		var w = new TestWatcher(() => n++);

		var e1 = new Effect(() => { _ = s.Value; });
		w.Watch(e1);

		var e2 = new Effect(() => { _ = s2.Value; });
		w.Watch(e2);

		s.Value = (4);
		Assert(That(n) == 1);
		//Assert(That(w.GetPending().Count) == 0);

		w.RunPending();

		s2.Value = (8);
		Assert(That(n) == 2);

		w.Unwatch(e1);
		s.Value = (3);
		Assert(That(n) == 2);

		w.RunPending();

		s2.Value = (3);
		Assert(That(n) == 3);

		w.RunPending();

		s.Value = (2);
		Assert(That(n) == 3);
	}

	[TestMethod(DisplayName = "It understands dynamic dependency sets")]
	public void Test2()
	{
		using var syncContext = TestSynchronizationContext.Scope();

		var w1 = 0;
		var u1 = 0;
		var w2 = 0;
		var u2 = 0;
		var n = 0;
		var d = 0;

		var s1 = new ReactiveValue<int>(1);
		s1.Watched += (_, _) => w1++;
		s1.Unwatched += (_, _) => u1++;

		var s2 = new ReactiveValue<int>(2);
		s2.Watched += (_, _) => w2++;
		s2.Unwatched += (_, _) => u2++;

		var which = new { Get = (Func<int>)(() => s1.Value) };

		var c = new ReactiveFunc<int>(() => { d++; return which.Get(); });

		Assert(That(w1 + w2 + u1 + u2 + n + d) == 0);
		Assert(That(Reactive.IsWatched(s1)) == false);
		Assert(That(Reactive.IsWatched(s2)) == false);

		var e = new Effect(() => _ = c.Value);
		var w = new TestWatcher(() => n++);

		w.Watch(e);
		w.RunPending();
		syncContext.RunQueue();

		Assert(That(c.Value) == 1);
		Assert(That(w1) == 1);
		Assert(That(u1) == 0);
		Assert(That(w2) == 0);
		Assert(That(u2) == 0);
		Assert(That(n) == 0);
		Assert(That(Reactive.IsWatched(s1)) == true);
		Assert(That(Reactive.IsWatched(s2)) == false);
		Assert(That(d) == 1);

		Assert(That(w.RunPending()).SequenceEqual(Array.Empty<Effect>()));
		syncContext.RunQueue();

		s1.Value = (3);
		Assert(That(w1) == 1);
		Assert(That(u1) == 0);
		Assert(That(w2) == 0);
		Assert(That(u2) == 0);
		Assert(That(n) == 1);
		Assert(That(Reactive.IsWatched(s1)) == true);
		Assert(That(Reactive.IsWatched(s2)) == false);
		Assert(That(d) == 1);

		Assert(That(w.RunPending()).SequenceEqual([e]));
		syncContext.RunQueue();

		Assert(That(c.Value) == 3);
		Assert(That(w1) == 1);
		Assert(That(u1) == 0);
		Assert(That(w2) == 0);
		Assert(That(u2) == 0);
		Assert(That(n) == 1);
		Assert(That(Reactive.IsWatched(s1)) == true);
		Assert(That(Reactive.IsWatched(s2)) == false);
		Assert(That(d) == 2);

		Assert(That(w.RunPending()).SequenceEqual(Array.Empty<Effect>()));
		which = new { Get = (Func<int>)(() => s2.Value) };
		syncContext.RunQueue();

		s1.Value = (4);
		Assert(That(w1) == 1);
		Assert(That(u1) == 0);
		Assert(That(w2) == 0);
		Assert(That(u2) == 0);
		Assert(That(n) == 2);
		Assert(That(Reactive.IsWatched(s1)) == true);
		Assert(That(Reactive.IsWatched(s2)) == false);
		Assert(That(d) == 2);

		Assert(That(w.RunPending()).SequenceEqual([e]));
		syncContext.RunQueue();

		Assert(That(c.Value) == 2);
		Assert(That(w1) == 1);
		Assert(That(u1) == 1);
		Assert(That(w2) == 1);
		Assert(That(u2) == 0);
		Assert(That(n) == 2);
		Assert(That(Reactive.IsWatched(s1)) == false);
		Assert(That(Reactive.IsWatched(s2)) == true);
		Assert(That(d) == 3);

		Assert(That(w.RunPending()).SequenceEqual(Array.Empty<Effect>()));
		which = new { Get = (Func<int>)(() => 10) };
		s1.Value = (5);
		w.RunPending();
		syncContext.RunQueue();

		Assert(That(c.Value) == 2);
		Assert(That(w1) == 1);
		Assert(That(u1) == 1);
		Assert(That(w2) == 1);
		Assert(That(u2) == 0);
		Assert(That(n) == 2);
		Assert(That(Reactive.IsWatched(s1)) == false);
		Assert(That(Reactive.IsWatched(s2)) == true);
		Assert(That(d) == 3);

		Assert(That(w.RunPending()).SequenceEqual(Array.Empty<Effect>()));
		syncContext.RunQueue();

		s2.Value = (0);
		Assert(That(w1) == 1);
		Assert(That(u1) == 1);
		Assert(That(w2) == 1);
		Assert(That(u2) == 0);
		Assert(That(n) == 3);
		Assert(That(Reactive.IsWatched(s1)) == false);
		Assert(That(Reactive.IsWatched(s2)) == true);
		Assert(That(d) == 3);

		Assert(That(w.RunPending()).SequenceEqual([e]));
		syncContext.RunQueue();

		Assert(That(c.Value) == 10);
		Assert(That(w1) == 1);
		Assert(That(u1) == 1);
		Assert(That(w2) == 1);
		Assert(That(u2) == 1);
		Assert(That(n) == 3);
		Assert(That(Reactive.IsWatched(s1)) == false);
		Assert(That(Reactive.IsWatched(s2)) == false);
		Assert(That(d) == 4);
		Assert(That(w.RunPending()).SequenceEqual(Array.Empty<Effect>()));
	}

	[TestMethod(DisplayName = "It can unwatch multiple signals")]
	public void Test3()
	{
		ImmutableArray<ReactiveValue<int>> signals = [.. Enumerable.Range(0, 7).Select(x => new ReactiveValue<int>(x))];
		ImmutableArray<Effect> effects = [.. signals.Select(x => new Effect(() => _ = x.Value))];

		var notify = ProxyAction.For(() => { });

		var watcher = new TestWatcher(notify.Invoke);

		void expectSources(IEnumerable<Effect> expected)
		{
			var sources = ReactiveDiagnostics.GetSources(watcher);
			Assert(That(sources).SequenceEqual(expected));
		}

		watcher.Watch([.. effects]);
		expectSources(effects);

		var split =
			effects.Select((x, i) => (x, i))
			.ToLookup(x => x.i is 0 or 3 or 4 or 6 ? "unwatched" : "watched", x => x.x);

		var unwatched = split["unwatched"];
		var watched = split["watched"];

		watcher.Unwatch([.. unwatched]);
		expectSources(watched);

		var expectedNotifyCalls = 0;
		foreach (var (signal, effect) in signals.Zip(effects))
		{
			signal.Value++;

			if (watched.Contains(effect)) { expectedNotifyCalls++; }

			Assert(That(notify.TimesCalled) == expectedNotifyCalls);

			watcher.RunPending();
		}
	}
}
