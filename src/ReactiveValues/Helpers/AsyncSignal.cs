using System.Runtime.CompilerServices;

namespace ReactiveValues.Helpers;

internal sealed class AsyncSignal
{
	private Action? continuation;
	private bool signaled;

	public void Signal()
	{
		if (continuation is not null)
		{
			Task.Run(continuation);
		}
		else
		{
			signaled = true;
		}
	}

	public Awaiter GetAwaiter() => new(this);

	public readonly struct Awaiter(AsyncSignal @this) : INotifyCompletion
	{
		public bool IsCompleted => @this.signaled;

		public void OnCompleted(Action continuation)
		{
			@this.continuation = continuation;
		}

		public void GetResult()
		{
			@this.signaled = false;
			@this.continuation = null;
		}
	}
}