//namespace ReactiveValues.DataTypes;

//internal sealed class FilteredReactiveList<TSource> : ReactiveObject, IReactiveList<TSource>
//{
//	sealed class Entry : ReactiveObject, IReactiveList<TSource>.Entry
//	{
//		private readonly IReactiveList<TSource>.Entry source;
//		private readonly Func<TSource, bool> predicate;

//		public Entry(IReactiveList<TSource>.Entry source, Func<TSource, bool> predicate)
//		{
//			this.source = source;
//			this.predicate = predicate;
//		}

//		public TSource Item => source.Item;
//		//public IReactiveList<TSource>.Entry? Previous => Computed(() => FindFiltered(source.Previous, predicate));
//		//public IReactiveList<TSource>.Entry? Next => Computed(() => FindFiltered(source.Next, predicate));
//		public bool Removed => source.Removed;
//		public int Index => Computed(() =>
//			Removed ? -1 :
//			Previous switch
//			{
//				null => 0,
//				{ } previous => previous.Index + 1,
//			});
//	}

//	private static Entry? FindFiltered(IReactiveList<TSource>.Entry? entry, Func<TSource, bool> predicate)
//	{
//		var current = entry;

//		while (true)
//		{
//			if (current is null) { return null; }
//			if (predicate(current.Item)) { return new Entry(current, predicate); }

//			current = current.Next;
//		}
//	}

//	private readonly IReactiveList<TSource> source;
//	private readonly Func<TSource, bool> predicate;

//	public FilteredReactiveList(IReactiveList<TSource> source, Func<TSource, bool> predicate)
//	{
//		this.source = source;
//		this.predicate = predicate;
//	}

//	public int Count => Computed(() =>
//	{
//		var current = First;
//		var count = 0;

//		while (current is not null)
//		{
//			count++;
//			current = current.Next;
//		}

//		return count;
//	});

//	public IReactiveList<TSource>.Entry? First => Computed(() => FindFiltered(source.First, predicate));
//}
