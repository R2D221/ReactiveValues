using System.Collections.Concurrent;
using System.Windows.Input;

namespace ReactiveValues.DataTypes;

public sealed class Command(Func<object?, bool> canExecute, Action<object?> execute) : CommandBase
{
	protected override bool CanExecute(object? parameter) =>
		canExecute(parameter);

	protected override void Execute(object? parameter) =>
		execute(parameter);
}

public abstract class CommandBase : ICommand
{
	private readonly ReactiveValue<object?> parameter;
	private readonly ReactiveFunc<bool> canExecute;
	private readonly ConcurrentDictionary<EventHandler, (Watcher watcher, Effect effect)>
		canExecuteChanged = new();

	protected CommandBase()
	{
		parameter = new(null);
		canExecute = new(() => CanExecute(parameter.Value));
	}

	protected abstract bool CanExecute(object? parameter);

	protected abstract void Execute(object? parameter);

	bool ICommand.CanExecute(object? parameter)
	{
		this.parameter.Value = parameter;
		return canExecute.Value;
	}

	void ICommand.Execute(object? parameter)
	{
		Execute(parameter);
	}

	event EventHandler? ICommand.CanExecuteChanged
	{
		add
		{
			if (value is null) { return; }

			_ = canExecuteChanged.AddOrUpdate(
				value,
				value =>
				{
					var watcher = EventHandlerWatcher.Current;

					var effect = Reactive.EventEffect(canExecute, () => value(this, EventArgs.Empty));

					watcher.Watch(effect);

					return (watcher, effect);
				},
				(value, _) => throw new InvalidOperationException()
				);
		}

		remove
		{
			if (value is null) { return; }

			if (canExecuteChanged.TryRemove(value, out var result) is false)
			{
				throw new InvalidOperationException();
			}

			result.watcher.Unwatch(result.effect);
		}
	}
}
