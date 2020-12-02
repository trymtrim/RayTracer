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
	}
}
