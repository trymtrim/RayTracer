using System;
using OpenTK;

namespace RayTracerTestBed
{
	class ReflectionRefractionMaterial : Material
	{
		public ReflectionRefractionMaterial(TextureType textureType, float indexOfRefraction, float reflectionAndRefractionValue = 1.0f, Vector3? color = null)
		{
			materialType = MaterialType.Reflection_Refraction;

			this.textureType = textureType;
			this.color = color.HasValue ? color.Value : _defaultColor;

			ior = indexOfRefraction;
			reflection = reflectionAndRefractionValue;
			//refraction = reflectionAndRefractionValue;
		}

		public ReflectionRefractionMaterial(TextureType textureType, string texturePath, float indexOfRefraction, float reflectionAndRefractionValue)
		{
			materialType = MaterialType.Reflection_Refraction;

			this.textureType = textureType;

			if (texturePath != null && texturePath != string.Empty)
				AddTexture(texturePath);

			ior = indexOfRefraction;
			reflection = reflectionAndRefractionValue;
			//refraction = reflectionAndRefractionValue;
		}
	}
}
