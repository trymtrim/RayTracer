using OpenTK;

namespace RayTracerTestBed
{
	class Ray
	{
		public Vector3 origin;
		public Vector3 direction;

		public Vector3 dirFrac;

		public Ray() { }

		public Ray(Vector3 origin, Vector3 direction)
		{
			this.origin = origin;
			this.direction = direction;
		}

		public Vector3 At(float t)
		{
			return origin + direction * t;
		}

		public void InitializeDirFrac()
		{
			dirFrac = new Vector3
			{
				X = 1.0f / direction.X,
				Y = 1.0f / direction.Y,
				Z = 1.0f / direction.Z
			};
		}
	}
}
