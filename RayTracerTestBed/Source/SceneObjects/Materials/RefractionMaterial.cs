using System;
using OpenTK;

namespace RayTracerTestBed
{
	class RefractionMaterial : Material
	{
		public RefractionMaterial(Texture texture, float indexOfRefraction, float refractionValue = 1.0f, Vector3? color = null)
		{
			materialType = MaterialType.Refraction;

			this.texture = texture;
			this.color = color.HasValue ? color.Value : _defaultColor;

			ior = indexOfRefraction;
			refraction = refractionValue;
		}
	}
}
