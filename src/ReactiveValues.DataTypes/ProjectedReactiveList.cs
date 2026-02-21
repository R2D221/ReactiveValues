using System.Collections;
using System.Runtime.CompilerServices;

namespace ReactiveValues.DataTypes;

internal sealed partial class ProjectedReactiveList<TSource, TResult> : IReactiveCollection<TResult>, IEnumerable<TResult>
{
	private readonly ConditionalWeakTable<IReactiveCollection<TSource>.INode, Node> projections = new();

	internal readonly IReactiveCollection<TSource> source;
	internal readonly Func<TSource, TResult> selector;

	private readonly ReactiveFunc<Node?> first;
	private readonly ReactiveFunc<Node?> last;

	public ProjectedReactiveList(IReactiveCollection<TSource> source, Func<TSource, TResult> selector)
	{
		this.source = source;
		this.selector = selector;

		first = new(() => Project(source.First));
		last = new(() => Project(source.Last));
	}

	public int Count => source.Count;

	public Node? First => first.Value;
	public Node? Last => last.Value;

	internal Node? Project(IReactiveCollection<TSource>.INode? node) =>
		node is null ? null : projections.GetValue(node, node => new(this, node));

	IReactiveCollection<TResult>.INode? IReactiveCollection<TResult>.First => First;
	IReactiveCollection<TResult>.INode? IReactiveCollection<TResult>.Last => Last;
	int IReactiveCollection<TResult>.Version => source.Version;



	#region Enumerator
	public Enumerator GetEnumerator() => new Enumerator(this);

	IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator() => GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public struct Enumerator : IEnumerator<TResult>
	{
		private readonly ProjectedReactiveList<TSource, TResult> list;
		private Node? node;
		private readonly int version;
		private TResult? current;

		internal Enumerator(ProjectedReactiveList<TSource, TResult> list)
		{
			this.list = list;
			version = list.source.Version;
			node = list.First;
			current = default;
		}

		public readonly TResult Current => current!;

		readonly object? IEnumerator.Current => current;

		public bool MoveNext()
		{
			if (version != list.source.Version)
			{
				throw new InvalidOperationException();
			}

			if (node is null)
			{
				return false;
			}

			current = node.Value;
			node = node.Next;
			return true;
		}

		void IEnumerator.Reset()
		{
			if (version != list.source.Version)
			{
				throw new InvalidOperationException();
			}

			current = default;
			node = list.First;
		}

		public readonly void Dispose() { }
	}

	#endregion
}
