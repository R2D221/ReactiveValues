using System.Collections.Concurrent;
using System.ComponentModel;

namespace ReactiveValues.DataTypes;

partial class ReactiveObject : INotifyPropertyChanged
{
	private readonly ConcurrentDictionary<PropertyChangedEventHandler, (Watcher watcher, ConcurrentDictionary<string, Effect> effects)>
		propertyChanged = new();

	private void HookPropertyChanged(string name, Reactive reactive)
	{
		foreach (var entry in propertyChanged)
		{
			var @event = entry.Key;
			var watcher = entry.Value.watcher;
			var effects = entry.Value.effects;

			CreateEffect(watcher, @event, name, effects, reactive);
		}
	}

	private void CreateEffect(Watcher watcher, PropertyChangedEventHandler @event, string name, ConcurrentDictionary<string, Effect> effects, Reactive reactive)
	{
		_ = effects.GetOrAdd(
			name,
			name =>
			{
				var args = new PropertyChangedEventArgs(name);

				var effect = Reactive.EventEffect(reactive, () => @event(this, args));

				watcher.Watch(effect);

				return effect;
			}
			);
	}

	event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			if (value is null) { return; }

			_ = propertyChanged.AddOrUpdate(
				value,
				value =>
				{
					var watcher = EventHandlerWatcher.Current;
					var effects = new ConcurrentDictionary<string, Effect>();

					foreach (var property in properties)
					{
						CreateEffect(watcher, value, property.Key, effects, property.Value);
					}

					return (watcher, effects);
				},
				(value, _) => throw new InvalidOperationException()
				);
		}

		remove
		{
			if (value is null) { return; }

			if (propertyChanged.TryRemove(value, out var result) is false)
			{
				throw new InvalidOperationException();
			}

			foreach (var effect in result.effects.Values)
			{
				result.watcher.Unwatch(effect);
			}
		}
	}
}