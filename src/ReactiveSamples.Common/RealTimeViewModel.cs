using ReactiveValues;
using ReactiveValues.DataTypes;

namespace ReactiveSamples.Common;

public sealed partial class RealTimeViewModel : ReactiveObject
{
	public int IntervalMs
	{
		get => Property(() => IntervalMs).Get(initialValue: () => 100);
		set => Property(() => IntervalMs).Set(value);
	}

	public TimeSpan Interval =>
		Property(() => Interval)
		.Computed(() => TimeSpan.FromMilliseconds(IntervalMs));

	public Selection WhatToShow
	{
		get => Property(() => WhatToShow).Get();
		set => Property(() => WhatToShow).Set(value);
	}

	public enum Selection { DateTime, Fps }

	//private ReactiveFunc<DateTime> InternalDateTimeNow => Computed(() => InternalDateTimeNow,
	//	() => Reactive.Throttle(Reactive.Volatile(() => DateTime.Now), Interval));

	//public DateTime DateTimeNow => Computed(() => DateTimeNow, () => InternalDateTimeNow.Value);

	public FPS FPS =>
		Property(() => FPS)
		.Computed(() => new FPS(Interval));
}
