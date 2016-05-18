using BefunExec.Logic;
using BefunExec.View.OpenGL.OGLMath;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;

namespace BefunExec.View
{
	public class GLStackViewControl : GLExtendedViewControl
	{
		private StringFontRasterSheet stackFont;

		public readonly List<long> CurrStack = new List<long>();
		private BefunProg prog;

		public GLStackViewControl()
		{
			Loaded = false;
		}

		public void DoInit(BefunProg p)
		{
			prog = p;

			MakeCurrent();

			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.DepthTest);

			stackFont = StringFontRasterSheet.Create(Properties.Resources.font, 24, Color.White);

			Loaded = true;
		}

		public void ReInit(BefunProg p)
		{
			Loaded = false;

			prog = p;

			Loaded = true;
		}

		public void DoRender()
		{
			#region INIT

			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.ClearColor(Color.Black);

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0.0, Width, 0.0, Height, 0.0, 4.0);

			GL.Color3(1.0, 1.0, 1.0);

			#endregion

			#region STACK

			CurrStack.Clear();

			lock (prog.Stack)
			{
				CurrStack.AddRange(prog.Stack);
			}

			stackFont.bind();

			if (RunOptions.SHOW_STACK_REVERSED)
				RenderStackFromTail();
			else
				RenderStackFromHead();

			#endregion

			#region FINISH

			SwapBuffers();

			#endregion
		}

		private void RenderStackFromHead()
		{
			float fh = 15 + RenderFont(Height, new Vec2D(10f, 15f), "Stack<" + CurrStack.Count + ">", -1, stackFont, false) * 1.15f;
			foreach (long val in CurrStack)
			{
				string sval;
				if (RunOptions.ASCII_STACK && val >= 32 && val <= 126)
					sval = string.Format("{0} <{1}>", val, (char)val);
				else
					sval = "" + val;

				fh += RenderFont(Height, new Vec2D(10f, fh), sval, -1, stackFont, false) * 1.15f;
				if (fh > 2 * Height)
					break;
			}
		}

		private void RenderStackFromTail()
		{
			float fh = 15 + RenderFont(Height, new Vec2D(10f, 15f), "Stack<" + CurrStack.Count + ">", -1, stackFont, false) * 1.15f;
			for (int i = 0; i < CurrStack.Count; i++)
			{
				long val = CurrStack[CurrStack.Count - i - 1];

				string sval;
				if (RunOptions.ASCII_STACK && val >= 32 && val <= 126)
					sval = string.Format("{0} <{1}>", val, (char)val);
				else
					sval = "" + val;

				fh += RenderFont(Height, new Vec2D(10f, fh), sval, -1, stackFont, false) * 1.15f;
				if (fh > 2 * Height)
					break;
			}
		}
	}
}
