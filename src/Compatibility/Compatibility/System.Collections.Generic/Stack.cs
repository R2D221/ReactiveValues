#if !NETCOREAPP
using System.Diagnostics.CodeAnalysis;

internal static partial class CompatibilityExtensions
{
	#region Stack<T>
	extension<T>(Stack<T> @this)
	{
		public bool TryPop([MaybeNullWhen(false)] out T result)
		{
			if (@this.Count == 0)
			{
				result = default;
				return false;
			}

			result = @this.Pop();
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