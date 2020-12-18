using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	class BVH
	{
		private BVHNode _root;

		public BVH(List<Mesh> allMeshes)
		{
			//Build the BVH
			List<int> meshIndices = new List<int>();

			for (int i = 0; i < allMeshes.Count; i++)
				meshIndices.Add(i);

			_root = new BVHNode(meshIndices, allMeshes);
		}

		public List<int> Traverse(Ray ray, bool trackStats = true)
		{
			Vector3 dirFrac = new Vector3
			{
				X = 1.0f / ray.direction.X,
				Y = 1.0f / ray.direction.Y,
				Z = 1.0f / ray.direction.Z
			};

			return _root.Traverse(ray, dirFrac, trackStats);
		}

		//List<BVHNode> stack = new List<BVHNode>();
		//stack.Add(_root);

		//while (stack.Count > 0)
		//{
		//	BVHNode currentNode = stack[stack.Count - 1];

		//	if (currentNode.isLeaf)
		//	{
		//		RayTracer.NearestIntersection(Game.settings.scene, currentNode.meshIndices, ray, out float distance, out int? indexOfNearest);

		//		if (indexOfNearest.HasValue)
		//		{
		//			return indexOfNearest;
		//			//return currentNode.meshIndices;
		//		}
		//	}
		//	else
		//	{
		//		BVHNode leftChildNode = currentNode.leftChildNode;
		//		BVHNode rightChildNode = currentNode.rightChildNode;

		//		float leftIntersectDistance = leftChildNode.Intersect(ray, dirFrac);
		//		float rightIntersectDistance = rightChildNode.Intersect(ray, dirFrac);

		//		bool leftIntersect = leftIntersectDistance >= 0.0f; //Only ==?
		//		bool rightIntersect = rightIntersectDistance >= 0.0f; //Only ==?

		//		if (leftIntersect && rightIntersect)
		//		{
		//			if (leftIntersectDistance < rightIntersectDistance)
		//			{
		//				//Push current.right, current.left in order (so that current.left is on top of stack)
		//				stack.Add(currentNode.rightChildNode);
		//				stack.Add(currentNode.leftChildNode);
		//			}
		//			else
		//			{
		//				//Push current.left, current.right in order (so that current.right is on top of stack)
		//				stack.Add(currentNode.leftChildNode);
		//				stack.Add(currentNode.rightChildNode);
		//			}
		//		}
		//		else
		//		{
		//			if (leftIntersect)
		//			{
		//				//Push left node to top of stack
		//				stack.Add(currentNode.leftChildNode);
		//			}
		//			else if (rightIntersect)
		//			{
		//				//Push right node to top of stack
		//				stack.Add(currentNode.rightChildNode);
		//			}
		//		}
		//	}

		//	stack.Remove(currentNode);
		//}

		//return null;
	}
}
