using System.Collections.Concurrent;
using System.Collections.Specialized;

namespace ReactiveValues.DataTypes;

partial class ReactiveList<T> : INotifyCollectionChanged
{
	private readonly ConcurrentDictionary<NotifyCollectionChangedEventHandler, (Watcher watcher, Effect effect)>
		collectionChanged = new();

	event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
	{
		add
		{
			if (value is null) { return; }

			_ = collectionChanged.AddOrUpdate(
				value,
				value =>
				{
					var watcher = EventHandlerWatcher.Current;

					var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

					var effect = Reactive.EventEffect(count, () => value(this, args));

					watcher.Watch(effect);

					return (watcher, effect);
				},
				(value, _) => throw new InvalidOperationException()
				);
		}

		remove
		{
			if (value is null) { return; }

			if (collectionChanged.TryRemove(value, out var result) is false)
			{
				throw new InvalidOperationException();
			}

			result.watcher.Unwatch(result.effect);
		}
	}
}
