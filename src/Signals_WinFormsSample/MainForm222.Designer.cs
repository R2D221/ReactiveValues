namespace Signals_WinFormsSample
{
	partial class MainForm222
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			listBox1 = new ListBox();
			contentPanel = new Panel();
			testPage11 = new Signals_WinFormsSample.Pages.TestPage1();
			testPage21 = new Signals_WinFormsSample.Pages.TestPage2();
			contentPanel.SuspendLayout();
			SuspendLayout();
			// 
			// listBox1
			// 
			listBox1.Dock = DockStyle.Left;
			listBox1.FormattingEnabled = true;
			listBox1.ItemHeight = 30;
			listBox1.Items.AddRange(new object[] { "AAA", "BBB" });
			listBox1.Location = new Point(0, 0);
			listBox1.Name = "listBox1";
			listBox1.Size = new Size(300, 1344);
			listBox1.TabIndex = 1;
			listBox1.SelectedValueChanged += listBox1_SelectedValueChanged;
			// 
			// contentPanel
			// 
			contentPanel.Controls.Add(testPage11);
			contentPanel.Controls.Add(testPage21);
			contentPanel.Dock = DockStyle.Fill;
			contentPanel.Location = new Point(300, 0);
			contentPanel.Name = "contentPanel";
			contentPanel.Size = new Size(1492, 1344);
			contentPanel.TabIndex = 2;
			// 
			// testPage11
			// 
			testPage11.Dock = DockStyle.Fill;
			testPage11.Location = new Point(0, 0);
			testPage11.Name = "testPage11";
			testPage11.Size = new Size(1492, 1344);
			testPage11.TabIndex = 0;
			// 
			// testPage21
			// 
			testPage21.Location = new Point(0, 0);
			testPage21.Name = "testPage21";
			testPage21.Size = new Size(1146, 727);
			testPage21.TabIndex = 1;
			// 
			// MainForm222
			// 
			AutoScaleDimensions = new SizeF(168F, 168F);
			AutoScaleMode = AutoScaleMode.Dpi;
			ClientSize = new Size(1792, 1344);
			Controls.Add(contentPanel);
			Controls.Add(listBox1);
			Margin = new Padding(5);
			Name = "MainForm222";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "MainForm222";
			Load += MainForm222_Load;
			contentPanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion
		private ListBox listBox1;
		private Panel contentPanel;
		private Pages.TestPage2 testPage21;
		private Pages.TestPage1 testPage11;
	}
}