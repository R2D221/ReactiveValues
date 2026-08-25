using ReactiveValues.DataTypes;

namespace ReactiveSamples.Common;

public sealed partial class DebounceThrottleViewModel : ReactiveObject
{
	public int IntervalMs
	{
		get => Get(() => IntervalMs, initialValue: () => 100);
		set => Set(() => IntervalMs, value);
	}

	public TimeSpan Interval => Computed(() => Interval,
		() => TimeSpan.FromMilliseconds(IntervalMs));

	public string Input
	{
		get => Get(() => Input, initialValue: () => "");
		set => Set(() => Input, value);
	}

	//public string DebouncedInput => Computed(() => DebouncedInput,
	//	() => Debounce(() => Input, Interval));

	//public string ThrottledInput => Computed(() => ThrottledInput,
	//	() => Throttle(() => Input, Interval));
}
