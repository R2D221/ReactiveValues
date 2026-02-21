#if !NETCOREAPP
internal sealed class TimerQueueTimer : ITimer
{
	private readonly Timer timer;

	public TimerQueueTimer(TimerCallback timerCallback, object? state, uint dueTime, uint period, bool flowExecutionContext)
	{
		if (flowExecutionContext)
		{
			timer = new(timerCallback, state, dueTime, period);
		}
		else
		{
			using (ExecutionContext.SuppressFlow())
			{
				timer = new(timerCallback, state, dueTime, period);
			}
		}
	}

	public bool Change(TimeSpan dueTime, TimeSpan period) => timer.Change(dueTime, period);

	public void Dispose() => timer.Dispose();

	public ValueTask DisposeAsync()
	{
		var waitHandle = new ManualResetEvent(false);
		var taskSource = new TaskCompletionSource<ValueTuple>();

		RegisteredWaitHandle registration = null!;
		registration = ThreadPool.RegisterWaitForSingleObject(
			waitHandle,
			(taskSource, _) =>
			{
				_ = registration.Unregister(null);
				((TaskCompletionSource<ValueTuple>)taskSource!).SetResult(default);
			},
			taskSource,
			Timeout.Infinite,
			executeOnlyOnce: true);

		if (timer.Dispose(waitHandle) is false)
		{
			_ = registration.Unregister(null);
			taskSource.SetResult(default);
		}
		else
		{
			return new(Task.CompletedTask);
		}

		return new(taskSource.Task);
	}
}
#endif
