using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracerTestBed
{
	class BVH
	{
		public static List<Mesh> Meshes; //TODO: Consider storing this elsewhere

		private BVHNode _root;

		public BVH(List<Mesh> meshes)
		{
			Meshes = meshes;

			//Build the BVH
			List<int> meshIndices = new List<int>();

			for (int i = 0; i < meshes.Count; i++)
				meshIndices.Add(i);

			_root = new BVHNode(meshIndices);
		}

		public List<int> Traverse(Ray ray)
		{
			return _root.Traverse(ray);
		}
	}
}
