using ReactiveValues.DataTypes;
using ReactiveValues.Wpf;
using System.Windows;
using System.Windows.Data;

namespace Signals_WpfTest
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}
	}

	public sealed class MainWindowViewModel : ReactiveObject
	{
		public string GivenName
		{
			get => Property(() => GivenName).Get(() => "");
			set => Property(() => GivenName).Set(value);
		}

		public string FamilyName
		{
			get => Property(() => FamilyName).Get(() => "");
			set => Property(() => FamilyName).Set(value);
		}

		public string Greeting =>
			Property(() => Greeting)
			.Computed(() => $"Hello, World! {GivenName} {FamilyName}");
	}
}

namespace Signals_WpfTest.Bindings.MainWindowViewModel
{
	using static Binder<Signals_WpfTest.MainWindowViewModel>;

	public sealed class GivenName_Input() : Bind<string>(x => x.GivenName, BindingMode.TwoWay);
	public sealed class FamilyName_Input() : Bind<string>(x => x.FamilyName, BindingMode.TwoWay);

	public sealed class Greeting() : Bind<string>(x => x.Greeting);
}
