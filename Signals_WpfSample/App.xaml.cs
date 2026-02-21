using System.Windows;

namespace Signals_WpfSample
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			//new MainWindow().Show();
			new XamlWindow().Show();
		}
	}
}
