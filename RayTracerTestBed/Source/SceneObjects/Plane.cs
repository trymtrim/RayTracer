using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	class Plane : Mesh
	{
		private Vector3 _normal;
		private float _distance;

		public Plane(Vector3 normal, float distance, string name = null)
		{
			_normal = normal;
			_distance = distance;
			this.name = name;
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
			return _normal * _normal;
		}

		public override List<string> DebugInfo()
		{
			List<string> debugInfo = new List<string>();

			debugInfo.Add("Position: (0, " + _distance + ", 0)");
			debugInfo.Add("Normal: " + _normal);

			return debugInfo;
		}
	}
}
