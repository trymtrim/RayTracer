using OpenTK;

namespace RayTracerTestBed
{
	enum TraceMethod
	{
		WhittedRayTracing,
		PathTracing
	}

	struct Settings
	{
		public int width;
		public int height;
		public Scene scene;
		public UserInterface ui;
		public int maxDepth; //This decides the amount of bounces
		public Vector3 backgroundColor;
		public TraceMethod traceMethod;
		public bool showUI;
		public int antiAliasing; //TODO: Implement this
	}
}
