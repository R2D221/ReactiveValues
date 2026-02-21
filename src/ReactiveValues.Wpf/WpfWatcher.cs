using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace ReactiveValues.Wpf;

public sealed class WpfWatcher : Watcher
{
	private static readonly ConditionalWeakTable<UIElement, WpfWatcher> watchers = new();
	public static WpfWatcher ForElement(UIElement element) => watchers.GetValue(element, element => new(element));

	private readonly Dispatcher dispatcher;

	private const int FALSE = 0;
	private const int TRUE = 1;

	private int pending = FALSE;

	private WpfWatcher(UIElement element)
	{
		dispatcher = element.Dispatcher;
	}

	protected override void OnNotified()
	{
		if (Interlocked.Exchange(ref pending, TRUE) is FALSE)
		{
			_ = dispatcher.InvokeAsync(RunPending, DispatcherPriority.DataBind);
		}
	}

	private void RunPending()
	{
		pending = FALSE;
		foreach (var effect in GetPending())
		{
			effect.Run();
		}
	}
}
