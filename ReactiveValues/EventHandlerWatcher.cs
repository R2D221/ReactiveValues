namespace ReactiveValues;

public sealed class EventHandlerWatcher : Watcher
{
	private static readonly ThreadLocal<EventHandlerWatcher> threadLocalWatcher = new(() => new());

	public static EventHandlerWatcher Current => threadLocalWatcher.Value ?? throw new InvalidOperationException();

	private readonly SynchronizationContext syncContext;

	private const int FALSE = 0;
	private const int TRUE = 1;

	private int pending = FALSE;

	public EventHandlerWatcher()
	{
		if (SynchronizationContext.Current is not { } syncContext)
		{
			throw new InvalidOperationException();
		}

		this.syncContext = syncContext;
	}

	protected internal override void OnNotified()
	{
		if (Interlocked.Exchange(ref pending, TRUE) is FALSE)
		{
			syncContext.Post(Callback, null);
		}
	}

	private void Callback(object? __)
	{
		pending = FALSE;

		foreach (var effect in GetPending())
		{
			effect.Run();
		}
	}
}
