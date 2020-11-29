using System;
using OpenTK;

namespace RayTracerTestBed
{
	class DiffuseMaterial : Material
	{
		public DiffuseMaterial(Texture texture, Vector3? color = null)
		{
			materialType = MaterialType.Diffuse;

			this.texture = texture;
			this.color = color.HasValue ? color.Value : _defaultColor;
		}
	}
}
