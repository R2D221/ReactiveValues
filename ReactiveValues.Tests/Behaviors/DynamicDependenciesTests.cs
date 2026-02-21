
using ReactiveValues;
using ReactiveValues.Diagnostics;

namespace Signals.Tests.Behaviors;

[TestClass]
public sealed class DynamicDependenciesTests
{
	[TestMethod(DisplayName = "It works live")]
	public void Test1() => Run(true);

	[TestMethod(DisplayName = "It works not live")]
	public void Test2() => Run(false);

	private static void Run(bool live)
	{
		var states = "abcdefgh".Select(s => new ReactiveValue<char>(s)).ToList();
		var sources = new ReactiveValue<List<ReactiveValue<char>>>(states);
		var computed = new ReactiveFunc<string>(() => string.Join("", sources.Value.Select(x => x.Value)));

		if (live)
		{
			var w = new TestWatcher(() => { });
			w.Watch(new Effect(() => _ = computed.Value));
		}

		Assert(That(computed.Value) == "abcdefgh");
		Assert(
			That(ReactiveDiagnostics.GetSources(computed)[1..].Cast<ReactiveValue<char>>())
			.SequenceEqual(states));

		sources.Value = (states[..5]);
		Assert(That(computed.Value) == "abcde");
		Assert(
			That(ReactiveDiagnostics.GetSources(computed)[1..].Cast<ReactiveValue<char>>())
			.SequenceEqual(states[..5]));

		sources.Value = (states[3..]);
		Assert(That(computed.Value) == "defgh");
		Assert(
			That(ReactiveDiagnostics.GetSources(computed)[1..].Cast<ReactiveValue<char>>())
			.SequenceEqual(states[3..]));

	}
}
