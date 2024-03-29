﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using KdTree;
using KdTree.Math;

namespace RayTracerTestBed
{
	class PhotonMapping
	{
		private static List<KdTree<float, Photon>> _globalPhotonMap;
		private static List<KdTree<float, Photon>> _causticPhotonMap;

		private static Photon _photon;

		private static bool _caustic = false;
		private static bool _shadowPhoton = false;

		public static void InitializePhotonMap(RenderSettings settings)
		{
			ClearPhotonMaps();

			for (int i = 0; i < settings.scene.meshes.Count; i++)
				_globalPhotonMap.Add(new KdTree<float, Photon>(3, new FloatMath()));
			for (int i = 0; i < settings.scene.meshes.Count; i++)
				_causticPhotonMap.Add(new KdTree<float, Photon>(3, new FloatMath()));

			GeneratePhotons(settings);

			for (int i = 0; i < _globalPhotonMap.Count; i++)
			{
				if (_globalPhotonMap[i].Count > 0)
					_globalPhotonMap[i].Balance();
			}

			for (int i = 0; i < _causticPhotonMap.Count; i++)
			{
				if (_causticPhotonMap[i].Count > 0)
					_causticPhotonMap[i].Balance();
			}
		}

		private static void ClearPhotonMaps()
		{
			_globalPhotonMap = new List<KdTree<float, Photon>>();
			_causticPhotonMap = new List<KdTree<float, Photon>>();
		}

		private static void GeneratePhotons(RenderSettings settings)
		{
			MathHelper.ResetStaticRandomSeed();

			for (int i = 0; i < settings.scene.lights.Count; i++)
			{
				Light light = settings.scene.lights[i];
				GeneratePhotonsFromLight(settings, light.mesh.Center());
			}
		}

		private static void GeneratePhotonsFromLight(RenderSettings settings, Vector3 position)
		{
			int numberOfGlobalPhotons = Config.PHOTON_COUNT;
			int numberOfcausticPhotons = Config.CAUSTIC_PHOTON_COUNT;

			//Global photons
			for (int i = 0; i < numberOfGlobalPhotons; i++)
			{
				//TODO: Make this random evenly distributed

				//Random direction from point light
				Vector3 randomDirection = new Vector3(MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f),
					MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f), MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f));

				Ray photonRay = new Ray(position, randomDirection);

				TracePhoton(Config.MAX_PHOTON_DEPTH, settings.scene, photonRay);
			}

			//Caustic photons
			for (int i = 0; i < numberOfcausticPhotons; i++)
			{
				//TODO: Make this random evenly distributed

				//Random direction from point light
				Vector3 randomDirection = new Vector3(MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f),
					MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f), MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f));

				Ray photonRay = new Ray(position, randomDirection);

				TracePhoton(Config.MAX_PHOTON_DEPTH, settings.scene, photonRay, true);
			}
		}

		private static void TracePhoton(int depth, Scene scene, Ray photonRay, bool causticTracing = false)
		{
			RayTracer.NearestIntersection(scene.meshes, photonRay, out float distance, out int? indexOfNearest);

			if (!indexOfNearest.HasValue)
				return;

			Vector3 intersection = photonRay.At(distance);
			Vector3 normal = scene.meshes[indexOfNearest.Value].Normal(intersection);
			int index = indexOfNearest.Value;
			Material material = scene.materials[index];

			_photon.L = photonRay.direction; //Incident direction
			_photon.position = intersection; //World space position of the photon hit

			//Initialize photon
			if (depth == Config.MAX_PHOTON_DEPTH && !_shadowPhoton)
			{
				_caustic = false;

				_photon.power = Vector3.One;
			}
			else if (_shadowPhoton) //Shadow photon (indirect illumination)
			{
				if (material.materialType == MaterialType.Diffuse)
				{
					_photon.power = new Vector3(-Config.SHADOW_STRENGTH);

					//TODO: This might not work with diffuse spheres right now as the shadow photon will stop inside the sphere (?)
					_globalPhotonMap[index].Add(new[] { _photon.position.X, _photon.position.Y, _photon.position.Z }, _photon); //Store photon
				}
				else
				{
					photonRay.origin = intersection + (photonRay.direction * Renderer.EPSILON);
					TracePhoton(0, scene, photonRay);
				}

				_shadowPhoton = false;
				return;
			}

			if (depth == 0)
				return;

			switch (material.materialType)
			{
				case MaterialType.Diffuse:
					{
						if (_caustic)
						{
							_causticPhotonMap[index].Add(new[] { _photon.position.X, _photon.position.Y, _photon.position.Z }, _photon); //Store photon
							_caustic = false;
						}
						else if (causticTracing)
						{
							_caustic = false;
							return;
						}
						else
						{
							_photon.power *= 1.0f / (float)Math.Sqrt(Config.MAX_PHOTON_DEPTH - depth + 1.0f);
							
							//TODO: This might not work with diffuse spheres right now as the shadow photon will stop inside the sphere (?)
							_globalPhotonMap[index].Add(new[] { _photon.position.X, _photon.position.Y, _photon.position.Z }, _photon); //Store photon

							//TODO: Randomly absorb or bounce (Russian roulette)?

							//Bounce the photon
							Vector3 newNormal = normal;
							if (Vector3.Dot(normal, photonRay.direction) > 0.0f)
								newNormal = -normal;

							var direction = MathHelper.RandomOnHemisphereWithStaticSeed(newNormal);
							Ray newRay = new Ray(intersection + direction * Renderer.EPSILON, direction);
							TracePhoton(depth - 1, scene, newRay);
						}

						break;
					}
				case MaterialType.Reflection:
					{
						if (causticTracing)
							_caustic = true;
						else
						{
							//Shadow photon ray (currently only doing it on reflection and refraction objects, since the scene only consists of those)
							_shadowPhoton = true;

							Vector3 shadowRayOrigin = intersection + (photonRay.direction * Renderer.EPSILON);
							Ray shadowRay = new Ray(shadowRayOrigin, photonRay.direction);
							TracePhoton(0, scene, shadowRay);

							_shadowPhoton = false;
						}

						bool outside = Vector3.Dot(photonRay.direction, normal) < 0.0f;
						Vector3 bias = Renderer.EPSILON * normal;

						Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
						Vector3 reflectionDirection = Renderer.Reflect(photonRay.direction, normal).Normalized();
						Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

						TracePhoton(depth - 1, scene, reflectionRay, causticTracing);

						break;
					}
				case MaterialType.Reflection_Refraction:
					{
						if (causticTracing)
							_caustic = true;
						else
						{
							//Shadow photon ray (currently only doing it on reflection and refraction objects, since the scene only consists of those)
							_shadowPhoton = true;

							Vector3 shadowRayOrigin = intersection + (photonRay.direction * Renderer.EPSILON);
							Ray shadowRay = new Ray(shadowRayOrigin, photonRay.direction);
							TracePhoton(0, scene, shadowRay);

							_shadowPhoton = false;
						}

						var ior = material.ior;
						bool outside = Vector3.Dot(photonRay.direction, normal) < 0.0f;
						Vector3 bias = Renderer.EPSILON * normal;

						float kr = Renderer.Fresnel(photonRay.direction, normal, ior);

						//Compute refraction if it is not a case of total internal reflection
						if (kr < 1.0f)
						{
							Vector3 refractionRayOrigin = outside ? intersection - bias : intersection + bias;
							Vector3 refractionDirection = Renderer.Refract(photonRay.direction, normal, ior).Normalized();
							Ray refractionRay = new Ray(refractionRayOrigin, refractionDirection);

							TracePhoton(depth - 1, scene, refractionRay, causticTracing);
						}
						else
						{
							Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
							Vector3 reflectionDirection = Renderer.Reflect(photonRay.direction, normal).Normalized();
							Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

							TracePhoton(depth - 1, scene, reflectionRay, causticTracing);
						}
						break;
					}
			}
		}

		public static Vector3 GatherPhotonEnergy(Vector3 position, Vector3 normal, int index)
		{
			Vector3 globalEnergy = Vector3.Zero;
			Vector3 causticEnergy = Vector3.Zero;

			var photonTree = _globalPhotonMap[index].RadialSearch(new[] { position.X, position.Y, position.Z }, Config.MAX_PHOTON_SEARCH_RADIUS);
			var photonEnumerator = photonTree.GetEnumerator();

			while (photonEnumerator.MoveNext())
			{
				var photon = ((KdTreeNode<float, Photon>)photonEnumerator.Current).Value;

				float distance = (position - photon.position).LengthSquared;
				float weight = Math.Max(0.0f, -Vector3.Dot(normal, photon.L));
				weight *= 1.0f - (float)Math.Sqrt(distance);

				globalEnergy += photon.power * weight;
			}

			photonTree = _causticPhotonMap[index].RadialSearch(new[] { position.X, position.Y, position.Z }, Config.MAX_PHOTON_SEARCH_RADIUS / 10.0f);
			photonEnumerator = photonTree.GetEnumerator();

			while (photonEnumerator.MoveNext())
			{
				var photon = ((KdTreeNode<float, Photon>)photonEnumerator.Current).Value;

				float distance = (position - photon.position).LengthSquared;
				float weight = Math.Max(0.0f, -Vector3.Dot(normal, photon.L));
				weight *= 1.0f - (float)Math.Sqrt(distance);

				causticEnergy += photon.power * weight;
			}

			Vector3 result = Vector3.Zero;

			if (Config.PHOTON_COUNT > 0)
				result += globalEnergy / (Config.PHOTON_COUNT * 0.0075f); //Not sure about this magic number here, but this works for now
			if (Config.CAUSTIC_PHOTON_COUNT > 0)
				result += causticEnergy / (Config.CAUSTIC_PHOTON_COUNT * 0.00006f); //Not sure about this magic number here, but this works for now

			return result;
		}

		public static Bitmap PhotonMapRender(RenderSettings settings, Camera camera)
		{
			Bitmap bitmap = new Bitmap(settings.width, settings.height);

			for (int i = 0; i < _globalPhotonMap.Count; i++)
			{
				var photonEnumerator = _globalPhotonMap[i].GetEnumerator();
				while (photonEnumerator.MoveNext())
				{
					var photon = ((KdTreeNode<float, Photon>)photonEnumerator.Current).Value;

					Vector3 position = photon.position;

					position -= camera.position * camera.direction * 2.0f; //TODO: Handle this properly

					int x = (int)(settings.width / 2.0f) + (int)(settings.width * position.X / position.Z);
					int y = (int)(settings.height / 2.0f) + (int)(settings.height * position.Y / position.Z);

					//TODO: Handle this properly
					if (position.X != 0.0f && position.Y != 0.0f && x >= 0.0f && y >= 0.0f && x < settings.width && y < settings.height)
						bitmap.SetPixel(x, y, photon.power.X >= 0.0f ? Color.White : Color.Blue);

					//Console.WriteLine("X: " + x + ", Y: " + y);
				}
			}

			for (int i = 0; i < _causticPhotonMap.Count; i++)
			{
				var photonEnumerator = _causticPhotonMap[i].GetEnumerator();
				while (photonEnumerator.MoveNext())
				{
					var photon = ((KdTreeNode<float, Photon>)photonEnumerator.Current).Value;

					Vector3 position = photon.position;

					position -= camera.position * camera.direction * 2.0f; //TODO: Handle this properly

					int x = (int)(settings.width / 2.0f) + (int)(settings.width * position.X / position.Z);
					int y = (int)(settings.height / 2.0f) + (int)(settings.height * position.Y / position.Z);

					//TODO: Handle this properly
					if (position.X != 0.0f && position.Y != 0.0f && x >= 0.0f && y >= 0.0f && x < settings.width && y < settings.height)
						bitmap.SetPixel(x, y, Color.Red);

					//Console.WriteLine("X: " + x + ", Y: " + y);
				}
			}

			return bitmap;
		}
	}
}
