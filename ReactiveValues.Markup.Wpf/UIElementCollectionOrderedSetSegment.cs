using ReactiveValues.DataTypes;
using System.Windows;
using System.Windows.Controls;

namespace ReactiveValues.Markup.Wpf;

internal sealed class UIElementCollectionOrderedSetSegment : ArrayListOrderedSetSegment<UIElement, UIElement>
{
	private readonly UIElement owner;
	private readonly UIElementCollection collection;

	public UIElementCollectionOrderedSetSegment(UIElement owner, UIElementCollection collection)
		: this(owner, collection, null)
	{
		if (collection.Count > 0)
		{
			throw new ArgumentException(message: null, paramName: nameof(collection));
		}
	}

	public UIElementCollectionOrderedSetSegment(UIElement owner, UIElementCollection collection, UIElementCollectionOrderedSetSegment? parent)
		: base(parent)
	{
		this.owner = owner;
		this.collection = collection;
	}

	protected override OrderedSetSegment<UIElement, UIElement> Constructor(OrderedSetSegment<UIElement, UIElement> parent) =>
		new UIElementCollectionOrderedSetSegment(owner, collection, (UIElementCollectionOrderedSetSegment)parent);

	public override UIElement Owner => owner;

	protected override void InnerInsert(UIElement item, int index)
	{
		collection.Insert(index, item);
	}

	protected override void InnerRemoveAt(int index)
	{
		collection.RemoveAt(index);
	}

	public new UIElementCollectionOrderedSetSegment CreateChildSegment() =>
		(UIElementCollectionOrderedSetSegment)base.CreateChildSegment();
}
