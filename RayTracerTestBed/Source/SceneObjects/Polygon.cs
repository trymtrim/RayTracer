using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	class Polygon : Mesh
	{
		private Vector3f[] vertices;
		private int numTriangles;
		private int[] vertexIndex;
		private Vector2f[] stCoordinates;

		public Polygon(Vector3f[] verts, int[] vertsIndex, int numTris, Vector2f[] st)
		{
			int maxIndex = 0;

			for (int i = 0; i < numTris; i++)
			{
				if (vertsIndex[i] > maxIndex)
					maxIndex = vertsIndex[i];
			}

			//TODO: ?
			//memcpy?
			//int32 etc?

			maxIndex += 1;

			vertices = verts;
			vertexIndex = vertsIndex;
			numTriangles = numTris;
			stCoordinates = st;
		}

		public override bool Intersect(Ray ray, out float tNear, out int index, out Vector2f uv)
		{
			//Default out values
			tNear = float.MaxValue;
			index = -1;
			uv = new Vector2f(0.0f);

			bool intersect = false;

			for (int i = 0; i < numTriangles; i++)
			{
				Vector3f v1 = vertices[vertexIndex[i * 3]];
				Vector3f v2 = vertices[vertexIndex[i * 3 + 1]];
				Vector3f v3 = vertices[vertexIndex[i * 3 + 2]];

				float t, u, v;

				if (Renderer.RayTriangleIntersect(v1, v2, v3, ray, out t, out u, out v)) //TODO: This is slow
				{
					tNear = t;
					uv.X = u;
					uv.Y = v;
					index = i;
					intersect |= true; //TODO: Unsure about this '|='
				}
			}

			return intersect;
		}

		public override void GetSurfaceProperties(Vector3f p, Vector3f i, int index, Vector2f uv, out Vector3f n, out Vector2f st)
		{
			Vector3f v1 = vertices[vertexIndex[index * 3]];
			Vector3f v2 = vertices[vertexIndex[index * 3 + 1]];
			Vector3f v3 = vertices[vertexIndex[index * 3 + 2]];

			Vector3f e1 = Vector3f.Normalize(v2 - v1);
			Vector3f e2 = Vector3f.Normalize(v3 - v2);
			n = Vector3f.Normalize(e1.Cross(e2));

			Vector2f st1 = stCoordinates[vertexIndex[index * 3]];
			Vector2f st2 = stCoordinates[vertexIndex[index * 3 + 1]];
			Vector2f st3 = stCoordinates[vertexIndex[index * 3 + 2]];

			st = st1 * (1.0f - uv.X - uv.Y) + st2 * uv.X + st3 * uv.Y;
		}

		public override Vector3f EvaluateDiffuseColor(Vector2f st)
		{
			float scale = 5.0f;
			float pattern = Math.Max((st.X * scale % 1.0f) > 0.5f ? 1.0f : 0.0f, (st.X * scale % 1.0f) > 0.5f ? 1.0f : 0.0f);

			return pattern == 0.0f ? new Vector3f(0.815f, 0.235f, 0.031f) : new Vector3f(0.937f, 0.937f, 0.231f);
		}
	}
}
