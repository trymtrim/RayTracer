using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	enum LightType
	{
		Spherical,
		Directional,
		Spot
	}

	class Light
	{
		public Mesh mesh;
		public Vector3? direction;
		public Vector3 color;

		public Light(LightType lightType, Vector3 color, Vector3? center, float? radius, Vector3? direction = null)
		{
			this.color = color;

			switch (lightType)
			{
				case LightType.Spherical:
					mesh = new Sphere(center.Value, radius.Value);
					this.direction = null;
					break;
				case LightType.Directional:
					mesh = new Plane(direction.Value, -1000.0f); //TODO: Distance is currently a magic number
					this.direction = direction.Value.Normalized();
					break;
				case LightType.Spot:
					mesh = new Sphere(center.Value, radius.Value);
					this.direction = direction.Value.Normalized();
					break;
			}
		}
	}
}
