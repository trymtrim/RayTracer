using System;
using OpenTK;

namespace RayTracerTestBed
{
	class RefractionMaterial : Material
	{
		public RefractionMaterial(TextureType textureType, float indexOfRefraction, float refractionValue = 1.0f, Vector3? color = null)
		{
			materialType = MaterialType.Refraction;

			this.textureType = textureType;
			this.color = color.HasValue ? color.Value : _defaultColor;

			ior = indexOfRefraction;
			refraction = refractionValue;
		}

		public RefractionMaterial(TextureType textureType, string texturePath, float indexOfRefraction, float refractionValue)
		{
			materialType = MaterialType.Refraction;

			this.textureType = textureType;

			if (texturePath != null && texturePath != string.Empty)
				AddTexture(texturePath);

			ior = indexOfRefraction;
			refraction = refractionValue;
		}
	}
}
