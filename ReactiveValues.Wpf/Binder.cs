using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Expression = System.Linq.Expressions.Expression;

namespace ReactiveValues.Wpf;

file static class ReactiveDataContexts<TDataContext>
{
	private static readonly ConditionalWeakTable<object, ReactiveFunc<TDataContext>> dataContexts = new();

	public static ReactiveFunc<TDataContext> Get(object targetObject)
	{
		return dataContexts.GetValue(targetObject, obj =>
		{
			ReactiveFunc<TDataContext> result;

			switch (obj)
			{
				case FrameworkElement fe:
					{
						result = Reactive.FromProperty(
							() => (TDataContext)fe.DataContext,
							x => new DependencyPropertyChangedEventHandler((_, _) => x.Invalidate()),
							handler => fe.DataContextChanged += handler,
							handler => fe.DataContextChanged -= handler
							);
					}
					break;
				case FrameworkContentElement fce:
					{
						result = Reactive.FromProperty(
							() => (TDataContext)fce.DataContext,
							x => new DependencyPropertyChangedEventHandler((_, _) => x.Invalidate()),
							handler => fce.DataContextChanged += handler,
							handler => fce.DataContextChanged -= handler
							);
					}
					break;
				default: throw new NotSupportedException();
			}

			return result;
		});
	}
}

file static class Helpers
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetRequiredService<T>(IServiceProvider serviceProvider)
	{
#nullable disable
		return (T)serviceProvider.GetService(typeof(T));
#nullable restore
	}
}

public static class Binder<TDataContext>
{
	public abstract class Bind<TValue> : MarkupExtension
	{
		private readonly Binding binding;
		private readonly Func<TDataContext, TValue> getter;
		private readonly Action<TDataContext, TValue> setter;

		protected Bind(Func<TDataContext, TValue> getter)
		{
			binding = new Binding { Mode = BindingMode.OneWay };
			this.getter = getter;
			setter = (_, _) => throw new NotSupportedException();
		}

		protected Bind(Expression<Func<TDataContext, TValue>> getter, BindingMode mode)
		{
			binding = new Binding { Mode = mode };

			this.getter = getter.Compile();

			var param = Expression.Parameter(typeof(TValue));

			setter =
				Expression.Lambda<Action<TDataContext, TValue>>(
					body: Expression.Assign(getter.Body, param),
					parameters: [.. getter.Parameters, param])
				.Compile();
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var provideValueTarget = Helpers.GetRequiredService<IProvideValueTarget>(serviceProvider);

			var reactiveContext = ReactiveDataContexts<TDataContext>.Get(provideValueTarget.TargetObject);

			var wrapper = new InpcProperty<TDataContext, TValue>(
				reactiveContext,
				getter,
				setter);

			binding.Source = wrapper;
			binding.Path = new(nameof(wrapper.Value));

			return binding.ProvideValue(serviceProvider);
		}
	}
}
