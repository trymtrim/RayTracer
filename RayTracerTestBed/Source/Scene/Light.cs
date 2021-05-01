using OpenTK;

namespace RayTracerTestBed
{
	enum LightType
	{
		Directional,
		Point,
		Spot
	}

	class Light
	{
		public Mesh mesh;
		public Vector3? direction;
		public Vector3 color;
		public float brightness;

		public Light(LightType lightType, Vector3 color, float brightness, Vector3? center, float? radius, Vector3? direction = null, Vector3? on = null)
		{
			this.color = color;
			this.brightness = brightness;

			switch (lightType)
			{
				case LightType.Directional:
					mesh = new Plane(direction.Value, new Vector3(0.0f, -1000.0f, 0.0f), new Vector3(1000.0f, 1000.0f, 1000.0f));
					this.direction = direction.Value.Normalized();
					break;
				case LightType.Point:
					mesh = new Sphere(center.Value, radius.Value);
					this.direction = null;
					break;
				case LightType.Spot:
					var dir = on.Value - center.Value;
					mesh = new Sphere(center.Value, radius.Value);
					this.direction = dir.Normalized();
					break;
			}
		}
	}
}
