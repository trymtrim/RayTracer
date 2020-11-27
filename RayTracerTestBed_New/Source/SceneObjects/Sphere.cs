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
		public float radius;

		public Sphere(Vector3 center, float radius)
		{
			this.center = center;
			this.radius = radius;
		}

		public override float? Intersect(Ray ray)
		{
			var a = Vector3.Dot(ray.direction, ray.direction);
			var centerToOrigin = ray.origin - center;
			var b = Vector3.Dot(ray.direction * 2.0f, centerToOrigin);
			var c = Vector3.Dot(centerToOrigin, centerToOrigin) - radius * radius; //TODO: Add parantheses? Probably not
			var underSqrt = b * b - 4.0f * a * c; //TODO: Add parantheses? Probably not

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

		public override Vector3 Normal(Vector3 point)
		{
			return (point - center).Normalized();
		}

		public override Vector3 Center() //TODO: Unsure if this is needed
		{
			return center;
		}
	}
}
