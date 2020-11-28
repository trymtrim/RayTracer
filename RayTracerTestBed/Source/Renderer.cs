using System;
using System.Collections.Generic;
using System.Drawing;
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

			float scale = (float)Math.Tan(MathHelper.DegreesToRadians(camera.fov * 0.5f));
			float imageAspectRatio = (float)settings.width / settings.height;

			Ray ray = new Ray();

			//Random randomGenerator = new Random();
			//float random = (float)randomGenerator.NextDouble();
			//var offset = 

			for (int j = 0; j < settings.height; ++j)
			{
				for (int i = 0; i < settings.width; ++i)
				{
					float x = (2.0f * (i + 0.5f) / settings.width - 1.0f) * imageAspectRatio * scale;
					float y = (1.0f - 2.0f * (j + 0.5f) / settings.height) * scale;

					//Reuse ray
					ray.origin = camera.origin;
					ray.direction = new Vector3(x, -y, 1.0f);

					var colorVector = Renderer.Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor);

					//Render UI
					Vector3? uiColor = RenderUI(settings, i, j);

					if (uiColor.HasValue)
						colorVector += uiColor.Value;

					//TODO: Do gamma correction?

					float red = MathHelper.Clamp(colorVector.X, 0.0f, 1.0f);
					float green = MathHelper.Clamp(colorVector.Y, 0.0f, 1.0f);
					float blue = MathHelper.Clamp(colorVector.Z, 0.0f, 1.0f);

					Color color = Color.FromArgb(255, (int)(red * 255), (int)(green * 255), (int)(blue * 255));
					bitmap.SetPixel(i, j, color);
				}
			}

			screen.UpdateSurface(bitmap);
		}

		private static Vector3? RenderUI(Settings settings, int widthIndex, int HeightIndex)
		{
			for (int i = 0; i < settings.ui.buttons.Count; i++)
			{
				Button button = settings.ui.buttons[i];
				float transparency = button.transparency;

				if (button.IsAtPosition(new Vector2(widthIndex, HeightIndex)))
					return button.color * (1.0f - transparency);
			}

			return null;
		}

		private static Vector3 Trace(int depth, Scene scene, Ray ray, Vector3 backgroundColor)
		{
			NearestIntersection(scene.meshes, ray, out float distance, out int? indexOfNearest);

			if (indexOfNearest.HasValue)
			{
				int index = indexOfNearest.Value;
				var material = scene.materials[index];
				Vector3 color = Vector3.Zero;

				if (material.texture == Texture.Checkerboard)
					color = material.CheckerboardPattern(ray, distance);
				else
					color = material.Color();

				var intersection = ray.At(distance);
				var normal = scene.meshes[index].Normal(intersection);

				var result = Vector3.Zero; //Black
				var specular = material.Specularity();
				var diffuse = 1.0f - specular;

				//Diffuse
				if (diffuse > 0.0f)
					result += diffuse * DirectIllumination(scene, ray.At(distance), normal);

				float ior = material.IndexOfRefraction();

				//Reflection
				if (specular > 0.0f && depth > 1)
				{
					var direction = ray.direction - 2.0f * Vector3.Dot(ray.direction, normal) * normal;
					Ray reflectionRay = new Ray(intersection + direction * EPSILON, direction); //REMINDER: This might be wrong - might want to initialize a new variable
					Vector3 reflectionColor = specular * Trace(depth - 1, scene, reflectionRay, backgroundColor);

					result += reflectionColor;

					//Reflection and refraction
					if (ior > 0.0f)
					{
						Vector3 refractionColor = Refract(depth - 1, scene, ray, backgroundColor, intersection, normal, ior);

						return reflectionColor + refractionColor * (1.0f - specular);
					}
				}
				else if (ior > 0.0f && depth > 1) //Refraction
				{
					//TODO: Implement this

					//Vector3 refractionColor = Refract(depth - 1, scene, ray, backgroundColor, intersection, normal, ior);
					//return color + refractionColor * 0.1f;
				}

				return color * result;
			}
			else
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

						color += light.color * light.mesh.Radius() * directionFactor / distanceDiv;
					}
				}
			}

			return color;
		}

		private static Vector3 Refract(int depth, Scene scene, Ray ray, Vector3 backgroundColor, Vector3 intersection, Vector3 normal, float ior)
		{
			Vector3 rayDirection = ray.direction;

			//Refraction
			float cosi = MathHelper.Clamp(Vector3.Dot(rayDirection, normal), -1.0f, 1.0f);
			float etai = 1.0f, etat = ior;
			Vector3 n_new = normal;

			if (cosi < 0.0f)
				cosi = -cosi;
			else
			{
				float buffer = etai;
				etai = etat;
				etat = buffer;

				n_new = -normal; //This might be wrong?
			}

			float eta = etai / etat;
			float k = 1.0f - eta * eta * (1.0f - cosi * cosi);

			Vector3 refractionDirection = k < 0.0f ? new Vector3(0.0f) : rayDirection * eta + n_new * (eta * cosi - (float)Math.Sqrt(k));
			refractionDirection = refractionDirection.Normalized();
			Vector3 refractionRayOrig = (Vector3.Dot(refractionDirection, normal) < 0.0f) ? intersection - normal * EPSILON : intersection + normal * EPSILON;

			Ray refractionRay = new Ray(refractionRayOrig, refractionDirection); //, objects, lights, depth + 1, settings);

			return Trace(depth, scene, refractionRay, backgroundColor);
		}
	}
}
