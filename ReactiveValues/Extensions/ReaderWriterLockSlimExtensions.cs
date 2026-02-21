using System.Diagnostics;

file static class Helpers
{
	public static bool Debug(Exception ex)
	{
		if (Debugger.IsAttached)
		{
			Debugger.Break();
		}

		return false;
	}
}

internal static class ReaderWriterLockSlimExtensions
{
	public static ReadLockScope ReadLockScope(this ReaderWriterLockSlim @lock) => new(@lock);
	public static WriteLockScope WriteLockScope(this ReaderWriterLockSlim @lock) => new(@lock);
	public static UpgradeableReadLockScope UpgradeableReadLockScope(this ReaderWriterLockSlim @lock) => new(@lock);
}

internal readonly ref struct ReadLockScope
{
	private readonly ReaderWriterLockSlim @lock;

	[Obsolete("Do not use", true)]
	public ReadLockScope() => throw new InvalidOperationException();

	public ReadLockScope(ReaderWriterLockSlim @lock)
	{
		try
		{
			this.@lock = @lock;
			this.@lock.EnterReadLock();
		}
		catch (Exception ex) when (Helpers.Debug(ex)) { throw; }
	}

	public void Dispose()
	{
		@lock.ExitReadLock();
	}
}
internal readonly ref struct WriteLockScope
{
	private readonly ReaderWriterLockSlim @lock;

	[Obsolete("Do not use", true)]
	public WriteLockScope() => throw new InvalidOperationException();

	public WriteLockScope(ReaderWriterLockSlim @lock)
	{
		try
		{
			this.@lock = @lock;
			this.@lock.EnterWriteLock();
		}
		catch (Exception ex) when (Helpers.Debug(ex)) { throw; }
	}

	public void Dispose()
	{
		@lock.ExitWriteLock();
	}
}
internal readonly ref struct UpgradeableReadLockScope
{
	private readonly ReaderWriterLockSlim @lock;

	[Obsolete("Do not use", true)]
	public UpgradeableReadLockScope() => throw new InvalidOperationException();

	public UpgradeableReadLockScope(ReaderWriterLockSlim @lock)
	{
		try
		{
			this.@lock = @lock;
			this.@lock.EnterUpgradeableReadLock();
		}
		catch (Exception ex) when (Helpers.Debug(ex)) { throw; }
	}

	public void Dispose()
	{
		@lock.ExitUpgradeableReadLock();
	}
}