using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracerTestBed
{
	class PhotonMapping
	{
		public static List<List<Photon>> globalPhotonMap;
		public static List<List<Photon>> causticPhotonMap;

		private static Photon photon;
		private static bool _caustic = false;

		public static void ClearPhotonMap()
		{
			globalPhotonMap = new List<List<Photon>>();
			causticPhotonMap = new List<List<Photon>>();
		}

		public static void GeneratePhotons(RenderSettings settings)
		{
			MathHelper.ResetStaticRandomSeed();

			for (int i = 0; i < settings.scene.lights.Count; i++)
			{
				Light light = settings.scene.lights[i];
				GeneratePhotonsFromLight(settings, light.mesh.Center(), light.brightness);
			}
		}

		private static void GeneratePhotonsFromLight(RenderSettings settings, Vector3 position, float intensity)
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

			Vector3 intersection;
			Vector3 normal;
			Material material;
			Mesh mesh;

			if (indexOfNearest.HasValue)
			{
				intersection = photonRay.At(distance);
				normal = scene.meshes[indexOfNearest.Value].Normal(intersection);
				material = scene.materials[indexOfNearest.Value];
				mesh = scene.meshes[indexOfNearest.Value];

				//Initialize photon
				if (depth == Config.MAX_PHOTON_DEPTH)
				{
					_caustic = false; //Reset value

					photon.power = Vector3.One; //Photon color
				}

				photon.L = photonRay.direction; //Incident direction
				photon.position = intersection; //World space position of the photon hit

				//globalPhotonMap[indexOfNearest.Value].Add(photon); //Store photon

				//Shadow photon
				//if (depth < Config.MAX_PHOTON_DEPTH)
				//AddShadowPhoton(photonRay, scene, intersection);
			}
			else
				return;

			Vector3 color = material.Color(mesh, photonRay, distance, intersection);

			if (depth > 1)
			{
				//TODO: Add russian roulette to determine if the photon should be reflected, transmitted or absorbed
				switch (material.materialType)
				{
					case MaterialType.Diffuse:
						{
							if (causticTracing && depth == Config.MAX_PHOTON_DEPTH)
								return;

							photon.power = color;

							if (_caustic)
							{
								causticPhotonMap[indexOfNearest.Value].Add(photon); //Store photon
								_caustic = false; //Reset value
							}
							else
							{
								globalPhotonMap[indexOfNearest.Value].Add(photon); //Store photon
								//AddShadowPhoton(photonRay, scene, intersection);
							}

							Vector3 newNormal = normal;
							if (Vector3.Dot(normal, photonRay.direction) > 0.0f)
								newNormal = -normal;

							var direction = RandomOnHemisphere(newNormal);
							Ray newRay = new Ray(intersection + direction * Renderer.EPSILON, direction);
							TracePhoton(depth - 1, scene, newRay);// * Vector3.Dot(newNormal, direction);

							//Bounce the photon
							//bool outside = Vector3.Dot(photonRay.direction, normal) < 0.0f;
							//Vector3 bias = Renderer.EPSILON * normal;

							//Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
							//Vector3 reflectionDirection = Renderer.Reflect(photonRay.direction, normal).Normalized();
							//Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

							//TracePhoton(depth - 1, scene, reflectionRay); //Trace it to the next location
							break;
						}
					case MaterialType.Reflection:
						{
							//if (causticTracing)
							//	_caustic = true;

							bool outside = Vector3.Dot(photonRay.direction, normal) < 0.0f;
							Vector3 bias = Renderer.EPSILON * normal;

							Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
							Vector3 reflectionDirection = Renderer.Reflect(photonRay.direction, normal).Normalized();
							Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

							//TracePhoton(depth - 1, scene, reflectionRay, causticTracing);

							break;
						}
					case MaterialType.Reflection_Refraction:
						{
							if (causticTracing)
								_caustic = true;

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
								Vector3 reflectionDirection = Renderer.Reflect(photonRay.direction, normal).Normalized();
								Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
								Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

								TracePhoton(depth - 1, scene, reflectionRay, causticTracing);
							}
							break;
						}
				}
			}
		}

		private static void AddShadowPhoton(Ray ray, Scene scene, Vector3 intersection)
		{
			Vector3 shadow = new Vector3(-0.25f); //-0.25f
			Vector3 tPoint = intersection;
			//Vector3 bias = Renderer.EPSILON * normal;
			Vector3 bumpedPoint = tPoint;// * (1.0f + Renderer.EPSILON);

			//Vector3 direction = scene.lights[0].mesh.Center() - tPoint;

			Ray shadowRay = new Ray(bumpedPoint, ray.direction);
			RayTracer.NearestIntersection(scene.meshes, shadowRay, out float distance, out int? indexOfNearest);

			//Vector3 v1 = tPoint;
			//Vector3 v2 = scene.lights[0].mesh.Center();

			//float distanceToLight = (float)Math.Sqrt((v2.X - v1.X) * (v2.X - v1.X) + (v2.Y - v1.Y) * (v2.Y - v1.Y) + (v2.Z - v1.Z) * (v2.Z - v1.Z));

			if (!indexOfNearest.HasValue)// || distance < distanceToLight)
				return;

			Photon photon;
			photon.L = shadowRay.direction; //Incident direction
			photon.position = shadowRay.At(distance); //World space position of the photon hit
			photon.power = shadow; //Photon color

			globalPhotonMap[indexOfNearest.Value].Add(photon); //Store shadow photon
		}

		public static Vector3 GatherPhotonEnergy(Vector3 position, Vector3 normal, int index)
		{
			Vector3 energy = Vector3.Zero;
			Vector3 causticEnergy = Vector3.Zero;

			var photons = globalPhotonMap[index];

			for (int i = 0; i < photons.Count; i++)
			{
				Photon photon = photons[i];
				float distance = (position - photon.position).LengthSquared;

				if (distance < Config.MAX_PHOTON_SEARCH_RADIUS)
				{
					float weight = Math.Max(0.0f, -Vector3.Dot(normal, photon.L));
					//weight *= (1.0f - (float)Math.Sqrt(distance));// / (Config.PHOTON_COUNT / 50.0f); //REMINDER: This is probably incorrect //TODO: Scale this by some factor?

					energy += photon.power; //new Vector3(weight);
				}
			}

			photons = causticPhotonMap[index];

			for (int i = 0; i < photons.Count; i++)
			{
				Photon photon = photons[i];
				float distance = (position - photon.position).LengthSquared;

				if (distance < Config.MAX_PHOTON_SEARCH_RADIUS / 100.0f)
				{
					float weight = Math.Max(0.0f, -Vector3.Dot(normal, photon.L));
					weight *= (1.0f - (float)Math.Sqrt(distance)) / (Config.PHOTON_COUNT / 50.0f); //REMINDER: This is probably incorrect //TODO: Scale this by some factor?

					causticEnergy += photon.power; //* 2; //* 3.0f; //Use weight?
				}
			}

			return energy / globalPhotonMap[index].Count * 50.0f;// + causticEnergy / causticPhotonMap[index].Count * 50.0f;// + causticEnergy / (causticPhotonMap.Count / 0.1f);// / (2.0f / globalPhotonMap.Count);
		}

		public static Bitmap PhotonMapRender(RenderSettings settings, Camera camera)
		{
			Bitmap bitmap = new Bitmap(settings.width, settings.height);

			foreach (var photons in globalPhotonMap)
			{
				foreach (Photon photon in photons)
				{
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

			foreach (var photons in causticPhotonMap)
			{
				foreach (Photon photon in photons)
				{
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

		private static Vector3 RandomOnHemisphere(Vector3 normal)
		{
			var x = MathHelper.RandomRange(-1.0f, 1.0f);
			var y = MathHelper.RandomRange(-1.0f, 1.0f);
			var z = MathHelper.RandomRange(-1.0f, 1.0f);

			var v = new Vector3(x, y, z).Normalized();

			if (Vector3.Dot(v, normal) < 0.0f)
				return -v;

			return v;
		}
	}
}
