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
		public static List<List<Photon>> causticsPhotonMap;

		public static void ClearPhotonMap()
		{
			globalPhotonMap = new List<List<Photon>>();
			causticsPhotonMap = new List<List<Photon>>();
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
			int numberOfPhotons = Config.PHOTON_COUNT;

			//Global photons
			for (int i = 0; i < numberOfPhotons; i++)
			{
				//TODO: Make this random evenly distributed

				//Random direction from point light
				Vector3 randomDirection = new Vector3(MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f),
					MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f), MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f));

				Ray photonRay = new Ray(position, randomDirection);

				TracePhoton(Config.MAX_PHOTON_DEPTH, settings.scene, photonRay);
			}
		}

		private static void TracePhoton(int depth, Scene scene, Ray photonRay)
		{
			RayTracer.NearestIntersection(scene.meshes, photonRay, out float distance, out int? indexOfNearest);

			Photon photon; //Create photon

			Vector3 intersection;
			Vector3 normal;
			Material material;

			//If a photon missed a surface, ignore it for now?
			if (indexOfNearest.HasValue) //REMINDER: This is currently different from ADVGR github
			{
				intersection = photonRay.At(distance);
				normal = scene.meshes[indexOfNearest.Value].Normal(intersection);
				material = scene.materials[indexOfNearest.Value];

				//Initialize photon
				photon.power = Vector3.One; //Photon color
				photon.L = photonRay.direction; //Incident direction
				photon.position = intersection; //World space position of the photon hit

				globalPhotonMap[indexOfNearest.Value].Add(photon); //Store photon

				//Shadow photon
				//if (depth < Config.MAX_PHOTON_DEPTH)
				AddShadowPhoton(photonRay, scene, intersection);
			}
			else
				return;

			if (depth > 1)
			{
				//TODO: Add russian roulette to determine if the photon should be reflected, transmitted or absorbed
				switch (material.materialType)
				{
					case MaterialType.Diffuse:
						{
							//Bounce the photon
							bool outside = Vector3.Dot(photonRay.direction, normal) < 0.0f;
							Vector3 bias = Renderer.EPSILON * normal;

							Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
							Vector3 reflectionDirection = Renderer.Reflect(photonRay.direction, normal).Normalized();
							Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

							TracePhoton(depth - 1, scene, reflectionRay); //Trace it to the next location
							break;
						}
					case MaterialType.Reflection:
						{
							break;
						}
					case MaterialType.Reflection_Refraction:
						{
							break;
						}
				}
			}
		}

		private static void AddShadowPhoton(Ray ray, Scene scene, Vector3 intersection)
		{
			Vector3 shadow = new Vector3(-0.25f); //-0.25f
			Vector3 tPoint = intersection;
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
			Vector3 energy = new Vector3(0.0f, 0.0f, 0.0f);

			var photons = globalPhotonMap[index];

			for (int i = 0; i < photons.Count; i++)
			{
				Photon photon = photons[i];
				float distance = (position - photon.position).LengthSquared;

				if (distance < Config.MAX_PHOTON_SEARCH_RADIUS)
				{
					float weight = Math.Max(0.0f, -Vector3.Dot(normal, photon.L));
					weight *= (1.0f - (float)Math.Sqrt(distance)) / (Config.PHOTON_COUNT / 50.0f); //REMINDER: This is probably incorrect //TODO: Scale this by some factor?

					//Add photon's energy to total
					//photon.power += new Vector3(weight); //TODO: Check if this is correct (this is probably not correct)

					energy += photon.power;// * weight;
				}
			}

			return energy / photons.Count * 75;
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

			return bitmap;
		}
	}
}
