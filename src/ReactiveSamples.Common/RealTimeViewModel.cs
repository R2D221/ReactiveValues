using ReactiveValues;
using ReactiveValues.DataTypes;

namespace ReactiveSamples.Common;

public sealed partial class RealTimeViewModel : ReactiveObject
{
	public int IntervalMs
	{
		get => Get(() => IntervalMs, initialValue: () => 100);
		set => Set(() => IntervalMs, value);
	}

	public TimeSpan Interval => Computed(() => Interval,
		() => TimeSpan.FromMilliseconds(IntervalMs));

	public Selection WhatToShow
	{
		get => Get(() => WhatToShow);
		set => Set(() => WhatToShow, value);
	}

	public enum Selection { DateTime, Fps }

	//private ReactiveFunc<DateTime> InternalDateTimeNow => Computed(() => InternalDateTimeNow,
	//	() => Reactive.Throttle(Reactive.Volatile(() => DateTime.Now), Interval));

	//public DateTime DateTimeNow => Computed(() => DateTimeNow, () => InternalDateTimeNow.Value);

	public FPS FPS => Computed(() => FPS,
		() => new FPS(Interval));
}
