using ReactiveValues;

namespace Signals.Tests.Behaviors;

[TestClass]
public sealed class CustomEqualityTests
{
	[TestMethod(DisplayName = "It works for State")]
	public void Test1()
	{
		var answer = true;

		var s = new ReactiveValue<int>(1,
			EqualityComparer<int>.Create((_, _) => answer));

		var n = 0;
		var c = new ReactiveFunc<int>(() => { n++; return s.Value; });

		Assert(That(c.Value) == 1);
		Assert(That(n) == 1);

		s.Value = (2);
		Assert(That(s.Value) == 1);
		Assert(That(c.Value) == 1);
		Assert(That(n) == 1);

		answer = false;
		s.Value = (2);
		Assert(That(s.Value) == 2);
		Assert(That(c.Value) == 2);
		Assert(That(n) == 2);

		s.Value = (2);
		Assert(That(s.Value) == 2);
		Assert(That(c.Value) == 2);
		Assert(That(n) == 3);
	}

	[TestMethod(DisplayName = "It works for Computed")]
	public void Test2()
	{
		var answer = true;
		var value = 1;
		var u = new ReactiveValue<int>(1);

		var s = new ReactiveFunc<int>(() => { _ = u.Value; return value; },
			EqualityComparer<int>.Create((_, _) => answer));
		var n = 0;
		var c = new ReactiveFunc<int>(() => { n++; return s.Value; });

		Assert(That(c.Value) == 1);
		Assert(That(n) == 1);

		u.Value = (2);
		value = 2;
		Assert(That(s.Value) == 1);
		Assert(That(c.Value) == 1);
		Assert(That(n) == 1);

		answer = false;
		u.Value = (3);
		Assert(That(s.Value) == 2);
		Assert(That(c.Value) == 2);
		Assert(That(n) == 2);

		u.Value = (4);
		Assert(That(s.Value) == 2);
		Assert(That(c.Value) == 2);
		Assert(That(n) == 3);
	}

	//[TestMethod("It does not leak tracking information")]
	//public void Test3()
	//{
	//	var exact = Signal.State(1.0);
	//	var epsilon = Signal.State(0.1);
	//	var counter = Signal.State(1);

	//	var cutoffCalledTimes = 0;
	//	bool cutoff(double a, double b) { cutoffCalledTimes++; return Math.Abs(a - b) < epsilon.Value; }

	//	var innerFnCalledTimes = 0;
	//	double innerFn() { innerFnCalledTimes++; return exact.Value; }
	//	var inner = Signal.Computed(innerFn,
	//		EqualityComparer<double>.Create(cutoff));

	//	var outerFnCalledTimes = 0;
	//	double outerFn()
	//	{
	//		outerFnCalledTimes++;
	//		counter.Value;
	//		return inner.Value;
	//	}
	//	var outer = Signal.Computed(outerFn);

	//	outer.Value;

	//	// Everything runs the first time.
	//	Assert.AreEqual(actual: innerFnCalledTimes, expected: 1);
	//	Assert.AreEqual(actual: outerFnCalledTimes, expected: 1);

	//	exact.Value = (2);
	//	counter.Value = (2);
	//	outer.Value;

	//	// `outer` reruns because `counter` changed, `inner` reruns when called by
	//	// `outer`, and `cutoff` is called for the first time.
	//	Assert.AreEqual(actual: innerFnCalledTimes, expected: 2);
	//	Assert.AreEqual(actual: outerFnCalledTimes, expected: 2);
	//	Assert.AreEqual(actual: cutoffCalledTimes, expected: 1);

	//	epsilon.Value = (0.2);
	//	outer.Value;

	//	// Changing something `cutoff` depends on makes `inner` need to rerun, but
	//	// (since the new and old values are equal) not `outer`.
	//	Assert.AreEqual(actual: innerFnCalledTimes, expected: 3);
	//	Assert.AreEqual(actual: outerFnCalledTimes, expected: 2);
	//	Assert.AreEqual(actual: cutoffCalledTimes, expected: 2);
	//}
}
