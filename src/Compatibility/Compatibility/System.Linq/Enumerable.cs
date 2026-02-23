#if !NETCOREAPP
internal static class EnumerableExtensions
{
	extension<T>(IEnumerable<T> source)
	{
		public IEnumerable<(T a, TSecond b)> Zip<TSecond>(IEnumerable<TSecond> second) =>
			source.Zip(second, (a, b) => (a, b));
	}
}
#endif