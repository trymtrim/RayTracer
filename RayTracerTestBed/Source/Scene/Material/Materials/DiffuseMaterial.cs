using OpenTK;

namespace RayTracerTestBed
{
	class DiffuseMaterial : Material
	{
		public DiffuseMaterial(TextureType textureType, Vector3? color = null)
		{
			materialType = MaterialType.Diffuse;

			this.textureType = textureType;
			this.color = color.HasValue ? color.Value : _defaultColor;
		}

		public DiffuseMaterial(TextureType textureType, string texturePath)
		{
			materialType = MaterialType.Diffuse;

			this.textureType = textureType;

			if (texturePath != null && texturePath != string.Empty)
				AddTexture(texturePath);
		}
	}
}
