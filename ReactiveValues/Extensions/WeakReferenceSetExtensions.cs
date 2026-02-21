using System.Diagnostics.CodeAnalysis;

//internal static class WeakReferenceSetExtensions
//{

//}

//internal readonly struct WeakEntry<T> : IEquatable<WeakEntry<T>> where T : class
//{
//	private readonly object? value;

//	public WeakEntry(T value) => this.value = value;

//	public WeakEntry(WeakReference<T> weakValue) => this.value = weakValue;

//	public T? Value => GetValue(this);

//	private static T? GetValue(WeakEntry<T> entry) =>
//		entry.value switch
//		{
//			T t => t,
//			WeakReference<T> w when w.TryGetTarget(out var tt) => tt,
//			_ => null,
//		};

//	public bool Equals(WeakEntry<T> other) =>
//		ReferenceEquals(GetValue(this), GetValue(other));

//	public override bool Equals([NotNullWhen(true)] object? obj) =>
//		obj is WeakEntry<T> other && Equals(other);

//	public override int GetHashCode() =>
//		GetValue(this)?.GetHashCode() ?? 0;
//}

internal sealed class WeakEqualityComparer<T> : EqualityComparer<WeakReference<T>>
	where T : class
{
	private static readonly EqualityComparer<T?> equality = EqualityComparer<T?>.Default;

	public static readonly WeakEqualityComparer<T> Instance = new();

	public override bool Equals(WeakReference<T>? x, WeakReference<T>? y)
	{
		T? xx = null;
		T? yy = null;

		if (x is not null) { _ = x.TryGetTarget(out xx); }
		if (y is not null) { _ = y.TryGetTarget(out yy); }

		return equality.Equals(xx, yy);
	}

	public override int GetHashCode([DisallowNull] WeakReference<T> obj)
	{
		return obj.TryGetTarget(out var xx) ? xx.GetHashCode() : 0;
	}
}
