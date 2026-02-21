using ReactiveValues.DataTypes;

public static class ReactiveLinq
{
	extension<TSource>(IReactiveCollection<TSource> @this)
	{
		public IReactiveCollection<TResult> Select<TResult>(Func<TSource, TResult> selector)
		{
			return new ProjectedReactiveList<TSource, TResult>(@this, selector);
		}

		public IReactiveCollection<TSource> Where(Func<TSource, bool> predicate)
		{
			throw new NotImplementedException();
		}
	}
}
