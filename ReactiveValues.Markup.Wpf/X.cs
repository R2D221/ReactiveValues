using ReactiveValues.DataTypes;
using System.Windows;
using System.Windows.Controls;

namespace ReactiveValues.Markup.Wpf;

public sealed class X<TUIElement>(TUIElement component, Attributes<UIElement, TUIElement> attributes) : XBase<UIElement, TUIElement>(WpfMarkupProvider.Current, component, attributes)
	where TUIElement : UIElement, new()
{
	internal object? Children { get; set; }
	internal TUIElement Component => component;
	internal MarkupProvider<UIElement> Provider => provider;

	public X() : this(new(), attributes: new())
	{
	}

	public X(Attributes<UIElement, TUIElement> attributes) : this(new(), attributes)
	{
	}

	public X(out TUIElement component) : this(attributes: new())
	{
		component = this.component;
	}

	public X(out TUIElement component, Attributes<UIElement, TUIElement> attributes) : this(attributes)
	{
		component = this.component;
	}
}

public static class XExtensions
{
	public static TControl X<TControl>(this X<TControl> builder)
		where TControl : UIElement, new()
	{
		return builder.Component;
	}
}

public static class PanelExtensions
{
	private static PanelChildren<TPanel> GetChildren<TPanel>(X<TPanel> builder)
		where TPanel : Panel, new()
	{
		if (builder.Children is not PanelChildren<TPanel> children)
		{
			builder.Children = (children = new(builder.Component));
		}

		return children;
	}

	public static void Add<TPanel, TChildComponent>(this X<TPanel> builder, TChildComponent child)
		where TPanel : Panel, new()
		where TChildComponent : UIElement, new()
	{
		var children = GetChildren(builder);
		children.Add(child);
	}

	public static void Add<TPanel, TChildComponent>(this X<TPanel> builder, Func<IEnumerable<TChildComponent>> func)
		where TPanel : Panel, new()
		where TChildComponent : UIElement, new()
	{
		var children = GetChildren(builder);
		children.Add(func);
	}

	public static void Add<TPanel, TChildComponent>(this X<TPanel> builder, Func<IReactiveCollection<TChildComponent>> func)
		where TPanel : Panel, new()
		where TChildComponent : UIElement, new()
	{
		var children = GetChildren(builder);
		children.Add(func);
	}
}

public static class ContentControlExtensions
{
	private static ContentChildren<TContentControl> GetChildren<TContentControl>(X<TContentControl> builder)
		where TContentControl : ContentControl, new()
	{
		if (builder.Children is not ContentChildren<TContentControl> children)
		{
			builder.Children = (children = new(builder.Component));
		}

		return children;
	}

	public static void Add<TContentControl, TChildComponent>(this X<TContentControl> builder, TChildComponent child)
		where TContentControl : ContentControl, new()
		where TChildComponent : UIElement, new()
	{
		var children = GetChildren(builder);
		children.Add(child);
	}
}
