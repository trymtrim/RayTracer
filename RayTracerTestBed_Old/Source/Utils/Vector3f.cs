using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracerTestBed
{
	class Vector3f
	{
		private static Vector3f zero = new Vector3f(0.0f, 0.0f, 0.0f);

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public Vector3f() { }

		public Vector3f(float value)
		{
			X = value;
			Y = value;
			Z = value;
		}

		public Vector3f(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static float Dot(Vector3f a, Vector3f b)
		{
			return b.X * a.X + b.Y * a.Y + b.Z * a.Z;
		}

		public float Dot(Vector3f b)
		{
			return b.X * X + b.Y * Y + b.Z * Z;
		}

		public Vector3f Cross(Vector3f b)
		{
			return new Vector3f()
			{
				X = Y * b.Z - Z * b.Y,
				Y = Z * b.X - X * b.Z,
				Z = X * b.Y - Y * b.X
			};
		}

		public override string ToString()
		{
			return string.Format("[{0}, {1}, {2}]", X, Y, Z);
		}

		public static Vector3f Normalize(Vector3f vector)
		{
			float l = Length(vector);
			return new Vector3f()
			{
				X = vector.X / l,
				Y = vector.Y / l,
				Z = vector.Z / l
			};
		}

		public static float Length(Vector3f vector)
		{
			return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
		}

		public static Vector3f operator +(Vector3f a, Vector3f b)
		{
			return new Vector3f(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Vector3f operator -(Vector3f a, Vector3f b)
		{
			return new Vector3f(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Vector3f operator *(Vector3f a, Vector3f b)
		{
			return new Vector3f(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
		}

		public static Vector3f operator *(Vector3f a, float b)
		{
			return new Vector3f(a.X * b, a.Y * b, a.Z * b);
		}
	}
}
