
namespace RayTracerTestBed
{
	static class Config
	{
		public const int ASPECT_RATIO_WIDTH = 1280; //640;
		public const int ASPECT_RATIO_HEIGHT = 720; //480;
		public const float FOV = 60.0f;
		public const int MAX_DEPTH = 5;
		public const int PATH_TRACING_SAMPLES = 8;

		public const TraceMethod DEFAULT_TRACE_METHOD = TraceMethod.PathTracing;
	}
}
