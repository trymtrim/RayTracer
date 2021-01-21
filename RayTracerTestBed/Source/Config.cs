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

/*
TODO:
- Uniform distribution of photons emitted from the point light
- Use russian roulette to determine if the photon should be reflected, transmitted or absorbed (based on the material's diffuse reflection, specular reflection, absorption/transmission etc)
- Trace photons based on whether they should be reflected, transmitted or absorbed
- Only store photons when they hit diffuse surfaces (not glass and mirror), including when they are absorbed (if that surface is diffuse)
//- Use standard ray tracing for glass and mirror
- Store photon map as array first - re-organize the map into a balanced kd-tree before rendering
- Divide the stored photons into 2 photon maps:
	- Global photon map: an approximate representation of the global illumination solution for the scene for all diffuse surfaces
	- Caustic photon map: Contains photons that have been through at least one specular reflection before hitting a diffuse surface
- Separate photon tracing passes/steps for global and caustic photons, caustic map needs more photons (and directed only towards transmissive objects)
(Use fresnel for caustic - look at how reflection/refraction is done in ray tracing)
- Use a (balanced) kd-tree to find nearest photons (within radius or n nearest photons? (radiance estimate = 100/1000)) - balance algorithm? (pseudo code for locating the nearest photons on page 36)
//- Filter the radiance estimate, helps caustics the most (page 32)
- Shadow photons/direct illumination (page 39-40)

- Separate rendering into Direct/Transmissive/Specular/Caustic/Indirect (?)

Notes:
- Only shoot caustic photons towards transmissive objects
- Flag variable in Photon for kd-tree?
- Have a small glossy/specular value (but only for photon rays?) for diffuse materials?
- Use distribution ray tracing? Significantly fewer photons are necessary when a distribution ray tracer is used to evaluate the first diffuse reflection (page 38/43)
- Use disc instead of sphere/radius to locate photons?
- Ward’s irradiance gradient caching scheme?
- https://www.cs.princeton.edu/courses/archive/fall18/cos526/papers/jensen01.pdf
- https://github.com/ReillyBova/Global-Illumination

 // Compute Reflection Coefficient, carry reflection portion to Specular
      R_coeff = 0;
      if (FRESNEL && brdf->IsTransparent()) {
        R_coeff = ComputeReflectionCoeff(cos_theta, brdf->IndexOfRefraction());
      }
*/
