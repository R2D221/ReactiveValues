using System.Collections.Concurrent;
using System.Windows.Input;

namespace ReactiveValues.DataTypes;

public abstract class ReactiveCommandBase<T> : ICommand
{
	private readonly ReactiveValue<T> reactiveParameter = new(default!);
	private readonly ReactiveFunc<bool> reactiveCanExecute;
	private readonly ConcurrentDictionary<EventHandler, (Watcher watcher, Effect effect)> canExecuteChanged = new();

	public ReactiveCommandBase()
	{
		reactiveCanExecute = new(() => CanExecute(reactiveParameter.Value));
	}

	protected abstract bool CanExecute(T parameter);

	protected abstract void Execute(T parameter);

	bool ICommand.CanExecute(object? parameter)
	{
		reactiveParameter.Value = (T)parameter!;
		return reactiveCanExecute.Value;
	}

	void ICommand.Execute(object? parameter)
	{
		Execute((T)parameter!);
	}

	event EventHandler? ICommand.CanExecuteChanged
	{
		add
		{
			if (value is null) { return; }

			_ = canExecuteChanged.GetOrAdd(
				value,
				value =>
				{
					var watcher = EventHandlerWatcher.Current;

					var effect = Reactive.EventEffect(reactiveCanExecute, () => value(this, EventArgs.Empty));

					watcher.Watch(effect);

					return (watcher, effect);
				});
		}

		remove
		{
			if (value is null) { return; }

			if (canExecuteChanged.TryRemove(value, out var result))
			{
				result.watcher.Unwatch(result.effect);
				return;
			}
		}
	}
}

public class ReactiveCommand<T>(Func<T, bool> canExecute, Action<T> execute) : ReactiveCommandBase<T>
{
	protected override bool CanExecute(T parameter) => canExecute(parameter);

	protected override void Execute(T parameter) => execute(parameter);
}

public class ReactiveCommand(Func<bool> canExecute, Action execute) : ReactiveCommand<object?>(_ => canExecute(), _ => execute());

public class ReactiveAsyncCommand<T>(Func<T, bool> canExecute, Func<T, Task> execute) : ReactiveCommandBase<T>
{
	private readonly ReactiveValue<bool> reactiveIsRunning = new(false);

	protected override bool CanExecute(T parameter)
	{
		return reactiveIsRunning.Value is false
			&& canExecute(parameter);
	}

	protected override void Execute(T parameter)
	{
		_ = ExecuteAsync();
		async Task ExecuteAsync()
		{
			reactiveIsRunning.Value = true;
			try
			{
				await execute(parameter);
			}
			finally
			{
				reactiveIsRunning.Value = false;
			}
		}
	}
}

public class ReactiveAsyncCommand(Func<bool> canExecute, Func<Task> execute) : ReactiveAsyncCommand<object?>(_ => canExecute(), _ => execute());
