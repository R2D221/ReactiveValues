using ReactiveValues.DataTypes;

namespace ReactiveSamples.Blazor.ViewModels;

public sealed class CounterViewModel : ReactiveObject
{
	private static readonly CounterInstance global = new();

	public CounterInstance Global => global;

	public CounterInstance Local { get; } = new();
}

public sealed class CounterInstance : ReactiveObject
{
	public int CurrentCount
	{
		get => Property(() => CurrentCount).Get();
		set => Property(() => CurrentCount).Set(value);
	}
}
