using System;
using System.Collections.Generic;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Palette;
using Tiled2ZXNext.Extensions;

namespace Tiled2ZXNext
{
    public class ProcessBackgroundColour : IProcess
    {
        private readonly Scene _scene;
        private readonly List<Property> _properties;
        private int _blockType;
        private int _size;
        public ProcessBackgroundColour(Layer layer, Scene scene, List<Property> properties)
        {
            _scene = scene;
            _properties = properties;
        }

        public StringBuilder Execute()
        {
            _size = 0;
            StringBuilder backgroundCode = new();
            Console.WriteLine("Background Colour");
            Layer layer = Utils.Find(_scene.Layers, "BackgroundColour");

            RGBA backColour;
            if (layer == null)
            {
                Console.WriteLine("Layer not defined, using color #000000");
                backColour = new RGBA("#000000ff");
                _blockType = 14;
            }
            else
            {
                Console.WriteLine("Layer " + layer.Name);
                layer.Properties.Merge(_properties);        // add parent extended properties
                _blockType = layer.Properties.GetPropertyInt("Type");
                backColour = new RGBA(layer.Colour);
            }

            int colourIndex = Controller.Palette.Colours.FindIndex(c => c.R == backColour.R && c.G == backColour.G && c.B == backColour.B && c.A == backColour.A);
            _size++;
            backgroundCode.Append("\t\tdb $").Append(_blockType.ToString("X2")).Append("\t\t; data block type Background Colour\r\n");
            backgroundCode.Append("\t\tdw $").Append(_size.ToString("X4")).Append("\t\t; Block size\r\n");
            backgroundCode.Append("\t\tdb $").Append(colourIndex.ToString("X2")).Append("\t\t;\t").Append("RGBA ").Append(backColour.R).Append(',').Append(backColour.G).Append(',').Append(backColour.B).Append(',').Append(backColour.A).AppendLine();
            return backgroundCode;
        }
    }
}
