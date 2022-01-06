using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Tiled2ZXNext
{
    static class ImageHelper
    {

        public static void t(string input, string output)
        {
            Image t = Image.FromFile(input);

            int height = t.Height;
            int width = t.Width;


            Bitmap raw = new Bitmap(t);
            var s = raw.PhysicalDimension;
            var p = raw.PixelFormat;



            using (var stream = File.Open(output, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    for (int row = 0; row < 8; row++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            Color c = raw.GetPixel(col, row);


                            byte cc= rgb888_to_rgb332(ToUint(c), color_mode_t.COLORMODE_DISTANCE);

                            byte r1 = (byte)((c.R & 0b111) << 5);
                            byte r2 = (byte)((c.G & 0b111) << 2);
                            byte r3 = (byte)(c.B & 0b11);

                            byte r = (byte)(r1 + r2 + r3);
                            //dest.SetPixel(row, col, c);
                            writer.Write((byte)cc);
                        }
                    }
                }
            }
        }

        private static uint ToUint(this Color c)
        {
            return (uint)(((c.A << 24) | (c.R << 16) | (c.G << 8) | c.B) & 0xffffffffL);
        }

        private static Color ToColor(this uint value)
        {
            return Color.FromArgb((byte)((value >> 24) & 0xFF),
                       (byte)((value >> 16) & 0xFF),
                       (byte)((value >> 8) & 0xFF),
                       (byte)(value & 0xFF));
        }


        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        enum color_mode_t
        {
            COLORMODE_DISTANCE,
            COLORMODE_ROUND,
            COLORMODE_FLOOR,
            COLORMODE_CEIL
        }



        static byte rgb888_to_rgb332(uint rgb888, color_mode_t color_mode)
        {
            byte r8 = (byte)(rgb888 >> 16);
            byte g8 = (byte)(rgb888 >> 8);
            byte b8 = (byte)rgb888;
            byte r3 = c8_to_c3(r8, color_mode);
            byte g3 = c8_to_c3(g8, color_mode);
            byte b2 = c8_to_c2(b8, color_mode);
            return RGB332(r3, g3, b2);
        }

        static byte RGB332(byte r3, byte g3, byte b2)
        {
            return (byte)((r3 << 5) | (g3 << 2) | b2);
        }

        static byte c8_to_c3(byte c8, color_mode_t color_mode)
        {
            double c3 = (c8 * 7.0) / 255.0;

            switch (color_mode)
            {
                case color_mode_t.COLORMODE_FLOOR:
                    return (byte)Math.Floor(c3);
                case color_mode_t.COLORMODE_CEIL:
                    return (byte)Math.Ceiling(c3);
                case color_mode_t.COLORMODE_ROUND:
                case color_mode_t.COLORMODE_DISTANCE:
                // Fall through
                default:
                    return (byte)Math.Round(c3);
            }
        }

        static byte c8_to_c2(byte c8, color_mode_t color_mode)
        {
            double c2 = (c8 * 3.0) / 255.0;
            switch (color_mode)
            {
                case color_mode_t.COLORMODE_FLOOR:
                    return (byte)Math.Floor(c2);
                case color_mode_t.COLORMODE_CEIL:
                    return (byte)Math.Ceiling(c2);
                case color_mode_t.COLORMODE_ROUND:
                case color_mode_t.COLORMODE_DISTANCE:
                // Fall through
                default:
                    return (byte)Math.Round(c2);
            }
        }


        //static void write_tiles_png(string png_filename, uint tile_width, uint tile_height, uint tile_offset, uint tile_count, uint tilesheet_width, uint bitmap_width, uint bitmap_height)
        //{
        //	uint tile_size = tile_width * tile_height;
        //	uint data_size = tile_count * tile_size;
        //	bitmap_width = Math.Min(tilesheet_width, tile_count * tile_width);
        //	bitmap_height = (uint)Math.Ceiling((double)data_size / bitmap_width / tile_height) * tile_height;
        //	uint bitmap_size = *bitmap_width * *bitmap_height;
        //	uint tile_cols = *bitmap_width / tile_width;

        //	uint8_t* p_image = malloc(bitmap_size);

        //	memset(p_image, 0, bitmap_size);

        //	for (int t = 0; t < tile_count; t++)
        //	{
        //		uint tile_id = tile_offset + t;
        //		uint tile_x = t % tile_cols;
        //		uint tile_y = t / tile_cols;
        //		uint src_offset = tile_id * tile_size;
        //		uint dst_offset = tile_y * *bitmap_width * tile_height + tile_x * tile_width;

        //		for (int y = 0; y < tile_height; y++)
        //		{
        //			for (int x = 0; x < tile_width; x++)
        //			{
        //				uint src_index = src_offset + y * tile_width + x;
        //				uint dst_index = dst_offset + y * *bitmap_width + x;

        //				if (m_args.colors_4bit)
        //					src_index >>= 1;

        //				if (m_args.colors_4bit)
        //				{
        //					if ((x & 1) == 0)
        //						p_image[dst_index] = m_tiles[src_index] >> 4;
        //					else
        //						p_image[dst_index] = m_tiles[src_index] & 0xf;
        //				}
        //				else
        //				{
        //					p_image[dst_index] = m_tiles[src_index];
        //				}
        //			}
        //		}
        //	}

        //	write_png_bits(png_filename, p_image, *bitmap_width, *bitmap_height, false);
        //}
    }
}
