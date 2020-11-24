using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracerTestBed
{
	class Vector2f
	{
		private static Vector2f zero = new Vector2f(0.0f, 0.0f);

		public float X { get; set; }
		public float Y { get; set; }

		public Vector2f() { }

		public Vector2f(float value)
		{
			X = value;
			Y = value;
		}

		public Vector2f(float x, float y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return string.Format("[{0}, {1}]", X, Y);
		}

		public static Vector2f operator +(Vector2f a, Vector2f b)
		{
			return new Vector2f(a.X + b.X, a.Y + b.Y);
		}

		public static Vector2f operator -(Vector2f a, Vector2f b)
		{
			return new Vector2f(a.X - b.X, a.Y - b.Y);
		}

		public static Vector2f operator *(Vector2f a, Vector2f b)
		{
			return new Vector2f(a.X * b.X, a.Y * b.Y);
		}

		public static Vector2f operator *(Vector2f a, float b)
		{
			return new Vector2f(a.X * b, a.X * b);
		}
	}
}
