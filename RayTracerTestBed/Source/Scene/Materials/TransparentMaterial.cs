using System;
using OpenTK;

namespace RayTracerTestBed
{
	class TransparentMaterial : Material
	{
		public TransparentMaterial(Texture texture, float transparencyValue, Vector3? color = null)
		{
			materialType = MaterialType.Transparent;

			this.texture = texture;
			this.color = color.HasValue ? color.Value : _defaultColor;

			transparency = transparencyValue;
		}
	}
}
