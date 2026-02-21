using ReactiveValues.DataTypes;

namespace ReactiveValues.Markup.WindowsForms;

internal sealed class ControlCollectionOrderedSetSegment : ArrayListOrderedSetSegment<Control, Control>
{
	private readonly Control.ControlCollection collection;

	public ControlCollectionOrderedSetSegment(Control.ControlCollection collection)
		: this(collection, null)
	{
		if (collection.Count > 0)
		{
			throw new ArgumentException(message: null, paramName: nameof(collection));
		}
	}

	public ControlCollectionOrderedSetSegment(Control.ControlCollection collection, ControlCollectionOrderedSetSegment? parent)
		: base(parent)
	{
		this.collection = collection;
	}

	protected override OrderedSetSegment<Control, Control> Constructor(OrderedSetSegment<Control, Control> parent) =>
		new ControlCollectionOrderedSetSegment(collection, (ControlCollectionOrderedSetSegment)parent);

	public override Control Owner => collection.Owner;

	protected override void InnerInsert(Control item, int index)
	{
		collection.Add(item);
		collection.SetChildIndex(item, index);
	}

	protected override void InnerRemoveAt(int index)
	{
		collection.RemoveAt(index);
	}

	public new ControlCollectionOrderedSetSegment CreateChildSegment() =>
		(ControlCollectionOrderedSetSegment)base.CreateChildSegment();
}
