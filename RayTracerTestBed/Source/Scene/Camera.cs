using OpenTK;
using System;

namespace RayTracerTestBed
{
	class Camera
	{
		public Vector3 position;
		public Vector3 direction;

		public float fov;

		private Vector3 originalPosition;
		private Vector3 originalDirection;

		private Vector3 upVector = new Vector3(0.0f, 1.0f, 0.0f);

		public Camera(float fov, Vector3 position, Vector3 direction)
		{
			this.position = position;
			this.fov = fov;
			this.direction = direction;

			originalPosition = position;
			originalDirection = direction;
		}

		public Ray RayThroughScreen(float x, float y)
		{
			Ray ray = new Ray();
			ray.origin = position;
			ray.direction = (Point(x, y) - position).Normalized();

			return ray;
		}

		private Vector3 Point(float x, float y)
		{
			Vector3 tempY = (upVector - direction * Vector3.Dot(direction, upVector)).Normalized();
			Vector3 tempX = Vector3.Cross(-direction, tempY) * (Config.ASPECT_RATIO_WIDTH / (float)Config.ASPECT_RATIO_HEIGHT);

			float fovRad = fov * (float)Math.PI / 180.0f;

			Vector3 center = position + direction / (float)Math.Tan(fovRad / 2.0f);

			Vector3 p0 = center - tempX - tempY;
			Vector3 p1 = center + tempX - tempY;
			Vector3 p2 = center - tempX + tempY;

			return p0 + (p1 - p0) * x + (p2 - p0) * y;
		}

		public void Reset()
		{
			position = originalPosition;
			direction = originalDirection;
		}
	}
}
