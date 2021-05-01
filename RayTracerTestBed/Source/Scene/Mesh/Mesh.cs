using System.Collections.Generic;
using OpenTK;
using System;

namespace RayTracerTestBed
{
	enum Shape
	{
		Plane,
		Sphere
	}

	abstract class Mesh
	{
		public string name;
		public Shape shape;
		protected int _index;

		public bool isSkyboxMesh = false; //TODO: Change how this is handled

		public void SetIndex()
		{
			_index = Scene.scene.meshes.Count;
		}

		public abstract float? Intersect(Ray ray);

		public abstract Vector2 TextureCoords(Vector3 intersection);

		public virtual Vector3 Normal(Vector3 point)
		{
			return Vector3.Zero;
		}

		public virtual Vector3 Center()
		{
			return Vector3.Zero;
		}

		public virtual float Radius()
		{
			return float.MaxValue;
		}

		public virtual List<string> DebugInfo()
		{
			List<string> debugInfo = new List<string>();

			Material material = Game.settings.scene.materials[_index];
			debugInfo.Add("Material: " + material.materialType.ToString());

			switch (material.materialType)
			{
				case MaterialType.Diffuse:
					debugInfo.Add("Color: " + material.color);
					break;
				case MaterialType.Reflection:
					debugInfo.Add("Color: " + material.color);
					debugInfo.Add("Reflection Rate: " + material.reflection);
					break;
				case MaterialType.Refraction:
					debugInfo.Add("Color: " + material.color);
					debugInfo.Add("Refraction Rate: " + material.refraction);
					debugInfo.Add("Index of Refraction: " + material.ior);
					break;
				case MaterialType.Reflection_Refraction:
					debugInfo.Add("Color: " + material.color);
					debugInfo.Add("R&R Rate: " + material.reflection);
					debugInfo.Add("Index of Refraction: " + material.ior);
					break;
				case MaterialType.Transparent:
					debugInfo.Add("Color: " + material.color);
					debugInfo.Add("Transparency: " + material.transparency);
					break;
			}

			return debugInfo;
		}
	}
}
