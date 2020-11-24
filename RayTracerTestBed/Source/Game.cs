using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;

namespace RayTracerTestBed
{
	//TODO: Consider removing/changing SceneObjects (to work with only triangles top-level)

	class Game
	{
		private Settings _settings;
		private DebugWindow _debugWindow = new DebugWindow();
		private Renderer _renderer = new Renderer();

		public void Init()
		{
			//Initialize settings
			_settings.width = 640;
			_settings.height = 480;
			_settings.maxDepth = 5; //Max 32 rays - TODO: Try reducing this number to increase performance
			_settings.fov = 90; //TODO: 45 or 90?
			_settings.backgroundColor = new Vector3f(0.235294f, 0.67451f, 0.843137f);
			_settings.bias = 0.00001f; //TODO, Unsure if this is actually epsilon
			//settings.antiAliasing = //TODO: Set this

			_renderer.SetGeometry();

			//screen.Clear(0x2222ff); //Make background blue
		}

		public void Tick()
		{
			//screen.Print("hello world!", 2, 2, 0xffffff);
		}

		public void Render()
		{
			_renderer.Render(_settings);

			//scene.Render();
			_debugWindow.Render();
		}
	}
}
