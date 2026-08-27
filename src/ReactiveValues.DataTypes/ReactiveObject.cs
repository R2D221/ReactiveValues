using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ReactiveValues.DataTypes;

public abstract partial class ReactiveObject
{
	private readonly ConcurrentDictionary<PropertyInfo, ReactiveProperty> properties = new();

	//private readonly ConditionalWeakTable<Delegate, Reactive> debounces = new();
	//private readonly ConditionalWeakTable<Delegate, Reactive> throttles = new();

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

	protected ReactiveProperty<T> Property<T>(Expression<Func<T>> expr, [CallerMemberName] string? name = null)
	{
		_ =
			expr.Body is MemberExpression
			{
				Member: PropertyInfo property,
				Expression: ConstantExpression
				{
					Value: var value
				},
			}
			&& value == this
			&& property.Name == name
			? true
			: throw new InvalidOperationException($"Invalid expression '{expr}' for property '{name}'.");

		return (ReactiveProperty<T>)properties.GetOrAdd(property, property => new ReactiveProperty<T>(this, property.Name));
	}
}
