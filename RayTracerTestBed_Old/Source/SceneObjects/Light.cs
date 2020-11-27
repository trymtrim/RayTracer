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
		public Vector3 position;
		public Vector3 intensity;
		public Vector3 color; //Unsure about this one

		public Light(Vector3 position, Vector3 intensity)
		{
			this.position = position;
			this.intensity = intensity;

			color = new Vector3(1.0f, 1.0f, 1.0f);
		}
	}
}
