﻿using System;
using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	class RayTracer
	{
		public static Vector3 Trace(int depth, Scene scene, Ray ray, Vector3 backgroundColor)
		{
			if (depth == Game.settings.maxDepth)
				Game.numPrimaryRays++;

			float distance = 0.0f;
			int? indexOfNearest = null;

			if (Config.USE_BVH)
			{
				List<int> meshIndices = scene.bvh.Traverse(ray);

				if (meshIndices.Count > 0)
				{
					Game.numRayTests += meshIndices.Count;

					NearestIntersection(scene, meshIndices, ray, out distance, out indexOfNearest);

					if (indexOfNearest.HasValue)
						indexOfNearest = meshIndices[indexOfNearest.Value];
				}
			}
			else
			{
				Game.numRayTests += scene.meshes.Count;

				NearestIntersection(scene.meshes, ray, out distance, out indexOfNearest);
			}

			if (indexOfNearest.HasValue)
			{
				Game.numRayIntersections++;

				int index = indexOfNearest.Value;
				Material material = scene.materials[index];
				Mesh mesh = scene.meshes[index];

				var intersection = ray.At(distance);
				var normal = mesh.Normal(intersection);

				Vector3 color = material.Color(mesh, ray, distance, intersection);

				var reflection = material.reflection;
				var refraction = material.refraction;
				var ior = material.ior;
				var transparency = material.transparency;

				var diffuse = 1.0f - reflection - refraction;

				var result = Vector3.Zero;

				//Diffuse
				if (diffuse > 0.0f)
					result += diffuse * DirectIllumination(scene, intersection, normal);

				if (mesh.isSkyboxMesh)
					return material.Color(mesh, ray, distance, intersection);

				if (depth > 1)
				{
					bool outside = Vector3.Dot(ray.direction, normal) < 0.0f;
					Vector3 bias = Renderer.EPSILON * normal;

					switch (material.materialType)
					{
						case MaterialType.Diffuse:
							{
								result = color * result;
								break;
							}
						case MaterialType.Reflection:
							{
								Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
								Vector3 reflectionDirection = Renderer.Reflect(ray.direction, normal).Normalized();
								Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

								Vector3 reflectionColor = Trace(depth - 1, scene, reflectionRay, backgroundColor);
								result += reflection * reflectionColor;

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
									Vector3 refractionRayOrigin = outside ? intersection - bias : intersection + bias;
									Vector3 refractionDirection = Renderer.Refract(ray.direction, normal, ior).Normalized();
									Ray refractionRay = new Ray(refractionRayOrigin, refractionDirection);

									refractionColor = Trace(depth - 1, scene, refractionRay, backgroundColor);
								}

								Vector3 reflectionDirection = Renderer.Reflect(ray.direction, normal).Normalized();
								Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
								Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

								Vector3 reflectionColor = Trace(depth - 1, scene, reflectionRay, backgroundColor);
								result += (reflectionColor * kr + refractionColor * (1.0f - kr)) * reflection;

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
				}

				if (material.selected)
				{
					//Outline
					float outlineSize = 0.01f;

					switch (mesh.shape)
					{
						case Shape.Plane:
							{
								if (mesh.TextureCoords(intersection).X < outlineSize || mesh.TextureCoords(intersection).X > 1.0f - outlineSize ||
									mesh.TextureCoords(intersection).Y < outlineSize || mesh.TextureCoords(intersection).Y > 1.0f - outlineSize)
									result = Vector3.One;

								break;
							}
						case Shape.Sphere:
							{
								float kr = Renderer.Fresnel(ray.direction, normal, 0.95f);
								result = Vector3.One * kr + result * (1.0f - kr);

								break;
							}
					}
				}

				return result;
			}

			return backgroundColor;
		}

		public static void NearestIntersection(List<Mesh> meshes, Ray ray, out float distance, out int? indexOfNearest)
		{
			distance = float.MaxValue;
			indexOfNearest = null;

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
					}
				}
			}
		}

		public static void NearestIntersection(Scene scene, List<int> meshIndices, Ray ray, out float distance, out int? indexOfNearest)
		{
			distance = float.MaxValue;
			indexOfNearest = null;

			for (int i = 0; i < meshIndices.Count; i++)
			{
				var intersection = scene.meshes[meshIndices[i]].Intersect(ray);

				if (intersection.HasValue)
				{
					float t = intersection.Value;

					if (t < distance)
					{
						distance = t;
						indexOfNearest = i;
					}
				}
			}
		}

		private static Vector3 DirectIllumination(Scene scene, Vector3 point, Vector3 normal)
		{
			Vector3 color = Vector3.Zero;
			float brightness = 1.0f;

			foreach (var light in scene.lights)
			{
				if (light.direction.HasValue)
				{
					var dir = light.direction.Value;

					var ray = new Ray(point - dir * Renderer.EPSILON, -dir);

					var distance = light.mesh.Intersect(ray);

					if (distance.HasValue)
					{
						float outDistance = 0.0f;
						int? outIndexOfNearest;

						if (Config.USE_BVH)
						{
							List<int> meshIndices = scene.bvh.Traverse(ray, false);

							if (meshIndices.Count > 0)
							{
								Game.numRayTests += meshIndices.Count;

								NearestIntersection(scene, meshIndices, ray, out outDistance, out outIndexOfNearest);

								if (outIndexOfNearest.HasValue)
									outIndexOfNearest = meshIndices[outIndexOfNearest.Value];
							}
						}
						else
						{
							Game.numRayTests += scene.meshes.Count;

							NearestIntersection(scene.meshes, ray, out outDistance, out outIndexOfNearest);
						}

						if (outDistance >= distance - Renderer.EPSILON)
						{
							var directionFactor = Vector3.Dot(-dir, normal); //Photon smearing
							color += light.color * directionFactor;
							brightness = light.brightness;
						}
					}
				}
				else
				{
					var lightCenter = light.mesh.Center();
					var path = point - lightCenter;
					var distance = path.Length;
					var pathNormalized = path / distance;

					var ray = new Ray(lightCenter, pathNormalized);

					float outDistance = 0.0f;
					int? outIndexOfNearest;

					if (Config.USE_BVH)
					{
						List<int> meshIndices = scene.bvh.Traverse(ray, false);

						if (meshIndices.Count > 0)
						{
							Game.numRayTests += meshIndices.Count;

							NearestIntersection(scene, meshIndices, ray, out outDistance, out outIndexOfNearest);

							if (outIndexOfNearest.HasValue)
								outIndexOfNearest = meshIndices[outIndexOfNearest.Value];
						}
					}
					else
					{
						NearestIntersection(scene.meshes, ray, out outDistance, out outIndexOfNearest);
					}

					if (outDistance >= distance - Renderer.EPSILON)
					{
						var directionFactor = Vector3.Dot(-pathNormalized, normal); //Photon smearing
						var distanceDiv = (float)Math.Pow(distance, 2.0f);

						color += light.color * light.mesh.Radius() * directionFactor / distanceDiv;
						brightness = light.brightness;
					}
				}
			}

			return color * brightness;
		}
	}
}
