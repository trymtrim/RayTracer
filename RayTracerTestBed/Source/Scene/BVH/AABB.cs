using System;

namespace RayTracerTestBed
{
	class AABB
	{
		public float minX, minY, minZ;
		public float maxX, maxY, maxZ;

		public AABB()
		{
			minX = minY = minZ = float.MaxValue;
			maxX = maxY = maxZ = float.MinValue;
		}

		public AABB(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
		{
			this.minX = minX;
			this.minY = minY;
			this.minZ = minZ;
			this.maxX = maxX;
			this.maxY = maxY;
			this.maxZ = maxZ;
		}

		public float SurfaceArea()
		{
			float boxWidth = Math.Abs(minX - maxX);
			float boxHeight = Math.Abs(minY - maxY);
			float boxForward = Math.Abs(minZ - maxZ);

			float a = boxWidth * boxHeight;
			float b = boxHeight * boxForward;
			float c = boxForward * boxWidth;

			return a + a + b + b + c + c;
		}
	}
}
