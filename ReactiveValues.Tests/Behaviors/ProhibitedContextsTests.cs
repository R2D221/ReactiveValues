using ReactiveValues;
using ReactiveValues.Exceptions;

namespace Signals.Tests.Behaviors;

[TestClass]
public sealed class ProhibitedContextsTests
{
	[TestMethod(DisplayName = "It allows writes during computed")]
	public void Test1()
	{
		var s = new ReactiveValue<int>(1);
		var c = new ReactiveFunc<int>(() => { s.Value++; return s.Value; });
		Assert(That(c.Value) == 2);
		Assert(That(s.Value) == 2);

		// Note: c is marked clean in this case, even though re-evaluating it
		// would cause it to change value (due to the set inside of it).
		Assert(That(c.Value) == 2);
		Assert(That(s.Value) == 2);

		s.Value = (3);

		Assert(That(c.Value) == 4);
		Assert(That(s.Value) == 4);
	}

	[TestMethod(DisplayName = "It disallows reads and writes during watcher notify")]
	public void Test2()
	{
		var s = new ReactiveValue<int>(1);
		var effect = new Effect(() => _ = s.Value);

		var w = new TestWatcher(() => _ = s.Value);

		w.Watch(effect);
		Assert(That(() => s.Value = (2)).Throws<FrozenReactiveGraphException>());
		w.Unwatch(effect);
		// Assert it doesn't throw
		s.Value = (3);

		var w2 = new TestWatcher(() => s.Value = (4));
		w2.Watch(effect);
		Assert(That(() => s.Value = (5)).Throws<FrozenReactiveGraphException>());
		w2.Unwatch(effect);
		s.Value = (3);
	}
}
