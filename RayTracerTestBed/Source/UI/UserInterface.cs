using System;
using System.Collections.Generic;
using OpenTK;

namespace RayTracerTestBed
{
	class UserInterface
	{
		public List<Button> buttons = new List<Button>();

		public UserInterface(int screenWidth, int screenHeight)
		{
			Vector3 buttonColor = new Vector3(0.0f, 0.0f, 0.3f); //-new Vector3(1.0f, 1.0f, 1.0f);

			Button button = new Button(new Vector2(screenWidth - 90, 30), new Vector2(160, 40), buttonColor, Game.DeselectObject, 0.3f, "Deselect");
			buttons.Add(button);

			Button button2 = new Button(new Vector2(screenWidth - 90, 75), new Vector2(160, 40), buttonColor, Game.ResetCamera, 0.3f, "Reset Camera");
			buttons.Add(button2);

			Button button3 = new Button(new Vector2(screenWidth - 90, 120), new Vector2(160, 40), buttonColor, Game.ChangeScene, 0.3f, "Change Scene");
			buttons.Add(button3);

			Button button4 = new Button(new Vector2(screenWidth - 90, 165), new Vector2(160, 40), buttonColor, Game.ChangeTraceMethod, 0.3f, "Ch.TraceType");
			buttons.Add(button4);
		}

		private void PrintErrorMessage()
		{
			Console.WriteLine("This button has no functionality");
		}

		public void RenderText()
		{
			for (int i = 0; i < buttons.Count; i++)
			{
				Button button = buttons[i];

				if (button.text != null && button.text != string.Empty)
					Renderer.screen.Print(button.text, (int)(button.position.X - button.size.X / 2.0f) + 10, (int)(button.position.Y - button.size.Y / 6), 0xffffff);
			}
		}
	}
}
