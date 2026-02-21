using ReactiveValues;
using ReactiveValues.DataTypes;
using System.Diagnostics;

namespace ReactiveSamples.Common;

public sealed class FPS(double refreshRateHz) : ReactiveObject
{
	private readonly Queue<long> queue = new Queue<long>();
	private readonly Stopwatch sw = Stopwatch.StartNew();

	private int Calculate()
	{
		var now = sw.ElapsedMilliseconds;
		while (queue.Count > 0 && queue.Peek() <= now - 1000)
		{
			queue.Dequeue();
		}
		queue.Enqueue(now);
		return queue.Count;
	}

	private ReactiveFunc<int> Volatile => field ??= Reactive.Volatile(() => Calculate());
	
	private ReactiveFunc<int> Throttled => field ??= Reactive.Throttle(() => Volatile.Value, TimeSpan.FromSeconds(1/refreshRateHz));

	public int Value => Computed(() => Value, () => Throttled.Value);
}