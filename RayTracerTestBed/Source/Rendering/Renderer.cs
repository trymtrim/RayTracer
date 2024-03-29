﻿using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace RayTracerTestBed
{
	class Renderer
	{
		public const float EPSILON = 0.0001f;

		public static Surface screen = new Surface();

		public static void Render(RenderSettings settings, Camera camera)
		{
			if (settings.traceMethod == TraceMethod.PhotonTracing)
			{
				if (Config.RENDER_PHOTON_MAP)
				{
					screen.UpdateSurface(PhotonMapping.PhotonMapRender(settings, camera));
					return;
				}
			}

			Bitmap bitmap = new Bitmap(settings.width, settings.height);

			for (int j = 0; j < settings.height; ++j)
			{
				for (int i = 0; i < settings.width; ++i)
				{
					Vector3 colorVector = settings.backgroundColor;

					float vx = i / (float)settings.width;
					float vy = j / (float)settings.height;

					Ray ray = camera.RayThroughScreen(vx, vy);

					switch (settings.traceMethod)
					{
						case TraceMethod.WhittedRayTracing:
							{
								colorVector = RayTracer.Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor);
								break;
							}
						case TraceMethod.PathTracing:
							{
								int sampleCount = Config.PATH_TRACING_SAMPLES;

								//TODO: Unsure if this should be here or if I should start random sampling loop from 0 instead of 1
								colorVector = PathTracer.Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor);

								for (int k = 1; k < sampleCount; k++)
								{
									var offsetXMin = -(0.5f / settings.width);
									var offsetXMax = (0.5f / settings.width);
									var offsetYMin = -(0.5f / settings.height);
									var offsetYMax = (0.5f / settings.height);

									var x = vx + MathHelper.RandomRange(offsetXMin, offsetXMax);
									var y = vy + MathHelper.RandomRange(offsetYMin, offsetYMax);

									ray = camera.RayThroughScreen(x, y);

									colorVector += PathTracer.Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor);
								}

								colorVector /= sampleCount;
								break;
							}
						case TraceMethod.PhotonTracing:
							{
								colorVector = PhotonTracer.Trace(settings.maxDepth, settings.scene, ray, settings.backgroundColor);
								break;
							}
					}

					if (settings.showUI)
					{
						//Render UI
						Vector3? uiColor = RenderUI(settings, i, j);

						if (uiColor.HasValue)
							colorVector += uiColor.Value;
					}

					float red = MathHelper.Clamp(colorVector.X, 0.0f, 1.0f);
					float green = MathHelper.Clamp(colorVector.Y, 0.0f, 1.0f);
					float blue = MathHelper.Clamp(colorVector.Z, 0.0f, 1.0f);

					Color color = Color.FromArgb(255, (int)(red * 255), (int)(green * 255), (int)(blue * 255));
					bitmap.SetPixel(i, j, color);
				}
			}

			screen.UpdateSurface(bitmap);
		}

		private static Vector3? RenderUI(RenderSettings settings, int widthIndex, int HeightIndex)
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

		public static Vector3 Reflect(Vector3 incidentDirection, Vector3 normal)
		{
			return incidentDirection - 2.0f * Vector3.Dot(incidentDirection, normal) * normal;
		}

		public static Vector3 Refract(Vector3 incidentDirection, Vector3 normal, float ior)
		{
			float cosI = MathHelper.Clamp(Vector3.Dot(incidentDirection, normal), -1.0f, 1.0f);
			float etaI = 1.0f, etaT = ior; //etaI is the index of refraction of the medium the ray is in before entering the second medium
			Vector3 n = normal;

			if (cosI < 0.0f)
				cosI = -cosI;
			else
			{
				//Swap the refraction indices
				float etaiBuffer = etaI;
				etaI = etaT;
				etaT = etaiBuffer;

				n = -normal;
			}

			float eta = etaI / etaT;
			float k = 1.0f - eta * eta * (1.0f - cosI * cosI);

			return k < 0.0f ? new Vector3(0.0f) : eta * incidentDirection + (eta * cosI - (float)Math.Sqrt(k)) * n;
		}

		public static float Fresnel(Vector3 incidentDirection, Vector3 normal, float ior)
		{
			float kr;

			float cosI = MathHelper.Clamp(Vector3.Dot(incidentDirection, normal), -1.0f, 1.0f);
			float etaI = 1.0f, etaT = ior; //etaI is the index of refraction of the medium the ray is in before entering the second medium

			if (cosI > 0.0f)
			{
				//Swap the refraction indices
				float etaiBuffer = etaI;
				etaI = etaT;
				etaT = etaiBuffer;
			}

			//Compute sinT using Snell's law
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

			return kr;
		}
	}
}
