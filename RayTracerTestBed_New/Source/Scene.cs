using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	enum SceneType
	{
		SpheresWithSpot,
		Mirrors
	}

	class Scene
	{
		public List<Mesh> meshes = new List<Mesh>();
		public List<Material> materials = new List<Material>(); //Meshes and materials with the same index correspond to each other
		public List<Light> lights = new List<Light>();

		public Scene(SceneType sceneType)
		{
			switch (sceneType)
			{
				case SceneType.SpheresWithSpot:
					SpawnMap1();
					break;
				case SceneType.Mirrors:
					SpawnMap2();
					break;
			}
		}

		private void SpawnMap1()
		{
			Light light = new Light(LightType.Directional, new Vector3(0.5f, 0.5f, 0.5f) , null, null, new Vector3(-1.0f, 2.0f, 1.0f)); //TODO: We are supposed to spawn spot light here
			lights.Add(light);

			Mesh plane = new Plane(new Vector3(0.0f, -1.0f, 0.0f), -2.0f); //Floor plane at y=2
			meshes.Add(plane);
			Material planeMaterial = new Material(); //TODO: This should be checkerboard pattern
			materials.Add(planeMaterial);

			Mesh sphere1 = new Sphere(new Vector3(-1.5f, 1.0f, 6.0f), 1.0f); //Grey sphere
			meshes.Add(sphere1);
			Material sphere1Material = new Material(new Vector3(0.7f, 0.7f, 0.7f));
			materials.Add(sphere1Material);

			Mesh sphere2 = new Sphere(new Vector3(2.0f, -2.5f, 9.0f), 2.0f); //Large blue sphere higher up
			meshes.Add(sphere2);
			Material sphere2Material = new Material(new Vector3(0.0f, 0.0f, 1.0f));
			materials.Add(sphere2Material);

			Mesh sphere3 = new Sphere(new Vector3(-0.5f, 0.5f, 11.0f), 1.5f); //Semi-specular sphere behind
			meshes.Add(sphere3);
			Material sphere3Material = new Material(new Vector3(0.0f, 1.0f, 0.0f), 0.75f);
			materials.Add(sphere3Material);
		}

		private void SpawnMap2()
		{
			//TODO: Add second map
		}
	}
}
