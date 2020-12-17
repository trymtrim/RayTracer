using System.Collections.Generic;

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

		public List<int> Traverse(Ray ray)
		{
			ray.InitializeDirFrac();
			return _root.Traverse(ray);
		}
	}
}
