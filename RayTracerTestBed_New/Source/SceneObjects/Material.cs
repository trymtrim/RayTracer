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

		//Diffuse
		public Material()
		{
			checkerboard = true;
		}

		//Diffuse
		public Material(Vector3 color)
		{
			this.color = color;
		}

		//Specularity
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
			var x = Vector3.Dot(axis1, p);
			var y = Vector3.Dot(axis2, p);

			//Console.WriteLine("x: " + x + " y: " + y );
			if (x % 2.0f > 1.0f != y % 2.0f > 1.0f) //TODO: This might be wrong
			{
				//Console.WriteLine("1");
				return new Vector3(1.0f, 1.0f, 1.0f); //White
			}
			else
			{
				//Console.WriteLine("2");
				return new Vector3(0.0f, 0.0f, 0.0f); //Black
			}
		}
	}
}
