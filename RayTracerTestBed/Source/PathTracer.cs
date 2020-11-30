using System;
using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	class PathTracer
	{
		private const float EPSILON = 0.0001f; //TODO: Maybe 0.001?

		public static Vector3 Trace(int depth, Scene scene, Ray ray, Vector3 backgroundColor, Vector2 pixel)
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
					var diffuse = 1.0f - reflection - refraction;

					Vector3 color;

					if (material.texture == Texture.Checkerboard)
						color = material.CheckerboardPattern(ray, distance);
					else
						color = material.color;

					var normal = scene.meshes[index].Normal(intersection);

					var choice = RandomChoice();

					//Console.WriteLine(choice);

					if (choice < diffuse)
						return DirectIllumination(depth, color, intersection, ray, normal, scene, backgroundColor, pixel);
					//else 
					//	return color;
					//return backgroundColor;
				}
			}

			return backgroundColor;
		}

		private static Vector3 DirectIllumination(int depth, Vector3 color, Vector3 intersection, Ray ray, Vector3 normal, Scene scene, Vector3 backgroundColor, Vector2 pixel)
		{
			Vector3 newNormal = normal;
			if (Vector3.Dot(normal, ray.direction) > 0.0f)
				newNormal = -normal;

			var direction = RandomOnHemisphere(newNormal, pixel);
			Ray newRay = new Ray(intersection + direction * EPSILON, direction);
			var irradiance = Trace(depth - 1, scene, newRay, backgroundColor, pixel) * Vector3.Dot(newNormal, direction);

			return 2.0f * color * irradiance;
		}

		private static Vector3 RandomOnHemisphere(Vector3 normal, Vector2 pixel)
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
