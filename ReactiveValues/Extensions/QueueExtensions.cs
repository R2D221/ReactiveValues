internal static class QueueExtensions
{
	extension<T>(Queue<T> @this)
	{
		public void RemoveAll(T item)
		{
			var equality = EqualityComparer<T>.Default;

			var count = @this.Count;

			for (var i = 0; i < count; i++)
			{
				var existing = @this.Dequeue();

				if (equality.Equals(item, existing))
				{
					continue;
				}

				@this.Enqueue(existing);
			}
		}
	}
}
