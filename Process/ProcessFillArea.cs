using System;
using System.Collections.Generic;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.Palette;
using Tiled2ZXNext.ProcessLayers;

namespace Tiled2ZXNext
{
    public class ProcessFillArea : IProcess
    {
        public Dictionary<Layer, List<Rectangle>> LayerAreas { get; private set; }
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
                if (layer.Visible && !IsLayerEmpty(layer.Data) ) ;
                {
                    layer.Properties.Merge(_properties);        // add parent extended properties

                    _blockType = layer.Properties.GetPropertyInt("Type");
                    if (_blockType == 15)
                    {
                        Console.WriteLine("Layer " + layer.Name);
                        List<Rectangle> areas = new LayerScanFill(layer).Scan();
                        LayerAreas.Add(layer, areas);
                    }
                }
            }

            StringBuilder backgroundFill = new();
            foreach (KeyValuePair<Layer, List<Rectangle>> data in LayerAreas)
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
                    backgroundFill.Append("\t\tdb $").Append(_blockType.ToString("X2"));
                    backgroundFill.AppendLine($"\t\t; data type - {layer.Name}");
                }
                _size = 0;

                int colourIndex = Controller.Palette.Colours.FindIndex(c => c.R == backColour.R && c.G == backColour.G && c.B == backColour.B && c.A == backColour.A);
                _size++;
                backgroundFill.Append("\t\tdb $").Append(_blockType.ToString("X2")).Append("\t\t; data block type\r\n");
                backgroundFill.Append("\t\tdw $").Append(_size.ToString("X4")).Append("\t\t; Block size\r\n");
                backgroundFill.Append("\t\tdb $").Append(colourIndex.ToString("X2")).Append("\t\t;\t").Append("RGBA ").Append(backColour.R).Append(',').Append(backColour.G).Append(',').Append(backColour.B).Append(',').Append(backColour.A).AppendLine();



                StringBuilder body = WriteAreas(data);
                backgroundFill.Append("\t\tdw $").Append(_size.ToString("X4")).Append($"\t\t; Size of block\r\n");
                backgroundFill.Append(body);
            }
            return backgroundFill;
        }


        private StringBuilder WriteAreas(KeyValuePair<Layer, List<Rectangle>> layer)
        {
            StringBuilder data = new(1024);
            //data.Append(WriteLayer2TileSets(layer.TileSet));
            data.Append("\t\tdb $").Append(layer.Value.Count.ToString("X2")).Append("\t\t; areas count\r\n");
            _size++;

            int areaIndex = 0;
            foreach (Rectangle area in layer.Value)
            {
                data.Append($"\t\t; area {areaIndex}\r\n");
                data.Append(AreaCode(area));
                areaIndex++;
            }
            return data;
        }

        private StringBuilder AreaCode(Rectangle rect)
        {
            StringBuilder areaCode = new();
            areaCode.Append("\t\tdb $");
            areaCode.Append(rect.X.ToString("X2"));
            areaCode.Append(",$");
            areaCode.Append(rect.Y.ToString("X2"));
            areaCode.Append(",$");
            areaCode.Append(rect.Width.ToString("X2"));
            areaCode.Append(",$");
            areaCode.Append(rect.Height.ToString("X2"));
            areaCode.Append("\t\t; x, y, width, height\r\n");
            _size += 4;
            return areaCode;
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
