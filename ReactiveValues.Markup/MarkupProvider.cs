using ReactiveValues.DataTypes;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ReactiveValues.Markup;

public abstract class MarkupProvider<TComponentBase>
	where TComponentBase : class
{
	private readonly ConditionalWeakTable<TComponentBase, List<Effect>> registeredEffects = new();
	protected readonly ConcurrentDictionary<PropertyInfo, Action<TComponentBase, Action>> twoWayCallbacks = new();

	protected internal abstract void InitLifetime(TComponentBase component);

	protected abstract Watcher GetWatcher(TComponentBase component);

	internal void AttachEffectToComponentLifetime(TComponentBase component, Effect effect)
	{
		effect.Run();
		if (effect.IsLive is false) { return; }

		GetWatcher(component).Watch(effect);
		registeredEffects.GetValue(component, _ => []).Add(effect);
	}

	internal void RemoveEffectFromComponentLifetime(TComponentBase component, Effect effect)
	{
		registeredEffects.GetValue(component, _ => []).Remove(effect);
		GetWatcher(component).Unwatch(effect);
	}

	protected void WatchAttachedEffects(TComponentBase component)
	{
		var watcher = GetWatcher(component);
		foreach (var effect in registeredEffects.GetValue(component, _ => []))
		{
			watcher.Watch(effect);
		}
	}

	protected void UnwatchAttachedEffects(TComponentBase component)
	{
		var watcher = GetWatcher(component);
		foreach (var effect in registeredEffects.GetValue(component, _ => []))
		{
			watcher.Unwatch(effect);
		}
	}

	internal void RenderChildren<TChildComponentBase, TChildComponent>(
		OrderedSetSegment<TComponentBase, TChildComponentBase> container,
		Func<IReactiveCollection<TChildComponent>> listFunc
		)
		where TChildComponentBase : class, TComponentBase
		where TChildComponent : class, TChildComponentBase, new()
	{
		var weakMap = new ConditionalWeakTable<IReactiveCollection<TChildComponent>.INode, TChildComponent>();

		var computedList = new ReactiveFunc<IReactiveCollection<TChildComponent>>(listFunc);

		AttachEffectToComponentLifetime(container.Owner, new Effect(() =>
		{
			var list = computedList.Value;
			var listLast = list.Last;

			using (Reactive.Untrack())
			{
				_ = RenderComponentOrNull(listLast);
			}
		}));

		(IReactiveCollection<TChildComponent>.INode item, TChildComponent component)?
			RenderComponentOrNull(
			IReactiveCollection<TChildComponent>.INode? item)
		{
			if (item is null) { return null; }
			return RenderComponent(item);
		}

		(IReactiveCollection<TChildComponent>.INode item, TChildComponent component)
			RenderComponent(
			IReactiveCollection<TChildComponent>.INode item)
		{
			if (weakMap.TryGetValue(item, out var component)) { return (item, component); }

			component = item.Value;
			weakMap.Add(item, component);

			var effectIsDisposed = false;

			Effect effect = null!;
			effect = new(() =>
			{
				if (effectIsDisposed) { return; }

				if (item.List != computedList.Value)
				{
					using (Reactive.Untrack())
					{
						try
						{
							effectIsDisposed = true;
							RemoveEffectFromComponentLifetime(container.Owner, effect);
							container.Remove(component);
							weakMap.Remove(item);
						}
						catch { }

						return;
					}
				}

				var previousItem = item.Previous;

				using (Reactive.Untrack())
				{
					var previous = RenderComponentOrNull(previousItem);

					static bool correct(
						OrderedSetSegment<TComponentBase, TChildComponentBase> container,
						TChildComponent? prev,
						TChildComponent curr)
						=>
						prev is null
						? container.FirstItem == curr
						: container.NextSibling(prev) == curr
						;

					var prev = previous;
					var curr = (item, component);

					while (correct(container, prev?.component, curr.component) is false)
					{
						container.Remove(curr.component);
						if (prev is { } prevComponent)
						{
							container.InsertBefore(curr.component, container.NextSibling(prevComponent.component));
						}
						else
						{
							container.InsertBefore(curr.component, container.FirstItem);
						}

						if (curr.item.Next is not { } next)
						{
							return;
						}

						prev = curr;
						curr = RenderComponent(next);
					}
				}
			});

			AttachEffectToComponentLifetime(container.Owner, effect);

			return (item, component);
		}
	}

	public void RegisterTwoWayCallback<TComponent, TAttribute>(Expression<Func<TComponent, TAttribute>> attribute, Action<TComponent, Action> xxx)
		where TComponent : TComponentBase
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

		twoWayCallbacks.AddOrUpdate(
			property,
			addValue: (obj, callback) => xxx((TComponent)obj, callback),
			updateValueFactory: (_, _) => throw new InvalidOperationException($"Expression '{attribute}' is already registered"));
	}

	internal Action<TComponentBase, Action> CachedGetCallback(PropertyInfo property) =>
		twoWayCallbacks.GetOrAdd(property, GetTwoWayCallback);

	protected abstract Action<TComponentBase, Action> GetTwoWayCallback(PropertyInfo property);
}
