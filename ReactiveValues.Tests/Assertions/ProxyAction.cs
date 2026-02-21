namespace Signals.Tests;

internal partial class ProxyAction
{
	public static ProxyAction For(Action action) => new(action);
}

internal partial class ProxyAction(Action action)
{
	public int TimesCalled { get; private set; }

	public void Invoke()
	{
		TimesCalled++;
		action();
	}
}
