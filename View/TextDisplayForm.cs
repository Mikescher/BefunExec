using System.Windows.Forms;

namespace BefunExec.View
{
	public sealed partial class TextDisplayForm : Form
	{
		public TextDisplayForm(string title, string txt)
		{
			InitializeComponent();

			Text = title;

			textBox1.Text = txt;
			textBox1.Select(0, 0);
		}
	}
}
