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

		public ReflectionMaterial(TextureType textureType, string texturePath, float reflectionValue)
		{
			materialType = MaterialType.Reflection;

			this.textureType = textureType;

			if (texturePath != null && texturePath != string.Empty)
				AddTexture(texturePath);

			reflection = reflectionValue;
		}
	}
}
