using ReactiveValues;
using System.Collections.Immutable;

namespace Signals.Tests;

internal sealed class TestWatcher(Action callback) : Watcher()
{
	protected override void OnNotified() => callback();

	public ImmutableArray<Effect> RunPending()
	{
		var builder = ImmutableArray.CreateBuilder<Effect>();

		foreach (var effect in GetPending())
		{
			effect.Run();
			builder.Add(effect);
		}

		return builder.ToImmutable();
	}
}
