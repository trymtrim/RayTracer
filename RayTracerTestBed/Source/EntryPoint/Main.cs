using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace RayTracerTestBed
{
	public class OpenTKApp : GameWindow
	{
		static private int _screenID;
		static private Game _game;

		//Called upon app init
		protected override void OnLoad(EventArgs e)
		{
			GL.ClearColor(Color.Black);
			GL.Enable(EnableCap.Texture2D);
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

			_game = new Game();
			_game.Init();

			Width = Game.settings.width;
			Height = Game.settings.height;

			ClientSize = new Size(Width, Height); //(512, 512)

			_screenID = Renderer.screen.GenTexture();
		}

		//Called upon app close
		protected override void OnUnload(EventArgs e)
		{
			GL.DeleteTextures(1, ref _screenID);
		}

		//Called upon window resize
		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
		}

		//Called once per frame; app logic
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			var keyboard = OpenTK.Input.Keyboard.GetState();

			if (keyboard[OpenTK.Input.Key.Escape])
				this.Exit();

			_game.OnUpdateFrame(keyboard);
		}
		
		protected override void OnMouseDown(MouseButtonEventArgs args)
		{
			_game.OnMouseButtonDown(new Vector2(args.X, args.Y));
		}

		//Called once per frame; render
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			_game.Tick();

			_game.Render();

			GL.BindTexture(TextureTarget.Texture2D, _screenID);
			GL.TexImage2D(TextureTarget.Texture2D,
						   0,
						   PixelInternalFormat.Rgba,
						   Renderer.screen.width,
						   Renderer.screen.height,
						   0,
						   OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
						   PixelType.UnsignedByte,
						   Renderer.screen.pixels
						 );
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
			GL.BindTexture(TextureTarget.Texture2D, _screenID);
			GL.Begin(PrimitiveType.Quads);
			GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1.0f, -1.0f);
			GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1.0f, -1.0f);
			GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1.0f, 1.0f);
			GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1.0f, 1.0f);
			GL.End();
			
			SwapBuffers();
		}

		[STAThread]
		public static void Main()
		{
			//Entry point
			using (OpenTKApp app = new OpenTKApp())
				app.Run(30.0, 0.0);
		}
	}
}
