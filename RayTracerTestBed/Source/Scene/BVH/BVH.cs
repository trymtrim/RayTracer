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

		public BVH(List<Mesh> meshes)
		{
			//Construct BVH

			//TODO: Construct BVH
			//Lecture 5 slides: the perfect BVH - SAH

			root = new BVHNode(meshes, 0); //Temp
		}

		public List<Mesh> Traverse(Ray ray)
		{
			return root.Traverse(ray);
		}
	}
}
