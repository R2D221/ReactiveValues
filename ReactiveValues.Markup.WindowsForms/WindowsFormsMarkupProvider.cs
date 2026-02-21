using ReactiveValues.DataTypes;
using ReactiveValues.WindowsForms;
using System.Reflection;

namespace ReactiveValues.Markup.WindowsForms
{
	public sealed class WindowsFormsMarkupProvider : MarkupProvider<Control>
	{
		private static readonly ThreadLocal<WindowsFormsMarkupProvider> threadLocal = new(() => new());
		public static WindowsFormsMarkupProvider Current => threadLocal.Value ?? throw new InvalidOperationException();

		private WindowsFormsMarkupProvider()
		{
			RegisterTwoWayCallback(
				(Control control) => control.Text,
				(control, callback) => control.Validating += (_, _) => callback());

			//RegisterTwoWayCallback(
			//	#warning DataSource?
			//	(ComboBox control) => control.SelectedItem,
			//	(control, callback) => control.SelectionChangeCommitted += (_, _) => callback());

			RegisterTwoWayCallback(
				(DateTimePicker control) => control.Checked,
				(control, callback) => control.Validating += (_, _) => callback());

			RegisterTwoWayCallback(
				(DateTimePicker control) => control.Value,
				(control, callback) => control.Validating += (_, _) => callback());
		}

		protected override Watcher GetWatcher(Control component) => WindowsFormsWatcher.ForControl(component);

		protected override void InitLifetime(Control component)
		{
			if (component is Form form)
			{
				InitForm(form);
			}
			else
			{
				InitControl(component);
			}
		}

		private void InitForm(Form form)
		{
			static bool IsVisible(Form form) =>
				Application.OpenForms.Cast<Form>().Contains(form)
				&&
				form.Visible
				&&
				form.WindowState is not FormWindowState.Minimized
				;

			var oldVisible = false;

			ToggleWatch();

			form.VisibleChanged += (_, _) => ToggleWatch();
			form.FormClosed += (_, _) => ToggleWatch();
			form.Resize += (_, _) => ToggleWatch();

			void ToggleWatch()
			{
				var newVisible = IsVisible(form);

				if (oldVisible == newVisible) { return; }

				if (newVisible)
				{
					WatchAttachedEffects(form);
				}
				else
				{
					UnwatchAttachedEffects(form);
				}

				oldVisible = newVisible;
			}
		}

		private void InitControl(Control control)
		{
			static bool IsVisible(Control control) =>
				control.Visible
				&&
				control.FindForm() is {/*notnull*/} form
				&&
				Application.OpenForms.Cast<Form>().Contains(form)
				&&
				form.WindowState is not FormWindowState.Minimized
				;

			var oldOwner = (Form?)null;
			var oldVisible = false;

			ToggleWatch();
			ReattachEvents();

			control.VisibleChanged += (_, _) =>
			{
				ToggleWatch();
				ReattachEvents();
			};

			void FormClosed(object? __, FormClosedEventArgs ___) => ToggleWatch();
			void Resize(object? __, EventArgs ___) => ToggleWatch();

			void ToggleWatch()
			{
				var newVisible = IsVisible(control);

				if (oldVisible == newVisible) { return; }

				if (newVisible)
				{
					WatchAttachedEffects(control);
				}
				else
				{
					UnwatchAttachedEffects(control);
				}

				oldVisible = newVisible;
			}

			void ReattachEvents()
			{
				var newOwner = control.FindForm();

				if (oldOwner == newOwner) { return; }

				if (oldOwner is not null)
				{
					oldOwner.FormClosed -= FormClosed;
					oldOwner.Resize -= Resize;
				}

				if (newOwner is not null)
				{
					newOwner.FormClosed += FormClosed;
					newOwner.Resize += Resize;
				}

				oldOwner = newOwner;
			}
		}

		protected override Action<Control, Action> GetTwoWayCallback(PropertyInfo property)
		{
			var @event = property.DeclaringType!.GetEvent($"{property.Name}Changed");

			if (@event is not null)
			{
				return (control, callback) =>
					@event.AddEventHandler(control, new EventHandler((_, _) => callback()));
			}

			throw new NotImplementedException();
		}
	}

	public sealed class Children<TParentComponent> : Children<Control, TParentComponent, Control>
		where TParentComponent : Control, new()
	{
		private readonly OrderedSetSegment<Control, Control> container;

		public Children(TParentComponent parent) : base(WindowsFormsMarkupProvider.Current, parent)
		{
			container = new ControlCollectionOrderedSetSegment(parent.Controls);
		}

		protected override OrderedSetSegment<Control, Control> Container => container;
	}
}
