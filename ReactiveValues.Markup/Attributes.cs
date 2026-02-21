using FastExpressionCompiler;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace ReactiveValues.Markup;

public enum BindingMode
{
	OneWay,
	TwoWay,
}

public readonly struct Attributes<TComponentBase, TComponent> : IEnumerable
	where TComponentBase : class
	where TComponent : class, TComponentBase, new()
{
	internal readonly List<Action<TComponent>> actions = [];
	internal readonly List<Func<MarkupProvider<TComponentBase>, Action<TComponent>>> setbacks = [];

	public Attributes() { }

	IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

	public void Add<TAttribute>(Expression<Func<TComponent, TAttribute>> attribute, Func<TAttribute> value)
	{
		var reactive = new ReactiveFunc<TAttribute>(value);
		var expression = (Expression<Func<TAttribute>>)(() => reactive.Value);

		var action =
			Expression.Lambda<Action<TComponent>>(
				Expression.Assign(attribute.Body, Expression.Invoke(expression)),
				attribute.Parameters)
			.CompileFast();

		actions.Add(action);
	}

	public void Add<TAttribute>(Expression<Func<TComponent, TAttribute>> attribute, Expression<Func<TAttribute>> value, BindingMode mode)
	{
		if (mode == BindingMode.TwoWay)
		{
			_ =
				attribute is
				{
					Body: MemberExpression
					{
						Expression.NodeType: ExpressionType.Parameter,
						Member: PropertyInfo property,
					}
				}
				? true
				: throw new ArgumentException(paramName: nameof(attribute), message: "The expression must be of the form 'x => x.Property'.");

			var setBack =
				Expression.Lambda<Action<TComponent>>(
					Expression.Assign(value.Body, attribute.Body),
					attribute.Parameters)
				.CompileFast();

			setbacks.Add(provider =>
			{
				var onChanged = provider.CachedGetCallback(property);

				return (x => onChanged(x, () => setBack(x)));
			});
		}

		Add(attribute, value.CompileFast());
	}

	[Obsolete("Incorrect syntax por property binding.", true)]
	public void Add<TAttribute>(Func<TComponent, TAttribute> @event) => throw new NotSupportedException();

	public void Add(Action<TComponent> action) => actions.Add(action);
}
