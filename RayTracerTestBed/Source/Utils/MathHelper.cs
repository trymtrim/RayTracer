using System;
using OpenTK;

namespace RayTracerTestBed
{
	static class MathHelper
	{
		private static float _seed;

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

		public static Vector3 RandomOnHemisphere(Vector3 normal, Vector2 pixel)
		{
			//Uniformly sample hemisphere direction
			float cosTheta = Random(pixel);
			float sinTheta = (float)Math.Sqrt(Math.Max(0.0f, 1.0f - cosTheta * cosTheta));
			float phi = 2 * (float)Math.PI * Random(pixel);
			Vector3 tangentSpaceDir = new Vector3((float)Math.Cos(phi) * sinTheta, (float)Math.Sin(phi) * sinTheta, cosTheta);
			
			//Transform direction to world space
			Matrix3 tangent = GetTangentSpace(normal);
			Vector3 result = tangent.Row0 * tangentSpaceDir;

			return result; //TODO: Not sure if this is correct
			//return mul(tangentSpaceDir, GetTangentSpace(normal));
		}

		private static Random random = new Random();
		public static float RandomRange(float min, float max)
		{
			return (float)(random.NextDouble() * (max - min) + min);
		}

		public static void ResetSeed()
		{
			_seed = 0.0f;
		}

		public static float Random(Vector2 pixel)
		{
			float value = ((float)Math.Sin(_seed / 100.0f * Vector2.Dot(pixel, new Vector2(12.9898f, 78.233f))) * 43758.5453f);
			float result = value - (float)Math.Truncate(value);
			_seed = RandomRange(1.0f, 10.0f);
			return result;
		}

		private static Matrix3 GetTangentSpace(Vector3 normal)
		{
			//Choose a helper vector for the cross product
			Vector3 helper = new Vector3(1.0f, 0.0f, 0.0f);

			if (Math.Abs(normal.X) > 0.99f)
				helper = new Vector3(0.0f, 0.0f, 1.0f);

			//Generate vectors
			Vector3 tangent = Vector3.Cross(normal, helper).Normalized();
			Vector3 binormal = Vector3.Cross(normal, tangent).Normalized();
			
			return new Matrix3(tangent, binormal, normal);
		}
	}
}
