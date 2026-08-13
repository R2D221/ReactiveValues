using System.Collections;

namespace ReactiveValues.DataTypes;

public sealed partial class ReactiveList<T> : IList<T>, IList, IReadOnlyList<T>, IReactiveCollection<T>
{
	private readonly Node marker;
	internal int version;

	private readonly ReactiveFunc<int> count;
	private readonly ReactiveFunc<Node?> first;
	private readonly ReactiveFunc<Node?> last;
	private readonly List<ReactiveFunc<Node>> indexer = [];

	public ReactiveList()
	{
		marker = new(this);
		marker.internalNext.Value = marker;
		marker.internalPrevious.Value = marker;

		count = new(() => marker.Index);

		first = new(() =>
			marker.internalNext.Value is { kind: ReactiveListNodeKind.Item } next
			? next
			: null
		);

		last = new(() =>
			marker.internalPrevious.Value is { kind: ReactiveListNodeKind.Item } previous
			? previous
			: null
		);
	}

	public ReactiveList(IEnumerable<T> collection) : this()
	{
		foreach (var item in collection)
		{
			Add(item);
		}
	}

	public int Count => count.Value;
	public Node? First => first.Value;
	public Node? Last => last.Value;

	public void Clear()
	{
		using (Reactive.Defer())
		using (Reactive.Untrack())
		{
			Node? current = marker.Next;
			while (current is not null)
			{
				var temp = current;
				current = current.Next;
				temp.Invalidate();
			}

			marker.internalNext.Value = marker;
			marker.internalPrevious.Value = marker;
			version++;
		}
	}

	public bool Contains(T value)
	{
		return Find(value) is not null;
	}

	public bool Remove(T value)
	{
		using (Reactive.Untrack())
		{
			var node = Find(value);
			if (node != null)
			{
				InternalRemoveNode(node);
				return true;
			}
			return false;
		}
	}

	#region Linked list

	public Node AddAfter(Node node, T value)
	{
		using (Reactive.Untrack())
		{
			ValidateNode(node);
			var result = new Node(value);
			InternalInsertNodeBefore(node.internalNext.Value!, result);
			return result;
		}
	}

	public void AddAfter(Node node, Node newNode)
	{
		using (Reactive.Untrack())
		{
			ValidateNode(node);
			ValidateNewNode(newNode);
			InternalInsertNodeBefore(node.internalNext.Value!, newNode);
		}
	}

	public Node AddBefore(Node node, T value)
	{
		using (Reactive.Untrack())
		{
			ValidateNode(node);
			var result = new Node(value);
			InternalInsertNodeBefore(node, result);
			return result;
		}
	}

	public void AddBefore(Node node, Node newNode)
	{
		using (Reactive.Untrack())
		{
			ValidateNode(node);
			ValidateNewNode(newNode);
			InternalInsertNodeBefore(node, newNode);
		}
	}

	public Node AddFirst(T value)
	{
		using (Reactive.Untrack())
		{
			var result = new Node(value);
			InternalInsertNodeBefore(marker.internalNext.Value!, result);
			return result;
		}
	}

	public void AddFirst(Node node)
	{
		using (Reactive.Untrack())
		{
			ValidateNewNode(node);
			InternalInsertNodeBefore(marker.internalNext.Value!, node);
		}
	}

	public Node AddLast(T value)
	{
		using (Reactive.Untrack())
		{
			var result = new Node(value);
			InternalInsertNodeBefore(marker, result);
			return result;
		}
	}

	public void AddLast(Node node)
	{
		using (Reactive.Untrack())
		{
			ValidateNewNode(node);
			InternalInsertNodeBefore(marker, node);
		}
	}

	public Node? Find(T value)
	{
		EqualityComparer<T> c = EqualityComparer<T>.Default;

		Node? current = marker.Next;
		while (current is not null)
		{
			if (c.Equals(current.Value, value))
			{
				return current;
			}

			current = current.Next;
		}

		return null;
	}

	public Node? FindLast(T value)
	{
		EqualityComparer<T> c = EqualityComparer<T>.Default;

		Node? current = marker.Previous;
		while (current is not null)
		{
			if (c.Equals(current.Value, value))
			{
				return current;
			}

			current = current.Previous;
		}

		return null;
	}

	public void Remove(Node node)
	{
		using (Reactive.Untrack())
		{
			ValidateNode(node);
			InternalRemoveNode(node);
		}
	}

	public void RemoveFirst()
	{
		using (Reactive.Untrack())
		{
			if (First is not {/*notnull*/} first) { throw new InvalidOperationException(); }

			InternalRemoveNode(first);
		}
	}

	public void RemoveLast()
	{
		using (Reactive.Untrack())
		{
			if (Last is not {/*notnull*/} last) { throw new InvalidOperationException(); }

			InternalRemoveNode(last);
		}
	}

	private void InternalInsertNodeBefore(Node node, Node newNode)
	{
		using (Reactive.Defer())
		{
			newNode.internalNext.Value = node;
			newNode.internalPrevious.Value = node.internalPrevious.Value;
			node.internalPrevious.Value!.internalNext.Value = newNode;
			node.internalPrevious.Value = newNode;

			version++;
		}
	}

	internal void InternalRemoveNode(Node node)
	{
		using (Reactive.Defer())
		{
			//Debug.Assert(node.list == this, "Deleting the node from another list!");
			//Debug.Assert(head != null, "This method shouldn't be called on empty list!");

			node.internalNext.Value!.internalPrevious.Value = node.internalPrevious.Value;
			node.internalPrevious.Value!.internalNext.Value = node.internalNext.Value;

			node.Invalidate();
			version++;
		}
	}

	internal static void ValidateNewNode(Node node)
	{
		if (node.kind is ReactiveListNodeKind.Marker || node.List != null)
		{
			throw new InvalidOperationException();
		}
	}

	internal void ValidateNode(Node node)
	{
		if (node.kind is ReactiveListNodeKind.Marker || node.List != this)
		{
			throw new InvalidOperationException();
		}
	}

	#endregion

	private Node InternalGetAt(int index)
	{
		if (indexer.Count > index)
		{
			return indexer[index].Value;
		}

		ReactiveFunc<Node> computed;

		if (index == 0)
		{
			computed = new(() => marker.internalNext.Value!);
		}
		else
		{
			computed = new(() =>
			{
				var previous = InternalGetAt(index - 1);

				if (previous == marker) { throw new IndexOutOfRangeException(); }

				return previous.internalNext.Value!;
			});
		}

		var node = computed.Value;

		indexer.Add(computed);

		return node;
	}

	public Node GetAt(int index)
	{
		var node = InternalGetAt(index);

		if (node == marker) { throw new IndexOutOfRangeException(); }

		return node;
	}

	#region Array list

	public T this[int index]
	{
		get => GetAt(index).Value;
		set
		{
			using (Reactive.Untrack())
			{
				GetAt(index).Value = value;
			}
		}
	}

	public void Add(T item)
	{
		using (Reactive.Untrack())
		{
			var newNode = new Node(item);
			InternalInsertNodeBefore(marker, newNode);
		}
	}

	public int IndexOf(T item) => Find(item)?.Index ?? -1;

	public void Insert(int index, T item)
	{
		using (Reactive.Untrack())
		{
			var newNode = new Node(item);
			var referenceNode = InternalGetAt(index);
			InternalInsertNodeBefore(referenceNode, newNode);
		}
	}

	public int LastIndexOf(T item) => FindLast(item)?.Index ?? -1;

	public void RemoveAt(int index)
	{
		using (Reactive.Untrack())
		{
			InternalRemoveNode(GetAt(index));
		}
	}

	#endregion

	#region Other interface members

	bool IList.IsFixedSize => false;
	bool ICollection<T>.IsReadOnly => false;
	bool IList.IsReadOnly => false;
	bool ICollection.IsSynchronized => false;
	object ICollection.SyncRoot => this;

	object? IList.this[int index]
	{
		get => this[index];
		set => this[index] = (T)value!;
	}

	int IList.Add(object? value)
	{
		using (Reactive.Untrack())
		{
			var node = AddLast((T)value!);
			return node.Index;
		}
	}

	bool IList.Contains(object? value) => Contains((T)value!);

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		using (Reactive.Untrack())
		{
			using var enumerator = GetEnumerator();

			for (var i = arrayIndex; i < array.Length && enumerator.MoveNext(); i++)
			{
				array[i] = enumerator.Current;
			}
		}
	}

	void ICollection.CopyTo(Array array, int index)
	{
		using (Reactive.Untrack())
		{
			using var enumerator = GetEnumerator();

			for (var i = index; i < array.Length && enumerator.MoveNext(); i++)
			{
				array.SetValue(enumerator.Current, i);
			}
		}
	}

	int IList.IndexOf(object? item) => IndexOf((T)item!);

	void IList.Insert(int index, object? item) => Insert(index, (T)item!);

	void IList.Remove(object? item) => Remove((T)item!);

	#endregion

	#region Enumerator
	public Enumerator GetEnumerator() => new Enumerator(this);

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public struct Enumerator : IEnumerator<T>
	{
		private readonly ReactiveList<T> list;
		private Node? node;
		private readonly int version;
		private T? current;

		internal Enumerator(ReactiveList<T> list)
		{
			this.list = list;
			version = list.version;
			node = list.marker.internalNext.Value;
			current = default;
		}

		public readonly T Current => current!;

		readonly object? IEnumerator.Current => current;

		public bool MoveNext()
		{
			if (version != list.version || node is null)
			{
				throw new InvalidOperationException();
			}

			if (node is { kind: ReactiveListNodeKind.Marker })
			{
				return false;
			}

			current = node.Value;
			node = node.internalNext.Value;
			return true;
		}

		void IEnumerator.Reset()
		{
			if (version != list.version)
			{
				throw new InvalidOperationException();
			}

			current = default;
			node = list.marker.internalNext.Value;
		}

		public readonly void Dispose() { }
	}

	#endregion

	IReactiveCollection<T>.INode? IReactiveCollection<T>.First => First;
	IReactiveCollection<T>.INode? IReactiveCollection<T>.Last => Last;
	int IReactiveCollection<T>.Version => version;
}
