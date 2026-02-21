namespace Signals_WinFormsSample;

public partial class MainForm222 : Form
{
	public MainForm222()
	{
		InitializeComponent();
	}

	private void listBox1_SelectedValueChanged(object sender, EventArgs e)
	{
		var index = 0;
		var selectedIndex = listBox1.SelectedIndex;
		foreach (Control control in contentPanel.Controls)
		{
			control.Visible = (index == selectedIndex);
			index++;
		}
	}

	private void MainForm222_Load(object sender, EventArgs e)
	{
		foreach (Control control in contentPanel.Controls)
		{
			control.Visible = false;
		}
	}
}

//public sealed class MainFormViewModel : SignalObject
//{
//	public int SelectedPage { get => Get<int>(); set => Set(value); }

//	public bool IsPage1 => Computed(() => SelectedPage == 1);
//	public bool IsPage2 => Computed(() => SelectedPage == 2);
//}
