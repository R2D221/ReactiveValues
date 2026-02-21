namespace ReactiveValues.DataTypes;

public abstract class ArrayListOrderedSetSegment<TComponentBase, TChildComponentBase> : OrderedSetSegment<TComponentBase, TChildComponentBase>
	where TComponentBase : class
	where TChildComponentBase : class, TComponentBase
{
	protected ArrayListOrderedSetSegment(ArrayListOrderedSetSegment<TComponentBase, TChildComponentBase>? parent)
		: base(parent) { }

	protected sealed override void InnerInsertBase(TChildComponentBase item, int index) =>
		InnerInsert(item, Delta + index);

	protected sealed override void InnerRemoveBase(TChildComponentBase item, int index) =>
		InnerRemoveAt(Delta + index);

	protected abstract void InnerInsert(TChildComponentBase item, int index);

	protected abstract void InnerRemoveAt(int index);
}

public abstract class OrderedSetSegment<TComponentBase, TChildComponentBase>
	where TComponentBase : class
	where TChildComponentBase : class, TComponentBase
{
	protected readonly OrderedSetSegment<TComponentBase, TChildComponentBase>? parent;
	private readonly (ReactiveList<TChildComponentBase> order, Dictionary<TChildComponentBase, ReactiveList<TChildComponentBase>.Node> set) items = ([], []);

	private OrderedSetSegment<TComponentBase, TChildComponentBase>? firstChild;
	private OrderedSetSegment<TComponentBase, TChildComponentBase>? lastChild;

	private OrderedSetSegment<TComponentBase, TChildComponentBase>? previousSibling;
	private OrderedSetSegment<TComponentBase, TChildComponentBase>? nextSibling;

	private readonly ReactiveFunc<int> itemCount;
	private readonly ReactiveFunc<int> delta;
	private readonly ReactiveFunc<TChildComponentBase?> lastItem;
	private readonly ReactiveFunc<TChildComponentBase?> previousItem;

	public OrderedSetSegment(OrderedSetSegment<TComponentBase, TChildComponentBase>? parent)
	{
		this.parent = parent;

		itemCount = new(() =>
		{
			var childrenCount = 0;
			var child = firstChild;
			while (child is not null)
			{
				childrenCount += child.ItemCount;
				child = child.nextSibling;
			}

			var itemCount = items.set.Count;

			return childrenCount + itemCount;
		});

		delta = new(() =>
			(previousSibling?.Delta + previousSibling?.ItemCount)
			??
			parent?.Delta
			??
			0
		);

		lastItem = new(() =>
			lastChild switch
			{
				{ } child => child.LastItem,
				null => items.order.Last?.Value,
			}
		);

		previousItem = new(() =>
			previousSibling switch
			{
				{ } prev => prev.LastItem ?? prev.PreviousItem,
				null => parent?.PreviousItem
			}
		);
	}

	protected abstract OrderedSetSegment<TComponentBase, TChildComponentBase> Constructor(OrderedSetSegment<TComponentBase, TChildComponentBase> parent);

	public abstract TComponentBase Owner { get; }

	public bool IsEmpty => items.set.Count == 0;

	public TChildComponentBase? FirstItem
	{
		get
		{
			using (Reactive.Untrack())
			{
				return items.order.First?.Value;
			}
		}
	}

	public OrderedSetSegment<TComponentBase, TChildComponentBase> CreateChildSegment()
	{
		if (items.set.Count > 0) { throw new Exception(); }

		var newChild = Constructor(this);

		if (lastChild is { } lastChildNotNull)
		{
			lastChildNotNull.nextSibling = newChild;
			newChild.previousSibling = lastChildNotNull;
		}
		else
		{
			firstChild = newChild;
		}

		lastChild = newChild;

		return newChild;
	}

	protected int ItemCount => itemCount.Value;

	protected int Delta => delta.Value;

	protected TChildComponentBase? LastItem => lastItem.Value;

	protected TChildComponentBase? PreviousItem => previousItem.Value;

	public bool Contains(TChildComponentBase item) => items.set.ContainsKey(item);

	public TChildComponentBase? PreviousSibling(TChildComponentBase item)
	{
		using (Reactive.Untrack())
		{
			var node = items.set[item];
			return node.Previous?.Value;
		}
	}

	public TChildComponentBase? NextSibling(TChildComponentBase item)
	{
		using (Reactive.Untrack())
		{
			var node = items.set[item];
			return node.Next?.Value;
		}
	}

	protected abstract void InnerInsertBase(TChildComponentBase item, int index); //innerList.Insert(Delta + index, item);
	protected abstract void InnerRemoveBase(TChildComponentBase item, int index); //innerList.RemoveAt(Delta + index);

	public void Add(TChildComponentBase item)
	{
		if (firstChild is not null) { throw new Exception(); }

		using (Reactive.Untrack())
		{
			var index = items.set.Count;

			var node = new ReactiveList<TChildComponentBase>.Node(item);
			items.set.Add(item, node);
			items.order.AddLast(node);

			InnerInsertBase(item, index);
		}
	}

	public void InsertBefore(TChildComponentBase newItem, TChildComponentBase? referenceItem)
	{
		if (referenceItem is null) { Add(newItem); return; }

		if (firstChild is not null) { throw new Exception(); }

		using (Reactive.Untrack())
		{
			var referenceNode = items.set[referenceItem];
			var index = referenceNode.Index;

			var newNode = new ReactiveList<TChildComponentBase>.Node(newItem);
			items.set.Add(newItem, newNode);
			items.order.AddBefore(referenceNode, newNode);

			InnerInsertBase(newItem, index);
		}
	}

	public void Remove(TChildComponentBase item)
	{
		if (firstChild is not null) { throw new Exception(); }

		using (Reactive.Untrack())
		{
			if (items.set.Remove(item, out var node) is false)
			{
				return;
			}

			var index = node.Index;

			items.order.Remove(node);

			InnerRemoveBase(item, index);
		}
	}

	public void Clear()
	{
		if (firstChild is not null) { throw new Exception(); }

		using (Reactive.Untrack())
		{
			var node = items.order.Last;
			while (node is not null)
			{
				InnerRemoveBase(node.Value, node.Index);
				node = node.Previous;
			}

			items.set.Clear();
			items.order.Clear();
		}
	}
}
