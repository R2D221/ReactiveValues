using ReactiveSamples.Common;
using ReactiveValues.Markup;
using ReactiveValues.Markup.WindowsForms;

namespace Signals_WinFormsSample;

public sealed class MainForm : Form
{
	private readonly ViewModel viewModel = new();
	////private readonly ReactiveFunc<DateTime> time = Reactive.Volatile(() => DateTime.Now);
	////private readonly ReactiveFunc<int> fps = Reactive.Volatile(() => FPS.Calculate());

	public MainForm() =>
		new X<MainForm>(this, new()
		{
			{ x => x.AutoScaleMode, () => AutoScaleMode.Dpi },
			{ x => x.AutoScaleDimensions, () => new(96F, 96F) },
			{ x => x.Width, () => 800 },
			{ x => x.Height, () => 600 },
			{ x => x.Text, () => "Signals Sample" },
			{ x => x.StartPosition, () => FormStartPosition.CenterScreen },
		})
		{
			new X<Panel>(new()
			{
				{ x => x.Dock, () => DockStyle.Top },
				{ x => x.AutoSize, () => true },
				{ x => x.Padding, () => new(5) },
			})
			{
				new X<Button>(new()
				{
					{ x => x.Text, () => "Click me" },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
					{ x => x.Click += (_, _) => MessageBox.Show("Hello, World!") },
				})
				.X<Button>(),

				////new X<Label>(new()
				////{
				////	//{ x => x.Text, () => $"The current time is {time.Value}" },
				////	{ x => x.Text, () => $"FPS: {fps.Value}" },
				////	{ x => x.AutoSize, () => true },
				////	{ x => x.Dock, () => DockStyle.Bottom },
				////})
				////.X<Label>(),

				new X<Label>(new()
				{
					{ x => x.Text, () => "Given name:" },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
				})
				.X<Label>(),

				new X<TextBox>(new()
				{
					{ x => x.Text, () => viewModel.GivenName, BindingMode.TwoWay },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
				})
				.X<TextBox>(),

				new X<Label>(new()
				{
					{ x => x.Text, () => "Family name:" },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
				})
				.X<Label>(),

				new X<TextBox>(new()
				{
					{ x => x.Text, () => viewModel.FamilyName, BindingMode.TwoWay },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
				})
				.X<TextBox>(),

				new X<Label>(new()
				{
					{ x => x.Text, () => "Full name:" },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
				})
				.X<Label>(),

				new X<TextBox>(new()
				{
					{ x => x.Text, () => viewModel.FullName },
					{ x => x.ReadOnly, () => true },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
				})
				.X<TextBox>(),

				new X<Label>(new()
				{
					{ x => x.Text, () => "Age:" },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
				})
				.X<Label>(),

				new X<TextBox>(new()
				{
					{ x => x.Text, Converters.Format(() => viewModel.Age), BindingMode.TwoWay },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
				})
				.X<TextBox>(),

				new X<TextBox>(new()
				{
					{ x => x.Text, () => $"{viewModel.Age}" },
					{ x => x.ReadOnly, () => true },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
				})
				.X<TextBox>(),

				new X<Label>(new()
				{
					{ x => x.Text, () => "Example 1. Multiple operations between renders" },
					{ x => x.AutoSize, () => true },
					{ x => x.Dock, () => DockStyle.Bottom },
				})
				.X<Label>(),

				() =>
					from item in viewModel.Items select
					new X<Label>(new()
					{
						{ x => x.Text, () => $"• {item}" },
						{ x => x.BackColor, () => Color.LightPink },
						{ x => x.AutoSize, () => true },
						{ x => x.Dock, () => DockStyle.Bottom },
					})
					.X<Label>(),
			}
			.X<Panel>(),
		}
		.X<MainForm>();
}