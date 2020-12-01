using System;
using OpenTK;

namespace RayTracerTestBed
{
	class ReflectionRefractionMaterial : Material
	{
		public ReflectionRefractionMaterial(Texture texture, float indexOfRefraction, float reflectionAndRefractionValue = 1.0f, Vector3? color = null)
		{
			materialType = MaterialType.Reflection_Refraction;

			this.texture = texture;
			this.color = color.HasValue ? color.Value : _defaultColor;

			ior = indexOfRefraction;
			reflection = reflectionAndRefractionValue;
			//refraction = reflectionAndRefractionValue;
		}
	}
}
