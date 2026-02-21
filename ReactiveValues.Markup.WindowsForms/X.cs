using ReactiveValues.DataTypes;

namespace ReactiveValues.Markup.WindowsForms;

public sealed class X<TControl>(TControl component, Attributes<Control, TControl> attributes) : XBase<Control, TControl>(WindowsFormsMarkupProvider.Current, component, attributes)
	where TControl : Control, new()
{
	internal Children<TControl> Children => field ??= new(component);
	internal TControl Component => component;

	public X() : this(new(), attributes: new())
	{
	}

	public X(Attributes<Control, TControl> attributes) : this(new(), attributes)
	{
	}

	public X(out TControl component) : this(attributes: new())
	{
		component = this.component;
	}

	public X(out TControl component, Attributes<Control, TControl> attributes) : this(attributes)
	{
		component = this.component;
	}
}

public static class XExtensions
{
	public static TControl X<TControl>(this X<TControl> builder)
		where TControl : Control, new()
	{
		return builder.Component;
	}
}

public static class PanelExtensions
{
	public static void Add<TPanel, TChildComponent>(this X<TPanel> builder, TChildComponent child)
		where TPanel : Panel, new()
		where TChildComponent : Control, new()
	{
		builder.Children.Add(child);
	}

	public static void Add<TPanel, TChildComponent>(this X<TPanel> builder, Func<IEnumerable<TChildComponent>> func)
		where TPanel : Panel, new()
		where TChildComponent : Control, new()
	{
		builder.Children.Add(func);
	}

	public static void Add<TPanel, TChildComponent>(this X<TPanel> builder, Func<IReactiveCollection<TChildComponent>> func)
		where TPanel : Panel, new()
		where TChildComponent : Control, new()
	{
		builder.Children.Add(func);
	}
}

public static class ContainerControlExtensions
{
	public static void Add<TContainerControl, TChildComponent>(this X<TContainerControl> builder, TChildComponent child)
		where TContainerControl : ContainerControl, new()
		where TChildComponent : Control, new()
	{
		builder.Children.Add(child);
	}

	public static void Add<TContainerControl, TChildComponent>(this X<TContainerControl> builder, Func<IEnumerable<TChildComponent>> func)
		where TContainerControl : ContainerControl, new()
		where TChildComponent : Control, new()
	{
		builder.Children.Add(func);
	}

	public static void Add<TContainerControl, TChildComponent>(this X<TContainerControl> builder, Func<IReactiveCollection<TChildComponent>> func)
		where TContainerControl : ContainerControl, new()
		where TChildComponent : Control, new()
	{
		builder.Children.Add(func);
	}
}
