using System.ComponentModel;

namespace ReactiveValues;

partial class Reactive
{
	internal static readonly ThreadLocal<Reactive?> currentReceiver = new();

	internal static void AddSource<T>(Reactive<T> source, int currentVersion, T currentValue)
	{
		if (currentReceiver.Value is {/*notnull*/} receiver)
		{
			System.Diagnostics.Debug.Assert(receiver.@lock.IsWriteLockHeld);

			using (source.@lock.WriteLockScope())
			{
				source.AddReceiver(receiver);
			}

			if (source.isLive)
			{
				receiver.AddSource(new ReactiveLastKnownValue<T>(source, currentVersion, currentValue));
				receiver.MarkLive();
			}
		}
	}

	internal static void AddSource<T>(Reactive<T> source, int currentVersion)
	{
		if (currentReceiver.Value is {/*notnull*/} receiver)
		{
			System.Diagnostics.Debug.Assert(receiver.@lock.IsWriteLockHeld);

			using (source.@lock.WriteLockScope())
			{
				source.AddReceiver(receiver);
			}

			receiver.AddSource(new ReactiveLastKnownValue(source, currentVersion));
		}
	}

	internal static TrackingScope Track(Reactive receiver) => new(currentReceiver, receiver);

	public static TrackingScope Untrack() => new(currentReceiver, null);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public readonly ref struct TrackingScope
	{
		private readonly ThreadLocal<Reactive?> threadLocal;
		private readonly Reactive? oldReceiver;

		[Obsolete("Do not use", true)]
		public TrackingScope() => throw new InvalidOperationException();

		internal TrackingScope(ThreadLocal<Reactive?> threadLocal, Reactive? newReceiver)
		{
			this.threadLocal = threadLocal;

			oldReceiver = threadLocal.Value;
			threadLocal.Value = newReceiver;
		}

		public void Dispose()
		{
			threadLocal.Value = oldReceiver;
		}
	}
}
