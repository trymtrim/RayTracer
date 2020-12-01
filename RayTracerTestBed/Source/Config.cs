
namespace RayTracerTestBed
{
	static class Config
	{
		public const int ASPECT_RATIO_WIDTH = 1280;
		public const int ASPECT_RATIO_HEIGHT = 720;
		public const float FOV = 60.0f;
		public const int MAX_DEPTH = 4;
		public const int PATH_TRACING_SAMPLES = 20;

		public const TraceMethod DEFAULT_TRACE_METHOD = TraceMethod.WhittedRayTracing;
	}
}
