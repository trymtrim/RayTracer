using System;
using OpenTK;

namespace RayTracerTestBed
{
	class Button
	{
		public Vector2 position;
		public Vector2 size;
		public Vector3 color;
		public float transparency;
		public string text;

		private Action onClickEvent;

		public Button(Vector2 position, Vector2 size, Vector3 color, Action onClickEvent, float transparency = 0.0f, string text = null)
		{
			this.position = position;
			this.size = size;
			this.color = color;
			this.transparency = transparency;
			this.text = text;
			this.onClickEvent += onClickEvent;
		}

		public bool IsAtPosition(Vector2 inputPosition)
		{
			int xPosition = (int)position.X;
			int yPosition = (int)position.Y;
			int width = (int)size.X;
			int height = (int)size.Y;

			return inputPosition.X <= xPosition + width / 2 && inputPosition.X >= xPosition - width / 2 && inputPosition.Y <= yPosition + height / 2 && inputPosition.Y >= yPosition - height / 2;
		}

		public void OnClick()
		{
			onClickEvent.Invoke();
		}
	}
}
