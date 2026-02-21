using System.Collections;

namespace ReactiveValues.Markup;

public abstract class XBase<TComponentBase, TComponent> : IEnumerable
	where TComponentBase : class
	where TComponent : class, TComponentBase, new()
{
	protected readonly MarkupProvider<TComponentBase> provider;
	protected readonly TComponent component;

	public XBase(MarkupProvider<TComponentBase> provider, TComponent component, Attributes<TComponentBase, TComponent> attributes)
	{
		this.provider = provider;
		this.component = component;
		provider.InitLifetime(component);

		foreach (var action in attributes.actions)
		{
			AttachEffectToComponentLifetime(action);
		}

		foreach (var setback in attributes.setbacks)
		{
			AttachEffectToComponentLifetime(setback(provider));
		}
	}

	private void AttachEffectToComponentLifetime(Action<TComponent> action)
	{
		var effect = new Effect(() => action(component));
		provider.AttachEffectToComponentLifetime(component, effect);
	}

	IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

	[Obsolete("Please specify the tag type.", true)]
	public void X() => throw new NotSupportedException();
}