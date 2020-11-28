using System;
using OpenTK;

namespace RayTracerTestBed
{
	enum Texture
	{
		Color,
		Checkerboard //Checkerboard is currently only supported on planes
	}

	class Material
	{
		public Texture texture;

		public Vector3 checkerboardSecondColor = Vector3.Zero; //Black as default - Only relevant for materials with checkerboard texture

		private Vector3 color; //Diffuse color
		private float specularity; //Reflaction rate
		private float ior; //Index of refraction (water: 1.3, glass: 1.5)

		public bool selected = false;
		private Vector3 _selectedColor = new Vector3(1.0f, 1.0f, 0.0f);

		public Material(Texture texture, Vector3 color, float specularity = 0.0f, float ior = 0.0f)
		{
			this.texture = texture;
			this.color = color;
			this.specularity = specularity;
			this.ior = ior;
		}

		public Vector3 Color()
		{
			return selected ? _selectedColor : color;
		}

		public float Specularity()
		{
			return specularity;
		}

		public float IndexOfRefraction()
		{
			return ior;
		}

		public Vector3 CheckerboardPattern(Ray ray, float t)
		{
			Vector3 axis1 = new Vector3(1.0f, 0.0f, 0.0f);
			Vector3 axis2 = new Vector3(0.0f, 0.0f, 1.0f);

			var p = ray.At(t);
			var xDot = Vector3.Dot(axis1, p);
			var x = Math.Abs(xDot);
			var yDot = Vector3.Dot(axis2, p);
			var y = Math.Abs(yDot);

			bool black;

			if (x % 2.0f > 1.0f != y % 2.0f > 1.0f)
				black = xDot < 0.0f ? false : true;
			else
				black = xDot < 0.0f ? true : false;

			if (yDot < 0.0f)
				black = !black;

			return black ? checkerboardSecondColor : selected ? _selectedColor : color;
		}
	}
}
