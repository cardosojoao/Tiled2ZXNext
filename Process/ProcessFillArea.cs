using System;
using System.Collections.Generic;
using System.Text;
using Tiled2dot8.Entities;
using Tiled2dot8.Extensions;
using Tiled2dot8.Palette;
using Tiled2dot8.ProcessLayers;
using Tiled2dot8.enums;

namespace Tiled2dot8
{
    public class ProcessFillArea : IProcess
    {
        public Dictionary<Layer, Dictionary<int, List<Rectangle>>> LayerAreas { get; private set; }
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        private readonly List<Property> _properties;
        private int _blockType;
        private int _size;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="scene"></param>
        /// <param name="properties"></param>
        public ProcessFillArea(Layer layer, Scene scene, List<Property> properties)
        {
            _rootLayer = layer;
            _scene = scene;
            _properties = properties;
            LayerAreas = new();
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + _rootLayer.Name);
            foreach (Layer layer in _rootLayer.Layers)
            {
                if (layer.Visible && !IsLayerEmpty(layer.Data)) ;
                {
                    layer.Properties.Merge(_properties);        // add parent extended properties

                    _blockType = layer.Properties.GetPropertyInt("Type");
                    if (_blockType == (int)BlockType.FillBlock)
                    {
                        Console.WriteLine("Layer " + layer.Name);
                        Dictionary<int, List<Rectangle>> areas = new LayerScanFill(layer).Scan();
                        LayerAreas.Add(layer, areas);
                    }
                }
            }

            StringBuilder backgroundFill = new();
            foreach (KeyValuePair<Layer, Dictionary<int, List<Rectangle>>> data in LayerAreas)
            {
                Layer layer = data.Key;
                RGBA backColour = new RGBA(layer.Colour);


                backgroundFill.Append('.');
                backgroundFill.Append(_scene.FileName);
                backgroundFill.Append('_');
                backgroundFill.Append(layer.Name.Replace(" ", "_"));
                backgroundFill.Append('_');
                backgroundFill.Append(layer.Id);
                backgroundFill.AppendLine(":");

                StringBuilder validator = Validator.ProcessLayerValidator(layer.Properties);
                if (validator.Length > 0)
                {
                    int prevBlockType = _blockType;
                    _blockType += 128;  // block with layer validator

                    backgroundFill.Append("\t\tdb $").Append(_blockType.ToString("X2"));
                    backgroundFill.AppendLine($"\t\t; data type {prevBlockType} with validator - {layer.Name}");
                    backgroundFill.Append(validator);

                }
                else
                {
                    backgroundFill.Append("\t\tdb $").Append(_blockType.ToString("X2")).AppendLine($"\t\t; data type - Fill Area");
                }
                _size = 0;

                int colourIndex = Controller.Palette.Colours.FindIndex(c => c.R == backColour.R && c.G == backColour.G && c.B == backColour.B && c.A == backColour.A);
                _size++;
                StringBuilder body = WriteAreas(data.Value);

                //backgroundFill.Append("\t\tdb $").Append(_blockType.ToString("X2")).Append("\t\t; data block type\r\n");
                backgroundFill.Append("\t\tdw $").Append(_size.ToString("X4")).Append("\t\t; Block size\r\n");
                
                backgroundFill.Append("\t\tdb $").Append(colourIndex.ToString("X2")).Append("\t\t;\t").Append("RGBA ").Append(backColour.R).Append(',').Append(backColour.G).Append(',').Append(backColour.B).Append(',').Append(backColour.A).AppendLine();
                backgroundFill.Append(body);
            }
            return backgroundFill;
        }


        private StringBuilder WriteAreas(Dictionary<int, List<Rectangle>> layer)
        {
            StringBuilder data = new(1024);
            data.Append("\t\tdb $").Append(layer.Count.ToString("X2")).Append("\t\t; blocks count\r\n");
            _size++;
            foreach (KeyValuePair<int, List<Rectangle>> kvp in layer)
            {
                data.Append("\t\tdb $").Append((kvp.Key*2).ToString("X2")).AppendLine("\t\t; block id");
                data.Append("\t\tdb $").Append(kvp.Value.Count.ToString("X2")).AppendLine("\t\t; areas count");
                _size+=2;
                foreach (Rectangle rectangle in kvp.Value)
                {
                    int x= rectangle.X - (kvp.Key * 8);

                    byte pos = (byte)((x<<5) + rectangle.Y);
                    byte dim = (byte)(((rectangle.Width-1) << 5) + rectangle.Height-1);
                    data.Append("\t\tdb $").Append(pos.ToString("X2")).Append(" ,$").Append(dim.ToString("X2")).Append("\t\t; area pos, dimemsion X=").Append(rectangle.X).Append(" Y=").Append(rectangle.Y).Append(" Width=").Append(rectangle.Width).Append(" Height=").Append(rectangle.Height).AppendLine();
                    _size += 2;
                }
            }
            return data;
        }


        private bool IsLayerEmpty(List<uint> data)
        {
            long result = 0;

            for (int i = 0; i < data.Count; i++)
            {
                result += data[i];
            }
            return result == 0;
        }
    }
}
