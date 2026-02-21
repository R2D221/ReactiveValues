using ReactiveValues;

namespace Signals.Tests.Misc;

[TestClass]
public sealed class UntrackTests
{
	[TestMethod(DisplayName = "It works")]
	public void Test1()
	{
		var state = new ReactiveValue<int>(1);
		var computed = new ReactiveFunc<int>(() =>
		{
			using (Reactive.Untrack())
			{
				return state.Value;
			}
		});

		Assert(That(computed.Value) == 1);

		state.Value = (2);
		Assert(That(computed.Value) == 1);
	}

	[TestMethod(DisplayName = "It works differently without untrack")]
	public void Test2()
	{
		var state = new ReactiveValue<int>(1);
		var computed = new ReactiveFunc<int>(() => state.Value);

		Assert(That(computed.Value) == 1);

		state.Value = (2);
		Assert(That(computed.Value) == 2);
	}
}
