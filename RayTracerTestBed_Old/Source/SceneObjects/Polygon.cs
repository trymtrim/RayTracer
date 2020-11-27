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
		private Vector3[] vertices;
		private int numTriangles;
		private int[] vertexIndex;
		private Vector2[] stCoordinates;

		public Polygon(Vector3[] verts, int[] vertsIndex, int numTris, Vector2[] st)
		{
			//TODO: ?
			//memcpy?
			//int32 etc?

			//int maxIndex = 0;

			//for (int i = 0; i < numTris; i++)
			//{
			//	if (vertsIndex[i] > maxIndex)
			//		maxIndex = vertsIndex[i];
			//}

			//maxIndex += 1;

			vertices = verts;
			vertexIndex = vertsIndex;
			numTriangles = numTris;
			stCoordinates = st;
		}

		public override bool Intersect(Ray ray, out float tNear, out int index, out Vector2 uv)
		{
			//Default out values
			tNear = float.MaxValue;
			index = -1;
			uv = new Vector2(0.0f);

			bool intersect = false;

			for (int i = 0; i < numTriangles; i++)
			{
				Vector3 v1 = vertices[vertexIndex[i * 3]];
				Vector3 v2 = vertices[vertexIndex[i * 3 + 1]];
				Vector3 v3 = vertices[vertexIndex[i * 3 + 2]];

				float t, u, v;

				if (Renderer.RayTriangleIntersect(v1, v2, v3, ray, out t, out u, out v)) //TODO: This is slow
				{
					tNear = t;
					uv.X = u;
					uv.Y = v;
					index = i;
					intersect |= true;
				}
			}

			return intersect;
		}

		public override void GetSurfaceProperties(Vector3 p, Vector3 i, int index, Vector2 uv, out Vector3 n, out Vector2 st)
		{
			Vector3 v1 = vertices[vertexIndex[index * 3]];
			Vector3 v2 = vertices[vertexIndex[index * 3 + 1]];
			Vector3 v3 = vertices[vertexIndex[index * 3 + 2]];

			Vector3 e1 = Vector3.Normalize(v2 - v1);
			Vector3 e2 = Vector3.Normalize(v3 - v2);
			n = Vector3.Normalize(Vector3.Cross (e1, e2));

			Vector2 st1 = stCoordinates[vertexIndex[index * 3]];
			Vector2 st2 = stCoordinates[vertexIndex[index * 3 + 1]];
			Vector2 st3 = stCoordinates[vertexIndex[index * 3 + 2]];

			st = st1 * (1.0f - uv.X - uv.Y) + st2 * uv.X + st3 * uv.Y;
		}

		public override Vector3 EvaluateDiffuseColor(Vector2 st)
		{
			if (vertices.Length > 6)
				return diffuseColor;

			float scale = 5.0f;
			float pattern = Math.Max((st.X * scale % 1.0f) > 0.5f ? 1.0f : 0.0f, (st.X * scale % 1.0f) > 0.5f ? 1.0f : 0.0f);

			return pattern == 0.0f ? new Vector3(0.815f, 0.235f, 0.031f) : new Vector3(0.937f, 0.937f, 0.231f);
		}
	}
}
