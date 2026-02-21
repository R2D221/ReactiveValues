using ReactiveValues.DataTypes;
using System.Collections;

namespace ReactiveValues.Markup;

public abstract class Children<TComponentBase, TParentComponent, TChildComponentBase>(MarkupProvider<TComponentBase> provider, TParentComponent parent) : IEnumerable
	where TComponentBase : class
	where TParentComponent : class, TComponentBase, new()
	where TChildComponentBase : class, TComponentBase
{
	protected abstract OrderedSetSegment<TComponentBase, TChildComponentBase> Container { get; }

	IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

	public void Add<TChildComponent>(TChildComponent child)
		where TChildComponent : class, TChildComponentBase, new()
	{
		var segment = Container.CreateChildSegment();
		segment.Add(child);
	}

	public void Add<TChildComponent>(Func<IEnumerable<TChildComponent>> func)
		where TChildComponent : class, TChildComponentBase, new()
	{
		var segment = Container.CreateChildSegment();

		var effect = new Effect(() =>
		{
			segment.Clear();

			foreach (var child in func())
			{
				segment.Add(child);
			}
		});

		provider.AttachEffectToComponentLifetime(parent, effect);
	}

	public void Add<TChildComponent>(Func<IReactiveCollection<TChildComponent>> func)
		where TChildComponent : class, TChildComponentBase, new()
	{
		var segment = Container.CreateChildSegment();

		provider.RenderChildren(segment, func);
	}
}