using OpenTK;

namespace RayTracerTestBed
{
	struct Settings
	{
		public int width;
		public int height;
		public Scene scene;
		public UserInterface ui;
		public int maxDepth; //This decides the amount of bounces
		public int antiAliasing;

		public Vector3 backgroundColor;
	}
}
