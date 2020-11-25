﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	//TODO: Decide between doing it this way or actually consider doing it the initial way

	class Renderer
	{
		public static Surface screen = new Surface(); //TODO: Consider removing/changing this

		private List<Mesh> _objects; //scene...
		private List<Light> _lights;

		//Set the OpenGL texture that serves as the render target
		public void SetTarget(/*GLTexture target*/)
		{
			//Not fully sure about this one
			//Could also still (maybe temp) go with Surface/Bitmap solution in the Surface class
		}

		//Set the geometry data 
		public void SetGeometry()
		{
			//TODO: Currently hardcoding scene here - make separate scene class
			_objects = new List<Mesh>();
			_lights = new List<Light>();

			Sphere sph1 = new Sphere(new Vector3(-1.0f, 0.0f, -12.0f), 2.0f);
			sph1.materialtype = MaterialType.DIFFUSE_AND_GLOSSY; //MaterialType.REFLECTION;
			sph1.diffuseColor = new Vector3(0.6f, 0.7f, 0.8f);

			Sphere sph2 = new Sphere(new Vector3(0.5f, -0.5f, -8.0f), 1.5f);
			sph2.materialtype = MaterialType.REFLECTION_AND_REFRACTION; //MaterialType.REFLECTION_AND_REFRACTION;
			sph2.diffuseColor = new Vector3(0.0f, 0.0f, 1.0f);
			sph2.ior = 1.5f;

			_objects.Add(sph2);
			_objects.Add(sph1);

			Vector3[] verts = { new Vector3(-5.0f, -3.0f, -6.0f), new Vector3(5.0f, -3.0f, -6.0f), new Vector3(5.0f, -3.0f, -16.0f), new Vector3(-5.0f, -3.0f, -16.0f) };
			int[] vertIndex = { 0, 1, 3, 1, 2, 3 };
			Vector2f[] st = { new Vector2f(0.0f, 0.0f), new Vector2f(1.0f, 0.0f), new Vector2f(1.0f, 1.0f), new Vector2f(0.0f, 1.0f) };
			Polygon mesh = new Polygon(verts, vertIndex, 2, st);
			mesh.materialtype = MaterialType.DIFFUSE_AND_GLOSSY; //MaterialType.DIFFUSE_AND_GLOSSY;

			_objects.Add(mesh);

			_lights.Add(new Light(new Vector3(-20.0f, 70.0f, 20.0f), new Vector3(0.5f)));
			_lights.Add(new Light(new Vector3(30.0f, 50.0f, -12.0f), new Vector3(1.0f)));
		}

		//Produce the image to be rendered on the screen
		public void Render(Settings settings/*, ViewPyramid view*/)
		{
			int screenHeight = 480;
			int screenWidth = 640;

			//TODO: Consider moving some of this out of this method - probably not neccessary though
			Vector3[] frameBuffer = new Vector3[screenHeight * screenWidth];
			//Color[,] bitmapData = new Color[screenWidth, screenHeight];
			Bitmap bitmap = new Bitmap(screenWidth, screenHeight);

			float scale = (float)Math.Tan(MathHelper.DegreesToRadians(settings.fov * 0.5f));
			float imageAspectRatio = (float)settings.width / settings.height;

			Ray ray = new Ray();

			for (int j = 0; j < screenHeight; ++j)
			{
				for (int i = 0; i < screenWidth; ++i)
				{
					//Generate primary ray direction
					float x = (2.0f * (i + 0.5f) / settings.width - 1.0f) * imageAspectRatio * scale;
					float y = (1.0f - 2.0f * (j + 0.5f) / settings.height) * scale;

					//TODO: Origin and direction should probably be based on camera
					ray.origin.X = 0.0f;
					ray.origin.Y = 0.0f;
					ray.origin.Z = 0.0f;
					ray.direction.X = x;
					ray.direction.Y = y;
					ray.direction.Z = -1.0f;

					Vector3 colorVector = CastRay(ray, _objects, _lights, 0, settings);

					float red = MathHelper.Clamp(colorVector.X, 0.0f, 1.0f);
					float green = MathHelper.Clamp(colorVector.Y, 0.0f, 1.0f);
					float blue = MathHelper.Clamp(colorVector.Z, 0.0f, 1.0f);

					//bitmapData[i, j] = Color.FromArgb(255, (int)(red * 255), (int)(green * 255), (int)(blue * 255));
					Color color = Color.FromArgb(255, (int)(red * 255), (int)(green * 255), (int)(blue * 255));
					bitmap.SetPixel(i, j, color);
				}
			}

			screen.UpdateSurface(bitmap);
		}

		private Vector3 CastRay(Ray ray, List<Mesh> objects, List<Light> lights, int depth, Settings settings)
		{
			if (depth > settings.maxDepth)
				return settings.backgroundColor;

			Vector3 hitColor = settings.backgroundColor;

			float tNear = float.MaxValue;
			int index = 0;
			Vector2f uv = null;
			Mesh hitObject = null;

			if (Trace(ray, objects, out tNear, out index, out uv, out hitObject))
			{
				Vector3 hitPoint = ray.origin + ray.direction * tNear;

				Vector3 n; //Normal
				Vector2f st; //St coordinates

				hitObject.GetSurfaceProperties(hitPoint, ray.direction, index, uv, out n, out st);

				hitColor = GetObjectSurfaceColor(ray, hitObject, hitPoint, objects, lights, n, st, depth, index, uv, settings);
			}

			return hitColor;
		}

		private bool Trace(Ray ray, List<Mesh> objects, out float tNear, out int index, out Vector2f uv, out Mesh hitObject)
		{
			//Deafult out values
			tNear = float.MaxValue;
			index = -1;
			uv = new Vector2f(0.0f);
			hitObject = null;

			for (int i = 0; i < objects.Count; i++)
			{
				float tNearK = float.MaxValue;

				int indexK;
				Vector2f uvK;

				if (objects[i].Intersect(ray, out tNearK, out indexK, out uvK)) //TODO: This is slow
				{
					if (tNearK < tNear)
					{
						hitObject = objects[i];
						tNear = tNearK;
						index = indexK;
						uv = uvK;
					}
				}
			}

			return hitObject != null;
		}

		//This is called from the meshes
		public static bool RayTriangleIntersect(Vector3 v1, Vector3 v2, Vector3 v3, Ray ray, out float tNear, out float u, out float v)
		{
			//Deafult out values
			tNear = 0.0f;
			u = 0.0f;
			v = 0.0f;

			// WITH CULLING (Not on spheres) //

			float epsilon = 0.00001f;

			Vector3 v0v1 = v2 - v1;
			Vector3 v0v2 = v3 - v1;
			Vector3 pVec = Vector3.Cross(ray.direction, v0v2);
			float det = Vector3.Dot(v0v1, pVec);

			if (det < epsilon)
				return false;

			if (Math.Abs(det) < epsilon)
				return false;

			float invDet = 1.0f / det;

			Vector3 tVec = ray.origin - v1;
			u = Vector3.Dot(tVec, pVec) * invDet;

			if (u < 0.0f || u > 1.0f)
				return false;

			Vector3 qVec = Vector3.Cross(tVec, v0v1);
			v = Vector3.Dot(ray.direction, qVec) * invDet;

			if (v < 0.0f || u + v > 1.0f)
				return false;

			tNear = Vector3.Dot(v0v2, qVec) * invDet;

			// WITHOUT CULLING //

			//Vector3 edge1 = v2 - v1;
			//Vector3 edge2 = v3 - v1;
			//Vector3 pVec = Vector3.Cross(ray.direction, edge2);

			//float det = Vector3.Dot(edge1, pVec);
			//if (det == 0.0f || det < 0.0f)
			//	return false;

			//Vector3 tVec = ray.origin - v1;
			//u = Vector3.Dot (tVec, pVec);
			//if (u < 0.0f || u > det)
			//	return false;

			//Vector3 qVec = Vector3.Cross(tVec, edge1);
			//v = Vector3.Dot(ray.direction, qVec);
			//if (v < 0.0f || u + v > det)
			//	return false;

			//float invDet = 1.0f / det;

			//tNear = Vector3.Dot(edge2, qVec) * invDet;
			//u *= invDet;
			//v *= invDet;

			return true;
		}

		private Vector3 GetObjectSurfaceColor(Ray ray, Mesh hitObject, Vector3 hitPoint, List<Mesh> objects, List<Light> lights, Vector3 n, Vector2f st, int depth, int index, Vector2f uv, Settings settings)
		{
			switch (hitObject.materialtype)
			{
				case MaterialType.REFLECTION_AND_REFRACTION:
					{
						Vector3 reflectionDirection = Vector3.Normalize(Reflect(ray.direction, n));
						Vector3 refractionDirection = Vector3.Normalize(Refract(ray.direction, n, hitObject.ior));
						Vector3 reflectionRayOrig = (Vector3.Dot(reflectionDirection, n) < 0.0f) ? hitPoint - n * settings.bias : hitPoint + n * settings.bias;
						Vector3 refractionRayOrig = (Vector3.Dot(refractionDirection, n) < 0.0f) ? hitPoint - n * settings.bias : hitPoint + n * settings.bias;
						Vector3 reflectionColor = CastRay(new Ray(reflectionRayOrig, reflectionDirection), objects, lights, depth + 1, settings); //TODO: Reuse ray
						Vector3 refractionColor = CastRay(new Ray(reflectionRayOrig, reflectionDirection), objects, lights, depth + 1, settings); //TODO: Reuse ray

						float kr;
						Fresnel(ray.direction, n, hitObject.ior, out kr);

						return reflectionColor * kr + refractionColor * (1.0f - kr);
					}
				case MaterialType.REFLECTION:
					{
						float kr;
						Fresnel(ray.direction, n, hitObject.ior, out kr);
						Vector3 reflectionDirection = Reflect(ray.direction, n);
						Vector3 reflectionRayOrig = (Vector3.Dot(reflectionDirection, n) < 0.0f) ? hitPoint + n * settings.bias : hitPoint - n * settings.bias;

						return CastRay(new Ray(reflectionRayOrig, reflectionDirection), objects, lights, depth + 1, settings) * kr; //TODO: Reuse ray
					}
				default:
					{
						//We use the Phong illumation model in the default case. The phong model is composed of a diffuse and a specular reflection component.
						Vector3 lightAmt = new Vector3(0.0f), specularColor = new Vector3(0.0f);
						Vector3 shadowPointOrig = (Vector3.Dot(ray.direction, n) < 0.0f) ? hitPoint + n * settings.bias : hitPoint - n * settings.bias;

						//Loop over all lights in the scene and sum their contribution up. We also apply the lambert cosine law here.
						for (int i = 0; i < lights.Count; ++i)
						{
							Vector3 lightDir = lights[i].position - hitPoint;

							//Square of the distance between hitPoint and the light
							float lightDistance2 = Vector3.Dot (lightDir, lightDir);
							lightDir = Vector3.Normalize(lightDir);
							float LdotN = Math.Max(0.0f, Vector3.Dot (lightDir, n));

							Mesh shadowHitObject = null;
							float tNearShadow = float.MaxValue;

							//Is the point in shadow, and is the nearest occluding object closer to the object than the light itself?
							bool inShadow = Trace(new Ray(shadowPointOrig, lightDir), objects, out tNearShadow, out index, out uv, out shadowHitObject) && tNearShadow * tNearShadow < lightDistance2; //TODO: Reuse ray
							lightAmt += lights[i].intensity * LdotN * (inShadow ? 0.0f : 1.0f);
							Vector3 reflectionDirection = Reflect(new Vector3(0.0f) - lightDir, n);
							specularColor += lights[i].intensity * (float)Math.Pow(Math.Max(0.0f, -Vector3.Dot(reflectionDirection, ray.direction)), hitObject.specularExponent);
						}

						//Console.WriteLine(hitObject.materialtype.ToString() + " : " + (lightAmt * hitObject.EvaluateDiffuseColor(st) * hitObject.kd + specularColor * hitObject.ks));
						//Console.WriteLine(hitObject.EvaluateDiffuseColor(st));
						return lightAmt * hitObject.EvaluateDiffuseColor(st) * hitObject.kd + specularColor * hitObject.ks; //new Vector3(0, 0, 1);
					}
			}
		}

		//Compute reflection direction 
		private Vector3 Reflect(Vector3 i, Vector3 n)
		{
			return i - n * 2.0f * Vector3.Dot(i, n);
		}

		//Compute refraction direction using Snell's law
		//We need to handle with care the two possible situations:
		//- When the ray is inside the object
		//- When the ray is outside.
		//If the ray is outside, you need to make cosi positive cosi = -N.I
		//If the ray is inside, you need to invert the refractive indices and negate the normal N
		private Vector3 Refract(Vector3 i, Vector3 n, float ior)
		{
			float cosi = MathHelper.Clamp(Vector3.Dot(i,n), -1.0f, 1.0f);
			float etai = 1.0f, etat = ior;
			Vector3 n_new = n;

			if (cosi < 0.0f)
				cosi = -cosi;
			else
			{
				float buffer = etai;
				etai = etat;
				etat = buffer;

				n_new = -n; //This might be wrong?
			}

			float eta = etai / etat;
			float k = 1.0f - eta * eta * (1.0f - cosi * cosi);

			return k < 0.0f ? new Vector3(0.0f) : i * eta + n_new * (eta * cosi - (float)Math.Sqrt(k)); //This might be wrong?
		}

		//Compute Fresnel equation
		//\param I is the incident view direction
		//\param N is the normal at the intersection point
		//\param ior is the mateural refractive index
		//\param[out] kr is the amount of light reflected
		private void Fresnel(Vector3 i, Vector3 n, float ior, out float kr)
		{
			float cosi = MathHelper.Clamp(Vector3.Dot(i,n), -1.0f, 1.0f);
			float etai = 1.0f, etat = ior;

			if (cosi > 0.0f)
			{
				float buffer = etai;
				etai = etat;
				etat = buffer;
			}

			//Compute sini using Snell's law
			float sint = etai / etat * (float)Math.Sqrt(Math.Max(0.0f, 1.0f - cosi * cosi));

			//Total internal reflection
			if (sint >= 1.0f)
				kr = 1.0f;
			else
			{
				float cost = (float)Math.Sqrt(Math.Max(0.0f, 1.0f - sint * sint));
				cosi = Math.Abs(cosi);
				float Rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
				float Rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));

				kr = (Rs * Rs + Rp * Rp) / 2.0f;
			}
			//As a consequence of the conservation of energy, transmittance is given by:
			//kt = 1 - kr;
		}
	}
}
