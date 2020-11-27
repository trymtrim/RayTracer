using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	class Material
	{
		public bool checkerboard = false;

		public float specularity;
		public Vector3 color;

		//Diffuse checkerboard
		public Material()
		{
			checkerboard = true;
		}

		//Specular checkerboard
		public Material(float specularity)
		{
			checkerboard = true;
			this.specularity = specularity;
		}

		//Diffuse
		public Material(Vector3 color)
		{
			this.color = color;
		}

		//Specular
		public Material(Vector3 color, float specularity)
		{
			this.color = color;
			this.specularity = specularity;
		}

		//Checkerboard pattern
		public Vector3 GetCheckerboard(Ray ray, float t)
		{
			Vector3 axis1 = new Vector3(1.0f, 0.0f, 0.0f);
			Vector3 axis2 = new Vector3(0.0f, 0.0f, 1.0f);

			var p = ray.At(t);
			var xDot = Vector3.Dot(axis1, p);
			var x = Math.Abs(xDot);
			var y = Math.Abs(Vector3.Dot(axis2, p));

			bool black;

			if (x % 2.0f > 1.0f != y % 2.0f > 1.0f)
				black = xDot < 0.0f ? false : true;
			else
				black = xDot < 0.0f ? true : false;

			return black ? Vector3.Zero : Vector3.One;
		}
	}
}
