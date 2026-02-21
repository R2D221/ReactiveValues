using ReactiveValues;

namespace Signals.Tests.Signals;

[TestClass]
public sealed class ComputedTests
{
	[TestMethod(DisplayName = "It should work")]
	public void Test1()
	{
		var stateSignal = new ReactiveValue<int>(1);

		var computedSignal = new ReactiveFunc<int>(() =>
			stateSignal.Value * 2
		);

		Assert(That(computedSignal.Value) == 2);

		stateSignal.Value = (5);

		Assert(That(stateSignal.Value) == 5);
		Assert(That(computedSignal.Value) == 10);
	}

	[TestClass]
	public sealed class ComparisonSemantics
	{
		[TestMethod(DisplayName = "It should track Computed by default equality comparer")]
		public void Test1()
		{
			var state = new ReactiveValue<int>(1);
			var value = 5.0;
			var computed = new ReactiveFunc<double>(() => { _ = state.Value; return value; });

			var c2Spy = ProxyFunc.For(() => computed.Value);
			var c2 = new ReactiveFunc<double>(c2Spy.Invoke);

			Assert(That(c2Spy.TimesCalled) == 0);
			Assert(That(c2.Value) == 5);
			Assert(That(c2Spy.TimesCalled) == 1);

			state.Value = (2);
			Assert(That(c2.Value) == 5);
			Assert(That(c2Spy.TimesCalled) == 1);

			value = double.NaN;
			Assert(That(c2.Value) == 5);
			Assert(That(c2Spy.TimesCalled) == 1);

			state.Value = (3);
			Assert(That(c2.Value) == double.NaN);
			Assert(That(c2Spy.TimesCalled) == 2);

			state.Value = (4);
			Assert(That(c2.Value) == double.NaN);
			Assert(That(c2Spy.TimesCalled) == 2);
		}

		[TestMethod(DisplayName = "It applies custom equality in Computed")]
		public void Test2()
		{
			var s = new ReactiveValue<int>(5);

			var ecSpy = ProxyFunc.For(() => false);

			var c1 = new ReactiveFunc<int>(() => { _ = s.Value; return 1; },
				EqualityComparer<int>.Create((_, _) => ecSpy.Invoke()));

			var c2Spy = ProxyFunc.For(() => c1.Value);
			var c2 = new ReactiveFunc<int>(c2Spy.Invoke);

			Assert(That(c2Spy.TimesCalled) == 0);
			Assert(That(ecSpy.TimesCalled) == 0);

			Assert(That(c2.Value) == 1);
			Assert(That(ecSpy.TimesCalled) == 0);
			Assert(That(c2Spy.TimesCalled) == 1);

			s.Value = (10);
			Assert(That(c2.Value) == 1);

			// Equality comparer is called 2 times because we cache the values of a Computed's
			// source and check equality again to decide if we need to recompute.
			Assert(That(ecSpy.TimesCalled) == 2);

			Assert(That(c2Spy.TimesCalled) == 2);
		}

		[TestMethod(DisplayName = "It shouldn't evaluate twice for the same inputs")]
		public void Test3()
		{
			var number = new ReactiveValue<int>(0);

			var isEvenSpy = ProxyFunc.For(() => number.Value % 2 == 0);
			var isEven = new ReactiveFunc<bool>(isEvenSpy.Invoke);

			Assert(That(isEven.Value) == true);
			Assert(That(isEvenSpy.TimesCalled) == 1);

			number.Value = (1);

			Assert(That(isEven.Value) == false);
			Assert(That(isEvenSpy.TimesCalled) == 2);

			number.Value = (2);
			number.Value = (1);

			Assert(That(isEven.Value) == false);
			Assert(That(isEvenSpy.TimesCalled) == 2);
		}
	}
}
