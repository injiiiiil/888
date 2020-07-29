#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using OpenRA.FileFormats;
using OpenRA.Primitives;

namespace OpenRA.Graphics
{
	public static class Util
	{
		// yes, our channel order is nuts.
		static readonly int[] ChannelMasks = { 2, 1, 0, 3 };

		public static void FastCreateQuad(Vertex[] vertices, float3 o, Sprite r, int2 samplers, float paletteTextureIndex, int nv, float3 size, float3 tint)
		{
			var b = new float3(o.X + size.X, o.Y, o.Z);
			var c = new float3(o.X + size.X, o.Y + size.Y, o.Z + size.Z);
			var d = new float3(o.X, o.Y + size.Y, o.Z + size.Z);
			FastCreateQuad(vertices, o, b, c, d, r, samplers, paletteTextureIndex, tint, nv);
		}

		public static void FastCreateQuad(Vertex[] vertices,
			float3 a, float3 b, float3 c, float3 d,
			Sprite r, int2 samplers, float paletteTextureIndex,
			float3 tint, int nv)
		{
			float sl = 0;
			float st = 0;
			float sr = 0;
			float sb = 0;

			// See shp.vert for documentation on the channel attribute format
			var attribC = r.Channel == TextureChannel.RGBA ? 0x02 : ((byte)r.Channel) << 1 | 0x01;
			attribC |= samplers.X << 6;
			var ss = r as SpriteWithSecondaryData;
			if (ss != null)
			{
				sl = ss.SecondaryLeft;
				st = ss.SecondaryTop;
				sr = ss.SecondaryRight;
				sb = ss.SecondaryBottom;

				attribC |= ((byte)ss.SecondaryChannel) << 4 | 0x08;
				attribC |= samplers.Y << 9;
			}

			var fAttribC = (float)attribC;
			vertices[nv] = new Vertex(a, r.Left, r.Top, sl, st, paletteTextureIndex, fAttribC, tint);
			vertices[nv + 1] = new Vertex(b, r.Right, r.Top, sr, st, paletteTextureIndex, fAttribC, tint);
			vertices[nv + 2] = new Vertex(c, r.Right, r.Bottom, sr, sb, paletteTextureIndex, fAttribC, tint);
			vertices[nv + 3] = new Vertex(c, r.Right, r.Bottom, sr, sb, paletteTextureIndex, fAttribC, tint);
			vertices[nv + 4] = new Vertex(d, r.Left, r.Bottom, sl, sb, paletteTextureIndex, fAttribC, tint);
			vertices[nv + 5] = new Vertex(a, r.Left, r.Top, sl, st, paletteTextureIndex, fAttribC, tint);
		}

		public static void FastCopyIntoChannel(Sprite dest, byte[] src)
		{
			var destData = dest.Sheet.GetData();
			var width = dest.Bounds.Width;
			var height = dest.Bounds.Height;

			if (dest.Channel == TextureChannel.RGBA)
			{
				var destStride = dest.Sheet.Size.Width;
				unsafe
				{
					// Cast the data to an int array so we can copy the src data directly
					fixed (byte* bd = &destData[0])
					{
						var data = (int*)bd;
						var x = dest.Bounds.Left;
						var y = dest.Bounds.Top;

						var k = 0;
						for (var j = 0; j < height; j++)
						{
							for (var i = 0; i < width; i++)
							{
								var r = src[k++];
								var g = src[k++];
								var b = src[k++];
								var a = src[k++];
								var cc = Color.FromArgb(a, r, g, b);

								data[(y + j) * destStride + x + i] = PremultiplyAlpha(cc).ToArgb();
							}
						}
					}
				}
			}
			else
			{
				var destStride = dest.Sheet.Size.Width * 4;
				var destOffset = destStride * dest.Bounds.Top + dest.Bounds.Left * 4 + ChannelMasks[(int)dest.Channel];
				var destSkip = destStride - 4 * width;

				var srcOffset = 0;
				for (var j = 0; j < height; j++)
				{
					for (var i = 0; i < width; i++, srcOffset++)
					{
						destData[destOffset] = src[srcOffset];
						destOffset += 4;
					}

					destOffset += destSkip;
				}
			}
		}

		public static void FastCopyIntoSprite(Sprite dest, Png src)
		{
			var destData = dest.Sheet.GetData();
			var destStride = dest.Sheet.Size.Width;
			var width = dest.Bounds.Width;
			var height = dest.Bounds.Height;

			unsafe
			{
				// Cast the data to an int array so we can copy the src data directly
				fixed (byte* bd = &destData[0])
				{
					var data = (int*)bd;
					var x = dest.Bounds.Left;
					var y = dest.Bounds.Top;

					var k = 0;
					for (var j = 0; j < height; j++)
					{
						for (var i = 0; i < width; i++)
						{
							Color cc;
							if (src.Palette == null)
							{
								var r = src.Data[k++];
								var g = src.Data[k++];
								var b = src.Data[k++];
								var a = src.Data[k++];
								cc = Color.FromArgb(a, r, g, b);
							}
							else
								cc = src.Palette[src.Data[k++]];

							data[(y + j) * destStride + x + i] = PremultiplyAlpha(cc).ToArgb();
						}
					}
				}
			}
		}

		public static Color PremultiplyAlpha(Color c)
		{
			if (c.A == byte.MaxValue)
				return c;
			var a = c.A / 255f;
			return Color.FromArgb(c.A, (byte)(c.R * a + 0.5f), (byte)(c.G * a + 0.5f), (byte)(c.B * a + 0.5f));
		}

		public static Color PremultipliedColorLerp(float t, Color c1, Color c2)
		{
			// Colors must be lerped in a non-multiplied color space
			var a1 = 255f / c1.A;
			var a2 = 255f / c2.A;
			return PremultiplyAlpha(Color.FromArgb(
				(int)(t * c2.A + (1 - t) * c1.A),
				(int)((byte)(t * a2 * c2.R + 0.5f) + (1 - t) * (byte)(a1 * c1.R + 0.5f)),
				(int)((byte)(t * a2 * c2.G + 0.5f) + (1 - t) * (byte)(a1 * c1.G + 0.5f)),
				(int)((byte)(t * a2 * c2.B + 0.5f) + (1 - t) * (byte)(a1 * c1.B + 0.5f))));
		}

		// TODO this should be refactored to not use float[].
		public static float[] MatrixAABBMultiply(FloatMatrix4x4 mtx, float[] bounds)
		{
			// Corner offsets
			var ix = new uint[] { 0, 0, 0, 0, 3, 3, 3, 3 };
			var iy = new uint[] { 1, 1, 4, 4, 1, 1, 4, 4 };
			var iz = new uint[] { 2, 5, 2, 5, 2, 5, 2, 5 };

			// Vectors to opposing corner
			var ret = new[]
			{
				float.MaxValue, float.MaxValue, float.MaxValue,
				float.MinValue, float.MinValue, float.MinValue
			};

			// Transform vectors and find new bounding box
			for (var i = 0; i < 8; i++)
			{
				var vec = new float4(bounds[ix[i]], bounds[iy[i]], bounds[iz[i]], 1);
				var tvec = mtx * vec;

				ret[0] = Math.Min(ret[0], tvec.X / tvec.W);
				ret[1] = Math.Min(ret[1], tvec.Y / tvec.W);
				ret[2] = Math.Min(ret[2], tvec.Z / tvec.W);
				ret[3] = Math.Max(ret[3], tvec.X / tvec.W);
				ret[4] = Math.Max(ret[4], tvec.Y / tvec.W);
				ret[5] = Math.Max(ret[5], tvec.Z / tvec.W);
			}

			return ret;
		}
	}
}
