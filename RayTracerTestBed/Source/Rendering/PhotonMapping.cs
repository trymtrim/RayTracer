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
		private static bool _shadowPhoton = false;

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

			if (!indexOfNearest.HasValue)
				return;

			Vector3 intersection = photonRay.At(distance);
			Vector3 normal = scene.meshes[indexOfNearest.Value].Normal(intersection);
			int index = indexOfNearest.Value;
			Material material = scene.materials[index];
			//Mesh mesh = scene.meshes[index];

			//if (_shadowPhoton && _caustic)
			//	Console.Write("YOO");

			photon.L = photonRay.direction; //Incident direction
			photon.position = intersection; //World space position of the photon hit
			
			//Initialize photon
			if (depth == Config.MAX_PHOTON_DEPTH && !_shadowPhoton)
			{
				//_caustic = false; //Reset value //TODO: Uncomment this?

				photon.power = Vector3.One; //Photon color
			}
			else if (_shadowPhoton) //Shadow photon (indirect illumination)
			{
				if (material.materialType == MaterialType.Diffuse)
				{
					photon.power = new Vector3(-0.15f); //Photon color

					globalPhotonMap[index].Add(photon); //TODO: This might not work with diffuse spheres right now as the shadow photon will stop inside the sphere (?)
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

			//Vector3 color = material.Color(mesh, photonRay, distance, intersection);

			//TODO: Add russian roulette to determine if the photon should be reflected, transmitted or absorbed
			switch (material.materialType)
			{
				case MaterialType.Diffuse:
					{
						if (_caustic)
						{
							causticPhotonMap[indexOfNearest.Value].Add(photon); //Store photon
							_caustic = false;
						}
						else if (causticTracing)
						{
							_caustic = false;
							return;
						}
						else
						{
							globalPhotonMap[indexOfNearest.Value].Add(photon); //Store photon

							//TODO: Randomly absorb or bounce (Russian roulette)

							//Bounce the photon
							//bool outside = Vector3.Dot(photonRay.direction, normal) < 0.0f;
							//Vector3 bias = Renderer.EPSILON * normal;

							//Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
							//Vector3 reflectionDirection = Renderer.Reflect(photonRay.direction, normal).Normalized();
							//Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

							//TracePhoton(depth - 1, scene, reflectionRay);

							Vector3 newNormal = normal;
							if (Vector3.Dot(normal, photonRay.direction) > 0.0f)
								newNormal = -normal;

							var direction = RandomOnHemisphere(newNormal);
							Ray newRay = new Ray(intersection + direction * Renderer.EPSILON, direction);
							TracePhoton(depth - 1, scene, newRay);// * Vector3.Dot(newNormal, direction);
						}

						//TODO: Try switching between the two bounce methods^

						break;
					}
				case MaterialType.Reflection:
					{
						if (causticTracing)
							_caustic = true;
						else
						{
							//Shoot shadow photon ray
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
							//Shoot shadow photon ray
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
							Vector3 reflectionDirection = Renderer.Reflect(photonRay.direction, normal).Normalized();
							Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
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

			var photons = globalPhotonMap[index];
			for (int i = 0; i < photons.Count; i++)
			{
				Photon photon = photons[i];
				float distance = (position - photon.position).LengthSquared;

				if (distance < Config.MAX_PHOTON_SEARCH_RADIUS)
				{
					float weight = Math.Max(0.0f, -Vector3.Dot(normal, photon.L));
					weight *= (1.0f - (float)Math.Sqrt(distance));// / (Config.PHOTON_COUNT / 50.0f); //REMINDER: This is probably incorrect //TODO: Scale this by some factor?

					globalEnergy += photon.power * weight; //new Vector3(weight);
				}
			}

			photons = causticPhotonMap[index];

			for (int i = 0; i < photons.Count; i++)
			{
				Photon photon = photons[i];
				float distance = (position - photon.position).LengthSquared;

				if (distance < Config.MAX_PHOTON_SEARCH_RADIUS / 200.0f)
				{
					float weight = Math.Max(0.0f, -Vector3.Dot(normal, photon.L));
					Vector3 weight2 = photon.power * (1.0f - (float)Math.Sqrt(distance));// / (Config.PHOTON_COUNT / 50.0f); //REMINDER: This is probably incorrect //TODO: Scale this by some factor?

					//TODO: Not sure if should use weight or photon.power
					causticEnergy += weight2; //photon.power;
				}
			}

			Vector3 result = Vector3.Zero;

			if (Config.PHOTON_COUNT > 0)
				result += globalEnergy / (Config.PHOTON_COUNT * 0.0125f);
			if (Config.CAUSTIC_PHOTON_COUNT > 0)
				result += causticEnergy / (Config.CAUSTIC_PHOTON_COUNT * 0.00003f);

			//TODO: Make dynamic according to Config.MAX_PHOTON_SEARCH_RADIUS
			return result;
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
