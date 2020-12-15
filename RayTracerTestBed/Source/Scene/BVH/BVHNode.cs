using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	class BVHNode
	{
		//TODO: Make all of this become a total of 32 bytes (?)
		public AABB bounds;
		public bool isLeaf;
		public BVHNode leftChildNode, rightChildNode;
		public List<int> meshIndices;
		//public List<Mesh> meshes; //Consider changing this to "int first, count;" instead?

		public BVHNode(List<int> meshIndices)
		{
			this.meshIndices = meshIndices;

			bounds = ConstructBoundingBox(meshIndices, true);

			//isLeaf = true;

			if (meshIndices.Count <= 3)
				isLeaf = true;
			else
			{
				//Subdivide

				List<int> leftSide = new List<int>();
				List<int> rightSide = new List<int>();

				float boxWidth = Math.Abs(bounds.minBounds.X - bounds.maxBounds.X);
				float boxHeight = Math.Abs(bounds.minBounds.Y - bounds.maxBounds.Y);
				bool splitVertically = boxWidth >= boxHeight;

				//TODO: Consider taking Z-axis into consideration too

				for (int i = 0; i < meshIndices.Count; i++)
				{
					Mesh mesh = BVH.Meshes[meshIndices[i]];

					if (splitVertically)
					{
						//Split vertically
						float xSplitPlanePosition = bounds.minBounds.X + boxWidth / 2.0f;

						//Console.WriteLine(mesh.Center().X);

						if (mesh.Center().X < xSplitPlanePosition)
						{
							leftSide.Add(meshIndices[i]);

							Console.WriteLine("Vertical Left");
						}
						else
						{
							rightSide.Add(meshIndices[i]);

							Console.WriteLine("Vertical Right");
						}
					}
					else
					{
						//Split horizontally
						float ySplitPlanePosition = bounds.minBounds.Y + boxHeight / 2.0f;

						//Console.WriteLine(ySplitPlanePosition);

						if (mesh.Center().Y < ySplitPlanePosition)
						{
							leftSide.Add(meshIndices[i]);

							Console.WriteLine("Horizontal Left");
						}
						else
						{
							rightSide.Add(meshIndices[i]);

							Console.WriteLine("Horizontal Right");
						}
					}
				}

				bounds = ConstructBoundingBox(meshIndices, true);

				//Temp?
				//if (leftSide.Count == 0 || rightSide.Count == 0)
				//	Console.WriteLine("ERROR");

				Console.WriteLine(meshIndices.Count);

				if (leftSide.Count > 0)
					leftChildNode = new BVHNode(leftSide);
				if (rightSide.Count > 0)
					rightChildNode = new BVHNode(rightSide);
			}
		}

		private AABB ConstructBoundingBox(List<int> meshIndices, bool recalculate = false)
		{
			Vector3 minBounds = new Vector3(float.MaxValue);
			Vector3 maxBounds = new Vector3(float.MinValue);

			for (int i = 0; i < meshIndices.Count; i++)
			{
				Mesh mesh = BVH.Meshes[meshIndices[i]];

				if (recalculate)
				{
					float xMax = mesh.Center().X + mesh.Radius();
					if (xMax > maxBounds.X)
						maxBounds.X = xMax;
					float yMax = mesh.Center().Y + mesh.Radius();
					if (yMax > maxBounds.Y)
						maxBounds.Y = yMax;
					float zMax = mesh.Center().Z + mesh.Radius();
					if (zMax > maxBounds.Z)
						maxBounds.Z = zMax;

					float xMin = mesh.Center().X - mesh.Radius();
					if (xMin < minBounds.X)
						minBounds.X = xMin;
					float yMin = mesh.Center().Y - mesh.Radius();
					if (yMin < minBounds.Y)
						minBounds.Y = yMin;
					float zMin = mesh.Center().Z - mesh.Radius();
					if (zMin < minBounds.Z)
						minBounds.Z = zMin;
				}
				else
				{
					float xMax = mesh.Center().X;
					if (xMax > maxBounds.X)
						maxBounds.X = xMax;
					float yMax = mesh.Center().Y;
					if (yMax > maxBounds.Y)
						maxBounds.Y = yMax;
					float zMax = mesh.Center().Z;
					if (zMax > maxBounds.Z)
						maxBounds.Z = zMax;

					float xMin = mesh.Center().X;
					if (xMin < minBounds.X)
						minBounds.X = xMin;
					float yMin = mesh.Center().Y;
					if (yMin < minBounds.Y)
						minBounds.Y = yMin;
					float zMin = mesh.Center().Z;
					if (zMin < minBounds.Z)
						minBounds.Z = zMin;
				}
			}

			AABB boundingBox = new AABB(minBounds, maxBounds);

			return boundingBox;
		}

		private bool Intersect(Ray ray)
		{
			Vector3 dirFrac = new Vector3
			{
				X = 1.0f / ray.direction.X,
				Y = 1.0f / ray.direction.Y,
				Z = 1.0f / ray.direction.Z
			};

			//TODO: This multiplication is slow
			float t1 = (bounds.minBounds.X - ray.origin.X) * dirFrac.X;
			float t2 = (bounds.maxBounds.X - ray.origin.X) * dirFrac.X;
			float t3 = (bounds.minBounds.Y - ray.origin.Y) * dirFrac.Y;
			float t4 = (bounds.maxBounds.Y - ray.origin.Y) * dirFrac.Y;
			float t5 = (bounds.minBounds.Z - ray.origin.Z) * dirFrac.Z;
			float t6 = (bounds.maxBounds.Z - ray.origin.Z) * dirFrac.Z;

			float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
			float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

			//If tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
			if (tmax < 0)
			{
				//t = tmax;
				return false;
			}

			//If tmin > tmax, ray doesn't intersect AABB
			if (tmin > tmax)
			{
				//t = tmax;
				return false;
			}

			//t = tmin;
			return true;
		}

		public List<int> Traverse(Ray ray)
		{
			if (!Intersect(ray))
				return new List<int>();

			if (isLeaf)
			{
				List<int> meshes = new List<int>();

				for (int i = 0; i < meshIndices.Count; i++)
					meshes.Add(meshIndices[i]);

				return meshes;
			}
			else
			{
				//TODO: Consider stopping after finding a box where the ray is intersecting with a mesh instead of adding everything to the mesh list before returning
				//Ordered traversal, option 1 (slide 39):
				//- Calculate distance to both child nodes
				//- Traverse the nearest child node first
				//- If the ray intersects with anything, don't traverse the second child node
				//(Look at "the perfect BVH" lecture slides first, we might not want to do any of the above, depending on how the SAH works)
				//Also consider rewatching lecture or lab recording

				//return meshes;

				List<int> meshesInNode = new List<int>();

				if (leftChildNode != null)
					meshesInNode.AddRange(leftChildNode.Traverse(ray));

				if (rightChildNode != null)
					meshesInNode.AddRange(rightChildNode.Traverse(ray));

				return meshesInNode;
			}
		}
	}
}
