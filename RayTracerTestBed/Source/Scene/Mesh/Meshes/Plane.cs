using System.Collections.Generic;
using OpenTK;
using System;
namespace RayTracerTestBed
{
	class Plane : Mesh
	{
		public Vector3 position;
		private Vector3 _normal;
		private Vector3? _halfSize = null;

		public Plane(Vector3 normal, Vector3 position, Vector3? size = null, string name = null)
		{
			_normal = normal;
			this.position = position;

			if (size.HasValue)
				_halfSize = size.Value / 2.0f;

			this.name = name;
			shape = Shape.Plane;

			SetIndex();
		}

		public override float? Intersect(Ray ray)
		{
			float t = -(Vector3.Dot(ray.origin, _normal) - position.Y) / Vector3.Dot(ray.direction, _normal);
			var intersection = ray.At(t);

			//TODO: Handle this better? (plane size)
			if (_halfSize.HasValue)
			{
				if (_normal.Y != 0.0f)
				{
					if (!(intersection.X < position.X + _halfSize.Value.X && intersection.X > position.X - _halfSize.Value.X &&
						intersection.Z < position.Z + _halfSize.Value.Z && intersection.Z > position.Z - _halfSize.Value.Z))
						return null;
				}
				else if (_normal.X != 0.0f)
				{
					t = -(Vector3.Dot(ray.origin, _normal) - position.X) / Vector3.Dot(ray.direction, _normal);
					intersection = ray.At(t);

					if (!(intersection.Y < position.Y + _halfSize.Value.X && intersection.Y > position.Y - _halfSize.Value.X &&
						intersection.Z < position.Z + _halfSize.Value.Z && intersection.Z > position.Z - _halfSize.Value.Z))
						return null;
				}
				else if (_normal.Z != 0.0f)
				{
					t = -(Vector3.Dot(ray.origin, _normal) - position.Z) / Vector3.Dot(ray.direction, _normal);
					intersection = ray.At(t);

					if (!(intersection.Y < position.Y + _halfSize.Value.X && intersection.Y > position.Y - _halfSize.Value.X &&
						intersection.X < position.X + _halfSize.Value.X && intersection.X > position.X - _halfSize.Value.X))
						return null;
				}
			}

			if (t <= 0.0f)
				return null;

			return t;
		}

		public override Vector2 TextureCoords(Vector3 intersection)
		{
			if (_halfSize == null)
				return new Vector2(0.0f, 0.0f);

			Vector3 xAxis = Vector3.Cross(_normal, new Vector3(0.0f, 0.0f, 1.0f));

			if (xAxis.Length == 0.0f)
				xAxis = Vector3.Cross(_normal, new Vector3(0.0f, 1.0f, 0.0f));

			Vector3 yAxis = Vector3.Cross(_normal, xAxis);

			Vector3 hitVector = intersection - position;

			float x = Vector3.Dot(-hitVector, xAxis) + _halfSize.Value.X;
			float y = Vector3.Dot(hitVector, yAxis) + _halfSize.Value.Z;

			x /= _halfSize.Value.X * 2.0f;
			y /= _halfSize.Value.Z * 2.0f;

			//TODO: Fix this ugly fix
			if (isSkyboxMesh)
			{
				if (_normal.Z > 0.0f)
					y = 1.0f - y;
				if (_normal.Z < 0.0f)
					x = 1.0f - x;
				if (_normal.X != 0.0f)
					x = 1.0f - x;
				if (_normal.Y != 0.0f)
					x = 1.0f - x;
			}

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

			debugInfo.Add("Position: " + position);
			debugInfo.Add("Normal: " + _normal);

			return debugInfo;
		}
	}
}
