using ReactiveValues.Helpers;

namespace ReactiveValues;

partial class Reactive
{
	public static ReactiveFunc<T> FromProperty<T, TEventHandler>(
		Func<T> valueFunc,
		Func<Invalidator, TEventHandler> create,
		Action<TEventHandler> addHandler,
		Action<TEventHandler> removeHandler) where TEventHandler : Delegate
	{
		var invalidator = new Invalidator();

		var computed = new ReactiveFunc<T>(() =>
		{
			invalidator.Register();
			return valueFunc();
		});

		var handler = create(invalidator);

		computed.Watched += (_, _) => addHandler(handler);
		computed.Unwatched += (_, _) => removeHandler(handler);

		return computed;
	}

	public static ReactiveFunc<T> FromEvent<T, TEventHandler>(
		T initialValue,
		Func<ReactiveValue<T>, TEventHandler> create,
		Action<TEventHandler> addHandler,
		Action<TEventHandler> removeHandler) where TEventHandler : Delegate
	{
		var state = new ReactiveValue<T>(initialValue);

		var handler = create(state);

		state.Watched += (_, _) => addHandler(handler);
		state.Unwatched += (_, _) => removeHandler(handler);

		return new ReactiveFunc<T>(() => state.Value);
	}

	public static ReactiveFunc<T> Volatile<T>(Func<T> valueFunc)
	{
		var invalidator = new Invalidator();

		return new ReactiveFunc<T>(() =>
		{
			invalidator.Register();
			_ = Task.Run(() => invalidator.Invalidate());

			return valueFunc();
		});
	}

	public static ReactiveFunc<T> Throttle<T>(ReactiveFunc<T> reactive, TimeSpan interval)
	{
		T value = default!;
		var invalidator = new Invalidator();

		var effect = new Effect(() =>
		{
			value = reactive.Value;
			invalidator.Invalidate();
		});

		var watcher = new ThrottleWatcher(interval);
		watcher.Watch(effect);

		var result = new ReactiveFunc<T>(() =>
		{
			invalidator.Register();
			return value;
		});

		return result;
	}

	private sealed class ThrottleWatcher : Watcher
	{
		private readonly AsyncSignal signal = new();

		public ThrottleWatcher(TimeSpan interval)
		{
			_ = Task.Run(async () =>
			{
				while (true)
				{
					await signal;

					using var timer = new PeriodicTimer(interval);
					while (await timer.WaitForNextTickAsync())
					{
						using var pendingEnumerator = GetPending().GetEnumerator();

						if (pendingEnumerator.MoveNext() is false)
						{
							break;
						}

						do
						{
							pendingEnumerator.Current.Run();
						}
						while (pendingEnumerator.MoveNext());
					}
				}
			});
		}

		protected internal override void OnNotified() => signal.Signal();
	}

	public static Effect EventEffect(Reactive reactive, Action raiseEvent) =>
		reactive.EventEffect(raiseEvent);
}
