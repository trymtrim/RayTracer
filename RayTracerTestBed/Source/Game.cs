using System;
using OpenTK;
using OpenTK.Input;

namespace RayTracerTestBed
{
	class Game
	{
		private const float MOVEMENT_SPEED = 0.5f;

		public static Settings settings;

		private static Camera _camera;

		public void Init()
		{
			//Initialize settings
			settings.width = GlobalOptions.ASPECT_RATIO_WIDTH;
			settings.height = GlobalOptions.ASPECT_RATIO_HEIGHT;
			settings.scene = new Scene(SceneType.PointLight_VariousMaterials);
			settings.ui = new UserInterface(settings.width, settings.height);
			settings.maxDepth = GlobalOptions.MAX_DEPTH;
			settings.backgroundColor = Vector3.Zero; //new Vector3(0.235294f, 0.67451f, 0.843137f);
			settings.antiAliasing = 4; //TODO: Implement anti-aliasing

			//Initialize camera
			float cameraFOV = GlobalOptions.FOV;
			var aspectRatio = settings.width / (float)settings.height;
			Vector3 cameraOrigin = new Vector3(0.0f, 0.0f, -2.0f);
			Vector3 cameraDirection = new Vector3(0.0f, 0.0f, 1.0f); //TODO: This is currently not used
			_camera = new Camera(cameraFOV, aspectRatio, cameraOrigin, cameraDirection);

			//Initialize debug window
			DebugUI.Initialize();
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
		}

		public void OnMouseButtonDown(Vector2 position)
		{
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
			else
			{
				float scale = (float)Math.Tan(MathHelper.DegreesToRadians(_camera.fov * 0.5f));
				float imageAspectRatio = (float)settings.width / settings.height;

				float x = (2.0f * (position.X + 0.5f) / settings.width - 1.0f) * imageAspectRatio * scale;
				float y = (1.0f - 2.0f * (position.Y + 0.5f) / settings.height) * scale;

				Renderer.NearestIntersection(settings.scene.meshes, new Ray(_camera.origin, new Vector3(x, -y, 1.0f)), out float distance, out int? indexOfNearest);

				if (indexOfNearest.HasValue)
					SelectObject(indexOfNearest.Value);
				else
					DeselectObject();
			}
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
				case SceneType.PointLight_VariousMaterials:
					settings.scene = new Scene(SceneType.SpotLight_ReflectionMaterials);
					break;
				case SceneType.SpotLight_ReflectionMaterials:
					settings.scene = new Scene(SceneType.DirectionalLight_ReflectionRefraction);
					break;
				case SceneType.DirectionalLight_ReflectionRefraction:
					settings.scene = new Scene(SceneType.DirectionalLight_VariousMaterials);
					break;
				case SceneType.DirectionalLight_VariousMaterials:
					settings.scene = new Scene(SceneType.Mirrors);
					break;
				case SceneType.Mirrors:
					settings.scene = new Scene(SceneType.PointLight_VariousMaterials);
					break;
			}	
		}

		public static void ResetCamera()
		{
			_camera.Reset();
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
			DebugUI.Render(settings);
			settings.ui.RenderText();
		}
	}
}
