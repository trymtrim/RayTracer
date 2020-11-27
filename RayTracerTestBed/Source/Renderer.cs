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
		public static Surface screen = new Surface();

		private const float EPSILON = 0.0001f;

		public static void Render(Settings settings, Camera camera)
		{
			Bitmap bitmap = new Bitmap(settings.width, settings.height);

			float scale = (float)Math.Tan(MathHelper.DegreesToRadians(camera.fov * 0.5f)); //Test
			float imageAspectRatio = (float)settings.width / settings.height; //Test

			Ray ray = new Ray();

			//Random randomGenerator = new Random();
			//float random = (float)randomGenerator.NextDouble();
			//var offset = 

			for (int j = 0; j < settings.height; ++j)
			{
				for (int i = 0; i < settings.width; ++i)
				{
					//if (_settings.antiAliasing <= 1)
					//{
					float x = (2.0f * (i + 0.5f) / settings.width - 1.0f) * imageAspectRatio * scale;
					float y = (1.0f - 2.0f * (j + 0.5f) / settings.height) * scale;

					//Reuse ray
					ray.origin = camera.origin;
					ray.direction = new Vector3(x, -y, 1.0f);

					//Console.WriteLine(ray.direction);

					var colorVector = Renderer.Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor);

					//TODO: Do gamma correction?

					float red = MathHelper.Clamp(colorVector.X, 0.0f, 1.0f);
					float green = MathHelper.Clamp(colorVector.Y, 0.0f, 1.0f);
					float blue = MathHelper.Clamp(colorVector.Z, 0.0f, 1.0f);

					Color color = Color.FromArgb(255, (int)(red * 255), (int)(green * 255), (int)(blue * 255));
					bitmap.SetPixel(i, j, color);
					//}
					//else
					//{
					//TODO: Implement anti-aliasing

					//var colorVector = new Vector3(0.0f, 0.0f, 0.0f);

					//for (int k = 0; k < _settings.antiAliasing; i++)
					//{
					//	float x = ((float)randomGenerator.NextDouble() + (2.0f * (i + 0.5f)) / _settings.width - 1.0f) * imageAspectRatio * scale; //Test
					//	float y = ((float)randomGenerator.NextDouble() + (1.0f - 2.0f * (j + 0.5f)) / _settings.height) * scale; //Test

					//	//Reuse ray
					//	ray.origin = camera.origin;
					//	ray.direction = new Vector3(x, -y, 1.0f);

					//	colorVector += Renderer.Trace(_settings.maxDepth, _settings.scene, ray, _settings.backgroundColor);
					//}

					//float red = MathHelper.Clamp(colorVector.X, 0.0f, 1.0f);
					//float green = MathHelper.Clamp(colorVector.Y, 0.0f, 1.0f);
					//float blue = MathHelper.Clamp(colorVector.Z, 0.0f, 1.0f);

					//Color color = Color.FromArgb(255, (int)(red * 255), (int)(green * 255), (int)(blue * 255));
					//bitmap.SetPixel(i, j, color);
					//}
				}
			}

			screen.UpdateSurface(bitmap);
		}

		private static Vector3 Trace(int depth, Scene scene, Ray ray, Vector3 backgroundColor)
		{ 
			NearestIntersection(scene.meshes, ray, out float distance, out int? indexOfNearest);

			if (indexOfNearest.HasValue)
			{
				int index = indexOfNearest.Value;

				var material = scene.materials[index];
				Vector3 color = Vector3.Zero;
				
				if (material.checkerboard)
					color = material.GetCheckerboard(ray, distance);
				else
					color = material.color;

				var intersection = ray.At(distance);
				var normal = scene.meshes[index].Normal(intersection);

				var result = Vector3.Zero; //Black
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
			Vector3 color = Vector3.Zero; //Black

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
