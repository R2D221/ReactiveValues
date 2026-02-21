using System.Threading.Channels;

namespace Signals.Tests;

public sealed class TestSynchronizationContext(SynchronizationContext? previous) :
	SynchronizationContext, IDisposable
{
	public static TestSynchronizationContext Scope()
	{
		var previous = Current;
		var context = new TestSynchronizationContext(previous);
		SetSynchronizationContext(context);
		return context;
	}

	private readonly Channel<Action> queue = Channel.CreateUnbounded<Action>();

	public override void Send(SendOrPostCallback d, object? state) => throw new NotSupportedException();

	public override SynchronizationContext CreateCopy() => throw new NotSupportedException();

	public override void Post(SendOrPostCallback d, object? state)
	{
		queue.Writer.TryWrite(() => d(state));
	}

	public void Dispose()
	{
		SetSynchronizationContext(previous);
	}

	public void RunQueue()
	{
		while (queue.Reader.TryRead(out var action))
		{
			action();
		}
	}
}
