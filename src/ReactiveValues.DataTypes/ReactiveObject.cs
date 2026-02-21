using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TypeNameFormatter;

namespace ReactiveValues.DataTypes;

public abstract class ReactiveObject : INotifyPropertyChanged
{
	private readonly ConcurrentDictionary<string, Reactive> properties = new();

	private readonly ConcurrentDictionary<PropertyChangedEventHandler, (Watcher watcher, ConcurrentDictionary<string, Effect> effects)>
		propertyChanged = new();

	private static TReactive Cast<TReactive, T>(Reactive reactive, string name)
		where TReactive : Reactive<T>
	{
		if (reactive is not TReactive value)
		{
			var reactiveName =
				typeof(TReactive) == typeof(ReactiveValue<T>) ? "stored value" :
				typeof(TReactive) == typeof(ReactiveFunc<T>) ? "computed value" :
				throw new Exception()
				;

			var typeName = typeof(T).GetFormattedName();

			throw new InvalidCastException($"Property '{name}' is not a {reactiveName} of type {typeName}.");
		}

		return value;
	}

	protected void Set<T>(Expression<Func<T>> expr, T value, [CallerMemberName] string? name = null)
	{
		ArgumentNullException.ThrowIfNull(name);
		Validate(expr, name);

		_ = properties.AddOrUpdate(
			name,
			(_, value) => new ReactiveValue<T>(value),
			(name, reactive, value) =>
			{
				var rvalue = Cast<ReactiveValue<T>, T>(reactive, name);
				rvalue.Value = value;
				return rvalue;
			},
			value);
	}

	private T Get<TReactive, T>(Reactive reactive, string name)
		where TReactive : Reactive<T>
	{
		var value = Cast<TReactive, T>(reactive, name);
		HookPropertyChanged(name, value);
		return value.Value;
	}

	protected T GetRequired<T>(Expression<Func<T>> expr, [CallerMemberName] string? name = null)
	{
		ArgumentNullException.ThrowIfNull(name);
		Validate(expr, name);

		if (properties.TryGetValue(name, out var reactive) is false)
		{
			throw new KeyNotFoundException($"Property '{name}' was not set.");
		}

		return Get<ReactiveValue<T>, T>(reactive, name);
	}

	[Conditional("DEBUG")]
	private void Validate(LambdaExpression expr, string name)
	{
		_ =
			expr.Body is MemberExpression
			{
				Member.Name: var memberName,
				Expression: ConstantExpression { Value: var value }
			}
			&&
			memberName == name
			&&
			value == this
			?
				true
			:
				throw new Exception();
	}

	protected T? Get<T>(Expression<Func<T>> expr, [CallerMemberName] string? name = null)
	{
		ArgumentNullException.ThrowIfNull(name);
		Validate(expr, name);

		var reactive = properties.GetOrAdd(name, (_) => new ReactiveValue<T?>(default));

		return Get<ReactiveValue<T>, T>(reactive, name);
	}

	protected T Get<T>(Expression<Func<T>> expr, Func<T> initialValue, [CallerMemberName] string? name = null)
	{
		ArgumentNullException.ThrowIfNull(name);
		Validate(expr, name);

		var reactive = properties.GetOrAdd(name, (_, initialValue) => new ReactiveValue<T>(initialValue()), initialValue);

		return Get<ReactiveValue<T>, T>(reactive, name);
	}

	protected T Computed<T>(Expression<Func<T>> expr, Func<T> valueFunc, [CallerMemberName] string? name = null)
	{
		ArgumentNullException.ThrowIfNull(name);
		Validate(expr, name);

		var reactive = properties.GetOrAdd(name, (_, valueFunc) => new ReactiveFunc<T>(valueFunc), valueFunc);

		return Get<ReactiveFunc<T>, T>(reactive, name);
	}

	protected static ICommand Command(
		Func<object?, bool> canExecute,
		Action<object?> execute)
	{
		return new Command(canExecute, execute);
	}

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
