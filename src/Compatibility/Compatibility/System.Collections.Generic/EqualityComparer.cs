#if !NETCOREAPP
internal static partial class CompatibilityExtensions
{
	#region EqualityComparer<T>
	extension<T>(EqualityComparer<T>)
	{
		public static EqualityComparer<T> Create(Func<T?, T?, bool> equals, Func<T, int>? getHashCode = null)
		{
			getHashCode ??= _ => throw new NotSupportedException();

			return new DelegateEqualityComparer<T>(equals, getHashCode);
		}
	}

	private sealed class DelegateEqualityComparer<T>(
		Func<T?, T?, bool> equals,
		Func<T, int> getHashCode)
		: EqualityComparer<T>
	{
		public override bool Equals(T? x, T? y) =>
			equals(x, y);

		public override int GetHashCode(T obj) =>
			getHashCode(obj);
	}
	#endregion
}
#endif