using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	struct Settings
	{
		public int width;
		public int height;
		public Scene scene;
		public int maxDepth; //This decides the amount of bounces
		public int antiAliasing;

		public Vector3 backgroundColor;
	}
}
