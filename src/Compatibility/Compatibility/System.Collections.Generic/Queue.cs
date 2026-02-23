#if !NETCOREAPP
using System.Diagnostics.CodeAnalysis;

internal static partial class CompatibilityExtensions
{
	#region Queue<T>
	extension<T>(Queue<T> @this)
	{
		public bool TryDequeue([MaybeNullWhen(false)] out T result)
		{
			if (@this.Count == 0)
			{
				result = default;
				return false;
			}

			result = @this.Dequeue();
			return true;
		}

		public bool TryPeek([MaybeNullWhen(false)] out T result)
		{
			if (@this.Count == 0)
			{
				result = default;
				return false;
			}

			result = @this.Peek();
			return true;
		}
	}
	#endregion
}
#endif