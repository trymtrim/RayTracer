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
	class Game
	{
		private const float MOVEMENT_SPEED = 5.5f;

		private Settings _settings;
		private DebugWindow _debugWindow = new DebugWindow();
		private Renderer _renderer = new Renderer();

		private float cameraFOV;
		private Vector3 cameraOrigin;
		private Vector3 cameraDirection;

		private Camera camera;

		public void Init()
		{
			//Initialize camera
			cameraFOV = 90.0f;
			cameraOrigin = new Vector3(0.0f, 0.0f, 0.0f);
			cameraDirection = new Vector3(0.0f, 0.0f, 1.0f);

			var aspectRatio = (float)_settings.width / (float)_settings.height; //TODO: Unsure about the floats here

			camera = new Camera(cameraFOV, aspectRatio, cameraOrigin, cameraDirection);

			//Initialize settings
			_settings.width = 640;
			_settings.height = 480;
			_settings.scene = new Scene(SceneType.SpheresWithSpot);
			_settings.maxDepth = 5;

			////Initialize settings
			//_settings.width = 640;
			//_settings.height = 480;
			//_settings.maxDepth = 5; //Max 32 rays - TODO: Try reducing this number to increase performance
			////_settings.fov = 90; //TODO: 45 or 90?
			//_settings.backgroundColor = new Vector3(0.235294f, 0.67451f, 0.843137f);
			////_settings.bias = 0.00001f; //TODO, Unsure if this is actually epsilon
			////settings.antiAliasing = //TODO: Set this

			////_renderer.SetGeometry();

			////screen.Clear(0x2222ff); //Make background blue
		}

		public void Tick()
		{
			var keyboard = OpenTK.Input.Keyboard.GetState();

			if (keyboard[OpenTK.Input.Key.W])
			{
				Console.WriteLine("Forward");
				MoveForward();
			}
			if (keyboard[OpenTK.Input.Key.S])
			{
				Console.WriteLine("Backward");
				MoveBackward();
			}
			if (keyboard[OpenTK.Input.Key.A])
			{
				Console.WriteLine("Left");
				MoveLeft();
			}
			if (keyboard[OpenTK.Input.Key.D])
			{
				Console.WriteLine("Right");
				MoveRight();
			}

			//screen.Print("hello world!", 2, 2, 0xffffff);
		}

		/*
		fn draw_line(&self, settings: &RenderSettings, camera: &Camera, y: usize, line: &mut [u8]) {
        for (x, pixel) in line.chunks_mut(4).enumerate() {
            let vx = x as f64 / settings.width as f64;
            let vy = y as f64 / settings.height as f64;
            let ray = camera.ray_through_screen(vx, vy);
            let color = trace(settings.max_path_depth, &settings.scene, &ray);
            let (r, g, b) = color.gamma_correct();
            pixel.copy_from_slice(&[r, g, b, 0xff]);
			}
		} 
		*/

		private void MoveForward()
		{
			camera.origin.Z += MOVEMENT_SPEED;

			Console.WriteLine(cameraDirection);
		}

		private void MoveBackward()
		{
			camera.origin.Z -= MOVEMENT_SPEED;
		}

		private void MoveLeft()
		{
			camera.origin.X -= MOVEMENT_SPEED; 

			Console.WriteLine(cameraDirection);
		}

		private void MoveRight()
		{
			camera.origin.X += MOVEMENT_SPEED;
		}

		public void Render()
		{
			Bitmap bitmap = new Bitmap(_settings.width, _settings.height);

			float scale = (float)Math.Tan(MathHelper.DegreesToRadians(cameraFOV * 0.5f)); //Test
			float imageAspectRatio = (float)_settings.width / _settings.height; //Test

			Ray ray = new Ray();

			for (int j = 0; j < _settings.height; ++j)
			{
				for (int i = 0; i < _settings.width; ++i)
				{
					//float x = i / _settings.width;
					//float y = j / _settings.height;

					float x = (2.0f * (i + 0.5f) / _settings.width - 1.0f) * imageAspectRatio * scale; //Test
					float y = (1.0f - 2.0f * (j + 0.5f) / _settings.height) * scale; //Test

					//Reuse ray
					ray.origin = camera.origin;
					ray.direction = new Vector3(x, -y, 1.0f);

					//Console.WriteLine(ray.direction);

					var colorVector = Renderer.Trace(_settings.maxDepth, _settings.scene, ray);

					//TODO: Do gamma correction

					float red = MathHelper.Clamp(colorVector.X, 0.0f, 1.0f);
					float green = MathHelper.Clamp(colorVector.Y, 0.0f, 1.0f);
					float blue = MathHelper.Clamp(colorVector.Z, 0.0f, 1.0f);
					
					Color color = Color.FromArgb(255, (int)(red * 255), (int)(green * 255), (int)(blue * 255));
					bitmap.SetPixel(i, j, color);
				}
			}

			Renderer.screen.UpdateSurface(bitmap);

			_debugWindow.Render();
		}
	}
}
