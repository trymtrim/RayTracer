using System;
using System.Collections.Generic;
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

		public BVHNode(List<int> meshIndices, List<Mesh> allMeshes)
		{
			this.meshIndices = meshIndices;

			bounds = ConstructBoundingBox(meshIndices, allMeshes);

			//Subdivide
			List<int> leftSide = new List<int>();
			List<int> rightSide = new List<int>();
			float minimalSplitCost = float.MaxValue;

			float boxWidth = Math.Abs(bounds.minBounds.X - bounds.maxBounds.X);
			float boxHeight = Math.Abs(bounds.minBounds.Y - bounds.maxBounds.Y);
			float boxForward = Math.Abs(bounds.minBounds.Z - bounds.maxBounds.Z);

			int splitBins = 8;

			for (int i = 0; i < 3; i++) //0 = X, 1 = Y, 2 = Z
			{
				float binSize = i == 0 ? boxWidth / splitBins : (i == 1 ? boxHeight / splitBins : boxForward / splitBins);

				for (int j = 0; j < splitBins; j++)
				{
					float splitInterval = j * (binSize);
					float splitCost = SplitCost(i, splitInterval, allMeshes, out List<int> leftSideMeshes, out List<int> rightSideMeshes);

					if (splitCost < minimalSplitCost)
					{
						leftSide = leftSideMeshes;
						rightSide = rightSideMeshes;

						minimalSplitCost = splitCost;
					}
				}
			}

			//Console.WriteLine(meshIndices.Count); //Debug

			if (meshIndices.Count <= 2 || minimalSplitCost >= bounds.SurfaceArea() * meshIndices.Count)
			{
				//Debug
				//if (meshIndices.Count > 1)
				//	Console.WriteLine(">1 mesh");

				isLeaf = true;
				return;
			}

			leftChildNode = new BVHNode(leftSide, allMeshes);
			rightChildNode = new BVHNode(rightSide, allMeshes);
		}

		private float SplitCost(int splitAxis, float splitInterval, List<Mesh> allMeshes, out List<int> leftSide, out List<int> rightSide)
		{
			leftSide = new List<int>();
			rightSide = new List<int>();

			for (int i = 0; i < meshIndices.Count; i++)
			{
				Mesh mesh = allMeshes[meshIndices[i]];

				switch (splitAxis)
				{
					case 0: //Split vertically
						{
							float xSplitPlanePosition = bounds.minBounds.X + splitInterval;

							if (mesh.Center().X < xSplitPlanePosition)
								leftSide.Add(meshIndices[i]);
							else
								rightSide.Add(meshIndices[i]);

							break;
						}
					case 1: //Split horizontally
						{
							float ySplitPlanePosition = bounds.minBounds.Y + splitInterval;

							if (mesh.Center().Y < ySplitPlanePosition)
								leftSide.Add(meshIndices[i]);
							else
								rightSide.Add(meshIndices[i]);

							break;
						}
					case 2: //Split vertically 90 degrees
						{
							float zSplitPlanePosition = bounds.minBounds.Z + splitInterval;

							if (mesh.Center().Z < zSplitPlanePosition)
								leftSide.Add(meshIndices[i]);
							else
								rightSide.Add(meshIndices[i]);

							break;
						}
				}
			}

			//TODO: These constructed bounding boxes can be passed on, instead of creating new ones when allocating child nodes

			float aLeft = ConstructBoundingBox(leftSide, allMeshes).SurfaceArea();
			float nLeft = leftSide.Count;
			float aRight = ConstructBoundingBox(rightSide, allMeshes).SurfaceArea();
			float nRight = rightSide.Count;

			return aLeft * nLeft + aRight * nRight;
		}

		private AABB ConstructBoundingBox(List<int> meshIndices, List<Mesh> allMeshes)
		{
			//TODO: This only works for spheres right now, make it work for planes too?

			Vector3 minBounds = new Vector3(float.MaxValue);
			Vector3 maxBounds = new Vector3(float.MinValue);

			for (int i = 0; i < meshIndices.Count; i++)
			{
				Mesh mesh = allMeshes[meshIndices[i]];

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
				return meshIndices;
			}
			else
			{
				//TODO: Stop after finding a box where the ray is intersecting with a mesh instead of adding everything to the mesh list before returning
				//Ordered traversal, option 1 (slide 39):
				//- Calculate distance to both child nodes
				//- Traverse the nearest child node first
				//- If the ray intersects with anything, don't traverse the second child node

				List<int> meshesInNode = new List<int>();

				meshesInNode.AddRange(leftChildNode.Traverse(ray));
				meshesInNode.AddRange(rightChildNode.Traverse(ray));

				return meshesInNode;
			}
		}
	}
}
