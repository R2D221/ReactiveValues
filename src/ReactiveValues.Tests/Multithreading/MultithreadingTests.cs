using System.Diagnostics;

namespace ReactiveValues.Tests.Multithreading;

[TestClass]
public sealed class MultithreadingTests
{
	[TestMethod(DisplayName = "Volatile works")]
	public async Task Test1()
	{
		var sw = Stopwatch.StartNew();
		var reactive = Reactive.Volatile(() => sw.Elapsed);

		var value1 = reactive.Value;

		await Task.Delay(100);

		var value2 = reactive.Value;

		Assert(That(value1) != value2);
	}
}
