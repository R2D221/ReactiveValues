using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.ComponentModel;

namespace ReactiveValues.Blazor;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class InternalReactiveComponentBase : ComponentBase, IHandleEvent, IDisposable
{
	private readonly BlazorWatcher watcher;
	private readonly Effect effect;

	private readonly ReactiveValue<RenderTreeBuilder> builder = new(null!);
	internal bool isBuilding;

	protected InternalReactiveComponentBase()
	{
		watcher = BlazorWatcher.ForComponent((ReactiveComponentBase)this);
		var action = (RenderTreeBuilder _) => { };
		effect = new Effect(() => action(builder.Value));
		watcher.Watch(effect);
		action = InternalBuildRenderTree;
	}

	protected sealed override void BuildRenderTree(RenderTreeBuilder builder)
	{
		isBuilding = true;
		this.builder.Value = builder;
		isBuilding = false;

		watcher.RunPending();
	}

	protected abstract void InternalBuildRenderTree(RenderTreeBuilder builder);

	public virtual void Dispose()
	{
		watcher.Unwatch(effect);
	}

	async Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem item, object? arg)
	{
		await item.InvokeAsync(arg);
	}
}

public abstract class ReactiveComponentBase : InternalReactiveComponentBase
{
	protected sealed override void InternalBuildRenderTree(RenderTreeBuilder builder) =>
		BuildRenderTree(builder);

	/// <inheritdoc cref="ComponentBase.BuildRenderTree(RenderTreeBuilder)"/>
	protected abstract new void BuildRenderTree(RenderTreeBuilder builder);

	internal void InternalStateHasChanged() => StateHasChanged();
	internal Task InternalInvokeAsync(Action workItem) => InvokeAsync(workItem);
}