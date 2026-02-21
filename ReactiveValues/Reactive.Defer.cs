using System.ComponentModel;
using System.Runtime.ExceptionServices;

namespace ReactiveValues;

partial class Reactive
{
	internal static readonly ThreadLocal<HashSet<Watcher>?> defer = new();

	public static DeferScope Defer() => new(defer);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public readonly ref struct DeferScope
	{
		private readonly ThreadLocal<HashSet<Watcher>?> defer;
		private readonly HashSet<Watcher>? oldValue;

		[Obsolete("Do not use", true)]
		public DeferScope() => throw new InvalidOperationException();

		internal DeferScope(ThreadLocal<HashSet<Watcher>?> defer)
		{
			this.defer = defer;

			oldValue = defer.Value;

			if (oldValue is null)
			{
				defer.Value = [];
			}
		}

		public void Dispose()
		{
			var value = defer.Value;
			try
			{
				var exceptions = new List<Exception>();

				if (oldValue is null && value is not null)
				{
					foreach (var watcher in value)
					{
						frozen.Value = true;

						try
						{
							watcher.OnNotified();
						}
						catch (Exception exception)
						{
							exceptions.Add(exception);
						}
						finally
						{
							frozen.Value = false;
						}
					}
				}

				switch (exceptions)
				{
				case []: break;
				case [var exception]:
				{
					ExceptionDispatchInfo.Capture(exception).Throw();
				}
				break;
				default:
				{
					throw new AggregateException(exceptions);
				}
				}
			}
			finally
			{
				defer.Value = oldValue;
			}
		}
	}
}