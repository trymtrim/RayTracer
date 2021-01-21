namespace RayTracerTestBed
{
	static class Config
	{
		//General
		public const int ASPECT_RATIO_WIDTH = 640; //1280
		public const int ASPECT_RATIO_HEIGHT = 480; //720
		public const float FOV = 60.0f;
		public const int MAX_DEPTH = 4;
		public const int PATH_TRACING_SAMPLES = 10;

		//Default states
		public const TraceMethod DEFAULT_TRACE_METHOD = TraceMethod.PhotonTracing;
		public const bool SHOW_UI_BY_DEFAULT = true;

		//BVH
		public const bool USE_BVH = false;
		public const int BINNING_SPLIT_PLANE_COUNT = 8;
		public const bool USE_ALTERNATIVE_BINNING_METHOD = true;

		//Photon mapping
		public const bool RENDER_PHOTON_MAP = false;
		public const int PHOTON_COUNT = 20000; //Millions?
		public const int CAUSTICS_PHOTON_COUNT = 4000;
		public const int MAX_PHOTON_DEPTH = 3;
		public const float MAX_PHOTON_SEARCH_RADIUS = 0.1f;
	}
}
