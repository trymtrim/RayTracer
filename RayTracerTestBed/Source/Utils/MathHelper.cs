using System;
using OpenTK;

namespace RayTracerTestBed
{
	static class MathHelper
	{
		private static Random random = new Random();

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

		public static float RandomRange(float min, float max)
		{
			return (float)(random.NextDouble() * (max - min) + min);
		}

		//public static float Random(Vector2 pixel)
		//{
		//	float value = ((float)Math.Sin(_seed / 100.0f * Vector2.Dot(pixel, new Vector2(12.9898f, 78.233f))) * 43758.5453f);
		//	float result = value - (float)Math.Truncate(value);
		//	_seed = RandomRange(1.0f, 10.0f);
		//	return result;
		//}
	}
}
