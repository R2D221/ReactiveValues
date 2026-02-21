using ReactiveValues.DataTypes;

namespace ReactiveSamples.Common;

public sealed partial class RealTimeViewModel : ReactiveObject
{
	public double RefreshRateHZ
	{
		get => Get(() => RefreshRateHZ);
		set => Set(() => RefreshRateHZ, value);
	}

	public FPS FPS => Computed(() => FPS,
		() => new(RefreshRateHZ));
}
