using ReactiveValues.Exceptions;

namespace ReactiveValues;

internal enum LockAction
{
	_,
	Recompute,
//	UpdateIsWatched,
}

internal static class Locks
{
	private static readonly ThreadLocal<CountdownEvent?> threadLocks = new();

	public static CountdownEvent GetLockForThread()
	{
		if (threadLocks.Value is { IsSet: false } @lock)
		{
			return @lock;
		}
		else
		{
			@lock = new CountdownEvent(0);
			threadLocks.Value = @lock;
			return @lock;
		}
	}

	public static LockScope AcquireLock(ref CountdownEvent? objLock, LockAction lockAction)
	{
		var threadLock = GetLockForThread();

		while (true)
		{
			var oldValue = objLock;
			if (oldValue == threadLock) { break; }

			if (oldValue is not null)
			{
				switch (lockAction)
				{
				case LockAction.Recompute:
				{
					oldValue.Wait();
				}
				break;
				//case LockAction.UpdateIsWatched:
				//{
				//	if (oldValue.IsSet is false)
				//	{
				//		throw new DeadlockException();
				//	}
				//}
				//break;
				default:
				{
					if (oldValue.Wait(TimeSpan.FromSeconds(1)) is false)
					{
						throw new DeadlockException();
					}
				}
				break;
				}
			}

			if (Interlocked.CompareExchange(ref objLock, threadLock, oldValue) == oldValue) { break; }
		}

		return new LockScope(threadLock);
	}

	public readonly struct LockScope : IDisposable
	{
		private readonly CountdownEvent @lock;

		public LockScope(CountdownEvent @lock)
		{
			this.@lock = @lock;
			if (this.@lock.IsSet)
			{
				this.@lock.Reset(1);
			}
			else
			{
				this.@lock.AddCount();
			}
		}

		public void Dispose()
		{
			@lock.Signal();
		}
	}
}
