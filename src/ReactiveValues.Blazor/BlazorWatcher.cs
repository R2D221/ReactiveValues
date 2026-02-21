using System.Runtime.CompilerServices;

namespace ReactiveValues.Blazor;

internal sealed class BlazorWatcher(ReactiveComponentBase component) : Watcher
{
	private static readonly ConditionalWeakTable<ReactiveComponentBase, BlazorWatcher> watchers = new();
	public static BlazorWatcher ForComponent(ReactiveComponentBase component) => watchers.GetValue(component, component => new(component));

	private const int FALSE = 0;
	private const int TRUE = 1;

	private int pending = FALSE;

	protected override void OnNotified()
	{
		if (component.isBuilding) { return; }

		if (Interlocked.Exchange(ref pending, TRUE) is FALSE)
		{
			_ = Task.Run(async () =>
				await component.InternalInvokeAsync(component.InternalStateHasChanged));
		}
	}

	public void RunPending()
	{
		pending = FALSE;
		foreach (var effect in GetPending())
		{
			effect.Run();
		}
	}
}
