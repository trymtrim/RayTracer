using System;
using OpenTK;

namespace RayTracerTestBed
{
	enum MaterialType
	{
		Diffuse,
		Reflection,
		Refraction, //Not fully sure if refraction alone should be a thing
		Reflection_Refraction,
		Transparent
	}

	enum Texture
	{
		Color,
		Checkerboard //Checkerboard is currently only supported on planes
	}

	abstract class Material
	{
		public MaterialType materialType { get; protected set; }

		public Texture texture;
		public Vector3 color;
		public float reflection; //Range [0-1]
		public float refraction; //Range [0-1]
		public float ior; //Index of refraction (water: 1.3, glass: 1.5, looking glass: 1.1, almost fully transparent glass: 1.01)
		public float transparency; //Range [0-1]

		public Vector3 checkerboardSecondColor = Vector3.Zero; //Black as default - Only relevant for materials with checkerboard texture

		public bool selected = false;

		protected Vector3 _defaultColor = Vector3.One;

		public Vector3 CheckerboardPattern(Ray ray, float t)
		{
			float fragmentMultiplier = 2.0f;

			Vector3 axis1 = new Vector3(fragmentMultiplier, 0.0f, 0.0f);
			Vector3 axis2 = new Vector3(0.0f, 0.0f, fragmentMultiplier);

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

			return black ? checkerboardSecondColor : color;
		}
	}
}
