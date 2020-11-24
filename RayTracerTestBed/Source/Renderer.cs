using System;
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

			Sphere sph1 = new Sphere(new Vector3f(-1.0f, 0.0f, -12.0f), 2.0f);
			sph1.materialtype = MaterialType.DIFFUSE_AND_GLOSSY;
			sph1.diffuseColor = new Vector3f(0.6f, 0.7f, 0.8f);

			Sphere sph2 = new Sphere(new Vector3f(0.5f, -0.5f, -8.0f), 1.5f);
			sph2.materialtype = MaterialType.REFLECTION_AND_REFRACTION;
			sph2.ior = 1.5f;

			_objects.Add(sph1);
			_objects.Add(sph2);

			Vector3f[] verts = { new Vector3f(-5.0f, -3.0f, -6.0f), new Vector3f(5.0f, -3.0f, -6.0f), new Vector3f(5.0f, -3.0f, -16.0f), new Vector3f(-5.0f, -3.0f, -16.0f) };
			int[] vertIndex = { 0, 1, 3, 1, 2, 3 };
			Vector2f[] st = { new Vector2f(0.0f, 0.0f), new Vector2f(1.0f, 0.0f), new Vector2f(1.0f, 1.0f), new Vector2f(0.0f, 1.0f) };
			Polygon mesh = new Polygon(verts, vertIndex, 2, st);
			mesh.materialtype = MaterialType.DIFFUSE_AND_GLOSSY;

			_objects.Add(mesh);

			_lights.Add(new Light(new Vector3f(-20.0f, 70.0f, 20.0f), new Vector3f(0.5f)));
			_lights.Add(new Light(new Vector3f(30.0f, 50.0f, -12.0f), new Vector3f(1.0f)));
		}

		//Produce the image to be rendered on the screen
		public void Render(Settings settings/*, ViewPyramid view*/)
		{
			int screenHeight = 480;
			int screenWidth = 640;

			//TODO: Consider moving some of this out of this method - probably not neccessary though
			Vector3f[] frameBuffer = new Vector3f[screenHeight * screenWidth];
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

					Vector3f colorVector = CastRay(ray, _objects, _lights, 0, settings);

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

		private Vector3f CastRay(Ray ray, List<Mesh> objects, List<Light> lights, int depth, Settings settings)
		{
			if (depth > settings.maxDepth)
				return settings.backgroundColor;

			Vector3f hitColor = settings.backgroundColor;

			float tNear = float.MaxValue;
			int index = 0;
			Vector2f uv = null;
			Mesh hitObject = null;

			if (Trace(ray, objects, out tNear, out index, out uv, out hitObject))
			{
				Vector3f hitPoint = ray.origin + ray.direction * tNear;

				Vector3f n; //Normal
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
		public static bool RayTriangleIntersect(Vector3f v1, Vector3f v2, Vector3f v3, Ray ray, out float tNear, out float u, out float v)
		{
			//Deafult out values
			tNear = 0.0f;
			u = 0.0f;
			v = 0.0f;

			Vector3f edge1 = v2 - v1;
			Vector3f edge2 = v3 - v1;
			Vector3f pVec = ray.direction.Cross(edge2);

			float det = edge1.Dot(pVec);
			if (det == 0.0f || det < 0.0f)
				return false;

			Vector3f tVec = ray.origin - v1;
			u = tVec.Dot(pVec);
			if (u < 0.0f || u > det)
				return false;

			Vector3f qVec = tVec.Cross(edge1);
			v = ray.direction.Dot(qVec);
			if (v < 0.0f || u + v > det)
				return false;

			float invDet = 1.0f / det;

			tNear = edge2.Dot(qVec) * invDet;
			u *= invDet;
			v *= invDet;

			return true;
		}

		private Vector3f GetObjectSurfaceColor(Ray ray, Mesh hitObject, Vector3f hitPoint, List<Mesh> objects, List<Light> lights, Vector3f n, Vector2f st, int depth, int index, Vector2f uv, Settings settings)
		{
			switch (hitObject.materialtype)
			{
			//	case MaterialType.REFLECTION_AND_REFRACTION:
			//		{
			//			Vector3f reflectionDirection = Vector3f.Normalize(Reflect(ray.direction, n));
			//			Vector3f refractionDirection = Vector3f.Normalize(Refract(ray.direction, n, hitObject.ior));
			//			Vector3f reflectionRayOrig = (reflectionDirection.Dot(n) < 0.0f) ? hitPoint - n * settings.bias : hitPoint + n * settings.bias;
			//			Vector3f refractionRayOrig = (refractionDirection.Dot(n) < 0.0f) ? hitPoint - n * settings.bias : hitPoint + n * settings.bias;
			//			Vector3f reflectionColor = CastRay(new Ray(reflectionRayOrig, reflectionDirection), objects, lights, depth + 1, settings); //TODO: Reuse ray
			//			Vector3f refractionColor = CastRay(new Ray(reflectionRayOrig, reflectionDirection), objects, lights, depth + 1, settings); //TODO: Reuse ray

			//			float kr;
			//			Fresnel(ray.direction, n, hitObject.ior, out kr);
						
			//			return reflectionColor * kr + refractionColor * (1.0f - kr);
			//		}
			//	case MaterialType.REFLECTION:
			//		{
			//			float kr;
			//			Fresnel(ray.direction, n, hitObject.ior, out kr);
			//			Vector3f reflectionDirection = Reflect(ray.direction, n);
			//			Vector3f reflectionRayOrig = (reflectionDirection.Dot(n) < 0.0f) ? hitPoint + n * settings.bias : hitPoint - n * settings.bias;
						
			//			return CastRay(new Ray(reflectionRayOrig, reflectionDirection), objects, lights, depth + 1, settings) * kr; //TODO: Reuse ray
			//		}
				default:
					{
						//We use the Phong illumation model in the default case. The phong model is composed of a diffuse and a specular reflection component.
						Vector3f lightAmt = new Vector3f(0.0f), specularColor = new Vector3f(0.0f);
						Vector3f shadowPointOrig = (ray.direction.Dot(n) < 0.0f) ? hitPoint + n * settings.bias : hitPoint - n * settings.bias;

						//Loop over all lights in the scene and sum their contribution up. We also apply the lambert cosine law here.
						for (int i = 0; i < lights.Count; ++i)
						{
							Vector3f lightDir = lights[i].position - hitPoint;

							//Square of the distance between hitPoint and the light
							float lightDistance2 = lightDir.Dot(lightDir);
							lightDir = Vector3f.Normalize(lightDir);
							float LdotN = Math.Max(0.0f, lightDir.Dot(n));

							Mesh shadowHitObject = null;
							float tNearShadow = float.MaxValue;

							//Is the point in shadow, and is the nearest occluding object closer to the object than the light itself?
							bool inShadow = Trace(new Ray(shadowPointOrig, lightDir), objects, out tNearShadow, out index, out uv, out shadowHitObject) && tNearShadow * tNearShadow < lightDistance2; //TODO: Reuse ray
							lightAmt += lights[i].intensity * LdotN * (inShadow ? 0.0f : 1.0f);
							Vector3f reflectionDirection = Reflect(new Vector3f(0.0f) - lightDir, n);
							specularColor += lights[i].intensity * (float)Math.Pow(Math.Max(0.0f, -reflectionDirection.Dot(ray.direction)), hitObject.specularExponent);
						}

						//Console.WriteLine(hitObject.materialtype.ToString() + " : " + (lightAmt * hitObject.EvaluateDiffuseColor(st) * hitObject.kd + specularColor * hitObject.ks));
						//Console.WriteLine(hitObject.EvaluateDiffuseColor(st));
						return lightAmt * hitObject.EvaluateDiffuseColor(st) * hitObject.kd + specularColor * hitObject.ks; //new Vector3f(0, 0, 1);
					}
			}
		}

		//Compute reflection direction 
		private Vector3f Reflect(Vector3f i, Vector3f n)
		{
			return i - n * 2.0f * i.Dot(n);
		}

		//Compute refraction direction using Snell's law
		//We need to handle with care the two possible situations:
		//- When the ray is inside the object
		//- When the ray is outside.
		//If the ray is outside, you need to make cosi positive cosi = -N.I
		//If the ray is inside, you need to invert the refractive indices and negate the normal N
		private Vector3f Refract(Vector3f i, Vector3f n, float ior)
		{
			float cosi = MathHelper.Clamp(i.Dot(n), -1.0f, 1.0f);
			float etai = 1.0f, etat = ior;
			Vector3f n_new = n;

			if (cosi < 0.0f)
				cosi = -cosi;
			else
			{
				float buffer = etai;
				etai = etat;
				etat = buffer;

				n_new = new Vector3f(0.0f) - n; //This might be wrong?
			}

			float eta = etai / etat;
			float k = 1.0f - eta * eta * (1.0f - cosi * cosi);

			return k < 0.0f ? new Vector3f(0.0f) : i * eta + n *(eta * cosi - (float)Math.Sqrt(k)); //This might be wrong?
		}

		//Compute Fresnel equation
		//\param I is the incident view direction
		//\param N is the normal at the intersection point
		//\param ior is the mateural refractive index
		//\param[out] kr is the amount of light reflected
		private void Fresnel(Vector3f i, Vector3f n, float ior, out float kr)
		{
			float cosi = MathHelper.Clamp(i.Dot(n), -1.0f, 1.0f);
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
