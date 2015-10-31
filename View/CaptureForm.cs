using BefunExec.Logic;
using BefunExec.View.OpenGL.OGLMath;
using Gif.Components;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace BefunExec.View
{
	public partial class CaptureForm : Form
	{
		private MainForm mainForm;
		private Logic.BefunProg prog;

		public CaptureForm(MainForm mainForm, Logic.BefunProg prog)
		{
			InitializeComponent();

			this.mainForm = mainForm;
			this.prog = prog;
		}

		private void btnPath_Click(object sender, System.EventArgs e)
		{
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				edPath.Text = saveFileDialog1.FileName;
				btnStart.Enabled = true;
			}
		}

		private void btnStart_Click(object sender, System.EventArgs eargs)
		{
			btnStart.Enabled = false;
			if (cbxAuto.Checked)
			{
				run_Auto();
			}
			else
			{
				run_Count();
			}
			Close();
		}

		private void run_Count()
		{
			prog.CurrSleeptimeFreq = RunOptions.FREQUENCY_SLIDER[15]; // +INF
			prog.Paused = true;
			Thread.Sleep(250);

			AnimatedGifEncoder e = new AnimatedGifEncoder();
			e.Start();
			e.SetRepeat(cbxRepeat.Checked ? 0 : -1);
			e.SetDelay((int)nuDelay.Value);

			Bitmap bmp = null;
			for (int i = 0; i < nudCount.Value; i++)
			{
				Console.Out.WriteLine("Capture Frame " + i);

				bmp = mainForm.GrabScreenshot();

				e.AddFrame(bmp);

				prog.DoSingleStep = true;

				while (prog.DoSingleStep)
				{
					Thread.Sleep(0);
				}
				Thread.Sleep(250);
			}
			e.SetDelay((int)nudFinalDelay.Value);
			e.AddFrame(bmp);

			e.Finish();
			e.Output(edPath.Text);
			Console.Out.WriteLine("GIF saved to \r\n  " + edPath.Text);
		}

		private void run_Auto()
		{
			Vec2i initial = new Vec2i(prog.PC);

			prog.CurrSleeptimeFreq = RunOptions.FREQUENCY_SLIDER[15]; // +INF
			prog.Paused = true;
			Thread.Sleep(250);

			AnimatedGifEncoder e = new AnimatedGifEncoder();
			e.Start();
			e.SetRepeat(cbxRepeat.Checked ? 0 : -1);
			e.SetDelay((int)nuDelay.Value);

			int i = 0;
			Bitmap bmp = null;
			while (i == 0 || (prog.PC != initial && !prog.Delta.isZero()))
			{
				Console.Out.WriteLine("Capture Frame " + i++);

				bmp = mainForm.GrabScreenshot();

				e.AddFrame(bmp);

				prog.DoSingleStep = true;

				while (prog.DoSingleStep)
				{
					Thread.Sleep(0);
				}
				Thread.Sleep(250);
			}
			e.SetDelay((int)nudFinalDelay.Value);
			e.AddFrame(bmp);

			e.Finish();
			e.Output(edPath.Text);
			Console.Out.WriteLine("GIF saved to \r\n  " + edPath.Text);
		}
	}
}
