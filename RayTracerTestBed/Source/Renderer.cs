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

			//float scale = (float)Math.Tan(MathHelper.DegreesToRadians(camera.fov * 0.5f));
			//float imageAspectRatio = (float)settings.width / settings.height;

			Ray ray = new Ray();

			//Console.WriteLine("scale: " + scale);
			//Console.WriteLine("ratio: " + imageAspectRatio);

			//Random randomGenerator = new Random();
			//float random = (float)randomGenerator.NextDouble();
			//var offset = 

			for (int j = 0; j < settings.height; ++j)
			{
				for (int i = 0; i < settings.width; ++i)
				{
					float vx = i / (float)settings.width;
					float vy = j / (float)settings.height;

					ray = camera.RayThroughScreen(vx, vy);
					Vector3 colorVector = Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor);

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

				var refraction = 0.0f;

				var result = Vector3.Zero; //Black
				var specular = material.Specularity();

				specular = 0.98f;

				var diffuse = 1.0f - specular - refraction;

				//Diffuse
				if (diffuse > 0.0f)
					result += diffuse * DirectIllumination(scene, ray.At(distance), normal);

				float ior = material.IndexOfRefraction();

				if (material.texture != Texture.Checkerboard)
				{
					//ior = 1.3f;
				}
				else
					return color * result;

				//Handle transparancy

				if (depth > 1)
				{
					//Reflection
					if (specular > 0.0f)
					{
						//Refraction//
						Vector3 refractionColor = new Vector3(0.0f);
						float kr = 1.0f;
						bool outside = true;
						Vector3 bias = EPSILON * normal;

						if (ior > 0.0f)
						{
							//ior = 1.0f; //If true transparancy
							ior = 1.5f;
							kr = Fresnel(ray.direction, normal, ior);

							outside = Vector3.Dot(ray.direction, normal) < 0.0f;

							//Compute refraction if it is not a case of total internal reflection
							if (kr < 1.0f)
							{
								Vector3 refractionRayOrigin = outside ? intersection - bias : intersection + bias;
								Vector3 refractionDirection = Refract(ray.direction, normal, ior).Normalized();
								Ray refractionRay = new Ray(refractionRayOrigin, refractionDirection);
								refractionColor = Trace(depth - 1, scene, refractionRay, backgroundColor);
							}
						}
						//Refraction end//

						Vector3 reflectionDirection = Reflect(ray.direction, normal).Normalized();
						Vector3 reflectionRayOrigin = outside ? intersection + bias : intersection - bias;
						Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

						//TODO: Consider making a variable for transparancy - if there should be no color on the object, return reflectionColor instead of adding it to result;
						//Console.WriteLine(kr);
						Vector3 reflectionColor = Trace(depth - 1, scene, reflectionRay, backgroundColor);
						result += specular * reflectionColor; //Reflection only (color * result)
						return color * result;

						//TODO: Specularity value is currently not taken into consideration
						//result = (reflectionColor * kr + refractionColor * (1.0f - kr)) * 0.2f + 0.8f * color; //Refraction and reflection // 1.1 = Looking glass // 1.01f = Cool almost-transparent glass effect // 1.3, 1.5 = water, glass
						//result += refraction * refractionColor; //Refraction only
						//result *= color;
						//result = Vector3.One * kr + color * result * (1.0f - kr); //Outline: ior = 0.95f
						//NOT SURE ABOUT THIS ONE //result = Vector3.Zero * kr + color * result * (1.0f - kr); //Fresnel: ior = 1.0f+ - adding color(result)/darkness(Vector.Zero) around edges (REMINDER: isn't zero * kr always 0?)

						//Transparancy
						//result = (color * result * 0.4f + refractionColor * 0.6f); //ior = 1.0f

						//kr = Fresnel(ray.direction, normal, 1.01f);
						//result = (color * result * 0.4f + refractionColor * 0.6f) * (1.0f - kr) + (color * result * 0.4f + refractionColor * 0.6f) * refractionColor * kr; 

						return result;
					}
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

		private static Vector3 Reflect(Vector3 incidentDirection, Vector3 normal)
		{
			return incidentDirection - 2.0f * Vector3.Dot(incidentDirection, normal) * normal;
		}

		private static Vector3 Refract(Vector3 incidentDirection, Vector3 normal, float ior)
		{
			float cosi = MathHelper.Clamp(Vector3.Dot(incidentDirection, normal), -1.0f, 1.0f);
			float etai = 1.0f, etat = ior; //etai is the index of refraction of the medium the ray is in before entering the second medium
			Vector3 n = normal;

			if (cosi < 0.0f)
				cosi = -cosi;
			else
			{
				//Swap the refraction indices
				float etaiBuffer = etai;
				etai = etat;
				etat = etaiBuffer;

				n = -normal;
			}

			float eta = etai / etat;
			float k = 1.0f - eta * eta * (1.0f - cosi * cosi);

			return k < 0.0f ? new Vector3(0.0f) : eta * incidentDirection + (eta * cosi - (float)Math.Sqrt(k)) * n;
		}

		private static float Fresnel(Vector3 incidentDirection, Vector3 normal, float ior)
		{
			float kr;

			float cosI = MathHelper.Clamp(Vector3.Dot(incidentDirection, normal), -1.0f, 1.0f);
			float etaI = 1.0f, etaT = ior; //etai is the index of refraction of the medium the ray is in before entering the second medium

			if (cosI > 0.0f)
			{
				//Swap the refraction indices
				float etaiBuffer = etaI;
				etaI = etaT;
				etaT = etaiBuffer;
			}

			//Compute sini using Snell's law
			float sinT = etaI / etaT * (float)Math.Sqrt(Math.Max(0.0f, 1.0f - cosI * cosI));

			//Total internal reflection
			if (sinT >= 1.0f)
				kr = 1.0f;
			else
			{
				float cosT = (float)Math.Sqrt(Math.Max(0.0f, 1.0f - sinT * sinT));
				cosI = Math.Abs(cosI);
				float rs = ((etaT * cosI) - (etaI * cosT)) / ((etaT * cosI) + (etaI * cosT));
				float rp = ((etaI * cosI) - (etaT * cosT)) / ((etaI * cosI) + (etaT * cosT));
				kr = (rs * rs + rp * rp) / 2.0f;
			}

			//As a consequence of the conservation of energy, transmittance is given by: kt = 1 - kr;

			return kr;
		}
	}
}
