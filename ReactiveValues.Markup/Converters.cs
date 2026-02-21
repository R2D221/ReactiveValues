using FastExpressionCompiler;
using System.Linq.Expressions;

namespace ReactiveValues.Markup;

file class Property<T>(Func<T> getter, Action<T> setter)
{
	public T Value
	{
		get => getter();
		set => setter(value);
	}
}

public static class Converters
{
	public static Expression<Func<TTo>> Convert<TFrom, TTo>(Expression<Func<TFrom>> value, Func<TFrom, TTo> convert, Func<TTo, TFrom> convertBack)
	{
		var getter = value.CompileFast();

		var parameter = Expression.Parameter(typeof(TFrom));

		var setter =
			Expression.Lambda<Action<TFrom>>(
				Expression.Assign(value.Body, parameter),
				parameter)
			.CompileFast();

		var x = new Property<TTo>(
			() => convert(getter()),
			value => setter(convertBack(value))
			);

		return () => x.Value;
	}

	public static Expression<Func<string>> Format<T>(Expression<Func<T>> value, string? format = null, IFormatProvider? formatProvider = null)
		where T : IFormattable
#if NETCOREAPP
		, IParsable<T>
#endif
		=>
		Convert(value,
			x => x.ToString(format, formatProvider),
			x => Parser.Parse<T>(x, formatProvider));

	public static Expression<Func<string>> EmptyIfNull(Expression<Func<string?>> value)
		=>
		Convert(value,
			x => x ?? "",
			x => x is "" ? null : x);
}
