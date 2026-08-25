using ReactiveValues;
using ReactiveValues.DataTypes;
using System.Diagnostics;

namespace ReactiveSamples.Common;

public sealed class FPS(TimeSpan interval) : ReactiveObject
{
	private readonly Queue<long> queue = new Queue<long>();
	private readonly Stopwatch sw = Stopwatch.StartNew();

	public int Calculate()
	{
		var now = sw.ElapsedMilliseconds;
		while (queue.Count > 0 && queue.Peek() <= now - 1000)
		{
			queue.Dequeue();
		}
		queue.Enqueue(now);
		return queue.Count;
	}

	//private ReactiveFunc<int> Throttled => field ??= Reactive.Throttle(Reactive.Volatile(Calculate), interval);

	//public int Value => Computed(() => Value, () => Throttled.Value);
}