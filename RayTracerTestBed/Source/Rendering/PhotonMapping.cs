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

		public static Bitmap PhotonMap(RenderSettings settings, Camera camera)
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
						bitmap.SetPixel(x, y, photon.power.X >= 0.0f ? Color.White : Color.Black);

					//Console.WriteLine("X: " + x + ", Y: " + y);
				}
			}

			return bitmap;
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
				//Random direction from point light
				Vector3 randomDirection = new Vector3(MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f),
					MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f), MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f));

				Ray photonRay = new Ray(position, randomDirection);

				TracePhoton(Config.MAX_PHOTON_DEPTH, settings.scene, photonRay);// photon, intersection, indexOfNearest.Value, materials, photonRay, normal); //- 1 (?)
			}

			//Caustic photons
			//for (int i = 0; i < numberOfPhotons * 2; i++)
			//{
			//	//Random direction from point light
			//	Vector3 randomDirection = new Vector3(MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f),
			//		MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f), MathHelper.RandomRangeWithStaticSeed(-1.0f, 1.0f));

			//	Ray photonRay = new Ray(position, randomDirection);

			//	TracePhoton(Config.MAX_PHOTON_DEPTH, settings.scene, photonRay, true);// photon, intersection, indexOfNearest.Value, materials, photonRay, normal); //- 1 (?)
			//}
		}

		private static void TracePhoton(int depth, Scene scene, Ray photonRay)
		{
			RayTracer.NearestIntersection(scene.meshes, photonRay, out float distance, out int? indexOfNearest);

			Photon photon; //Create photon

			Material material;
			Vector3 intersection;
			Vector3 normal;

			//If a photon missed a surface, ignore it for now?
			if (indexOfNearest.HasValue) //REMINDER: This is currently different from ADVGR github
			{
				intersection = photonRay.At(distance);
				normal = scene.meshes[indexOfNearest.Value].Normal(intersection);

				material = scene.materials[indexOfNearest.Value];

				//Initialize photon
				//photon.power = Vector3.One; //color;// * diffuse; //Current power level for the photon - color?
				photon.L = photonRay.direction; //Incident direction
				photon.position = photonRay.At(distance); //World space position of the photon hit

				Vector3 color = material.Color(scene.meshes[indexOfNearest.Value], photonRay, distance, intersection);

				//var reflection = material.reflection;
				//var refraction = material.refraction;

				//var diffuse = 1.0f - reflection - refraction;

				Vector3 rgb = color * (1.0f / (float)Math.Sqrt(Config.MAX_PHOTON_DEPTH - depth + 1)); //* diffuse
				photon.power = rgb; //Photon color

				globalPhotonMap[indexOfNearest.Value].Add(photon); //Store photon

				//Shadow photon
				//if (depth < Config.MAX_PHOTON_DEPTH)
				AddShadowPhoton(photonRay, scene, intersection);
			}
			else
				return;

			if (depth > 1)
			{
				//TODO: Handle as mentioned in the paper
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
					//case MaterialType.Refraction:
					//	break;
					case MaterialType.Reflection_Refraction:
						{
							break;

							//Bounce the photon
							//bool outside = Vector3.Dot(photonRay.direction, normal) < 0.0f;
							//Vector3 bias = Renderer.EPSILON * normal;

							for (int i = 0; i < 2; i++)
							{
								Vector3 reflectionRayOrigin = intersection; //outside ? intersection + bias : intersection - bias;
								Vector3 reflectionDirection = photonRay.direction;
								Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

								RayTracer.NearestIntersection(scene.meshes, reflectionRay, out distance, out indexOfNearest);

								//If a photon missed a surface, ignore it for now?
								if (indexOfNearest.HasValue) //REMINDER: This is currently different from ADVGR github
								{
									intersection = reflectionRay.At(distance);
									normal = scene.meshes[indexOfNearest.Value].Normal(intersection);

									material = scene.materials[indexOfNearest.Value];

									//Initialize photon
									//photon.power = Vector3.One; //color;// * diffuse; //Current power level for the photon - color?
									photon.L = reflectionRay.direction; //Incident direction
									photon.position = reflectionRay.At(distance); //World space position of the photon hit

									//Vector3 color = material.Color(scene.meshes[indexOfNearest.Value], reflectionRay, distance, intersection);

									//var reflection = material.reflection;
									//var refraction = material.refraction;

									//var diffuse = 1.0f - reflection - refraction;

									if (material.materialType == MaterialType.Diffuse)
									{
										Vector3 rgb = new Vector3(0.0f, 0.0f, 1.0f);// color * (1.0f / (float)Math.Sqrt(Config.MAX_PHOTON_DEPTH - depth + 1)); //* diffuse
										photon.power = rgb; //Photon color

										globalPhotonMap[indexOfNearest.Value].Add(photon); //Store photon
									}
								}
							}

							//TracePhoton(0, scene, reflectionRay); //Trace it to the next location
							break;
						}
						//case MaterialType.Transparent:
						//	break;
				}
			}
		}

		private static void TracePhoton(int depth, Scene scene, Ray photonRay, bool caustics)
		{
			RayTracer.NearestIntersection(scene.meshes, photonRay, out float distance, out int? indexOfNearest);

			Photon photon; //Create photon

			Material material;
			Vector3 intersection;
			Vector3 normal;

			//If a photon missed a surface, ignore it for now?
			if (indexOfNearest.HasValue) //REMINDER: This is currently different from ADVGR github
			{
				intersection = photonRay.At(distance);
				normal = scene.meshes[indexOfNearest.Value].Normal(intersection);

				material = scene.materials[indexOfNearest.Value];
			}
			else
				return;

			if (depth > 1)
			{
				//TODO: Handle as mentioned in the paper
				switch (material.materialType)
				{
					case MaterialType.Reflection_Refraction:
						{
							for (int i = 0; i < 2; i++)
							{
								bool outside = Vector3.Dot(photonRay.direction, normal) < 0.0f;
								Vector3 bias = Renderer.EPSILON * normal;

								Vector3 reflectionRayOrigin = intersection; //outside ? intersection - bias : intersection + bias;
								Vector3 reflectionDirection = photonRay.direction;
								photonRay = new Ray(reflectionRayOrigin, reflectionDirection);

								RayTracer.NearestIntersection(scene.meshes, photonRay, out distance, out indexOfNearest);

								//If a photon missed a surface, ignore it for now?
								if (indexOfNearest.HasValue) //REMINDER: This is currently different from ADVGR github
								{
									intersection = photonRay.At(distance);
									normal = scene.meshes[indexOfNearest.Value].Normal(intersection);

									material = scene.materials[indexOfNearest.Value];

									//Initialize photon
									//photon.power = Vector3.One; //color;// * diffuse; //Current power level for the photon - color?
									photon.L = photonRay.direction; //Incident direction
									photon.position = photonRay.At(distance); //World space position of the photon hit

									//Vector3 color = material.Color(scene.meshes[indexOfNearest.Value], reflectionRay, distance, intersection);

									//var reflection = material.reflection;
									//var refraction = material.refraction;

									//var diffuse = 1.0f - reflection - refraction;

									if (material.materialType == MaterialType.Diffuse)
									{
										Vector3 rgb = Vector3.One; //new Vector3(1.0f, 0.0f, 0.0f);// color * (1.0f / (float)Math.Sqrt(Config.MAX_PHOTON_DEPTH - depth + 1)); //* diffuse
										photon.power = rgb; //Photon color

										causticsPhotonMap[indexOfNearest.Value].Add(photon); //Store photon
									}
								}
							}

							break;
						}
				}
			}
		}
		private static void AddShadowPhoton(Ray ray, Scene scene, Vector3 intersection)
		{
			Vector3 shadow = new Vector3(-0.25f);
			Vector3 tPoint = intersection;
			Vector3 bumpedPoint = tPoint * (1.0f + Renderer.EPSILON);

			Ray shadowRay = new Ray(bumpedPoint, ray.direction);
			RayTracer.NearestIntersection(scene.meshes, shadowRay, out float distance, out int? indexOfNearest);

			if (!indexOfNearest.HasValue)
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

				//TODO: Make a constant for maxDistance/radius
				if (distance < 1.5f)
				{
					float weight = Math.Max(0.0f, -Vector3.Dot(normal, photon.L));
					weight *= (1.0f - (float)Math.Sqrt(distance)) / (Config.PHOTON_COUNT / 50.0f); //REMINDER: This is probably incorrect //TODO: Scale this by some factor?

					//Add photon's energy to total
					//photon.power += new Vector3(weight); //TODO: Check if this is correct (this is probably not correct)

					energy += photon.power * new Vector3(weight);
				}
			}

			//photons = causticsPhotonMap[index];

			//for (int i = 0; i < photons.Count; i++)
			//{
			//	Photon photon = photons[i];
			//	float distance = (position - photon.position).LengthSquared;

			//	//TODO: Make a constant for maxDistance/radius
			//	if (distance < 1.0f)
			//	{
			//		float weight = Math.Max(0.0f, -Vector3.Dot(normal, photon.L));
			//		weight *= (1.0f - (float)Math.Sqrt(distance)) / (Config.PHOTON_COUNT / 500.0f); //REMINDER: This is probably incorrect //TODO: Scale this by some factor?

			//		//Add photon's energy to total
			//		//photon.power += new Vector3(weight); //TODO: Check if this is correct (this is probably not correct)

			//		energy += photon.power * new Vector3(weight);
			//	}
			//}

			return energy;
		}
	}
}
