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

		var effect = new Effect(() => value = reactive.Value);

		var watcher = new ThrottleWatcher(interval, invalidator);
		watcher.Watch(effect);

		var result = new ReactiveFunc<T>(() =>
		{
			watcher.Activate();
			invalidator.Register();
			return value;
		});

		return result;
	}

	private sealed class ThrottleWatcher : Watcher
	{
		private readonly AsyncSignal activateSignal = new();
		private readonly AsyncSignal notifiedSignal = new();

		private readonly TimeSpan interval;
		private readonly Invalidator invalidator;
		private readonly PeriodicTimer timer;

		public ThrottleWatcher(TimeSpan interval, Invalidator invalidator)
		{
			this.interval = interval;
			this.invalidator = invalidator;
			timer = new PeriodicTimer(Timeout.InfiniteTimeSpan);

			_ = Task.Run(Run);
		}

		public void Activate() => activateSignal.Signal();

		protected internal override void OnNotified() => notifiedSignal.Signal();

		private void RunPending()
		{
			foreach (var pending in GetPending())
			{
				pending.Run();
			}

			invalidator.Invalidate();
		}

		private async Task Run()
		{
			await activateSignal;
			await notifiedSignal;

			while (true)
			{
				timer.Period = interval;

				while (true)
				{
					await timer.WaitForNextTickAsync();

					var activateIsSignaled = activateSignal.IsSignaled;
					activateSignal.Reset();

					var notifiedIsSignaled = notifiedSignal.IsSignaled;
					notifiedSignal.Reset();

					RunPending();

					if (activateIsSignaled is false)
					{
						await activateSignal;
						break;
					}

					if (notifiedIsSignaled is false)
					{
						await notifiedSignal;
						break;
					}
				}
			}
		}
	}

	public static ReactiveFunc<T> Debounce<T>(ReactiveFunc<T> reactive, TimeSpan interval)
	{
		T value = default!;
		var invalidator = new Invalidator();

		var effect = new Effect(() =>
		{
			value = reactive.Value;
			invalidator.Invalidate();
		});

		var watcher = new DebounceWatcher(interval);
		watcher.Watch(effect);

		var result = new ReactiveFunc<T>(() =>
		{
			watcher.Activate();
			invalidator.Register();
			return value;
		});

		return result;
	}

	private sealed class DebounceWatcher : Watcher
	{
		private readonly AsyncSignal activateSignal = new();
		private readonly AsyncSignal notifiedSignal = new();
		private readonly TimeSpan interval;
		private readonly PeriodicTimer timer;

		public DebounceWatcher(TimeSpan interval)
		{
			this.interval = interval;
			timer = new PeriodicTimer(Timeout.InfiniteTimeSpan);

			_ = Task.Run(Run);
		}

		public void Activate() => activateSignal.Signal();

		protected internal override void OnNotified()
		{
			timer.Period = interval;
			notifiedSignal.Signal();
		}

		private async Task Run()
		{
			while (true)
			{
				throw new NotImplementedException();
				await activateSignal;
				await notifiedSignal;

				//while (true)
				//{
				await timer.WaitForNextTickAsync();

				using var pendingEnumerator = GetPending().GetEnumerator();

				if (pendingEnumerator.MoveNext() is false)
				{
					//		break;
				}

				do
				{
					pendingEnumerator.Current.Run();
				}
				while (pendingEnumerator.MoveNext());
				//}
			}
		}
	}

	public static Effect EventEffect(Reactive reactive, Action raiseEvent) =>
		reactive.EventEffect(raiseEvent);
}
