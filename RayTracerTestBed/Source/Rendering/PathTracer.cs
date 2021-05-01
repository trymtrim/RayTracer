using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	class PathTracer
	{
		public static Vector3 Trace(int depth, Scene scene, Ray ray, Vector3 backgroundColor)
		{
			if (depth == Game.settings.maxDepth)
				Game.numPrimaryRays++;

			float distance = 0.0f;
			int? indexOfNearest = null;
			bool isLight = false;

			if (Config.USE_BVH)
			{
				List<int> meshIndices = scene.bvh.Traverse(ray);

				if (meshIndices.Count > 0)
				{
					//TODO: This is a bit ugly right now - should do the same as in RayTracer
					List<Mesh> meshes = new List<Mesh>();

					for (int i = 0; i < meshIndices.Count; i++)
						meshes.Add(scene.meshes[meshIndices[i]]);

					NearestIntersectionIncludingLights(meshes, scene.lights, ray, out distance, out indexOfNearest, out isLight);

					if (!isLight && indexOfNearest.HasValue)
						indexOfNearest = meshIndices[indexOfNearest.Value];
				}
			}
			else
			{
				NearestIntersectionIncludingLights(scene.meshes, scene.lights, ray, out distance, out indexOfNearest, out isLight);
			}

			if (indexOfNearest.HasValue)
			{
				Game.numRayIntersections++;

				int index = indexOfNearest.Value;
				var intersection = ray.At(distance);

				if (isLight)
				{
					Light light = scene.lights[index];

					var normal = light.mesh.Normal(intersection);

					if (Vector3.Dot(normal, ray.direction) < 0.0f)
						return light.color * light.brightness;
					else
						return Vector3.Zero; //Black
				}
				else
				{
					if (depth <= 1)
						return Vector3.Zero; //Black

					Mesh mesh = scene.meshes[index];
					Material material = scene.materials[index];

					if (mesh.isSkyboxMesh)
						return material.Color(mesh, ray, distance, intersection);

					var reflection = material.reflection;
					var refraction = material.refraction;
					var ior = material.ior;
					var transparency = material.transparency;

					var materialType = material.materialType;

					Vector3 color = material.Color(mesh, ray, distance, intersection);

					var normal = scene.meshes[index].Normal(intersection);

					bool outside = Vector3.Dot(ray.direction, normal) < 0.0f;
					Vector3 bias = Renderer.EPSILON * normal;

					var result = Vector3.Zero;

					switch (materialType)
					{
						case MaterialType.Diffuse:
							{
								return DirectIllumination(depth, color, intersection, ray, normal, scene, backgroundColor);
							}
						case MaterialType.Reflection:
							{
								Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
								Vector3 reflectionDirection = Renderer.Reflect(ray.direction, normal).Normalized();
								Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

								Vector3 reflectionColor = Trace(depth - 1, scene, reflectionRay, backgroundColor);
								result = reflection * reflectionColor;

								result = color * result;
								break;
							}
						case MaterialType.Refraction:
							{
								float kr = Renderer.Fresnel(ray.direction, normal, ior);

								if (kr < 1.0f) //TODO: Not sure if this is needed anymore
								{
									Vector3 refractionRayOrigin = outside ? intersection - bias : intersection + bias;
									Vector3 refractionDirection = Renderer.Refract(ray.direction, normal, ior).Normalized();
									Ray refractionRay = new Ray(refractionRayOrigin, refractionDirection);

									Vector3 refractionColor = Trace(depth - 1, scene, refractionRay, backgroundColor);
									result += refraction * refractionColor;
								}

								result = color * result;
								break;
							}
						case MaterialType.Reflection_Refraction:
							{
								Vector3 refractionColor = new Vector3(0.0f);

								float kr = Renderer.Fresnel(ray.direction, normal, ior);

								//Compute refraction if it is not a case of total internal reflection
								if (kr < 1.0f)
								{
									Vector3 refractionDirection = Renderer.Refract(ray.direction, normal, ior).Normalized();
									Vector3 refractionRayOrigin = outside ? intersection - bias : intersection + bias;
									Ray refractionRay = new Ray(refractionRayOrigin, refractionDirection);

									refractionColor = Trace(depth - 1, scene, refractionRay, backgroundColor);
								}

								Vector3 reflectionDirection = Renderer.Reflect(ray.direction, normal).Normalized();
								Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
								Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

								Vector3 reflectionColor = Trace(depth - 1, scene, reflectionRay, backgroundColor);
								result = (reflectionColor * kr + refractionColor * (1.0f - kr)) * reflection;

								result = color * result;
								break;
							}
						case MaterialType.Transparent:
							{
								Vector3 transparentRayOrigin = outside ? intersection - bias : intersection + bias;
								Vector3 transparentDirection = ray.direction;
								Ray transparentRay = new Ray(transparentRayOrigin, transparentDirection);

								Vector3 transparentColor = Trace(depth - 1, scene, transparentRay, backgroundColor);

								result = color * result * (1.0f - transparency) + transparentColor * transparency;
								break;
							}
					}

					if (material.selected)
					{
						//Outline
						float kr = Renderer.Fresnel(ray.direction, normal, 0.95f);
						result = Vector3.One * kr + result * (1.0f - kr);
					}

					return result;
				}
			}

			return backgroundColor;
		}

		private static void NearestIntersectionIncludingLights(List<Mesh> meshes, List<Light> lights, Ray ray, out float distance, out int? indexOfNearest, out bool isLight)
		{
			distance = float.MaxValue;
			indexOfNearest = null;
			isLight = false;

			for (int i = 0; i < meshes.Count; i++)
			{
				Game.numRayTests++;

				var intersection = meshes[i].Intersect(ray);

				if (intersection.HasValue)
				{
					float t = intersection.Value;

					if (t < distance)
					{
						distance = t;
						indexOfNearest = i;
						isLight = false;
					}
				}
			}

			for (int i = 0; i < lights.Count; i++)
			{
				Game.numRayTests++;

				var intersection = lights[i].mesh.Intersect(ray);

				if (intersection.HasValue)
				{
					float t = intersection.Value;

					if (t < distance)
					{
						distance = t;
						indexOfNearest = i;
						isLight = true;
					}
				}
			}
		}

		private static Vector3 DirectIllumination(int depth, Vector3 color, Vector3 intersection, Ray ray, Vector3 normal, Scene scene, Vector3 backgroundColor) //Indirect
		{
			Vector3 newNormal = normal;
			if (Vector3.Dot(normal, ray.direction) > 0.0f)
				newNormal = -normal;

			var direction = RandomOnHemisphere(newNormal);
			Ray newRay = new Ray(intersection + direction * Renderer.EPSILON, direction);
			var irradiance = Trace(depth - 1, scene, newRay, backgroundColor) * Vector3.Dot(newNormal, direction);

			return 2.0f * color * irradiance;
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
