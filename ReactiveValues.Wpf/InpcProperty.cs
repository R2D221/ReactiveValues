using System.Collections.Concurrent;
using System.ComponentModel;

namespace ReactiveValues.Wpf;

internal sealed class InpcProperty<TDataContext, TValue>(
	ReactiveFunc<TDataContext> reactiveDataContext,
	Func<TDataContext, TValue> getter,
	Action<TDataContext, TValue> setter)
	: INotifyPropertyChanged
{
	private readonly ReactiveFunc<TValue> getter = new(() => getter(reactiveDataContext.Value));

	public InpcProperty(ReactiveFunc<TDataContext> reactiveDataContext, Func<TDataContext, TValue> getter)
		: this(reactiveDataContext, getter, (_, _) => throw new NotSupportedException()) { }

	public TValue Value
	{
		get => getter.Value;
		set => setter(reactiveDataContext.Value, value);
	}

	private readonly ConcurrentDictionary<PropertyChangedEventHandler, (Watcher watcher, Effect effect)>
		propertyChanged = new();

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

					var args = new PropertyChangedEventArgs(nameof(Value));
					var effect = Reactive.EventEffect(getter, () => value(this, args));

					watcher.Watch(effect);

					return (watcher, effect);
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

			result.watcher.Unwatch(result.effect);
		}
	}
}
