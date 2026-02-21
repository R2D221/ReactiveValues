using ReactiveSamples.Common;
using ReactiveValues.Markup;
using ReactiveValues.Markup.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Signals_WpfSample;

internal sealed class MainWindow : Window
{
	private readonly ViewModel viewModel = new();
	////private readonly ReactiveFunc<DateTime> time = Reactive.Volatile(() => DateTime.Now);
	////private readonly ReactiveFunc<int> fps = Reactive.Volatile(() => FPS.Calculate());

	public MainWindow() =>
		new X<MainWindow>(this, new()
		{
			{ x => x.Width, () => 800 },
			{ x => x.Height, () => 600 },
			{ x => x.Title, () => "Signals Sample" },
			{ x => x.WindowStartupLocation, () => WindowStartupLocation.CenterScreen },
		})
		{
			new X<StackPanel>(new()
			{
				{ x => x.Margin, () => new(5) },
			})
			{
				new X<Button>(new()
				{
					{ x => x.Content, () => "Click me" },
					{ x => x.Click += (_, _) => MessageBox.Show("Hello, World!") }
				})
				.X<Button>(),

				////new X<Label>(new()
				////{
				////	//{ x => x.Content, () => $"The current time is {time.Value}" },
				////	{ x => x.Content, () => $"FPS: {fps.Value}" },
				////})
				////.X<Label>(),

				new X<Label>(new()
				{
					{ x => x.Content, () => "Given name:" },
				})
				.X<Label>(),

				new X<TextBox>(new()
				{
					{ x => x.Text, () => viewModel.GivenName, BindingMode.TwoWay },
				})
				.X<TextBox>(),

				new X<Label>(new()
				{
					{ x => x.Content, () => "Family name:" },
				})
				.X<Label>(),

				new X<TextBox>(new()
				{
					{ x => x.Text, () => viewModel.FamilyName, BindingMode.TwoWay },
				})
				.X<TextBox>(),

				new X<Label>(new()
				{
					{ x => x.Content, () => "Full name:" },
				})
				.X<Label>(),

				new X<TextBox>(new()
				{
					{ x => x.Text, () => viewModel.FullName },
					{ x => x.IsReadOnly, () => true },
				})
				.X<TextBox>(),
			}
			.X<StackPanel>(),
		}
		.X<MainWindow>();
}
