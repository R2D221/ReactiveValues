using ReactiveSamples.Common;
using System.Windows;

namespace Signals_WpfSample
{
	/// <summary>
	/// Interaction logic for XamlWindow.xaml
	/// </summary>
	public partial class XamlWindow : Window
	{
		public XamlWindow()
		{
			InitializeComponent();
			DataContext = new ViewModel();
		}
	}
}
