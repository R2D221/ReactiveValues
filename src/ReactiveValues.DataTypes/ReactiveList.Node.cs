namespace ReactiveValues.DataTypes;

internal enum ReactiveListNodeKind { Item, Marker }

partial class ReactiveList<T>
{
	public sealed class Node : IReactiveCollection<T>.INode
	{
		internal readonly ReactiveListNodeKind kind;

		private readonly ReactiveFunc<ReactiveList<T>?> list;
		private readonly ReactiveValue<T> value;

		internal readonly ReactiveValue<Node?> internalPrevious = new(default);
		private readonly ReactiveFunc<Node?> previous;
		private ReactiveFunc<Node?> InitPrevious() =>
			new(() =>
				internalPrevious.Value is { kind: ReactiveListNodeKind.Item } previous
				? previous
				: null
			);

		internal readonly ReactiveValue<Node?> internalNext = new(default);
		private readonly ReactiveFunc<Node?> next;
		private ReactiveFunc<Node?> InitNext() =>
			new(() =>
				internalNext.Value is { kind: ReactiveListNodeKind.Item } next
				? next
				: null
			);

		private readonly ReactiveFunc<int> index;
		private ReactiveFunc<int> InitIndex() =>
			new(() =>
				internalPrevious.Value is { kind: ReactiveListNodeKind.Item } previous
				? previous.Index + 1
				: 0
			);

		internal Node(ReactiveList<T> list)
		{
			kind = ReactiveListNodeKind.Marker;

			this.list = new(() => list);
			value = null!;
			previous = InitPrevious();
			next = InitNext();
			index = InitIndex();
		}

		public Node(T value)
		{
			kind = ReactiveListNodeKind.Item;

			list = new(() => internalPrevious.Value?.List);
			this.value = new(value);
			previous = InitPrevious();
			next = InitNext();
			index = InitIndex();
		}

		public ReactiveList<T>? List => list.Value;

		public T Value
		{
			get => value.Value;
			set => this.value.Value = value;
		}

		public Node? Previous => previous.Value;

		public Node? Next => next.Value;

		public int Index => index.Value;

		internal void Invalidate()
		{
			internalNext.Value = null;
			internalPrevious.Value = null;
		}

		IReactiveCollection<T>? IReactiveCollection<T>.INode.List => List;
		IReactiveCollection<T>.INode? IReactiveCollection<T>.INode.Previous => Previous;
		IReactiveCollection<T>.INode? IReactiveCollection<T>.INode.Next => Next;
	}
}
