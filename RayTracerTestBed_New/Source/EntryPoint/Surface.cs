﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace RayTracerTestBed
{
	public class Surface
	{
		public int width, height;
		public int[] pixels;
		static private bool _fontReady = false;
		static private Surface _font;
		static private int[] _fontRedir;
		static private string ch = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_-+={}[];:<>,.?/\\ ";

		private Bitmap bmp;

		public Surface(string fileName)
		{
			Bitmap bmp = new Bitmap(fileName);

			width = bmp.Width;
			height = bmp.Height;
			pixels = new int[width * height];

			BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			IntPtr ptr = data.Scan0;

			System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, width * height);
			bmp.UnlockBits(data);
		}

		//Initialization
		public Surface()
		{
			_font = new Surface("../../assets/font.png");
		}

		public void UpdateSurface(Bitmap bmp)
		{
			this.bmp = bmp;

			width = bmp.Width;
			height = bmp.Height;
			pixels = new int[width * height];

			BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			IntPtr ptr = data.Scan0;

			System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, width * height);
			bmp.UnlockBits(data);
		}

		public int GenTexture()
		{
			int id = GL.GenTexture();

			GL.BindTexture(TextureTarget.Texture2D, id);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

			return id;
		}

		public void Clear(int c)
		{
			for (int s = width * height, p = 0; p < s; p++)
				pixels[p] = c;
		}

		public void Box(int x1, int y1, int x2, int y2, int c)
		{
			int dest = y1 * width;

			for (int y = y1; y <= y2; y++, dest += width)
			{
				pixels[dest + x1] = c;
				pixels[dest + x2] = c;
			}

			int dest1 = y1 * width;
			int dest2 = y2 * width;

			for (int x = x1; x <= x2; x++)
			{
				pixels[dest1 + x] = c;
				pixels[dest2 + x] = c;
			}
		}

		public void Bar(int x1, int y1, int x2, int y2, int c)
		{
			int dest = y1 * width;

			for (int y = y1; y <= y2; y++, dest += width)
			{
				for (int x = x1; x <= x2; x++)
					pixels[dest + x] = c;
			}
		}

		public void Line(int x1, int y1, int x2, int y2, int c)
		{
			if ((x1 < 0) || (y1 < 0) || (x2 < 0) || (y2 < 0) ||
				(x1 >= width) || (x2 >= width) || (y1 >= height) || (y2 >= height))
				return;

			if (Math.Abs(x2 - x1) > Math.Abs(y2 - y1))
			{
				if (x2 < x1) { int h = x1; x1 = x2; x2 = h; h = y2; y2 = y1; y1 = h; }

				int l = x2 - x1;
				int dy = ((y2 - y1) * 8192) / l;
				y1 *= 8192;

				for (int i = 0; i < l; i++)
				{
					pixels[x1++ + (y1 / 8192) * width] = c;
					y1 += dy;
				}
			}
			else
			{
				if (y2 < y1) { int h = x1; x1 = x2; x2 = h; h = y2; y2 = y1; y1 = h; }

				int l = y2 - y1;
				int dx = ((x2 - x1) * 8192) / l;
				x1 *= 8192;

				for (int i = 0; i < l; i++)
				{
					pixels[x1 / 8192 + y1++ * width] = c;
					x1 += dx;
				}
			}
		}

		public void Plot(int x, int y, int c)
		{
			if ((x >= 0) && (y >= 0) && (x < width) && (y < height))
				pixels[x + y * width] = c;
		}

		public void Print(string t, int x, int y, int c)
		{
			if (!_fontReady)
			{
				_fontRedir = new int[256];

				for (int i = 0; i < 256; i++)
					_fontRedir[i] = 0;

				for (int i = 0; i < ch.Length; i++)
				{
					int l = (int)ch[i];
					_fontRedir[l & 255] = i;
				}

				_fontReady = true;
			}

			for (int i = 0; i < t.Length; i++)
			{
				int f = _fontRedir[(int)t[i] & 255];
				int dest = x + i * 12 + y * width;
				int src = f * 12;

				for (int v = 0; v < _font.height; v++, src += _font.width, dest += width)
				{
					for (int u = 0; u < 12; u++)
					{
						if ((_font.pixels[src + u] & 0xffffff) != 0)
							pixels[dest + u] = c;
					}
				}
			}
		}
	}
}
