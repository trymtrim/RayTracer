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
		private const float MOVEMENT_SPEED = 0.5f;

		public Settings settings;

		private DebugWindow _debugWindow = new DebugWindow();
		private Renderer _renderer = new Renderer();

		private float _cameraFOV;
		private Vector3 _cameraOrigin;
		private Vector3 _cameraDirection;

		private Camera _camera;

		public void Init()
		{
			//Initialize settings
			settings.width = 1280; //640;
			settings.height = 720; //480;
			settings.scene = new Scene(SceneType.SpheresWithSpot);
			settings.maxDepth = 5;
			settings.backgroundColor = new Vector3(0.235294f, 0.67451f, 0.843137f);
			settings.antiAliasing = 4; //TODO: Implement anti-aliasing

			//Initialize camera
			_cameraFOV = 60.0f;
			_cameraOrigin = new Vector3(0.0f, 0.0f, 0.0f);
			_cameraDirection = new Vector3(0.0f, 0.0f, 1.0f);

			var aspectRatio = settings.width / (float)settings.height;

			_camera = new Camera(_cameraFOV, aspectRatio, _cameraOrigin, _cameraDirection);
		}

		public void Tick()
		{
			//screen.Print("hello world!", 2, 2, 0xffffff);
		}

		public void OnUpdateFrame()
		{
			var keyboard = OpenTK.Input.Keyboard.GetState();

			if (keyboard[OpenTK.Input.Key.W])
				MoveForward();
			if (keyboard[OpenTK.Input.Key.S])
				MoveBackward();
			if (keyboard[OpenTK.Input.Key.A])
				MoveLeft();
			if (keyboard[OpenTK.Input.Key.D])
				MoveRight();
		}

		private void MoveForward()
		{
			_camera.origin.Z += MOVEMENT_SPEED;
		}

		private void MoveBackward()
		{
			_camera.origin.Z -= MOVEMENT_SPEED;
		}

		private void MoveLeft()
		{
			_camera.origin.X -= MOVEMENT_SPEED;
		}

		private void MoveRight()
		{
			_camera.origin.X += MOVEMENT_SPEED;
		}

		public void Render()
		{
			Bitmap bitmap = new Bitmap(settings.width, settings.height);

			float scale = (float)Math.Tan(MathHelper.DegreesToRadians(_cameraFOV * 0.5f)); //Test
			float imageAspectRatio = (float)settings.width / settings.height; //Test

			Ray ray = new Ray();

			Random randomGenerator = new Random();
			float random = (float)randomGenerator.NextDouble();
			//var offset = 

			for (int j = 0; j < settings.height; ++j)
			{
				for (int i = 0; i < settings.width; ++i)
				{
					//if (_settings.antiAliasing <= 1)
					//{
						float x = (2.0f * (i + 0.5f) / settings.width - 1.0f) * imageAspectRatio * scale; //Test
						float y = (1.0f - 2.0f * (j + 0.5f) / settings.height) * scale; //Test

						//Reuse ray
						ray.origin = _camera.origin;
						ray.direction = new Vector3(x, -y, 1.0f);

						var colorVector = Renderer.Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor);

						//TODO: Do gamma correction

						float red = MathHelper.Clamp(colorVector.X, 0.0f, 1.0f);
						float green = MathHelper.Clamp(colorVector.Y, 0.0f, 1.0f);
						float blue = MathHelper.Clamp(colorVector.Z, 0.0f, 1.0f);

						Color color = Color.FromArgb(255, (int)(red * 255), (int)(green * 255), (int)(blue * 255));
						bitmap.SetPixel(i, j, color);
					//}
					//else
					//{
						//TODO: Implement anti-aliasing

						//var colorVector = new Vector3(0.0f, 0.0f, 0.0f);

						//for (int k = 0; k < _settings.antiAliasing; i++)
						//{
						//	float x = ((float)randomGenerator.NextDouble() + (2.0f * (i + 0.5f)) / _settings.width - 1.0f) * imageAspectRatio * scale; //Test
						//	float y = ((float)randomGenerator.NextDouble() + (1.0f - 2.0f * (j + 0.5f)) / _settings.height) * scale; //Test

						//	//Reuse ray
						//	ray.origin = camera.origin;
						//	ray.direction = new Vector3(x, -y, 1.0f);

						//	colorVector += Renderer.Trace(_settings.maxDepth, _settings.scene, ray, _settings.backgroundColor);
						//}

						//float red = MathHelper.Clamp(colorVector.X, 0.0f, 1.0f);
						//float green = MathHelper.Clamp(colorVector.Y, 0.0f, 1.0f);
						//float blue = MathHelper.Clamp(colorVector.Z, 0.0f, 1.0f);

						//Color color = Color.FromArgb(255, (int)(red * 255), (int)(green * 255), (int)(blue * 255));
						//bitmap.SetPixel(i, j, color);
					//}
				}
			}

			Renderer.screen.UpdateSurface(bitmap);

			_debugWindow.Render();
		}
	}
}
