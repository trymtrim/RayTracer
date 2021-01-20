using System;
using OpenTK;

namespace RayTracerTestBed
{
	static class MathHelper
	{
		private static Random random = new Random();
		private static Random staticSeedRandom = new Random(1000);
		
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
			return (float)Math.PI / 180.0f * angle;
		}

		public static float RandomRange(float min, float max)
		{
			return (float)(random.NextDouble() * (max - min) + min);
		}

		public static float RandomRangeWithStaticSeed(float min, float max)
		{
			return (float)(staticSeedRandom.NextDouble() * (max - min) + min);
		}

		public static void ResetStaticRandomSeed()
		{
			staticSeedRandom = new Random(1000);
		}
	}
}
