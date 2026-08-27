using ReactiveValues.DataTypes;

namespace ReactiveSamples.Common;

public sealed partial class DebounceThrottleViewModel : ReactiveObject
{
	public int IntervalMs
	{
		get => Property(() => IntervalMs).Get(initialValue: () => 100);
		set => Property(() => IntervalMs).Set(value);
	}

	public TimeSpan Interval =>
		Property(() => Interval)
		.Computed(() => TimeSpan.FromMilliseconds(IntervalMs));

	public string Input
	{
		get => Property(() => Input).Get(initialValue: () => "");
		set => Property(() => Input).Set(value);
	}

	//public string DebouncedInput => Computed(() => DebouncedInput,
	//	() => Debounce(() => Input, Interval));

	//public string ThrottledInput => Computed(() => ThrottledInput,
	//	() => Throttle(() => Input, Interval));
}
