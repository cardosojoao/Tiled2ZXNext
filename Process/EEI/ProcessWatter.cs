﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.ProcessLayers;


namespace Tiled2ZXNext.Process.EEI
{
    /// <summary>
    /// Process Environment Element Interaction
    /// </summary>
    public class ProcessWatter : ProcessMaster // IProcess
    {
        //private readonly Layer _layer;
        //private readonly Scene _scene;
        //private readonly List<Property> _properties;
        public ProcessWatter(Layer layer, Scene scene, List<Property> properties) : base(layer, scene, properties)
        {
            //_layer = layer;
            //_scene = scene;
            //_properties = properties;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + _layer.Name);
            StringBuilder all = new();
            if (_layer.Visible)
            {
                //Console.WriteLine("Layer " + _layer.Name);
                string fileName = _scene.Properties.GetProperty("FileName");
                all.Append('.').Append(fileName).Append('_').Append(_layer.Name).Append('_').Append(_layer.Id).AppendLine(":");
                all.Append(WriteObjectsLayer(_layer));
            }
            return all;
        }

        /// <summary>
        /// write objects layer compressed, there is an header with the shared properties and then each line contain the object property
        /// </summary>
        /// <param name="layer">layer</param>
        /// <returns>string builder collection with header and data</returns>
        private StringBuilder WriteObjectsLayer(Layer layer)
        {
            //int lengthData = 0;
            //StringBuilder data = new(1024);
            //StringBuilder headerType = new(200);
            //StringBuilder header = new(200);
            //List<StringBuilder> output = new() { header, data };

            layer.Properties.Merge(_properties);
            CheckValidator();

            //int blockType = layer.Properties.GetPropertyInt("Type");        // this type will be used by the engine to map the parser

            //StringBuilder validator = Validator.ProcessLayerValidator(layer.Properties);
            //if (validator.Length > 0)
            //{
            //    int prevBlockType = blockType;
            //    blockType += 128;
            //    headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine($"\t\t; data block type {prevBlockType} with Validator.");
            //    headerType.Append(validator);
            //}
            //else
            //{
            //    headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine("\t\t; data block type");
            //}

            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible).ToString("X2")).Append("\t\t; Objects count\r\n");
            lengthData++;


            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    int x = (int)obj.X + 8 + 32;     // middle of first sprite, left start with 0
                    int y = (int)obj.Y - 8 + 32;      // middle of first sprite, top starts with 16
                    int length = obj.Properties.GetPropertyInt("Length");

                    data.Append("\t\tdw $").Append(x.Int2Hex("X4")).Append(", $").Append(y.Int2Hex("X4"));
                    data.Append("\t\t; x, y\r\n");
                    data.Append("\t\tdb $").Append(length.Int2Hex("X2"));
                    data.Append("\t\t; length ID\r\n");
                    lengthData += 5;
                }
            }
            // size must be 2Bytes long (map is over 256 Bytes)
            headerType.Append("\t\tdw $").Append(lengthData.ToString("X4")).Append("\t\t; Block size\r\n");
            // insert header at begin
            header.Insert(0, headerType);
            header.Append(data);
            return header;
        }
    }
}