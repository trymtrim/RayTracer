using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RayTracerTestBed
{
	class DebugWindow
	{
		private static Stopwatch stopwatch;
		private static int fpsTemp = 0;

		private static int fps = 0;



		public static void Initialize()
		{
			stopwatch = new Stopwatch();
			stopwatch.Start();
		}

		public static void Render()
		{
			//TODO: Make a better way of tracking FPS
			fpsTemp++;

			if (stopwatch.Elapsed.TotalMilliseconds >= 1000)
			{
				fps = fpsTemp;

				fpsTemp = 0;
				stopwatch.Restart();
			}

			Console.WriteLine(fps);

			Renderer.screen.Print("FPS: " + fps, 2, 2, 0xffffff);
			//Game.screen.Print("FPS", 2, 20, 0xffffff);
			//Game.screen.Print("FPS", 2, 38, 0xffffff);
			//Game.screen.Print("FPS", 2, 56, 0xffffff);
		}
	}
}
