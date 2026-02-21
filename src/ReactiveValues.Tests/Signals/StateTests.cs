using ReactiveValues;

namespace Signals.Tests.Signals;

[TestClass]
public sealed class StateTests
{
	[TestMethod(DisplayName = "It should work")]
	public void Test1()
	{
		var stateSignal = new ReactiveValue<int>(0);

		Assert(That(stateSignal.Value) == 0);

		stateSignal.Value = (10);

		Assert(That(stateSignal.Value) == 10);
	}

	[TestClass]
	public sealed class ComparisonSemantics
	{
		[TestMethod(DisplayName = "It should cache State by default equality comparer")]
		public void Test1()
		{
			var state = new ReactiveValue<double>(double.NaN);

			var computedSpy = ProxyFunc.For(() => state.Value);
			var computed = new ReactiveFunc<double>(computedSpy.Invoke);

			Assert(That(computedSpy.TimesCalled) == 0);
			Assert(That(computed.Value) == double.NaN);
			Assert(That(computedSpy.TimesCalled) == 1);

			state.Value = (double.NaN);

			Assert(That(computed.Value) == double.NaN);
			Assert(That(computedSpy.TimesCalled) == 1);
		}

		[TestMethod(DisplayName = "It applies custom equality in State")]
		public void Test2()
		{
			var ecSpy = ProxyFunc.For(() => false);

			var state = new ReactiveValue<int>(1,
				EqualityComparer<int>.Create((_, _) => ecSpy.Invoke()));

			var computedSpy = ProxyFunc.For(() => state.Value);
			var computed = new ReactiveFunc<int>(computedSpy.Invoke);

			Assert(That(computedSpy.TimesCalled) == 0);
			Assert(That(ecSpy.TimesCalled) == 0);

			Assert(That(computed.Value) == 1);
			Assert(That(ecSpy.TimesCalled) == 0);
			Assert(That(computedSpy.TimesCalled) == 1);

			state.Value = (1);
			Assert(That(computed.Value) == 1);

			// Equality comparer is called 2 times because we cache the values of a Computed's
			// source and check equality again to decide if we need to recompute.
			Assert(That(ecSpy.TimesCalled) == 2);

			Assert(That(computedSpy.TimesCalled) == 2);
		}
	}
}
