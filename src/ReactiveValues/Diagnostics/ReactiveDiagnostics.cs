using System.Collections.Immutable;

namespace ReactiveValues.Diagnostics;

public static class ReactiveDiagnostics
{
	public static ImmutableArray<Effect> GetSources(Watcher watcher) =>
		[.. GetSources(watcher.node).Select(x => ((EffectNode)x).Effect)];

	public static ImmutableArray<Reactive> GetSources(Effect effect) =>
		GetSources(effect.Node);

	public static ImmutableArray<Reactive> GetSources(Reactive receiver)
	{
		using (receiver.@lock.ReadLockScope())
		{
			return [.. receiver.sources.Keys];
		}
	}

	public static ImmutableArray<Reactive> GetReceivers(Reactive reactive)
	{
		using (reactive.@lock.ReadLockScope())
		{
			return [.. reactive.EnumerateLiveReceivers()];
		}
	}
}
