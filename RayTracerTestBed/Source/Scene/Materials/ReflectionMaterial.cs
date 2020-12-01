using System;
using OpenTK;

namespace RayTracerTestBed
{
	class ReflectionMaterial : Material
	{
		public ReflectionMaterial(Texture texture, float reflectionValue, Vector3? color = null)
		{
			materialType = MaterialType.Reflection;

			this.texture = texture;
			this.color = color.HasValue ? color.Value : _defaultColor;

			reflection = reflectionValue;
		}
	}
}
