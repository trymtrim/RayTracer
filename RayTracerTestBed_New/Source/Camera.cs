using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RayTracerTestBed
{
	class Camera
	{
		public Vector3 origin;
		//public ViewPort viewPort;

		//FOV is vertical field of view in degrees
		public Camera(float fov, float aspectRatio, Vector3 origin, Vector3 direction)
		{
			//var fovRad = fov * (float)Math.PI / 180.0f;
			//var dir = direction.Normalized();
			//var center = origin + dir / (float)Math.Tan(fovRad / 2.0f);

			this.origin = origin;
			//viewPort = new ViewPort(aspectRatio, center, dir);
		}

		//public Ray RayThrough(Vector3 point)
		//{
		//	return new Ray(origin, (point - origin).Normalized());
		//}

		////x and y in range [0,1]
		//public Ray RayThroughScreen(float x, float y)
		//{
		//	return RayThrough(viewPort.Point(x, y));
		//}
	}

	//class ViewPort
	//{
	//	internal Vector3 p0;
	//	internal Vector3 p1;
	//	internal Vector3 p2;

	//	public ViewPort(float aspectRatio, Vector3 center, Vector3 direction)
	//	{
	//		//A viewport of height 2 and width 2 * aspectRatio

	//		var upVector = new Vector3(0.0f, 1.0f, 0.0f);

	//		var y = -(upVector - direction * Vector3.Dot(direction, upVector)).Normalized();
	//		var x = Vector3.Cross(-direction, y) * aspectRatio; //REMINDER: This might be wrong

	//		p0 = center - x - y;
	//		p1 = center + x - y;
	//		p2 = center - x + y;
	//	}

	//	//x and y in range [0,1]
	//	public Vector3 Point(float x, float y)
	//	{
	//		return p0 + (p1 - p0) * x + (p2 - p0) * y;
	//	}
	//}
}
