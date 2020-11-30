using System.Collections.Generic;
using System.Diagnostics;

namespace RayTracerTestBed
{
	class DebugUI
	{
		public static Mesh selectedObject;
		public static Material selectedMaterial;

		private static Stopwatch _stopwatch;
		private static int _fpsTemp = 0;
		private static int _fps = 0;

		public static void Initialize()
		{
			_stopwatch = new Stopwatch();
			_stopwatch.Start();
		}

		public static void Render(Settings settings)
		{
			//TODO: Make a better way of tracking FPS?
			_fpsTemp++;

			if (_stopwatch.Elapsed.TotalMilliseconds >= 1000)
			{
				_fps = _fpsTemp;

				_fpsTemp = 0;
				_stopwatch.Restart();
			}

			Renderer.screen.Print("FPS: " + _fps, settings.width / 2 - 35, 2, 0xffffff);

			Renderer.screen.Print("Trace method: " + settings.traceMethod.ToString(), 10, 10, 0xffffff);
			Renderer.screen.Print("Scene: " + settings.scene.sceneType.ToString(), 10, 40, 0xffffff);
			Renderer.screen.Print(selectedObject == null ? "No selected object" : selectedObject.name == null ? "Unnamed object" : "Selected: " + selectedObject.name, 10, 70, 0xffffff);

			if (selectedObject != null)
			{
				List<string> debugInfo = selectedObject.DebugInfo();

				int space = 0;

				for (int i = 0; i < debugInfo.Count; i++)
				{
					if (debugInfo[i].Contains("Position"))
						space = 10;

					Renderer.screen.Print(debugInfo[i], 10, 100 + 20 * i + space, 0xffffff);
				}
			}
		}
	}
}
