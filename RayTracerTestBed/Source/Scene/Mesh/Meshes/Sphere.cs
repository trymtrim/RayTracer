﻿using System;
using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	class Sphere : Mesh
	{
		private Vector3 _center;
		private float _radius;

		public Sphere(Vector3 center, float radius, string name = null)
		{
			_center = center;
			_radius = radius;
			this.name = name;
			shape = Shape.Sphere;

			SetIndex();
		}

		public override float? Intersect(Ray ray)
		{
			var a = Vector3.Dot(ray.direction, ray.direction);
			var centerToOrigin = ray.origin - _center;

			var b = Vector3.Dot(ray.direction * 2.0f, centerToOrigin);
			var c = Vector3.Dot(centerToOrigin, centerToOrigin) - _radius * _radius;
			var underSqrt = b * b - 4.0f * a * c;

			if (underSqrt <= 0.0f)
				return null;

			var t1 = (-b - (float)Math.Sqrt(underSqrt)) / (2.0f * a);
			var t2 = (-b + (float)Math.Sqrt(underSqrt)) / (2.0f * a);

			//We know t2 is larger than t1
			if (t1 > 0.0f)
				return t1;

			if (t2 > 0.0f)
				return t2;

			return null; //Behind camera
		}

		public override Vector2 TextureCoords(Vector3 intersection)
		{
			Vector3 hitVector = intersection - _center;

			float x = 1.0f + ((float)Math.Atan2(hitVector.Z, hitVector.X) / (float)Math.PI) * 0.5f;
			float y = (float)Math.Acos(hitVector.Y / _radius) / (float)Math.PI;

			if (x >= 1.0f)
				x = 1.0f;

			if (y >= 1.0f)
				y = 1.0f;

			return new Vector2(x, y);
		}

		public override Vector3 Normal(Vector3 point)
		{
			return (point - _center).Normalized();
		}

		public override Vector3 Center()
		{
			return _center;
		}

		public override float Radius()
		{
			return _radius;
		}

		public override List<string> DebugInfo()
		{
			List<string> debugInfo = base.DebugInfo();

			debugInfo.Add("Position: " + -_center);
			debugInfo.Add("Radius: " + _radius);

			return debugInfo;
		}
	}
}
