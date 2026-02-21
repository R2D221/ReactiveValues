using System.ComponentModel;

namespace ReactiveValues;

partial class Reactive
{
	internal static readonly ThreadLocal<Internals?> currentReceiver = new();

	internal static void AddSource<T>(ReactiveFunc<T> sourceNode, int currentVersion, T currentValue)
	{
		if (currentReceiver.Value is {/*notnull*/} receiver)
		{
			using var source = sourceNode.GetInternals(LockAction.Recompute);

			source.AddReceiver(receiver.Reactive);

			if (source.IsLive)
			{
				receiver.AddSource(new ReactiveLastKnownValue<T>(sourceNode, currentVersion, currentValue));
				receiver.MarkLive();
			}
		}
	}

	internal static void AddSource<T>(ReactiveFunc<T> sourceNode, int currentVersion)
	{
		if (currentReceiver.Value is {/*notnull*/} receiver)
		{
			using var source = sourceNode.GetInternals(LockAction.Recompute);

			source.AddReceiver(receiver.Reactive);
			receiver.AddSource(new ReactiveLastKnownValue(sourceNode, currentVersion));
		}
	}

	internal static TrackingScope Track(Internals receiver) => new(currentReceiver, receiver);

	public static TrackingScope Untrack() => new(currentReceiver, null);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public readonly ref struct TrackingScope
	{
		private readonly ThreadLocal<Internals?> threadLocal;
		private readonly Internals? oldReceiver;

		[Obsolete("Do not use", true)]
		public TrackingScope() => throw new InvalidOperationException();

		internal TrackingScope(ThreadLocal<Internals?> threadLocal, Internals? newReceiver)
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
