using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;

namespace RayTracerTestBed
{
	class Game
	{
		private const float MOVEMENT_SPEED = 0.5f;
		private const float ROTATION_SPEED = 0.5f;

		public static RenderSettings settings;

		private static Camera _camera;

		private float _angle = 0.0f;

		private Stopwatch _stopwatch = new Stopwatch();

		//Stats
		public static Stopwatch renderTimeStopwatch = new Stopwatch();
		public static int numPrimaryRays = 0;
		public static int numRayTests = 0;
		public static int numRayIntersections = 0;

		public void Init()
		{
			//Initialize settings
			settings.width = Config.ASPECT_RATIO_WIDTH;
			settings.height = Config.ASPECT_RATIO_HEIGHT;
			settings.ui = new UserInterface(settings.width, settings.height);
			settings.maxDepth = Config.MAX_DEPTH;
			settings.backgroundColor = Vector3.Zero;
			settings.scene = new Scene(SceneType.Everything);
			settings.traceMethod = Config.DEFAULT_TRACE_METHOD;
			settings.showUI = true;
			//settings.antiAliasing = 4; //TODO: Implement anti-aliasing

			//Initialize camera
			Vector3 cameraPosition = new Vector3(0.0f, 0.5f, -1.75f); //new Vector3(0.0f, 0.0f, 0.0f); //(0.0f, 0.0f, -2.0f) //new Vector3(0.0f, 0.5f, -1.75f)
			Vector3 cameraDirection = new Vector3(0.0f, 0.0f, 1.0f);
			_camera = new Camera(Config.FOV, cameraPosition, cameraDirection);

			//Initialize debug window
			DebugUI.Initialize();

			//Start render frame stopwatch
			renderTimeStopwatch.Start();
		}

		public void Tick()
		{

		}

		public void OnUpdateFrame(KeyboardState keyboard)
		{
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

			if (keyboard[OpenTK.Input.Key.Z])
			{
				if (_stopwatch.IsRunning)
				{
					if (_stopwatch.Elapsed.TotalMilliseconds > 1000)
					{
						Console.WriteLine("Toggle");
						ToggleUI();
						_stopwatch.Restart();
					}
				}
				else
				{
					ToggleUI();
					_stopwatch.Start();

					Console.WriteLine("Toggle First");
				}
			}
		}

		public void OnMouseButtonDown(Vector2 position)
		{
			bool buttonHit = HandleButtonEvents(position);

			if (!buttonHit)
				HandleObjectSelection(position);
		}

		private bool HandleButtonEvents(Vector2 position)
		{
			//Handle button events
			bool buttonHit = false;
			Button button = null;

			for (int i = 0; i < settings.ui.buttons.Count; i++)
			{
				button = settings.ui.buttons[i];

				if (button.IsAtPosition(position))
				{
					buttonHit = true;
					break;
				}
			}

			if (buttonHit)
				button.OnClick();

			return buttonHit;
		}

		private void HandleObjectSelection(Vector2 position)
		{
			float scale = (float)Math.Tan(MathHelper.DegreesToRadians(_camera.fov * 0.5f));
			float imageAspectRatio = (float)settings.width / settings.height;

			float x = (2.0f * (position.X + 0.5f) / settings.width - 1.0f) * imageAspectRatio * scale;
			float y = (1.0f - 2.0f * (position.Y + 0.5f) / settings.height) * scale;

			RayTracer.NearestIntersection(settings.scene.meshes, new Ray(_camera.position, new Vector3(x, -y, 1.0f)), out float distance, out int? indexOfNearest);

			if (indexOfNearest.HasValue)
				SelectObject(indexOfNearest.Value);
			else
				DeselectObject();
		}

		private void SelectObject(int index)
		{
			DeselectObject();

			Mesh mesh = settings.scene.meshes[index];
			Material material = settings.scene.materials[index];

			DebugUI.selectedObject = mesh;
			DebugUI.selectedMaterial = material;
			material.selected = true;
		}

		public static void DeselectObject()
		{
			if (DebugUI.selectedMaterial == null)
				return;

			DebugUI.selectedMaterial.selected = false;

			DebugUI.selectedObject = null;
			DebugUI.selectedMaterial = null;
		}

		public static void ChangeScene()
		{
			DeselectObject();

			ResetCamera();

			switch (settings.scene.sceneType)
			{
				case SceneType.Skybox:
					settings.scene = new Scene(SceneType.SpotLight);
					break;
				case SceneType.SpotLight:
					settings.scene = new Scene(SceneType.Photograph);
					break;
				case SceneType.Photograph:
					settings.scene = new Scene(SceneType.Everything);
					break;
				case SceneType.Everything:
					settings.scene = new Scene(SceneType.Mirrors);
					break;
				case SceneType.Mirrors:
					settings.scene = new Scene(SceneType.Room);
					break;
				case SceneType.Room:
					settings.scene = new Scene(SceneType.Skybox);
					break;
			}
		}

		public static void ChangeTraceMethod()
		{
			switch (settings.traceMethod)
			{
				case TraceMethod.WhittedRayTracing:
					settings.traceMethod = TraceMethod.PathTracing;
					break;
				case TraceMethod.PathTracing:
					settings.traceMethod = TraceMethod.WhittedRayTracing;
					break;
			}
		}

		public static void ResetCamera()
		{
			_camera.Reset();
		}

		private void MoveForward()
		{
			_camera.position += _camera.direction * MOVEMENT_SPEED;
		}

		private void MoveBackward()
		{
			_camera.position -= _camera.direction * MOVEMENT_SPEED;
		}

		private void MoveLeft()
		{
			Vector3 upVector = new Vector3(0.0f, 1.0f, 0.0f);
			_camera.position += Vector3.Cross(_camera.direction, upVector) * MOVEMENT_SPEED;
		}

		private void MoveRight()
		{
			Vector3 upVector = new Vector3(0.0f, 1.0f, 0.0f);
			_camera.position -= Vector3.Cross(_camera.direction, upVector) * MOVEMENT_SPEED;
		}

		private void MoveDown()
		{
			_camera.position.Y += MOVEMENT_SPEED;
		}

		private void MoveUp()
		{
			_camera.position.Y -= MOVEMENT_SPEED;
		}

		private void RotateLeft()
		{
			_angle += ROTATION_SPEED;

			var v = _camera.direction;
			_camera.direction = new Vector3(
				v.X * (float)Math.Cos(_angle) + v.Z * (float)Math.Sin(_angle),
				v.Y,
				-v.X * (float)Math.Sin(_angle) + v.Z * (float)Math.Cos(_angle));
		}

		private void RotateRight()
		{
			_angle -= ROTATION_SPEED;

			var v = _camera.direction;
			_camera.direction = new Vector3(
				v.X * (float)Math.Cos(_angle) + v.Z * (float)Math.Sin(_angle),
				v.Y,
				-v.X * (float)Math.Sin(_angle) + v.Z * (float)Math.Cos(_angle));
		}

		private void ToggleUI()
		{
			settings.showUI = !settings.showUI;
		}

		public void Render()
		{
			renderTimeStopwatch.Restart();

			Renderer.Render(settings, _camera);

			PrintStats();

			//TODO: For debugging - render bounding box edges in scene as lines (?)

			if (settings.showUI)
			{
				DebugUI.Render(settings);
				settings.ui.RenderText();
			}

			//TODO: Handle this
			//if (settings.scene.skybox != null)
			//	settings.scene.skybox.Update(_camera.position);
		}

		private void PrintStats()
		{
			double renderTime = renderTimeStopwatch.Elapsed.TotalMilliseconds / 1000.0f;

			Console.WriteLine("--------------------");
			Console.WriteLine("Primitives: " + settings.scene.meshes.Count);
			Console.WriteLine("Render time: " + renderTime + "\n");
			Console.WriteLine("Primary rays: " + numPrimaryRays);
			Console.WriteLine("Intersection tests: " + numRayTests);
			Console.WriteLine("Ray intersections: " + numRayIntersections);

			numPrimaryRays = 0;
			numRayTests = 0;
			numRayIntersections = 0;
		}
	}
}
