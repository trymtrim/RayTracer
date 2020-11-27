using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	class Plane : Mesh
	{
		private Vector3 _normal;
		private float _distance;

		public Plane(Vector3 normal, float distance)
		{
			_normal = normal;
			_distance = distance;
		}

		public override float? Intersect(Ray ray)
		{
			var t = -(Vector3.Dot(ray.origin, _normal) - _distance) / Vector3.Dot(ray.direction, _normal);

			if (t <= 0.0f)
				return null;

			return t;
		}

		public override Vector3 Normal(Vector3 point)
		{
			return _normal;
		}

		public override Vector3 Center() //TODO: Unsure if this is needed
		{
			return Vector3.Zero - (_normal * _normal); //TODO: Remove the "Zero -"
		}
	}
}
