using ReactiveValues.DataTypes;
using ReactiveValues.Wpf;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ReactiveValues.Markup.Wpf
{
	public sealed class WpfMarkupProvider : MarkupProvider<UIElement>
	{
		private static readonly ThreadLocal<WpfMarkupProvider> threadLocal = new(() => new());
		public static WpfMarkupProvider Current => threadLocal.Value ?? throw new InvalidOperationException();

		private WpfMarkupProvider()
		{
			RegisterTwoWayCallback(
				(TextBox textBox) => textBox.Text,
				(textBox, callback) => textBox.LostFocus += (_, _) => callback());

			//RegisterTwoWayCallback(
			//	(CheckBox control) => control.Checked,
			//	(control, callback) => control.CheckedChanged += (_, _) => callback());

			//RegisterTwoWayCallback(
			//	#warning DataSource?
			//	(ComboBox control) => control.SelectedItem,
			//	(control, callback) => control.SelectionChangeCommitted += (_, _) => callback());

			//RegisterTwoWayCallback(
			//	(DateTimePicker control) => control.Checked,
			//	(control, callback) => control.ValueChanged += (_, _) => callback());

			//RegisterTwoWayCallback(
			//	(DateTimePicker control) => control.Value,
			//	(control, callback) =>
			//	{
			//		var validating = false;
			//		control.ValueChanged += (_, _) =>
			//		{
			//			if (validating)
			//			{
			//				callback();
			//				validating = false;
			//			}
			//		};

			//		control.Validating += (_, _) => validating = true;
			//	});

			//RegisterTwoWayCallback(
			//	(RadioButton control) => control.Checked,
			//	(control, callback) => control.CheckedChanged += (_, _) => callback());
		}

		protected override Watcher GetWatcher(UIElement component) => WpfWatcher.ForElement(component);

		protected override void InitLifetime(UIElement component)
		{
			if (component is Window window)
			{
				InitWindow(window);
			}
			else
			{
				InitControl(component);
			}
		}

		private void InitWindow(Window window)
		{
			static bool IsVisible(Window window) =>
				window.IsVisible
				&&
				window.WindowState is not WindowState.Minimized
				;

			var oldVisible = false;

			ToggleWatch();

			window.IsVisibleChanged += (_, _) => ToggleWatch();
			window.StateChanged += (_, _) => ToggleWatch();

			void ToggleWatch()
			{
				var newVisible = IsVisible(window);

				if (oldVisible == newVisible) { return; }

				if (newVisible)
				{
					WatchAttachedEffects(window);
				}
				else
				{
					UnwatchAttachedEffects(window);
				}

				oldVisible = newVisible;
			}
		}

		private void InitControl(UIElement element)
		{
			static bool IsVisible(UIElement element) =>
				element.IsVisible
				&&
				Window.GetWindow(element) is {/*notnull*/} window
				&&
				window.WindowState is not WindowState.Minimized
				;

			var oldOwner = (Window?)null;
			var oldVisible = false;

			ToggleWatch();
			ReattachEvents();

			element.IsVisibleChanged += (_, _) =>
			{
				ToggleWatch();
				ReattachEvents();
			};

			void StateChanged(object? __, EventArgs ___) => ToggleWatch();

			void ToggleWatch()
			{
				var newVisible = IsVisible(element);

				if (oldVisible == newVisible) { return; }

				if (newVisible)
				{
					WatchAttachedEffects(element);
				}
				else
				{
					UnwatchAttachedEffects(element);
				}

				oldVisible = newVisible;
			}

			void ReattachEvents()
			{
				var newOwner = Window.GetWindow(element);

				if (oldOwner == newOwner) { return; }

				if (oldOwner is not null)
				{
					oldOwner.StateChanged -= StateChanged;
				}

				if (newOwner is not null)
				{
					newOwner.StateChanged += StateChanged;
				}

				oldOwner = newOwner;
			}
		}

		protected override Action<UIElement, Action> GetTwoWayCallback(PropertyInfo property)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class ContentChildren<TParentComponent> : Children<UIElement, TParentComponent, UIElement>
		where TParentComponent : ContentControl, new()
	{
		private readonly OrderedSetSegment<UIElement, UIElement> container;

		public ContentChildren(TParentComponent parent) : base(WpfMarkupProvider.Current, parent)
		{
			container = new UIElementContentOrderedSetSegment(parent, () => (UIElement)parent.Content, x => parent.Content = x);
		}

		protected override OrderedSetSegment<UIElement, UIElement> Container => container;
	}

	public sealed class PanelChildren<TParentComponent> : Children<UIElement, TParentComponent, UIElement>
		where TParentComponent : Panel, new()
	{
		private readonly OrderedSetSegment<UIElement, UIElement> container;

		public PanelChildren(TParentComponent parent) : base(WpfMarkupProvider.Current, parent)
		{
			container = new UIElementCollectionOrderedSetSegment(parent, parent.Children);
		}

		protected override OrderedSetSegment<UIElement, UIElement> Container => container;
	}
}
