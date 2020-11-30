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
			MathHelper.ResetSeed(); //Temp

			Bitmap bitmap = new Bitmap(settings.width, settings.height);

			Ray ray = new Ray();

			for (int j = 0; j < settings.height; ++j)
			{
				for (int i = 0; i < settings.width; ++i)
				{
					Vector3 colorVector;

					float vx = i / (float)settings.width;
					float vy = j / (float)settings.height;

					ray = camera.RayThroughScreen(vx, vy);

					if (settings.traceMethod == TraceMethod.WhittedRayTracing)
						colorVector = Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor); //TODO: Move RayTracing methods to separate class
					else
					{
						int samplePoints = 100;

						colorVector = PathTracer.Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor, new Vector2(vx, vy));

						for (int k = 1; k < samplePoints; k++)
						{
							var offsetXMin = -(0.5f / settings.width);
							var offsetXMax = (0.5f / settings.width);
							var offsetYMin = -(0.5f / settings.height);
							var offsetYMax = (0.5f / settings.height);

							var x = vx + MathHelper.RandomRange(offsetXMin, offsetXMax);
							var y = vy + MathHelper.RandomRange(offsetYMin, offsetYMax);

							ray = camera.RayThroughScreen(x, y);

							colorVector += PathTracer.Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor, new Vector2(x, y));
						}

						colorVector /= samplePoints;
					}

					if (settings.showUI)
					{
						//Render UI
						Vector3? uiColor = RenderUI(settings, i, j);

						if (uiColor.HasValue)
							colorVector += uiColor.Value;
					}

					//TODO: Do gamma correction? Apply post processing?

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

				Vector3 color;

				if (material.texture == Texture.Checkerboard)
					color = material.CheckerboardPattern(ray, distance);
				else
					color = material.color;

				var intersection = ray.At(distance);
				var normal = scene.meshes[index].Normal(intersection);

				var result = Vector3.Zero; //Black

				var reflection = material.reflection;
				var refraction = material.refraction;
				var ior = material.ior;
				var transparency = material.transparency;

				var diffuse = 1.0f - reflection - refraction;

				//Diffuse
				if (diffuse > 0.0f)
					result += diffuse * DirectIllumination(scene, intersection, normal);

				//TODO: NOT SURE ABOUT THIS ONE //result = Vector3.Zero * kr + color * result * (1.0f - kr); //Fresnel: ior = 1.0f+ - adding color(result)/darkness(Vector.Zero) around edges

				if (depth > 1)
				{
					bool outside = Vector3.Dot(ray.direction, normal) < 0.0f;
					Vector3 bias = EPSILON * normal;

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
								Vector3 reflectionDirection = Reflect(ray.direction, normal).Normalized();
								Ray reflectionRay = new Ray(reflectionRayOrigin, reflectionDirection);

								Vector3 reflectionColor = Trace(depth - 1, scene, reflectionRay, backgroundColor);
								result += reflection * reflectionColor;

								result = color * result;
								break;
							}
						case MaterialType.Refraction:
							{
								float kr = Fresnel(ray.direction, normal, ior);

								if (kr < 1.0f) //TODO: Not sure if this is needed anymore
								{
									Vector3 refractionRayOrigin = outside ? intersection - bias : intersection + bias;
									Vector3 refractionDirection = Refract(ray.direction, normal, ior).Normalized();
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

								float kr = Fresnel(ray.direction, normal, ior);

								//Compute refraction if it is not a case of total internal reflection
								if (kr < 1.0f)
								{
									Vector3 refractionDirection = Refract(ray.direction, normal, ior).Normalized();
									Vector3 refractionRayOrigin = outside ? intersection - bias : intersection + bias;
									Ray refractionRay = new Ray(refractionRayOrigin, refractionDirection);

									refractionColor = Trace(depth - 1, scene, refractionRay, backgroundColor);
								}

								Vector3 reflectionDirection = Reflect(ray.direction, normal).Normalized();
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
					float kr = Fresnel(ray.direction, normal, 0.95f);
					result = Vector3.One * kr + result * (1.0f - kr);
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
