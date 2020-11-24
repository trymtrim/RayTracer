using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	//Currently assuming Whitted-style raytracer
	struct Settings
	{
		public int width;
		public int height;
		public int maxDepth; //Bounces?
		public int fov;
		public Vector3f backgroundColor;
		public int antiAliasing;
		public float bias; //TODO, Unsure if this is actually epsilon
	}

	enum MaterialType
	{
		DIFFUSE_AND_GLOSSY,
		REFLECTION_AND_REFRACTION,
		REFLECTION
	}
}
