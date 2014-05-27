using System.Diagnostics;
using System.Windows.Forms;

namespace BefungExec.View
{
	public partial class AboutForm : Form
	{
		public AboutForm()
		{
			InitializeComponent();

			redLicense.Text = Properties.Resources.license;
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://www.opentk.com/");
		}

		private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://www.opentk.com/project/QuickFont");
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://www.mikescher.de/");
		}

		private void redLicense_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
		}
	}
}
