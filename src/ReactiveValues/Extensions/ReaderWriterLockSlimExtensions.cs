internal static class ReaderWriterLockSlimExtensions
{
	public static LockScope ReadLockScope(
		this ReaderWriterLockSlim @lock,
		Func<Exception>? exceptionFactory = null)
		=>
		new(
			@lock.EnterReadLock,
			@lock.ExitReadLock,
			exceptionFactory);

	public static LockScope WriteLockScope(
		this ReaderWriterLockSlim @lock,
		Func<Exception>? exceptionFactory = null)
		=>
		new(
			@lock.EnterWriteLock,
			@lock.ExitWriteLock,
			exceptionFactory);

	public static LockScope UpgradeableReadLockScope(
		this ReaderWriterLockSlim @lock,
		Func<Exception>? exceptionFactory = null)
		=>
		new(
			@lock.EnterUpgradeableReadLock,
			@lock.ExitUpgradeableReadLock,
			exceptionFactory);
}


internal readonly ref struct LockScope
{
	private readonly Action exit;

	[Obsolete("Do not use", true)] public LockScope() => throw new InvalidOperationException();

	internal LockScope(Action enter, Action exit, Func<Exception>? exceptionFactory)
	{
		try
		{
			enter.Invoke();
		}
		catch (LockRecursionException)
		when (exceptionFactory is not null)
		{
			throw exceptionFactory();
		}
		//catch (LockRecursionException ex)
		//when (Debug(ex)) { throw; }
		//static bool Debug(LockRecursionException ex)
		//{
		//	_ = ex;
		//	if (Debugger.IsAttached) { Debugger.Break(); }
		//	return false;
		//}

		this.exit = exit;
	}

	public void Dispose() => exit.Invoke();
}
