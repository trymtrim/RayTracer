using System;
using OpenTK;

namespace RayTracerTestBed
{
	class ReflectionMaterial : Material
	{
		public ReflectionMaterial(TextureType textureType, float reflectionValue, Vector3? color = null)
		{
			materialType = MaterialType.Reflection;

			this.textureType = textureType;
			this.color = color.HasValue ? color.Value : _defaultColor;

			reflection = reflectionValue;
		}
	}
}
