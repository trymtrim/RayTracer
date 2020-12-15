using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracerTestBed
{
	class BVH
	{
		private BVHNode root;

		public static List<Mesh> Meshes;

		public BVH(List<Mesh> meshes)
		{
			Meshes = meshes;

			//Build the BVH
			List<int> meshIndices = new List<int>();

			for (int i = 0; i < meshes.Count; i++)
				meshIndices.Add(i);

			root = new BVHNode(meshIndices);
		}

		public List<int> Traverse(Ray ray)
		{
			return root.Traverse(ray);
		}
	}
}
