using System.Collections.Concurrent;

namespace ReactiveValues.Helpers;

internal sealed class Event<TEventArgs>
{
	private readonly ConcurrentDictionary<Action<object?, TEventArgs>, SynchronizationContext?> handlers = new();

	public void AddEventHandler(Action<object?, TEventArgs> handler)
	{
		handlers[handler] = SynchronizationContext.Current;
	}

	public void RemoveEventHandler(Action<object?, TEventArgs> handler)
	{
		handlers.TryRemove(handler, out _);
	}

	public void Raise(object? sender, TEventArgs args)
	{
		foreach (var (handler, context) in handlers)
		{
			if (context is not null)
			{
				context.Post(_ => handler(sender, args), null);
			}
			else
			{
				Task.Run(() => handler(sender, args));
			}
		}
	}
}
