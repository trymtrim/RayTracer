﻿using System;
using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	enum SceneType
	{
		Skybox,
		SpotLight,
		Photograph,
		Everything,
		Mirrors,
		Room
	}

	class Scene
	{
		public static Scene scene; //TODO: Unsure if I want to handle this this way

		public Skybox skybox;

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
				case SceneType.Skybox:
					SpawnMap1();
					break;
				case SceneType.SpotLight:
					SpawnMap2();
					break;
				case SceneType.Photograph:
					SpawnMap3();
					break;
				case SceneType.Everything:
					SpawnMap4();
					break;
				case SceneType.Mirrors:
					SpawnMap5();
					break;
				case SceneType.Room:
					SpawnMap6();
					break;
			}
		}

		private void SpawnMap1()
		{
			//Skybox
			skybox = new Skybox(scene);

			//Directional light
			Light light = new Light(LightType.Directional, new Vector3(1.0f, 1.0f, 1.0f), 2.4f, null, null, new Vector3(1.0f, 0.8f, 1.0f));
			lights.Add(light);

			Mesh sphere1 = new Sphere(new Vector3(-4.0f, 1.5f, 6.0f), 1.0f, "Sphere1");
			meshes.Add(sphere1);
			Material sphere1Material = new ReflectionMaterial(TextureType.Color, 0.8f, new Vector3(0.8f));
			materials.Add(sphere1Material);

			Mesh sphere2 = new Sphere(new Vector3(1.0f, -2.0f, 9.0f), 2.0f, "Sphere2");
			meshes.Add(sphere2);
			Material sphere2Material = new ReflectionMaterial(TextureType.Color, 0.8f, new Vector3(0.8f));
			materials.Add(sphere2Material);

			Mesh sphere3 = new Sphere(new Vector3(-2.0f, 1.0f, 11.0f), 1.5f, "Sphere3");
			meshes.Add(sphere3);
			Material sphere3Material = new ReflectionMaterial(TextureType.Color, 0.8f, new Vector3(0.8f));
			materials.Add(sphere3Material);

			Mesh sphere5 = new Sphere(new Vector3(-3.0f, 1.0f, 20.0f), 1.0f, "Sphere5");
			meshes.Add(sphere5);
			Material sphere5Material = new ReflectionMaterial(TextureType.Color, 0.8f, new Vector3(0.8f));
			materials.Add(sphere5Material);

			Mesh sphere4 = new Sphere(new Vector3(3.0f, 1.2f, 6.0f), 1.5f, "Sphere4");
			meshes.Add(sphere4);
			Material sphere4Material = new ReflectionMaterial(TextureType.Color, 0.8f, new Vector3(0.8f));
			materials.Add(sphere4Material);
		}

		private void SpawnMap2()
		{
			skybox = null;

			//Spot light
			Light light = new Light(LightType.Spot, new Vector3(0.9f, 0.8f, 0.3f), 1.5f, new Vector3(5.0f, -5.0f, 7.0f), 6.0f, null, new Vector3(2.0f, -1.0f, 8.0f));
			lights.Add(light);

			//Floor
			Mesh plane = new Plane(new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), null, "Floor");
			meshes.Add(plane);
			Material planeMaterial = new DiffuseMaterial(TextureType.Checkerboard); //new Vector3(0.75f, 0.25f, 0.25f));
			materials.Add(planeMaterial);

			Mesh sphere1 = new Sphere(new Vector3(-4.0f, 1.0f, 6.0f), 1.0f, "Sphere1");
			meshes.Add(sphere1);
			Material sphere1Material = new ReflectionMaterial(TextureType.Color, 0.75f, new Vector3(0.1f, 1.0f, 0.1f));
			materials.Add(sphere1Material);

			Mesh sphere2 = new Sphere(new Vector3(-2.0f, 0.5f, 11.0f), 1.5f, "Sphere2");
			meshes.Add(sphere2);
			Material sphere2Material = new ReflectionMaterial(TextureType.Color, 0.75f, new Vector3(0.1f, 0.1f, 1.0f));
			materials.Add(sphere2Material);

			Mesh sphere3 = new Sphere(new Vector3(3.0f, 0.5f, 6.0f), 1.5f, "Sphere3");
			meshes.Add(sphere3);
			Material sphere3Material = new ReflectionMaterial(TextureType.Color, 0.75f, new Vector3(1.0f, 0.1f, 0.1f));
			materials.Add(sphere3Material);
		}

		private void SpawnMap3()
		{
			skybox = null;

			//Point light
			Light light = new Light(LightType.Point, new Vector3(0.7f, 0.7f, 0.7f), 15.0f, new Vector3(0.0f, -1.45f, 2.0f), 0.5f);
			lights.Add(light);

			//Floor
			Mesh floor = new Plane(new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), null, "Floor");
			meshes.Add(floor);
			Material planeMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(planeMaterial);

			//Roof
			Mesh roof = new Plane(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, -1.5f, 0.0f), null, "Roof");
			meshes.Add(roof);
			Material roofMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(roofMaterial);

			//Left wall
			Mesh leftWall = new Plane(new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), null, "Left Wall");
			meshes.Add(leftWall);
			Material leftWallMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.75f, 0.25f, 0.25f));
			materials.Add(leftWallMaterial);

			//Right wall
			Mesh rightWall = new Plane(new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), null, "Right Wall");
			meshes.Add(rightWall);
			Material rightWallMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.25f, 0.25f, 0.75f));
			materials.Add(rightWallMaterial);

			//Front wall
			Mesh frontWall = new Plane(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -4.2f, 0.0f), null, "Front Wall");
			meshes.Add(frontWall);
			Material frontWallMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(frontWallMaterial);

			//Back wall
			Mesh backWall = new Plane(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -5.0f, 0.0f), null, "Back Wall");
			meshes.Add(backWall);
			Material backWallMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(backWallMaterial);

			//Mirror
			Mesh mirror = new Plane(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, -4.19f), new Vector3(2.0f, 1.0f, 2.0f * 1.26666f), "Mirror");
			meshes.Add(mirror);
			Material mirrorMaterial = new DiffuseMaterial(TextureType.Texture, "../../Assets/Textures/AilinTrym.png");
			materials.Add(mirrorMaterial);

			//Mirror ball
			Mesh sphere1 = new Sphere(new Vector3(-0.9f, 1.35f, 2.0f), 0.65f, "Mirror Ball");
			meshes.Add(sphere1);
			Material sphere1Material = new DiffuseMaterial(TextureType.Texture, "../../Assets/Textures/test.png");
			materials.Add(sphere1Material);

			//Glass ball
			Mesh sphere2 = new Sphere(new Vector3(1.0f, 1.35f, 1.75f), 0.65f, "Glass Ball");
			meshes.Add(sphere2);
			Material sphere2Material = new ReflectionRefractionMaterial(TextureType.Color, 1.5f);
			materials.Add(sphere2Material);
		}

		private void SpawnMap4()
		{
			//TODO: Implement this - everything should be in here

			//Skybox
			skybox = new Skybox(scene);

			//Floor plane
			Mesh plane = new Plane(new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, -2.5f, 4.0f), new Vector3(6.0f), "Floor");
			meshes.Add(plane);
			Material planeMaterial = new DiffuseMaterial(TextureType.Checkerboard, new Vector3(1, 0, 0));
			materials.Add(planeMaterial);
			skybox = null;

			////Floor plane
			//Mesh plane = new Plane(new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), new Vector3(10.0f, 10.0f, 10.0f), "Floor");
			//meshes.Add(plane);
			//Material planeMaterial = new ReflectionMaterial(TextureType.Checkerboard, 0.9f, new Vector3(1,0,0));
			//materials.Add(planeMaterial);

			Light light = new Light(LightType.Directional, new Vector3(1.0f, 1.0f, 1.0f), 2.4f, null, null, new Vector3(1.0f, 0.8f, 1.0f));
			lights.Add(light);

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
			skybox = null;

			//Point light
			Light light = new Light(LightType.Point, new Vector3(0.7f, 0.7f, 0.7f), 20.0f, new Vector3(0.0f, -1.45f, 2.0f), 0.5f);
			lights.Add(light);

			//Floor
			Mesh floor = new Plane(new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), null, "Floor");
			meshes.Add(floor);
			Material planeMaterial = new ReflectionMaterial(TextureType.Checkerboard, 0.8f, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(planeMaterial);

			//Roof
			Mesh roof = new Plane(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, -1.5f, 0.0f), null, "Roof");
			meshes.Add(roof);
			Material roofMaterial = new ReflectionMaterial(TextureType.Color, 0.8f, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(roofMaterial);

			//Left wall
			Mesh leftWall = new Plane(new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), null, "Left Wall");
			meshes.Add(leftWall);
			Material leftWallMaterial = new ReflectionMaterial(TextureType.Color, 0.8f, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(leftWallMaterial);

			//Right wall
			Mesh rightWall = new Plane(new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), null, "Right Wall");
			meshes.Add(rightWall);
			Material rightWallMaterial = new ReflectionMaterial(TextureType.Color, 0.8f, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(rightWallMaterial);

			//Front wall
			Mesh frontWall = new Plane(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -4.2f, 0.0f), null, "Front Wall");
			meshes.Add(frontWall);
			Material frontWallMaterial = new ReflectionMaterial(TextureType.Color, 0.8f, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(frontWallMaterial);

			//Back wall
			Mesh backWall = new Plane(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -5.0f, 0.0f), null, "Back Wall");
			meshes.Add(backWall);
			Material backWallMaterial = new ReflectionMaterial(TextureType.Color, 0.8f, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(backWallMaterial);

			//Mirror ball
			Mesh sphere1 = new Sphere(new Vector3(-0.9f, 1.35f, 3.0f), 0.65f, "Mirror Ball");
			meshes.Add(sphere1);
			Material sphere1Material = new ReflectionMaterial(TextureType.Color, 0.98f);
			materials.Add(sphere1Material);

			//Glass ball
			Mesh sphere2 = new Sphere(new Vector3(1.0f, 1.35f, 2.25f), 0.65f, "Glass Ball");
			meshes.Add(sphere2);
			Material sphere2Material = new ReflectionMaterial(TextureType.Color, 0.98f);
			materials.Add(sphere2Material);
		}

		private void SpawnMap6()
		{
			skybox = null;

			//Point light
			Light light = new Light(LightType.Point, new Vector3(0.7f, 0.7f, 0.7f), 20.0f, new Vector3(0.0f, -1.45f, 2.0f), 0.5f);
			lights.Add(light);

			//Floor
			Mesh floor = new Plane(new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), null, "Floor");
			meshes.Add(floor);
			//Material planeMaterial = new ReflectionMaterial(Texture.Checkerboard, 1.0f, new Vector3(0.75f, 0.75f, 0.75f));
			Material planeMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(planeMaterial);

			//Roof
			Mesh roof = new Plane(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, -1.5f, 0.0f), null, "Roof");
			meshes.Add(roof);
			Material roofMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(roofMaterial);

			//Left wall
			Mesh leftWall = new Plane(new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), null, "Left Wall");
			meshes.Add(leftWall);
			Material leftWallMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.75f, 0.25f, 0.25f));
			materials.Add(leftWallMaterial);

			//Right wall
			Mesh rightWall = new Plane(new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -2.0f, 0.0f), null, "Right Wall");
			meshes.Add(rightWall);
			Material rightWallMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.25f, 0.25f, 0.75f));
			materials.Add(rightWallMaterial);

			//Front wall
			Mesh frontWall = new Plane(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -4.2f, 0.0f), null, "Front Wall");
			meshes.Add(frontWall);
			Material frontWallMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(frontWallMaterial);

			//Back wall
			Mesh backWall = new Plane(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -5.0f, 0.0f), null, "Back Wall");
			meshes.Add(backWall);
			Material backWallMaterial = new DiffuseMaterial(TextureType.Color, new Vector3(0.75f, 0.75f, 0.75f));
			materials.Add(backWallMaterial);

			//Mirror ball
			Mesh sphere1 = new Sphere(new Vector3(-0.9f, 1.35f, 3.0f), 0.65f, "Mirror Ball");
			meshes.Add(sphere1);
			Material sphere1Material = new ReflectionMaterial(TextureType.Color, 0.98f);
			materials.Add(sphere1Material);

			//Glass ball
			Mesh sphere2 = new Sphere(new Vector3(1.0f, 1.35f, 2.25f), 0.65f, "Glass Ball");
			meshes.Add(sphere2);
			Material sphere2Material = new ReflectionRefractionMaterial(TextureType.Color, 1.5f);
			materials.Add(sphere2Material);
		}
	}
}
