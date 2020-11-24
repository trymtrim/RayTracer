using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	class Light
	{
		public Vector3f position;
		public Vector3f intensity;

		public Light(Vector3f position, Vector3f intensity)
		{
			this.position = position;
			this.intensity = intensity;
		}
	}
}
