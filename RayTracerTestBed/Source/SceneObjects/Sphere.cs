using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	class Sphere : Mesh
	{
		public Vector3 center;
		public float radius, radius2;

		public Sphere(Vector3 c, float r)
		{
			center = c;
			radius = r;
			radius2 = r * r;
		}

		public override bool Intersect(Ray ray, out float tNear, out int index, out Vector2f uv)
		{
			//Default out values
			tNear = float.MaxValue;
			index = -1;
			uv = new Vector2f(0.0f);

			//Analytic solution
			Vector3 l = ray.origin - center;

			float a = Vector3.Dot(ray.direction, ray.direction);
			float b = 2.0f * Vector3.Dot(ray.direction, l);
			float c = Vector3.Dot(l, l) - radius2;

			float t1, t2;

			//TODO: Maybe change to only using/checking triangles everywhere
			if (!SolveQuadratic(a, b, c, out t1, out t2))
				return false;

			if (t1 < 0.0f)
				t1 = t2;

			if (t1 < 0.0f)
				return false;

			tNear = t1;

			return true;
		}

		public override void GetSurfaceProperties(Vector3 p, Vector3 i, int index, Vector2f uv, out Vector3 n, out Vector2f st)
		{
			n = Vector3.Normalize(p - center);
			st = new Vector2f(0.0f); //Not used, default out value
		}
	}
}
