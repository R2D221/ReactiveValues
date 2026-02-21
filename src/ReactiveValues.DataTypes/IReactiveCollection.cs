namespace ReactiveValues.DataTypes;

public interface IReactiveCollection<T> : IReadOnlyCollection<T>
{
	internal int Version { get; }

	INode? First { get; }
	INode? Last { get; }

	public interface INode
	{
		IReactiveCollection<T>? List { get; }
		T Value { get; }
		INode? Previous { get; }
		INode? Next { get; }
	}
}