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
		node.@lock.EnterWriteLock();
		try
		{
			while (node.TryDequeueModifiedSource() is { } sourceWithValue)
			{
				var source = sourceWithValue.Reactive;

				bool sourceIsValid;
				using (source.@lock.ReadLockScope())
				{
					sourceIsValid = source.isValid;
				}

				if (sourceIsValid is false)
				{
					yield return ((EffectNode)sourceWithValue.Reactive).Effect;

					using (source.@lock.ReadLockScope())
					{
						sourceIsValid = source.isValid;
					}

					if (sourceIsValid is false)
					{
						node.MarkSourceModified(sourceWithValue.Reactive);
					}
				}
			}

			node.MarkValid();
		}
		finally
		{
			node.@lock.ExitWriteLock();
		}
	}
}

internal sealed class WatcherNode : Reactive<ValueTuple>
{
	private readonly Watcher watcher;

	public WatcherNode(Watcher watcher) : base(() => default, EqualityComparer<ValueTuple>.Default)
	{
		this.watcher = watcher;

		SetValue(default);
	}

	public void Watch(Effect effect)
	{
		if (frozen.Value) { throw new FrozenReactiveGraphException(); }

		using (@lock.WriteLockScope())
		{
			using (Reactive.Track(this))
			{
				effect.Run();
			}
		}

		effect.Node.UpdateIsWatched();
	}

	public void Watch(params ReadOnlySpan<Effect> effects)
	{
		if (frozen.Value) { throw new FrozenReactiveGraphException(); }

		using (@lock.WriteLockScope())
		{
			using (Reactive.Track(this))
			{
				foreach (var effect in effects)
				{
					effect.Run();
				}
			}
		}

		foreach (var effect in effects)
		{
			effect.Node.UpdateIsWatched();
		}
	}

	public void Unwatch(Effect effect)
	{
		if (frozen.Value) { throw new FrozenReactiveGraphException(); }

		var watcher = this;
		var effectNode = effect.Node;

		using (@lock.WriteLockScope())
		using (effectNode.@lock.WriteLockScope())
		{
			watcher.RemoveSource(effect.Node);
			effectNode.RemoveReceiver(watcher);

			if (watcher.modifiedSources.Count == 0)
			{
				watcher.MarkValid();
			}
		}

		effectNode.UpdateIsWatched();
	}

	public void Unwatch(params ReadOnlySpan<Effect> effects)
	{
		if (frozen.Value) { throw new FrozenReactiveGraphException(); }

		var watcher = this;

		using (@lock.WriteLockScope())
		{
			foreach (var effect in effects)
			{
				var effectNode = effect.Node;

				using (effectNode.@lock.WriteLockScope())
				{
					watcher.RemoveSource(effect.Node);
					effectNode.RemoveReceiver(watcher);
				}
			}

			if (watcher.modifiedSources.Count == 0)
			{
				watcher.MarkValid();
			}
		}

		foreach (var effect in effects)
		{
			effect.Node.UpdateIsWatched();
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
