using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TypeNameFormatter;

namespace ReactiveValues.DataTypes;

public abstract partial class ReactiveObject
{
	private readonly ConcurrentDictionary<string, Reactive> properties = new();
	private readonly ConditionalWeakTable<Delegate, Reactive> debounces = new();
	private readonly ConditionalWeakTable<Delegate, Reactive> throttles = new();

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

	protected void Set<T>(Func<T> expr, T value, [CallerMemberName] string? name = null, [CallerArgumentExpression(nameof(expr))] string? exprString = null)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(exprString);
		Validate(name, exprString);

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

	protected T GetRequired<T>(Func<T> expr, [CallerMemberName] string? name = null, [CallerArgumentExpression(nameof(expr))] string? exprString = null)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(exprString);
		Validate(name, exprString);

		if (properties.TryGetValue(name, out var reactive) is false)
		{
			throw new KeyNotFoundException($"Property '{name}' was not set.");
		}

		return Get<ReactiveValue<T>, T>(reactive, name);
	}

	[Conditional("DEBUG")]
	private void Validate(string name, string exprString)
	{
		if ($"() => {name}" != exprString)
		{
			throw new Exception();
		}
	}

	protected T? Get<T>(Func<T> expr, [CallerMemberName] string? name = null, [CallerArgumentExpression(nameof(expr))] string? exprString = null)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(exprString);
		Validate(name, exprString);

		var reactive = properties.GetOrAdd(name, (_) => new ReactiveValue<T?>(default));

		return Get<ReactiveValue<T>, T>(reactive, name);
	}

	protected T Get<T>(Func<T> expr, Func<T> initialValue, [CallerMemberName] string? name = null, [CallerArgumentExpression(nameof(expr))] string? exprString = null)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(exprString);
		Validate(name, exprString);

		var reactive = properties.GetOrAdd(name, (_, initialValue) => new ReactiveValue<T>(initialValue()), initialValue);

		return Get<ReactiveValue<T>, T>(reactive, name);
	}

	protected T Computed<T>(Func<T> expr, Func<T> valueFunc, [CallerMemberName] string? name = null, [CallerArgumentExpression(nameof(expr))] string? exprString = null)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(exprString);
		Validate(name, exprString);

		var reactive = properties.GetOrAdd(name, (_, valueFunc) => new ReactiveFunc<T>(valueFunc), valueFunc);

		return Get<ReactiveFunc<T>, T>(reactive, name);
	}

	//protected T Debounce<T>(Func<T> expr, TimeSpan interval)
	//{
	//	var result =
	//		(ReactiveFunc<T>)debounces.GetValue(
	//			expr,
	//			expr => Reactive.Debounce(new ReactiveFunc<T>((Func<T>)expr), interval));

	//	return result.Value;
	//}

	//protected T Throttle<T>(Func<T> expr, TimeSpan interval)
	//{
	//	var result =
	//		(ReactiveFunc<T>)throttles.GetValue(
	//			expr,
	//			expr => Reactive.Throttle(new ReactiveFunc<T>((Func<T>)expr), interval));

	//	return result.Value;
	//}

	protected static ICommand Command(
		Func<object?, bool> canExecute,
		Action<object?> execute)
	{
		return new Command(canExecute, execute);
	}
}
