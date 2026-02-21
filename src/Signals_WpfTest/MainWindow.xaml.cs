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
			get => Get(() => GivenName, () => "");
			set => Set(() => GivenName, value);
		}

		public string FamilyName
		{
			get => Get(() => FamilyName, () => "");
			set => Set(() => FamilyName, value);
		}

		public string Greeting => Computed(() => Greeting,
			() => $"Hello, World! {GivenName} {FamilyName}");
	}
}

namespace Signals_WpfTest.Bindings.MainWindowViewModel
{
	using static Binder<Signals_WpfTest.MainWindowViewModel>;

	public sealed class GivenName_Input() : Bind<string>(x => x.GivenName, BindingMode.TwoWay);
	public sealed class FamilyName_Input() : Bind<string>(x => x.FamilyName, BindingMode.TwoWay);

	public sealed class Greeting() : Bind<string>(x => x.Greeting);
}
