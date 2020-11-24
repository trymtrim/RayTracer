using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracerTestBed
{
	class Ray
	{
		public Vector3f origin;
		public Vector3f direction;

		public Ray()
		{
			origin = new Vector3f();
			direction = new Vector3f();
		}

		public Ray(Vector3f origin, Vector3f direction)
		{
			this.origin = origin;
			this.direction = direction;
		}
	}
}
