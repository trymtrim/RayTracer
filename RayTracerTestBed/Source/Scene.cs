using System;
using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	enum SceneType
	{
		PointLight_VariousMaterials,
		SpotLight_ReflectionMaterials,
		DirectionalLight_ReflectionRefraction,
		DirectionalLight_VariousMaterials,
		Mirrors
	}

	class Scene
	{
		public static Scene scene; //TODO: Unsure if I want to handle this this way

		public List<Mesh> meshes = new List<Mesh>();
		public List<Material> materials = new List<Material>(); //Meshes and materials with the same index correspond to each other
		public List<Light> lights = new List<Light>();

		public SceneType sceneType;

		public Scene(SceneType sceneType)
		{
			scene = this;

			this.sceneType = sceneType;

			switch (sceneType)
			{
				case SceneType.PointLight_VariousMaterials:
					SpawnMap1();
					break;
				case SceneType.SpotLight_ReflectionMaterials:
					SpawnMap2();
					break;
				case SceneType.DirectionalLight_ReflectionRefraction:
					SpawnMap3();
					break;
				case SceneType.DirectionalLight_VariousMaterials:
					SpawnMap4();
					break;
				case SceneType.Mirrors:
					SpawnMap5();
					break;
			}
		}

		private void SpawnMap1()
		{
			//Point light
			//Light light = new Light(LightType.Directional, new Vector3(1.0f, 1.0f, 1.0f) * 1.0f, null, null, new Vector3(1.0f, 0.8f, 1.0f));
			Light light = new Light(LightType.Point, new Vector3(0.7f, 0.7f, 0.7f), new Vector3(0.0f, 0.0f, 4.0f), 35.0f);
			lights.Add(light);

			//Floor plane at y=2
			Mesh plane = new Plane(new Vector3(0.0f, -1.0f, 0.0f), -2.0f, "Floor");
			meshes.Add(plane);
			Material planeMaterial = new ReflectionMaterial(Texture.Checkerboard, 0.9f, new Vector3(1,0,0));
			materials.Add(planeMaterial);

			//Grey diffuse sphere
			Mesh sphere1 = new Sphere(new Vector3(-4.0f, 1.0f, 6.0f), 1.0f, "Sphere1");
			meshes.Add(sphere1);
			Material sphere1Material = new DiffuseMaterial(Texture.Color, new Vector3(0.7f, 0.7f, 0.7f));
			materials.Add(sphere1Material);

			//Blue refraction and reflection sphere
			Mesh sphere2 = new Sphere(new Vector3(1.0f, -2.5f, 9.0f), 2.0f, "Sphere2");
			meshes.Add(sphere2);
			Material sphere2Material = new ReflectionRefractionMaterial(Texture.Color, 1.5f, 1.0f, new Vector3(0.0f, 0.0f, 1.0f));
			materials.Add(sphere2Material);

			//Refraction sphere
			Mesh sphere3 = new Sphere(new Vector3(-2.0f, 0.5f, 11.0f), 1.5f, "Sphere3");
			meshes.Add(sphere3);
			Material sphere3Material = new RefractionMaterial(Texture.Color, 1.5f);
			materials.Add(sphere3Material);

			//Yellow sphere behind refraction sphere
			Mesh sphere5 = new Sphere(new Vector3(-3.0f, 0.5f, 20.0f), 1.0f, "Sphere5");
			meshes.Add(sphere5);
			Material sphere5Material = new DiffuseMaterial(Texture.Color, new Vector3(1.0f, 1.0f, 0.0f));
			materials.Add(sphere5Material);

			//Red reflection sphere
			Mesh sphere4 = new Sphere(new Vector3(3.0f, 0.5f, 6.0f), 1.5f, "Sphere4");
			meshes.Add(sphere4);
			Material sphere4Material = new ReflectionMaterial(Texture.Color, 0.9f);
			materials.Add(sphere4Material);
		}

		private void SpawnMap2()
		{
			////Spot light
			//Light light = new Light(LightType.Spot, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(5.0f, -5.0f, 7.0f), 6.0f, null, new Vector3(2.0f, -1.0f, 8.0f));
			//lights.Add(light);

			////Floor plane at y=2
			//Mesh plane = new Plane(new Vector3(0.0f, -1.0f, 0.0f), -2.0f, "Floor");
			//meshes.Add(plane);
			//Material planeMaterial = new Material(Texture.Color, Vector3.One, 0.8f);
			//materials.Add(planeMaterial);

			//Mesh sphere1 = new Sphere(new Vector3(-4.0f, 1.0f, 6.0f), 1.0f, "Sphere1");
			//meshes.Add(sphere1);
			//Material sphere1Material = new Material(Texture.Color, new Vector3(1.0f, 1.0f, 1.0f), 0.6f);
			//materials.Add(sphere1Material);

			//Mesh sphere2 = new Sphere(new Vector3(-2.0f, 0.5f, 11.0f), 1.5f, "Sphere2");
			//meshes.Add(sphere2);
			//Material sphere2Material = new Material(Texture.Color, new Vector3(0.1f, 0.1f, 1.0f), 0.75f);
			//materials.Add(sphere2Material);

			//Mesh sphere3 = new Sphere(new Vector3(3.0f, 0.5f, 6.0f), 1.5f, "Sphere3");
			//meshes.Add(sphere3);
			//Material sphere3Material = new Material(Texture.Color, new Vector3(1.0f, 0.0f, 0.0f), 0.75f);
			//materials.Add(sphere3Material);
		}

		private void SpawnMap3()
		{
			////Directional light
			//Light light = new Light(LightType.Directional, new Vector3(1.0f, 1.0f, 1.0f) , null, null, new Vector3(-1.0f, 2.0f, 1.0f));
			//lights.Add(light);

			////Floor plane at y=2
			//Mesh plane = new Plane(new Vector3(0.0f, -1.0f, 0.0f), -2.0f, "Floor");
			//meshes.Add(plane);
			//Material planeMaterial = new Material(Texture.Checkerboard, Vector3.One, 0.6f);
			//materials.Add(planeMaterial);

			//Mesh sphere1 = new Sphere(new Vector3(-4.0f, 1.0f, 6.0f), 1.0f, "Sphere1");
			//meshes.Add(sphere1);
			//Material sphere1Material = new Material(Texture.Color, new Vector3(0.7f, 0.7f, 0.7f), 0.5f, 1.5f);
			//materials.Add(sphere1Material);

			//Mesh sphere2 = new Sphere(new Vector3(1.0f, -2.5f, 9.0f), 2.0f, "Sphere2");
			//meshes.Add(sphere2);
			//Material sphere2Material = new Material(Texture.Color, new Vector3(0.0f, 0.0f, 1.0f), 0.9f, 1.5f);
			//materials.Add(sphere2Material);

			//Mesh sphere3 = new Sphere(new Vector3(-2.0f, 0.5f, 11.0f), 1.5f, "Sphere3");
			//meshes.Add(sphere3);
			//Material sphere3Material = new Material(Texture.Color, new Vector3(1.0f, 1.0f, 1.0f), 0.3f, 1.5f);
			//materials.Add(sphere3Material);

			//Mesh sphere4 = new Sphere(new Vector3(3.0f, 0.5f, 6.0f), 1.5f, "Sphere4");
			//meshes.Add(sphere4);
			//Material sphere4Material = new Material(Texture.Color, new Vector3(1.0f, 0.0f, 0.0f), 0.75f, 1.5f);
			//materials.Add(sphere4Material);
		}

		private void SpawnMap4()
		{
			////Directional light
			//Light light = new Light(LightType.Directional, new Vector3(1.0f, 1.0f, 1.0f), null, null, -new Vector3(-2.0f, -7.0f, 2.0f)); //new Light(LightType.Directional, new Vector3(1f, 1f, 1f), new Vector3(-2.0f, -7.0f, 2.0f), 50.0f); //0.2, 0.2, 0.2
			//lights.Add(light);

			////Floor plane at y=3
			//Mesh plane = new Plane(new Vector3(0.0f, -1.0f, 0.0f), -3.0f, "Floor");
			//meshes.Add(plane);
			//Material planeMaterial = new Material(Texture.Checkerboard, new Vector3(0.937f, 0.937f, 0.231f));
			//planeMaterial.checkerboardSecondColor = new Vector3(0.815f, 0.235f, 0.031f);
			//materials.Add(planeMaterial);

			//Mesh sphere1 = new Sphere(new Vector3(-1.0f, 0.0f, 12.0f), 2.0f, "Sphere1");
			//meshes.Add(sphere1);
			//Material sphere1Material = new Material(Texture.Color, new Vector3(0.6f, 0.7f, 0.8f));
			//materials.Add(sphere1Material);

			//Mesh sphere2 = new Sphere(new Vector3(0.5f, 0.5f, 8.0f), 1.5f, "Sphere2");
			//meshes.Add(sphere2);
			//Material sphere2Material = new Material(Texture.Color, new Vector3(0.0f, 0.0f, 0.0f), 0.8f, 1.3f);
			//materials.Add(sphere2Material);
		}

		private void SpawnMap5()
		{
			//Light light = new Light(LightType.Directional, new Vector3(0.5f, 0.5f, 0.5f), null, null, new Vector3(-1.0f, 2.0f, 1.0f));
			//lights.Add(light);

			////Floor plane at y=2
			//Mesh plane = new Plane(new Vector3(0.0f, -1.0f, 0.0f), -2.0f, "Floor");
			//meshes.Add(plane);
			//Material planeMaterial = new Material(Texture.Checkerboard, Vector3.One, 0.3f);
			//materials.Add(planeMaterial);

			//Mesh plane2 = new Plane(new Vector3(0.0f, 0.0f, -1.0f), -20.0f, "Wall1");
			//meshes.Add(plane2);
			//Material planeMaterial2 = new Material(Texture.Color, Vector3.One, 0.8f);
			//materials.Add(planeMaterial2);

			//Mesh plane3 = new Plane(new Vector3(1.0f, 0.0f, 0.0f), -5.0f, "Wall2");
			//meshes.Add(plane3);
			//Material planeMaterial3 = new Material(Texture.Color, Vector3.One, 0.8f);
			//materials.Add(planeMaterial3);

			//Mesh sphere1 = new Sphere(new Vector3(3.5f, -1.0f, 13.0f), 1.5f, "Sphere1");
			//meshes.Add(sphere1);
			//Material sphere1Material = new Material(Texture.Color, new Vector3(0.6f, 0.6f, 1.0f), 0.99f);
			//materials.Add(sphere1Material);

			//Mesh sphere2 = new Sphere(new Vector3(-1.5f, -3.0f, 11.0f), 3.0f, "Sphere2");
			//meshes.Add(sphere2);
			//Material sphere2Material = new Material(Texture.Color, new Vector3(1.0f, 0.6f, 0.6f), 0.99f);
			//materials.Add(sphere2Material);
		}
	}
}
