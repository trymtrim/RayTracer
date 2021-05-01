using System;
using OpenTK;

namespace RayTracerTestBed
{
	class TransparentMaterial : Material
	{
		public TransparentMaterial(TextureType textureType, float transparencyValue, Vector3? color = null)
		{
			materialType = MaterialType.Transparent;

			this.textureType = textureType;
			this.color = color.HasValue ? color.Value : _defaultColor;

			transparency = transparencyValue;
		}

		public TransparentMaterial(TextureType textureType, string texturePath, float transparencyValue)
		{
			materialType = MaterialType.Transparent;

			this.textureType = textureType;

			if (texturePath != null && texturePath != string.Empty)
				AddTexture(texturePath);

			transparency = transparencyValue;
		}
	}
}
