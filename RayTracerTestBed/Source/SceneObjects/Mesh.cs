using System.Collections.Generic;
using OpenTK;
using System;

namespace RayTracerTestBed
{
	class Mesh
	{
		public string name;
		protected int _index;

		public void SetIndex()
		{
			_index = Scene.scene.meshes.Count;
		}

		public virtual float? Intersect(Ray ray)
		{
			return float.MaxValue;
		}

		public virtual Vector3 Normal(Vector3 point)
		{
			return Vector3.Zero;
		}

		public virtual Vector3 Center()
		{
			return Vector3.Zero;
		}

		public virtual float Radius()
		{
			return float.MaxValue;
		}

		public virtual List<string> DebugInfo()
		{
			return new List<string>();
		}
	}
}
