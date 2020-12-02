using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracerTestBed
{
	class Skybox
	{
		private const float distance = 10000.0f;

		private List<Plane> meshes = new List<Plane>();

		public Skybox(Scene scene)
		{
			Vector3 size = new Vector3(distance * 2.0f);

			//Bottom
			Plane floor = new Plane(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, distance, 0.0f), size, "Skybox Bottom");
			floor.isSkyboxMesh = true;
			scene.meshes.Add(floor);
			meshes.Add(floor);
			Material planeMaterial = new DiffuseMaterial(TextureType.Texture, "../../Assets/Textures/Skybox/Down.jpg");
			scene.materials.Add(planeMaterial);

			//Top
			Plane roof = new Plane(new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, distance, 0.0f), size, "Skybox Top");
			roof.isSkyboxMesh = true;
			scene.meshes.Add(roof);
			meshes.Add(roof);
			Material roofMaterial = new DiffuseMaterial(TextureType.Texture, "../../Assets/Textures/Skybox/Up.jpg");
			scene.materials.Add(roofMaterial);

			//Left wall
			Plane leftWall = new Plane(new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(distance, 0.0f, 0.0f), size, "Skybox Left");
			leftWall.isSkyboxMesh = true;
			scene.meshes.Add(leftWall);
			meshes.Add(leftWall);
			Material leftWallMaterial = new DiffuseMaterial(TextureType.Texture, "../../Assets/Textures/Skybox/Left.jpg");
			scene.materials.Add(leftWallMaterial);

			//Right wall
			Plane rightWall = new Plane(new Vector3(1.0f, 0.0f, 0.0f), new Vector3(distance, 0.0f, 0.0f), size, "Skybox Right");
			rightWall.isSkyboxMesh = true;
			scene.meshes.Add(rightWall);
			Material rightWallMaterial = new DiffuseMaterial(TextureType.Texture, "../../Assets/Textures/Skybox/Right.jpg");
			scene.materials.Add(rightWallMaterial);

			//Front wall
			Plane frontWall = new Plane(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, distance), size, "Skybox Front");
			frontWall.isSkyboxMesh = true;
			scene.meshes.Add(frontWall);
			meshes.Add(rightWall);
			Material frontWallMaterial = new DiffuseMaterial(TextureType.Texture, "../../Assets/Textures/Skybox/Front.jpg");
			scene.materials.Add(frontWallMaterial);

			//Back wall
			Plane backWall = new Plane(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, distance), size, "Skybox Back");
			backWall.isSkyboxMesh = true;
			scene.meshes.Add(backWall);
			meshes.Add(backWall);
			Material backWallMaterial = new DiffuseMaterial(TextureType.Texture, "../../Assets/Textures/Skybox/Back.jpg");
			scene.materials.Add(backWallMaterial);
		}

		public void Update(Vector3 cameraPosition)
		{
			//TODO: Update skybox position based on camera position

			//foreach (Plane mesh in meshes)
			//{
			//	mesh.position = mesh.position + cameraPosition;
			//}
		}
	}
}
