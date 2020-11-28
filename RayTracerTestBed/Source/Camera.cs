using OpenTK;

namespace RayTracerTestBed
{
	class Camera
	{
		public Vector3 origin;
		public float fov;
		public Vector3 direction;

		private Vector3 originalOrigin;
		private float originalFOV;
		private Vector3 originalDirection;

		//FOV is vertical field of view in degrees
		public Camera(float fov, float aspectRatio, Vector3 origin, Vector3 direction)
		{
			this.origin = origin;
			this.fov = fov;
			this.direction = direction;

			originalOrigin = origin;
			originalFOV = fov;
			originalDirection = direction;
		}

		public void Reset()
		{
			origin = originalOrigin;
			fov = originalFOV;
			direction = originalDirection;
		}
	}
}
