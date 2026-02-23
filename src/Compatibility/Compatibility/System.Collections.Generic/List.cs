#if !NETCOREAPP
internal static partial class CompatibilityExtensions
{
	extension<T>(IEnumerable<T> source)
	{
		public List2<T> ToList() => [.. source];
	}
}

namespace System.Collections.Generic
{
	internal sealed class List2<T> : List<T>
	{
		public List2() : base() { }
		public List2(int capacity) : base(capacity) { }
		public List2(IEnumerable<T> collection) : base(collection) { }

		public List2<T> Slice(int index, int length) =>
			[.. GetRange(index, length)];
	}
}
#endif