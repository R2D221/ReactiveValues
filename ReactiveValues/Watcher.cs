using ReactiveValues.Exceptions;
using System.Diagnostics;

namespace ReactiveValues;

public abstract class Watcher
{
	internal readonly WatcherNode node;

	public Watcher()
	{
		node = new(this);
	}

	public void Watch(Effect effect) => node.Watch(effect);

	public void Watch(params ReadOnlySpan<Effect> effects) => node.Watch(effects);

	public void Unwatch(Effect effect) => node.Unwatch(effect);

	public void Unwatch(params ReadOnlySpan<Effect> effects) => node.Unwatch(effects);

	protected internal abstract void OnNotified();

	protected IEnumerable<Effect> GetPending()
	{
		using var i = node.GetInternals(LockAction.Recompute);

		while (i.TryDequeueModifiedSource() is { } sourceWithValue)
		{
			using var source = sourceWithValue.Reactive.GetInternals(LockAction.Recompute);

			if (source.IsValid is false)
			{
				yield return ((EffectNode)sourceWithValue.Reactive).Effect;

				if (source.IsValid is false)
				{
					i.MarkSourceModified(sourceWithValue.Reactive);
				}
			}
		}

		i.MarkValid();
	}
}

internal sealed class WatcherNode : Reactive<ValueTuple>
{
	private readonly Watcher watcher;

	public WatcherNode(Watcher watcher) : base(() => default, EqualityComparer<ValueTuple>.Default)
	{
		this.watcher = watcher;

		using var i = GetInternals(LockAction._);
		i.SetValue(default);
	}

	public void Watch(Effect effect)
	{
		if (frozen.Value) { throw new FrozenReactiveGraphException(); }

		using var i = GetInternals(LockAction.Recompute);

		using (Reactive.Track(i))
		{
			effect.Run();

			using var x = effect.Node.GetInternals(LockAction.Recompute);
			x.UpdateIsWatched();
		}
	}

	public void Watch(params ReadOnlySpan<Effect> effects)
	{
		if (frozen.Value) { throw new FrozenReactiveGraphException(); }

		using var i = GetInternals(LockAction.Recompute);

		using (Reactive.Track(i))
		{
			foreach (var effect in effects)
			{
				effect.Run();

				using var x = effect.Node.GetInternals(LockAction.Recompute);
				x.UpdateIsWatched();
			}
		}
	}

	public void Unwatch(Effect effect)
	{
		if (frozen.Value) { throw new FrozenReactiveGraphException(); }

		using var watcher = GetInternals(LockAction._);

		using var effectInternals = effect.Node.GetInternals(LockAction._);

		watcher.RemoveSource(effect.Node);
		effectInternals.RemoveReceiver(this);

		effectInternals.UpdateIsWatched();

		if (watcher.ModifiedSources.Any() is false)
		{
			watcher.MarkValid();
		}
	}

	public void Unwatch(params ReadOnlySpan<Effect> effects)
	{
		if (frozen.Value) { throw new FrozenReactiveGraphException(); }

		using var watcher = GetInternals(LockAction._);

		foreach (var effectNode in effects)
		{
			using var effect = effectNode.Node.GetInternals(LockAction._);

			watcher.RemoveSource(effectNode.Node);
			effect.RemoveReceiver(this);

			effect.UpdateIsWatched();
		}

		if (watcher.ModifiedSources.Any() is false)
		{
			watcher.MarkValid();
		}
	}

	protected sealed override void Notify()
	{
		if (Reactive.defer.Value is {/*notnull*/} deferred)
		{
			deferred.Add(watcher);
		}
		else
		{
			throw new UnreachableException();
		}
	}
}
