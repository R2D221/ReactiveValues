internal static class WeakReferenceListExtensions
{
	public static void Add<T>(this List<WeakReference<T>> list, T item)
		where T : class
	{
		list.Add(new(item));
	}

	public static IEnumerable<T> EnumerateAlive<T>(this List<WeakReference<T>> list)
		where T : class
	{
		var freeIndex = 0;
		var current = 0;

		try
		{
			var size = list.Count;

			while (freeIndex < size)
			{
				if (list[freeIndex].TryGetTarget(out var item))
				{
					yield return item;
					freeIndex++;
				}
				else
				{
					break;
				}
			}

			if (freeIndex >= size) { yield break; }

			current = freeIndex + 1;
			while (current < size)
			{
				while (current < size)
				{
					if (list[current].TryGetTarget(out var item))
					{
						yield return item;
						break;
					}
					else
					{
						current++;
					}
				}

				if (current < size)
				{
					list[freeIndex++] = list[current++];
				}
			}
		}
		finally
		{
			if (current != 0)
			{
				list.RemoveRange(freeIndex, current - freeIndex);
			}
		}
	}

	public static bool Remove<T>(this List<WeakReference<T>> list, T item)
		where T : class
	{
		var index = list.FindIndex(@ref => @ref.TryGetTarget(out var x) && x == item);

		if (index < 0)
		{
			return false;
		}

		list.RemoveAt(index);
		return true;
	}

	public static void RemoveAll<T>(this List<WeakReference<T>> list, T item)
		where T : class
	{
		list.RemoveAll(@ref => @ref.TryGetTarget(out var x) is false || x == item);
	}
}
