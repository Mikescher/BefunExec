using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BefungExec.View
{
	public partial class TextDisplayForm : Form
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
