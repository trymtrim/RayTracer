using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	class AABB
	{
		public Vector3 minBounds;
		public Vector3 maxBounds;

		public AABB(Vector3 minBounds, Vector3 maxBounds)
		{
			this.minBounds = minBounds;
			this.maxBounds = maxBounds;

			//minBounds.Y = -minBounds.Y;
			//maxBounds.Y = - maxBounds.Y;
		}
	}
}
