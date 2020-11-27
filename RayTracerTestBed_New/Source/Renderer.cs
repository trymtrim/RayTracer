using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	class Renderer
	{
		public static Surface screen = new Surface(); //TODO: Consider removing/changing position of this

		private const float EPSILON = 0.0001f;

		public static Vector3 Trace(int depth, Scene scene, Ray ray, Vector3 backgroundColor)
		{ 
			NearestIntersection(scene.meshes, ray, out float distance, out int? indexOfNearest);

			//TODO: This fails

			if (indexOfNearest.HasValue)
			{
				//Console.WriteLine("YO");
				int index = indexOfNearest.Value;

				var material = scene.materials[index];
				Vector3 color = Vector3.Zero;
				
				if (material.checkerboard)
					color = material.GetCheckerboard(ray, distance);
				else
					color = material.color;

				var intersection = ray.At(distance); //REMINDER: This might be wrong
				var normal = scene.meshes[index].Normal(intersection);

				var result = new Vector3(0.0f, 0.0f, 0.0f); //Black
				var specular = material.specularity;
				var diffuse = 1.0f - specular;
				
				if (diffuse > 0.0f)
					result += diffuse * DirectIllumination(scene, ray.At(distance), normal);

				if (specular > 0.0f && depth > 1)
				{
					var direction = ray.direction - 2.0f * Vector3.Dot(ray.direction, normal) * normal;
					ray = new Ray(intersection + direction * EPSILON, direction); //REMINDER: This might be wrong - want want to initialize a new variable
					result += specular * Trace(depth - 1, scene, ray, backgroundColor);
				}

				return color * result;
			}
			else
				return backgroundColor;
		}

		private static void NearestIntersection(List<Mesh> meshes, Ray ray, out float distance, out int? indexOfNearest)
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

		private static Vector3 DirectIllumination(Scene scene, Vector3 point, Vector3 normal)
		{
			Vector3 color = new Vector3(0.0f, 0.0f, 0.0f); //Black

			foreach (var light in scene.lights)
			{
				if (light.direction.HasValue)
				{
					var dir = light.direction.Value; //REMINDER: This might be wrong

					var ray = new Ray(point - dir * EPSILON, -dir);

					var distance = light.mesh.Intersect(ray);

					if (distance.HasValue)
					{
						float outDistance;
						int? outIndexOfNearest;

						NearestIntersection(scene.meshes, ray, out outDistance, out outIndexOfNearest);

						if (outDistance >= distance - EPSILON)
						{
							var directionFactor = Vector3.Dot(-dir, normal); //Photon smearing
							color += light.color * directionFactor;
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

					float outDistance;
					int? outIndexOfNearest;

					NearestIntersection(scene.meshes, ray, out outDistance, out outIndexOfNearest);

					if (outDistance >= distance - EPSILON)
					{
						var directionFactor = Vector3.Dot(-pathNormalized, normal); //Photon smearing
						var distanceDiv = (float)Math.Pow(distance, 2.0f);

						color += light.color * directionFactor / distanceDiv;
					}
				}
			}

			return color;
		}
	}
}
