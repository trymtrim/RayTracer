using System;
using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	class BVHNode
	{
		private AABB bounds;
		private bool isLeaf;
		private BVHNode leftChildNode, rightChildNode;
		private List<int> meshIndices;

		public BVHNode(List<int> meshIndices, List<Mesh> allMeshes)
		{
			this.meshIndices = meshIndices;

			bounds = ConstructBoundingBox(meshIndices, allMeshes);
			Subdivide(allMeshes);
			RecalculateBoundingBoxWithFullExtents(meshIndices, allMeshes);
		}

		private void Subdivide(List<Mesh> allMeshes)
		{
			List<int> leftSide = null;
			List<int> rightSide = null;

			float minimalSplitCost = float.MaxValue;

			float boxWidth = Math.Abs(bounds.minX - bounds.maxX);
			float boxHeight = Math.Abs(bounds.minY - bounds.maxY);
			float boxForward = Math.Abs(bounds.minZ - bounds.maxZ);

			int splitPlaneCount = Config.BINNING_SPLIT_PLANE_COUNT;

			GenerateBins(allMeshes, splitPlaneCount, boxWidth, boxHeight, boxForward, out List<List<int>> xBins, out List<List<int>> yBins, out List<List<int>> zBins);

			for (int i = 0; i < 3; i++) //0 = X, 1 = Y, 2 = Z
			{
				for (int j = 0; j < splitPlaneCount; j++)
				{
					float splitCost = SplitCost(i, j, allMeshes, xBins, yBins, zBins, out List<int> leftSideMeshes, out List<int> rightSideMeshes);

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

		private void GenerateBins(List<Mesh> allMeshes, int splitPlaneCount, float boxWidth, float boxHeight, float boxForward, out List<List<int>> xBins, out List<List<int>> yBins, out List<List<int>> zBins)
		{
			xBins = new List<List<int>>();
			yBins = new List<List<int>>();
			zBins = new List<List<int>>();

			for (int i = 0; i < splitPlaneCount; i++)
			{
				xBins.Add(new List<int>());
				yBins.Add(new List<int>());
				zBins.Add(new List<int>());
			}

			float xBinSize = boxWidth / splitPlaneCount;
			float yBinSize = boxHeight / splitPlaneCount;
			float zBinSize = boxForward / splitPlaneCount;

			for (int i = 0; i < meshIndices.Count; i++)
			{
				Mesh mesh = allMeshes[meshIndices[i]];

				for (int j = 0; j < splitPlaneCount; j++)
				{
					float xSplitInterval = j * xBinSize;
					float xPosition = mesh.Center().X;

					if (j == splitPlaneCount - 1)
					{
						if (xPosition >= bounds.minX + xSplitInterval && xPosition < bounds.minX + xSplitInterval + xBinSize + Renderer.EPSILON)
							xBins[j].Add(meshIndices[i]);
					}
					else
					{
						if (xPosition >= bounds.minX + xSplitInterval && xPosition < bounds.minX + xSplitInterval + xBinSize)
							xBins[j].Add(meshIndices[i]);
					}

					float ySplitInterval = j * yBinSize;
					float yPosition = mesh.Center().Y;

					if (j == splitPlaneCount - 1)
					{
						if (yPosition >= bounds.minY + ySplitInterval && yPosition < bounds.minY + ySplitInterval + yBinSize + Renderer.EPSILON)
							yBins[j].Add(meshIndices[i]);
					}
					else
					{
						if (yPosition >= bounds.minY + ySplitInterval && yPosition < bounds.minY + ySplitInterval + yBinSize)
							yBins[j].Add(meshIndices[i]);
					}

					float zSplitInterval = j * zBinSize;
					float zPosition = mesh.Center().Z;

					if (j == splitPlaneCount - 1)
					{
						if (zPosition >= bounds.minZ + zSplitInterval && zPosition < bounds.minZ + zSplitInterval + zBinSize + Renderer.EPSILON)
							zBins[j].Add(meshIndices[i]);
					}
					else
					{
						if (zPosition >= bounds.minZ + zSplitInterval && zPosition < bounds.minZ + zSplitInterval + zBinSize)
							zBins[j].Add(meshIndices[i]);
					}
				}
			}
		}

		private float SplitCost(int splitAxis, int binIndex, List<Mesh> allMeshes, List<List<int>> xBins, List<List<int>> yBins, List<List<int>> zBins,out List<int> leftSide, out List<int> rightSide)
		{
			leftSide = new List<int>();
			rightSide = new List<int>();

			switch (splitAxis)
			{
				case 0: //Split vertically
					{
						for (int i = 0; i < xBins.Count; i++)
						{
							if (i > binIndex)
								rightSide.AddRange(xBins[i]);
							else
								leftSide.AddRange(xBins[i]);
						}

						break;
					}
				case 1: //Split horizontally
					{
						for (int i = 0; i < yBins.Count; i++)
						{
							if (i > binIndex)
								rightSide.AddRange(yBins[i]);
							else
								leftSide.AddRange(yBins[i]);
						}

						break;
					}
				case 2: //Split vertically 90 degrees
					{
						for (int i = 0; i < zBins.Count; i++)
						{
							if (i > binIndex)
								rightSide.AddRange(zBins[i]);
							else
								leftSide.AddRange(zBins[i]);
						}

						break;
					}
			}

			float aLeft = ConstructBoundingBox(leftSide, allMeshes).SurfaceArea();
			float nLeft = leftSide.Count;
			float aRight = ConstructBoundingBox(rightSide, allMeshes).SurfaceArea();
			float nRight = rightSide.Count;

			return aLeft * nLeft + aRight * nRight;
		}

		private AABB ConstructBoundingBox(List<int> meshes, List<Mesh> allMeshes)
		{
			//This only works for spheres
			AABB boundingBox = new AABB();

			for (int i = 0; i < meshes.Count; i++)
			{
				Mesh mesh = allMeshes[meshes[i]];

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

		private void RecalculateBoundingBoxWithFullExtents(List<int> meshes, List<Mesh> allMeshes)
		{
			//This only works for spheres
			bounds.minX = bounds.minY = bounds.minZ = float.MaxValue;
			bounds.maxX = bounds.maxY = bounds.maxZ = float.MinValue;

			for (int i = 0; i < meshes.Count; i++)
			{
				Mesh mesh = allMeshes[meshes[i]];

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
