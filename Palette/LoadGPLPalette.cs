using System;
using System.IO;
using System.Text;

namespace Tiled2dot8.Palette
{
    public static class LoadGplPalette 
    {
        public static GPL Load(string inputFile)
        {
            string[] input = File.ReadAllLines(inputFile, Encoding.ASCII);
            GPL palette = ConvertData(input);
            return palette;
        }


        /// <summary>
        /// convert asm file to collection of bytes
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static GPL ConvertData(string[] input)
        {
            GPL palette = new GPL();
            int index = 0;
            foreach (string line in input)
            {
                if (LineValid(line))
                {
                    byte[] rgb = SplitLine(line);
                    palette.Colours.Add(new RGBA(rgb[0], rgb[1], rgb[2], rgb[3]));
                    index++;
                }
                else
                {
                    palette.Headers.Add(line);
                }
            }
            return palette;
        }

        /// <summary>
        /// decide if is a valida line to be consumed or ignored e.g. emppty lines or comment lines should be ignored
        /// </summary>
        /// <param name="line">line to be validated</param>
        /// <returns>true if is valid to be parsed</returns>
        private static bool LineValid(string line)
        {
            line = line.Trim().Replace("\t", string.Empty);
            return !(line.Length == 0 || line.StartsWith("Name:") || line.StartsWith("Channels:") || line.StartsWith("Columns:") || line.StartsWith('#') || line.Equals("GIMP Palette", StringComparison.InvariantCultureIgnoreCase));
        }

        private static byte[] SplitLine(string line)
        {
            line = string.Join(" ", line.Replace('\t', ' ').Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            string[] cols = line.Split(new char[] { ' ' });
            byte[] rgba = new byte[4];
            rgba[3] = 255;      // default value
            for (int i = 0; i < rgba.Length; i++)
            {
                rgba[i] = Convert.ToByte(cols[i]);
            }
            return rgba;
        }
    }
}
