namespace ReactiveValues;

internal sealed class AlwaysFalseEqualityComparer<T> : EqualityComparer<T>
{
	public static readonly AlwaysFalseEqualityComparer<T> Instance = new();

	public override bool Equals(T? x, T? y) => false;

	public override int GetHashCode(T obj) => throw new NotSupportedException();
}