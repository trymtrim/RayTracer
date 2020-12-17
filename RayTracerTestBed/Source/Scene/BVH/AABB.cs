using System;
using OpenTK;

namespace RayTracerTestBed
{
	class AABB
	{
		public Vector3 minBounds;
		public Vector3 maxBounds;

		public AABB(Vector3 minBounds, Vector3 maxBounds)
		{
			this.minBounds = minBounds;
			this.maxBounds = maxBounds;
		}

		public float SurfaceArea()
		{
			float boxWidth = Math.Abs(minBounds.X - maxBounds.X);
			float boxHeight = Math.Abs(minBounds.Y - maxBounds.Y);
			float boxForward = Math.Abs(minBounds.Z - maxBounds.Z);

			float a = boxWidth * boxHeight;
			float b = boxHeight * boxForward;
			float c = boxForward * boxWidth;

			return a + a + b + b + c + c;
		}
	}
}
