#if !NETCOREAPP
using System.Collections.Concurrent;

internal static partial class CompatibilityExtensions
{
	#region ConcurrentDictionary<TKey, TValue>
	extension<TKey, TValue>(ConcurrentDictionary<TKey, TValue> @this)
	{
		public TValue AddOrUpdate<TArg>(
			TKey key,
			Func<TKey, TArg, TValue> addValueFactory,
			Func<TKey, TValue, TArg, TValue> updateValueFactory,
			TArg factoryArgument)
		{
			return @this.AddOrUpdate(key, k => addValueFactory(k, factoryArgument), (k, v) => updateValueFactory(k, v, factoryArgument));
		}

		public TValue GetOrAdd<TArg>(
			TKey key,
			Func<TKey, TArg, TValue> valueFactory,
			TArg factoryArgument)
		{
			return @this.GetOrAdd(key, k => valueFactory(k, factoryArgument));
		}
	}
	#endregion
}
#endif