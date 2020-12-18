namespace RayTracerTestBed
{
	static class Config
	{
		//General
		public const int ASPECT_RATIO_WIDTH = 640; //1280
		public const int ASPECT_RATIO_HEIGHT = 480; //720
		public const float FOV = 60.0f;
		public const int MAX_DEPTH = 4;
		public const int PATH_TRACING_SAMPLES = 300;

		//Default states
		public const TraceMethod DEFAULT_TRACE_METHOD = TraceMethod.WhittedRayTracing;
		public const bool SHOW_UI_BY_DEFAULT = false;

		//BVH
		public const bool USE_BVH = true;
		public const int BINNING_SPLIT_PLANE_COUNT = 8;
	}
}
