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
		private Camera _camera;

		public void Init()
		{
			//Initialize settings
			settings.width = 640; //1280;
			settings.height = 480; //720;
			settings.scene = new Scene(SceneType.SpheresWithSpot);
			settings.maxDepth = 5;
			settings.backgroundColor = Vector3.Zero; //new Vector3(0.235294f, 0.67451f, 0.843137f);
			settings.antiAliasing = 4; //TODO: Implement anti-aliasing

			//Initialize camera
			float cameraFOV = 90.0f;
			var aspectRatio = settings.width / (float)settings.height;
			Vector3 cameraOrigin = Vector3.Zero;
			Vector3 cameraDirection = new Vector3(0.0f, 0.0f, 1.0f); //TODO: This is currently not used

			_camera = new Camera(cameraFOV, aspectRatio, cameraOrigin, cameraDirection);

			//Initialize debug window
			DebugWindow.Initialize();
		}

		public void Tick()
		{

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
			if (keyboard[OpenTK.Input.Key.ControlLeft])
				MoveDown();
			if (keyboard[OpenTK.Input.Key.Space])
				MoveUp();
			if (keyboard[OpenTK.Input.Key.Q])
				RotateLeft();
			if (keyboard[OpenTK.Input.Key.E])
				RotateRight();
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

		private void MoveDown()
		{
			_camera.origin.Y += MOVEMENT_SPEED;
		}

		private void MoveUp()
		{
			_camera.origin.Y -= MOVEMENT_SPEED;
		}

		private void RotateLeft()
		{
			//TODO: Implement this

			//float angle = 25;

			//var v = _camera.direction;
			//_camera.direction = new Vector3(
			//	v.X * (float)Math.Cos(angle) + v.Z * (float)Math.Sin(angle),
			//	v.Y,
			//	-v.X * (float) Math.Sin(angle) + v.Z * (float)Math.Cos(angle));
		}

		private void RotateRight()
		{
			//TODO: Implement this
		}

		public void Render()
		{
			Renderer.Render(settings, _camera);
			DebugWindow.Render();
		}
	}
}
