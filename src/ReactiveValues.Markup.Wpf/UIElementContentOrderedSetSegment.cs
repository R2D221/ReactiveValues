using ReactiveValues.DataTypes;
using System.Windows;

namespace ReactiveValues.Markup.Wpf;

internal sealed class UIElementContentOrderedSetSegment : ArrayListOrderedSetSegment<UIElement, UIElement>
{
	private readonly UIElement owner;
	private readonly Func<UIElement?> getter;
	private readonly Action<UIElement?> setter;

	public UIElementContentOrderedSetSegment(UIElement owner, Func<UIElement?> getter, Action<UIElement?> setter)
		: this(owner, getter, setter, null)
	{
		if (getter() is not null)
		{
			throw new ArgumentException(message: null, paramName: nameof(getter));
		}
	}

	public UIElementContentOrderedSetSegment(UIElement owner, Func<UIElement?> getter, Action<UIElement?> setter, UIElementContentOrderedSetSegment? parent)
		: base(parent)
	{
		this.owner = owner;
		this.getter = getter;
		this.setter = setter;
	}

	protected override OrderedSetSegment<UIElement, UIElement> Constructor(OrderedSetSegment<UIElement, UIElement> parent) =>
		new UIElementContentOrderedSetSegment(owner, getter, setter, (UIElementContentOrderedSetSegment)parent);

	public override UIElement Owner => owner;

	protected override void InnerInsert(UIElement item, int index)
	{
		if (index != 0) { throw new NotSupportedException(); }
		if (getter() is { } existing && existing != item) { throw new NotSupportedException(); }

		setter(item);
	}

	protected override void InnerRemoveAt(int index)
	{
		if (index != 0) { throw new NotSupportedException(); }
		setter(null);
	}

	public new UIElementContentOrderedSetSegment CreateChildSegment() =>
		(UIElementContentOrderedSetSegment)base.CreateChildSegment();
}
