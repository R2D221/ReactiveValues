namespace ReactiveValues;

partial class Reactive
{
	public static ReactiveFunc<T> FromProperty<T, TEventHandler>(
		Func<T> valueFunc,
		Func<Invalidator, TEventHandler> create,
		Action<TEventHandler> addHandler,
		Action<TEventHandler> removeHandler) where TEventHandler : Delegate
	{
		var state = new ReactiveValue<ValueTuple>(default, equality: AlwaysFalseEqualityComparer<ValueTuple>.Instance);

		var computed = new ReactiveFunc<T>(() =>
		{
			_ = state.Value;
			return valueFunc();
		});

		var notifier = new Invalidator(state);

		var handler = create(notifier);

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
		var invalidator = new ReactiveValue<ValueTuple>(default, equality: AlwaysFalseEqualityComparer<ValueTuple>.Instance);

		return new ReactiveFunc<T>(() =>
		{
			_ = invalidator.Value;
			_ = Task.Run(() => invalidator.Value = default);

			return valueFunc();
		});
	}

	//private static bool first = true;

	//	if (first)
	//	{
	//		first = false;
	//		return new ReactiveFunc<T>(() => default!);
	//	}
	//	else
	//	{
	//	}








	public static ReactiveFunc<T> Throttle2<T>(ReactiveFunc<T> reactive, TimeSpan interval)
	{
		PeriodicTimer? timer = null;
		var invalidator = new ReactiveValue<long>(0);

		//var xxx = new Effect(() =>
		//{
		//	version.Value++;
		//	_ = reactive.Value;
		//});

		var result = new ReactiveFunc<T>(() =>
		{
			_ = invalidator.Value;

			NextTick();

			using (Reactive.Untrack())
			{
				return reactive.Value;
			}
		});

		void NextTick()
		{
			_ = Task.Run(async () =>
			{
				timer ??= new(interval);

				if (await timer.WaitForNextTickAsync())
				{
					invalidator.Value++;
				}
			});
		}

		return result;
	}











	public static ReactiveFunc<T> Throttle<T>(Func<T> valueFunc, TimeSpan interval)
	{
		Task<bool> throttledTask = Task.FromResult(true);

		var wrapper = new ReactiveValue<T>(default!);

		var timer = new PeriodicTimer(Timeout.InfiniteTimeSpan);

		var version = new ReactiveValue<long>(0);
		var effect = new Effect(() =>
		{
			version.Value++;
			wrapper.Value = valueFunc();
		});

		LambdaWatcher watcher = default!;
		watcher = new LambdaWatcher(() =>
		{
			_ = Task.Run(async () =>
			{
				if (await throttledTask is false)
				{
					_ = watcher.RunPending();
				}
			});
		});

		watcher.Watch(effect);

		return new ReactiveFunc<T>(() =>
		{
			timer.Period = interval;
			throttledTask = Task.Run(async () =>
			{
				if (await timer.WaitForNextTickAsync() is false)
				{
					return false;
				}

				var didRun = watcher.RunPending();

				if (didRun is false)
				{
					timer.Period = Timeout.InfiniteTimeSpan;
				}

				return didRun;
			});
			_ = version.Value;
			return wrapper.Value;
		});
	}

	private class LambdaWatcher(Action action) : Watcher
	{
		protected internal override void OnNotified() => action();

		public bool RunPending()
		{
			var result = false;

			foreach (var effect in GetPending())
			{
				result = true;
				effect.Run();
			}

			return result;
		}
	}

	public static Effect EventEffect(Reactive reactive, Action raiseEvent) =>
		reactive.EventEffect(raiseEvent);
}

public sealed class Invalidator
{
	private readonly ReactiveValue<ValueTuple> state;

	internal Invalidator(ReactiveValue<ValueTuple> state)
	{
		this.state = state;
	}

	public void Invalidate()
	{
		state.Value = default;
	}
}
