using System.Collections.Generic;
using OpenTK;
using System;
namespace RayTracerTestBed
{
	class Plane : Mesh
	{
		private Vector3 _normal;
		private Vector3 _position;
		private Vector3? _halfSize = null;

		public Plane(Vector3 normal, Vector3 position, Vector3? size = null, string name = null)
		{
			_normal = normal;
			_position = position;

			if (size.HasValue)
				_halfSize = size.Value / 2.0f;

			this.name = name;

			SetIndex();
		}

		public override float? Intersect(Ray ray)
		{
			var t = -(Vector3.Dot(ray.origin, _normal) - _position.Y) / Vector3.Dot(ray.direction, _normal);

			var intersection = ray.At(t);

			if (_halfSize.HasValue)
			{
				if (!(intersection.X < _position.X + _halfSize.Value.X && intersection.X > _position.X - _halfSize.Value.X &&
					intersection.Z < _position.Z + _halfSize.Value.Z && intersection.Z > _position.Z - _halfSize.Value.Z))
					return null;
			}

			if (t <= 0.0f)
				return null;

			return t;
		}

		public override Vector3 Normal(Vector3 point)
		{
			return _normal;
		}

		public override Vector3 Center()
		{
			return _normal * _normal;
		}

		public override List<string> DebugInfo()
		{
			List<string> debugInfo = base.DebugInfo();

			debugInfo.Add("Position: " + _position);
			debugInfo.Add("Normal: " + _normal);

			return debugInfo;
		}
	}
}
