using System;
using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	class BVHNode
	{
		public AABB bounds;
		public bool isLeaf;
		public BVHNode leftChildNode, rightChildNode;
		public List<int> meshIndices;

		public BVHNode(List<int> meshIndices, List<Mesh> allMeshes)
		{
			this.meshIndices = meshIndices;

			bounds = ConstructBoundingBox(meshIndices, allMeshes);
			Subdivide(allMeshes);
			RecalculateBoundingBoxWithFullExtents(meshIndices, allMeshes);
		}

		private void Subdivide(List<Mesh> allMeshes)
		{
			List<int> leftSide = new List<int>();
			List<int> rightSide = new List<int>();
			float minimalSplitCost = float.MaxValue;

			float boxWidth = Math.Abs(bounds.minX - bounds.maxX);
			float boxHeight = Math.Abs(bounds.minY - bounds.maxY);
			float boxForward = Math.Abs(bounds.minZ - bounds.maxZ);

			int splitBins = Config.SPLIT_PLANE_BINS;

			for (int i = 0; i < 3; i++) //0 = X, 1 = Y, 2 = Z
			{
				float binSize = i == 0 ? boxWidth / splitBins : (i == 1 ? boxHeight / splitBins : boxForward / splitBins);

				for (int j = 0; j < splitBins; j++)
				{
					float splitInterval = j * binSize;
					float splitCost = SplitCost(i, splitInterval, allMeshes, out List<int> leftSideMeshes, out List<int> rightSideMeshes);

					if (splitCost < minimalSplitCost)
					{
						leftSide = leftSideMeshes;
						rightSide = rightSideMeshes;

						minimalSplitCost = splitCost;
					}
				}
			}

			if (minimalSplitCost >= bounds.SurfaceArea() * meshIndices.Count)
			{
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
							float xSplitPlanePosition = bounds.minX + splitInterval;

							if (mesh.Center().X < xSplitPlanePosition)
								leftSide.Add(meshIndices[i]);
							else
								rightSide.Add(meshIndices[i]);

							break;
						}
					case 1: //Split horizontally
						{
							float ySplitPlanePosition = bounds.minY + splitInterval;

							if (mesh.Center().Y < ySplitPlanePosition)
								leftSide.Add(meshIndices[i]);
							else
								rightSide.Add(meshIndices[i]);

							break;
						}
					case 2: //Split vertically 90 degrees
						{
							float zSplitPlanePosition = bounds.minZ + splitInterval;

							if (mesh.Center().Z < zSplitPlanePosition)
								leftSide.Add(meshIndices[i]);
							else
								rightSide.Add(meshIndices[i]);

							break;
						}
				}
			}

			float aLeft = ConstructBoundingBox(leftSide, allMeshes).SurfaceArea();
			float nLeft = leftSide.Count;
			float aRight = ConstructBoundingBox(rightSide, allMeshes).SurfaceArea();
			float nRight = rightSide.Count;

			return aLeft * nLeft + aRight * nRight;
		}

		private AABB ConstructBoundingBox(List<int> meshIndices, List<Mesh> allMeshes)
		{
			//This only works for spheres
			AABB boundingBox = new AABB();

			for (int i = 0; i < meshIndices.Count; i++)
			{
				Mesh mesh = allMeshes[meshIndices[i]];

				float xMax = mesh.Center().X;// + mesh.Radius();
				if (xMax > boundingBox.maxX)
					boundingBox.maxX = xMax;
				float yMax = mesh.Center().Y;// + mesh.Radius();
				if (yMax > boundingBox.maxY)
					boundingBox.maxY = yMax;
				float zMax = mesh.Center().Z;// + mesh.Radius();
				if (zMax > boundingBox.maxZ)
					boundingBox.maxZ = zMax;

				float xMin = mesh.Center().X;// - mesh.Radius();
				if (xMin < boundingBox.minX)
					boundingBox.minX = xMin;
				float yMin = mesh.Center().Y;// - mesh.Radius();
				if (yMin < boundingBox.minY)
					boundingBox.minY = yMin;
				float zMin = mesh.Center().Z;// - mesh.Radius();
				if (zMin < boundingBox.minZ)
					boundingBox.minZ = zMin;
			}

			return boundingBox;
		}

		private void RecalculateBoundingBoxWithFullExtents(List<int> meshIndices, List<Mesh> allMeshes)
		{
			//This only works for spheres
			bounds.minX = bounds.minY = bounds.minZ = float.MaxValue;
			bounds.maxX = bounds.maxY = bounds.maxZ = float.MinValue;

			for (int i = 0; i < meshIndices.Count; i++)
			{
				Mesh mesh = allMeshes[meshIndices[i]];

				float xMax = mesh.Center().X + mesh.Radius();
				if (xMax > bounds.maxX)
					bounds.maxX = xMax;
				float yMax = mesh.Center().Y + mesh.Radius();
				if (yMax > bounds.maxY)
					bounds.maxY = yMax;
				float zMax = mesh.Center().Z + mesh.Radius();
				if (zMax > bounds.maxZ)
					bounds.maxZ = zMax;

				float xMin = mesh.Center().X - mesh.Radius();
				if (xMin < bounds.minX)
					bounds.minX = xMin;
				float yMin = mesh.Center().Y - mesh.Radius();
				if (yMin < bounds.minY)
					bounds.minY = yMin;
				float zMin = mesh.Center().Z - mesh.Radius();
				if (zMin < bounds.minZ)
					bounds.minZ = zMin;
			}
		}

		private bool Intersect(Ray ray, Vector3 dirFrac, bool trackStats = true)
		{
			if (trackStats)
				Game.numBoxRayTests++;

			float tx1 = (bounds.minX - ray.origin.X) * dirFrac.X;
			float tx2 = (bounds.maxX - ray.origin.X) * dirFrac.X;
			float ty1 = (bounds.minY - ray.origin.Y) * dirFrac.Y;
			float ty2 = (bounds.maxY - ray.origin.Y) * dirFrac.Y;
			float tz1 = (bounds.minZ - ray.origin.Z) * dirFrac.Z;
			float tz2 = (bounds.maxZ - ray.origin.Z) * dirFrac.Z;

			float tMin = Math.Max(Math.Max(Math.Min(tx1, tx2), Math.Min(ty1, ty2)), Math.Min(tz1, tz2));
			float tMax = Math.Min(Math.Min(Math.Max(tx1, tx2), Math.Max(ty1, ty2)), Math.Max(tz1, tz2));

			if (tMax < 0)
				return false;

			return tMax >= tMin;
		}

		public List<int> Traverse(Ray ray, Vector3 dirFrac, bool trackStats = true)
		{
			if (!Intersect(ray, dirFrac, trackStats))
				return new List<int>();

			Game.numBoxRayIntersections++;

			if (isLeaf)
			{
				return meshIndices;
			}
			else
			{
				List<int> meshesInNode = new List<int>();

				meshesInNode.AddRange(leftChildNode.Traverse(ray, dirFrac, trackStats));
				meshesInNode.AddRange(rightChildNode.Traverse(ray, dirFrac, trackStats));

				return meshesInNode;
			}
		}
	}
}
