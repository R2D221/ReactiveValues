using ReactiveValues;

namespace Signals.Tests.Behaviors;

[TestClass]
public sealed class PruningTests
{
	[TestMethod(DisplayName = "It only recalculates until things are equal")]
	public void Test1()
	{
		var s = new ReactiveValue<int>(0);
		var n = 0;
		var c = new ReactiveFunc<int>(() => { n++; return s.Value; });
		var n2 = 0;
		var c2 = new ReactiveFunc<int>(() => { n2++; _ = c.Value; return 5; });
		var n3 = 0;
		var c3 = new ReactiveFunc<int>(() => { n3++; return c2.Value; });

		Assert(That(n) == 0);
		Assert(That(n2) == 0);
		Assert(That(n3) == 0);

		Assert(That(c3.Value) == 5);
		Assert(That(n) == 1);
		Assert(That(n2) == 1);
		Assert(That(n3) == 1);

		s.Value = (1);
		Assert(That(n) == 1);
		Assert(That(n2) == 1);
		Assert(That(n3) == 1);

		Assert(That(c3.Value) == 5);
		Assert(That(n) == 2);
		Assert(That(n2) == 2);
		Assert(That(n3) == 1);
	}

	[TestMethod(DisplayName = "It does similar pruning for live signals")]
	public void Test2()
	{
		var s = new ReactiveValue<int>(0);
		var n = 0;
		var c = new ReactiveFunc<int>(() => { n++; return s.Value; });
		var n2 = 0;
		var c2 = new ReactiveFunc<int>(() => { n2++; _ = c.Value; return 5; });
		var n3 = 0;
		var c3 = new ReactiveFunc<int>(() => { n3++; return c2.Value; });

		var w = new TestWatcher(() => { });

		// We deviate from the spec here...
		// Should watcher.watch() evaluate the signal immediately?

		Assert(That(n) == 0);
		Assert(That(n2) == 0);
		Assert(That(n3) == 0);

		var effect = new Effect(() => _ = c3.Value);
		w.Watch(effect);

		Assert(That(c3.Value) == 5);
		Assert(That(n) == 1);
		Assert(That(n2) == 1);
		Assert(That(n3) == 1);

		s.Value = (1);
		Assert(That(n) == 1);
		Assert(That(n2) == 1);
		Assert(That(n3) == 1);

		Assert(That(w.RunPending().Length) == 1);

		Assert(That(c3.Value) == 5);
		Assert(That(n) == 2);
		Assert(That(n2) == 2);
		Assert(That(n3) == 1);

		Assert(That(w.RunPending().Length) == 0);
	}
}
