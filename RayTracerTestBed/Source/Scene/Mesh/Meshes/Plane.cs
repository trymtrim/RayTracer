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
			shape = Shape.Plane;

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

		public override Vector2 TextureCoords(Vector3 intersection)
		{
			//float xIntersect = 0.5f + intersection.X / (_halfSize.Value.X * 2.0f);
			//float yIntersect = 0.5f + -intersection.Z / (_halfSize.Value.Z * 2.0f);

			//float x = xIntersect;
			//float y = yIntersect;

			Vector3 xAxis = Vector3.Cross(_normal, new Vector3(0.0f, 0.0f, 1.0f));

			if (xAxis.Length == 0.0f)
				xAxis = Vector3.Cross(_normal, new Vector3(0.0f, 1.0f, 0.0f));

			Vector3 yAxis = Vector3.Cross(_normal, xAxis);

			Vector3 hitVector = intersection - _position; //Center(); //_position;

			float x = Vector3.Dot(-hitVector, xAxis) + 2.0f;
			float y = Vector3.Dot(hitVector, yAxis) + 2.0f;

			x /= 4.0f;
			y /= 4.0f;

			//Console.WriteLine(x);

			return new Vector2(x, y);
		}

		public override Vector3 Normal(Vector3 point)
		{
			return _normal;
		}

		public override Vector3 Center()
		{
			return -(_normal * _normal);
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
