using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext.Palette
{
    public class GPL
    {
        public List<string> Headers { get; set; } = new();
        public List<RGBA> Colours { get; set; } = new();
    }

    public class RGBA
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }
        public RGBA(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public RGBA(string hexaColour)
        {
            if (hexaColour[..1] == "#")
            {
                hexaColour = hexaColour[1..];
            }
            byte[] rgba = new byte[4];
            rgba[3] = 255;
            for (int i = 0; i < hexaColour.Length / 2; i++)
            {
                rgba[i] = Convert.ToByte(hexaColour.Substring(i * 2, 2), 16);
            }
            R = rgba[0];
            G = rgba[1];
            B = rgba[2];
            A = rgba[3];
        }


    }
}