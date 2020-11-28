using System;

namespace RayTracerTestBed
{
	static class MathHelper
	{
		public static float Clamp(float value, float min, float max)
		{
			if (value < min)
				return min;
			else if (value > max)
				return max;
			else
				return value;
		}

		public static float DegreesToRadians(float angle)
		{
			return ((float)Math.PI / 180.0f) * angle;
		}
	}
}
