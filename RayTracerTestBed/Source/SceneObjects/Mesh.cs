using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	class Mesh
	{
		//TODO: Move this into a separate material class
		//Material properties
		public MaterialType materialtype;
		public float ior;
		public float kd, ks;
		public Vector3f diffuseColor;
		public float specularExponent;

		public Mesh()
		{
			//Set default material values
			materialtype = MaterialType.DIFFUSE_AND_GLOSSY;
			ior = 1.3f; //Index of refraction - water
			kd = 0.8f;
			ks = 0.2f;
			diffuseColor = new Vector3f(0.2f);
			specularExponent = 25.0f;
		}

		public virtual bool Intersect(Ray ray, out float tNear, out int index, out Vector2f uv)
		{
			//Default out values
			tNear = float.MaxValue;
			index = -1;
			uv = new Vector2f(0.0f);

			return false;
		}

		public virtual void GetSurfaceProperties(Vector3f p, Vector3f i, int index, Vector2f uv, out Vector3f n, out Vector2f st)
		{
			//Default out values
			n = new Vector3f(0.0f);
			st = new Vector2f(0.0f);
		}

		public virtual Vector3f EvaluateDiffuseColor(Vector2f st)
		{
			return diffuseColor;
		}

		public bool SolveQuadratic(float a, float b, float c, out float x1, out float x2)
		{
			//Default out values
			x1 = 0.0f;
			x2 = 0.0f;

			float discr = b * b - 4 * a * c;

			if (discr < 0.0f)
				return false;
			else if (discr == 0)
				x1 = x2 = -0.5f * b / a;
			else
			{
				float q = b > 0.0f ?
					-0.5f * (b + (float)Math.Sqrt(discr)) :
					-0.5f * (b - (float)Math.Sqrt(discr));
			}

			if (x1 > x2)
			{
				float x1_buffer = x1;

				x1 = x2;
				x2 = x1_buffer;
			}

			return true;
		}
	}
}
