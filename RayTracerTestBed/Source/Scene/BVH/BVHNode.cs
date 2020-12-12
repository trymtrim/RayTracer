using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracerTestBed
{
	class BVHNode
	{
		//TODO: Make all of this become a total of 32 bytes
		public AABB bounds;
		public bool isLeaf;
		public BVHNode left, right; //Unsure about this - child nodes
		public List<Mesh> meshes; //Consider changing this to "int first, count;" instead?

		private int _subdivisions = 0; //TODO: Another number? Move this elsewhere?

		public BVHNode(List<Mesh> meshes, int subdivisionCount)
		{
			this.meshes = meshes;

			bounds = new AABB(); //TODO: Implement correct AABB

			if (subdivisionCount == _subdivisions)
				isLeaf = true;
			else
			{
				//Subdivide

				//TODO: Implement this

				//left = new BVHNode ...
				//right = new BVHNode ...
			}
		}

		public List<Mesh> Traverse(Ray ray)
		{
			if (!Intersect(ray))
				return new List<Mesh>();

			if (isLeaf)
			{
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

				List<Mesh> meshesInNode = left.Traverse(ray);
				meshesInNode.AddRange(right.Traverse(ray));

				return meshesInNode;
			}
		}

		private bool Intersect(Ray ray)
		{
			//TODO: Implement bounding box intersection

			return true; //Temp
		}
	}
}
