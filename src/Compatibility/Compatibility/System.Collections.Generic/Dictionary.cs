#if !NETCOREAPP
using System.Diagnostics.CodeAnalysis;

internal static partial class CompatibilityExtensions
{
	#region Dictionary<TKey, TValue>
	extension<TKey, TValue>(Dictionary<TKey, TValue> @this)
		where TKey : notnull
	{
		public bool TryAdd(TKey key, TValue value)
		{
			if (@this.ContainsKey(key))
			{
				return false;
			}
			else
			{
				@this.Add(key, value);
				return true;
			}
		}

		public bool Remove(
			TKey key,
			[MaybeNullWhen(false)] out TValue value)
		{
			if (@this.TryGetValue(key, out value))
			{
				@this.Remove(key);
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	extension<TKey, TValue>(KeyValuePair<TKey, TValue> @this)
		where TKey : notnull
	{
		public void Deconstruct(out TKey key, out TValue value)
		{
			key = @this.Key;
			value = @this.Value;
		}
	}
	#endregion
}
#endif