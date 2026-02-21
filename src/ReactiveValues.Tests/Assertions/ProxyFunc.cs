namespace Signals.Tests;

internal static class ProxyFunc
{
	public static ProxyFunc<T> For<T>(Func<T> func) => new(func);
}

internal sealed class ProxyFunc<T>(Func<T> func)
{
	public int TimesCalled { get; private set; }

	public T Invoke()
	{
		TimesCalled++;
		return func();
	}
}
