using BefunExec.Logic;
using BefunExec.View.OpenGL.OGLMath;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;

namespace BefunExec.View
{
    public class GLStackViewControl : GLExtendedViewControl
	{
		private StringFontRasterSheet StackFont;

		public List<long> currStack = new List<long>();
		private BefunProg prog;

		public GLStackViewControl()
		{
			loaded = false;
		}

		public void DoInit(BefunProg p)
		{
			this.prog = p;

			this.MakeCurrent();

			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.DepthTest);

		    StackFont = StringFontRasterSheet.create(Properties.Resources.font, 24, Color.White);

			loaded = true;
		}

		public void ReInit(BefunProg p)
		{
			loaded = false;

			this.prog = p;

			loaded = true;
		}

		public void DoRender()
		{
			#region INIT

			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.ClearColor(Color.Black);

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0.0, this.Width, 0.0, this.Height, 0.0, 4.0);

			GL.Color3(1.0, 1.0, 1.0);

			#endregion

			#region STACK

			currStack.Clear();

			lock (prog.Stack)
			{
				currStack.AddRange(prog.Stack);
			}

            StackFont.bind();

			if (RunOptions.SHOW_STACK_REVERSED)
				renderStackFromTail();
			else
				renderStackFromHead();

			#endregion

			#region FINISH

			this.SwapBuffers();

			#endregion
		}

		private void renderStackFromHead()
		{
			float fh = 15 + RenderFont(this.Height, new Vec2d(10f, 15f), "Stack<" + currStack.Count + ">", -1, StackFont, false) * 1.15f;
			for (int i = 0; i < currStack.Count; i++)
			{
				long val = currStack[i];

				string sval;
				if (RunOptions.ASCII_STACK && val >= 32 && val <= 126)
					sval = string.Format("{0} <{1}>", val, (char)val);
				else
					sval = "" + val;

				fh += RenderFont(this.Height, new Vec2d(10f, fh), sval, -1, StackFont, false) * 1.15f;
				if (fh > 2 * this.Height)
					break;
			}
		}

		private void renderStackFromTail()
		{
			float fh = 15 + RenderFont(this.Height, new Vec2d(10f, 15f), "Stack<" + currStack.Count + ">", -1, StackFont, false) * 1.15f;
			for (int i = 0; i < currStack.Count; i++)
			{
				long val = currStack[currStack.Count - i - 1];

				string sval;
				if (RunOptions.ASCII_STACK && val >= 32 && val <= 126)
					sval = string.Format("{0} <{1}>", val, (char)val);
				else
					sval = "" + val;

				fh += RenderFont(this.Height, new Vec2d(10f, fh), sval, -1, StackFont, false) * 1.15f;
				if (fh > 2 * this.Height)
					break;
			}
		}
	}
}
