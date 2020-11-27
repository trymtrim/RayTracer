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
		public virtual float? Intersect(Ray ray)
		{
			return float.MaxValue;
		}

		public virtual Vector3 Normal(Vector3 point)
		{
			return Vector3.Zero;
		}

		public virtual Vector3 Center() //TODO: Unsure if this is needed
		{
			return Vector3.Zero;
		}
	}
}
