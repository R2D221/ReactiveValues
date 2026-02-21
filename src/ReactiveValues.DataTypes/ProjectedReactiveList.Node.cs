namespace ReactiveValues.DataTypes;

partial class ProjectedReactiveList<TSource, TResult>
{
	public sealed class Node : IReactiveCollection<TResult>.INode
	{
		private readonly ReactiveFunc<ProjectedReactiveList<TSource, TResult>?> list;
		private readonly ReactiveFunc<TResult> value;
		private readonly ReactiveFunc<Node?> previous;
		private readonly ReactiveFunc<Node?> next;
		private readonly ReactiveFunc<int> index;

		public Node(ProjectedReactiveList<TSource, TResult> list, IReactiveCollection<TSource>.INode sourceNode)
		{
			if (list.source != sourceNode.List)
			{
				throw new InvalidOperationException();
			}

			this.list = new(() => list.source == sourceNode.List ? list : null);
			value = new(() => list.selector(sourceNode.Value));
			previous = new(() => list.Project(sourceNode.Previous));
			next = new(() => list.Project(sourceNode.Next));
			index = new(() => Previous is { } previous ? previous.Index + 1 : 0);
		}

		public ProjectedReactiveList<TSource, TResult>? List => this.list.Value;
		public TResult Value => value.Value;
		public Node? Previous => previous.Value;
		public Node? Next => next.Value;
		public int Index => index.Value;

		IReactiveCollection<TResult>? IReactiveCollection<TResult>.INode.List => List;
		IReactiveCollection<TResult>.INode? IReactiveCollection<TResult>.INode.Previous => Previous;
		IReactiveCollection<TResult>.INode? IReactiveCollection<TResult>.INode.Next => Next;
	}
}