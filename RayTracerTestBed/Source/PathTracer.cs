using System;
using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	class PathTracer
	{
		private const float EPSILON = 0.0001f; //TODO: Maybe 0.001?

		public static Vector3 Trace(int depth, Scene scene, Ray ray, Vector3 backgroundColor)
		{
			float emittance = 5.0f; //Temp

			NearestIntersectionIncludingLights(scene.meshes, scene.lights, ray, out float distance, out int? indexOfNearest, out bool isLight);

			if (indexOfNearest.HasValue)
			{
				int index = indexOfNearest.Value;
				var intersection = ray.At(distance);

				if (isLight)
				{
					Light light = scene.lights[index];

					var normal = light.mesh.Normal(intersection);

					if (Vector3.Dot(normal, ray.direction) < 0.0f)
						return light.color * emittance;
					else
						return Vector3.Zero; //Black - From the back - TODO: Should this be background color?
				}
				else
				{
					if (depth <= 1)
						return Vector3.Zero; //Black - TODO: Should this be background color?

					Material material = scene.materials[index];
					var reflection = material.reflection;
					var refraction = material.refraction;
					var ior = material.ior;

					var materialType = material.materialType;

					if (refraction > 0.0f)
					{
						reflection = 0.0f;
						refraction = 1.0f;
					}

					var diffuse = 1.0f - reflection - refraction;

					Vector3 color;

					if (material.texture == Texture.Checkerboard)
						color = material.CheckerboardPattern(ray, distance);
					else
						color = material.color;

					var normal = scene.meshes[index].Normal(intersection);

					var choice = RandomChoice(); //TODO: Unsure if this is needed - maybe just use the MaterialType enum

					if (materialType == MaterialType.Diffuse)
					{
						return DirectIllumination(depth, color, intersection, ray, normal, scene, backgroundColor);
					}
					else if (materialType == MaterialType.Reflection)
					{
						bool outside = Vector3.Dot(ray.direction, normal) < 0.0f;
						Vector3 bias = EPSILON * normal;

						Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
						Vector3 reflectionDirection = Renderer.Reflect(ray.direction, normal).Normalized();
						Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

						Vector3 reflectionColor = Trace(depth - 1, scene, reflectionRay, backgroundColor);
						Vector3 result = reflection * reflectionColor;

						return color * result;
					}
					else if (materialType == MaterialType.Reflection_Refraction)
					{
						bool outside = Vector3.Dot(ray.direction, normal) < 0.0f;
						Vector3 bias = EPSILON * normal;

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
						Vector3 result = (reflectionColor * kr + refractionColor * (1.0f - kr)) * reflection;

						return color * result;
					}
				}
			}

			return backgroundColor;
		}

		private static Vector3 DirectIllumination(int depth, Vector3 color, Vector3 intersection, Ray ray, Vector3 normal, Scene scene, Vector3 backgroundColor)
		{
			Vector3 newNormal = normal;
			if (Vector3.Dot(normal, ray.direction) > 0.0f)
				newNormal = -normal;

			var direction = RandomOnHemisphere(newNormal);
			Ray newRay = new Ray(intersection + direction * EPSILON, direction);
			var irradiance = Trace(depth - 1, scene, newRay, backgroundColor) * Vector3.Dot(newNormal, direction);

			return 2.0f * color * irradiance;
		}

		private static Vector3 Reflect(int depth, Vector3 color, Vector3 intersection, Ray ray, Vector3 normal, Scene scene, Vector3 backgroundColor)
		{
			var direction = ray.direction - 2.0f * Vector3.Dot(ray.direction, normal) * normal;
			var newRay = new Ray(intersection + direction * EPSILON, direction);

			return color * Trace(depth - 1, scene, ray, backgroundColor);
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

		private static float RandomChoice()
		{
			return MathHelper.RandomRange(0.0f, 1.0f);
		}

		private static void NearestIntersectionIncludingLights(List<Mesh> meshes, List<Light> lights, Ray ray, out float distance, out int? indexOfNearest, out bool isLight)
		{
			distance = float.MaxValue;
			indexOfNearest = null;
			isLight = false;

			for (int i = 0; i < meshes.Count; i++)
			{
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
				//TODO: This might be worng - we wight want to check interaction by calculating distance instead of using mesh.Intersect

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
	}
}
