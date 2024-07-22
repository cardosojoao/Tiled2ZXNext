using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.ProcessLayers;


namespace Tiled2ZXNext
{
    public class ProcessLocations : IProcess
    {
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        private readonly List<Property> _properties;
        public ProcessLocations(Layer layer, Scene scene, List<Property> properties)
        {
            _rootLayer = layer;
            _scene = scene;
            _properties = properties;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + _rootLayer.Name);
            StringBuilder locationsCode = new();
            string fileName = _scene.Properties.GetProperty("FileName");
            foreach (Layer layer in _rootLayer.Layers)
            {
                if (layer.Visible)
                {
                    Console.WriteLine("Layer " + layer.Name);
                    locationsCode.Append('.');
                    locationsCode.Append(fileName);
                    locationsCode.Append('_');
                    locationsCode.Append(layer.Name);
                    locationsCode.Append('_');
                    locationsCode.Append(layer.Id);
                    locationsCode.AppendLine(":");
                    locationsCode.Append(WriteObjectsLayer(layer));
                }
            }
            return locationsCode;
        }

        /// <summary>
        /// write objects layer compressed, there is an header with the shared properties and then each line contain the object property
        /// </summary>
        /// <param name="layer">layer</param>
        /// <returns>string builder collection with header and data</returns>
        private StringBuilder WriteObjectsLayer(Layer layer)
        {
            int lengthData = 0;
            StringBuilder data = new(1024);
            StringBuilder headerType = new(200);
            StringBuilder header = new(200);
            List<StringBuilder> output = new() { header, data };


            int blockType = layer.Properties.GetPropertyInt("Type");

            layer.Properties.Merge(_properties);

            StringBuilder validator = Validator.ProcessValidator(layer.Properties);

            if (validator.Length > 0)
            {
                int prevBlockType = blockType;
                blockType += 128;
                headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine($"\t\t; data block type {prevBlockType} with validator");
                headerType.Append(validator);
            }
            else
            {
                headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).Append("\t\t; data block type\r\n");
            }
            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible).ToString("X2")).Append("\t\t; Objects count\r\n");
            lengthData += 1;

            data.Append("\t\t; X, Y\r\n");
            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    data.Append("\t\tdw $").Append((obj.X + Controller.Config.Offset.x).Double2Hex("X4"));
                    data.Append(",$").Append((obj.Y + Controller.Config.Offset.y).Double2Hex("X4"));
                    data.Append("\r\n");
                    lengthData += 4;
                }
            }

            // size must be 2B long (map is over 256 Bytes)
            headerType.Append("\t\tdw $").Append(lengthData.ToString("X4")).Append("\t\t; Block size\r\n");
            // insert header at begin
            header.Insert(0, headerType);
            header.Append(data);
            return header;
        }
    }
}