using ReactiveValues;
using ReactiveValues.Exceptions;

namespace Signals.Tests.Behaviors;

[TestClass]
public sealed class CyclesTests
{
	[TestMethod(DisplayName = "It detects trivial cycles")]
	public void Test1()
	{
		ReactiveFunc<int?>? c = null;
		c = new(() => c?.Value);

		Assert(That(() => _ = c.Value).Throws<CircularReferenceException>());
	}

	[TestMethod(DisplayName = "It detects slightly larger cycles")]
	public void Test2()
	{
		ReactiveFunc<int?>? c = null;
		ReactiveFunc<int?>? c2 = null;
		ReactiveFunc<int?>? c3 = null;

		c = new(() => c2?.Value);
		c2 = new(() => c?.Value);
		c3 = new(() => c2?.Value);

		Assert(That(() => _ = c3.Value).Throws<CircularReferenceException>());
	}
}
