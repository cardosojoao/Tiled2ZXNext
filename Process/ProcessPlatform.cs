using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;


namespace Tiled2ZXNext
{
    /// <summary>
    /// Process Environment Element Interaction
    /// </summary>
    public class ProcessPlatform : IProcess
    {
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        public ProcessPlatform(Layer layer, Scene scene)
        {
            _rootLayer = layer;
            _scene = scene;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + _rootLayer.Name);
            Layer layer = _rootLayer;
            StringBuilder locationsCode = new();
            string fileName = _scene.Properties.GetProperty("FileName");
            if (layer.Visible)
            {
                Console.WriteLine("Layer " + layer.Name);
                locationsCode.Append(fileName);
                locationsCode.Append('_');
                locationsCode.Append(layer.Name);
                locationsCode.Append('_');
                locationsCode.Append(layer.Id);
                locationsCode.Append(":\r\n");
                locationsCode.Append(WriteObjectsLayer(layer));
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

            StringBuilder error = new();

            int blockType = layer.Properties.GetPropertyInt("Type");        // this type will be used by the engine to map the parser

            headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).Append("\t\t; data block type\r\n");
            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible).ToString("X2")).Append("\t\t; Objects count\r\n");
            lengthData++;

            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    int pathObjectId = obj.Properties.GetPropertyInt("Path");
                    int pathId = GetPathId(pathObjectId);
                    int TemplateType = obj.Properties.GetPropertyInt("TemplateId");


                    data.Append("\t\tdb $").Append(TemplateType.Int2Hex("X2"));
                    data.Append("\t\t; Template ID\r\n");
                    lengthData++;

                    data.Append("\t\tdb $").Append(pathId.Int2Hex("X2"));
                    data.Append("\t\t; Path Id\r\n");
                    lengthData++;
                }
            }
            // size must be 2B long (map is over 256 Bytes)
            headerType.Append("\t\tdw $").Append(lengthData.ToString("X4")).Append("\t\t; Block size\r\n");
            // insert header at begin
            header.Insert(0, headerType);
            header.Append(data);
            return header;
        }



        private int GetPathId(int pathObjectId)
        {
            int id = -1;
            Layer pathLayer = _scene.Layers.Find(l => l.Name.Equals("path", StringComparison.InvariantCultureIgnoreCase));
            pathLayer = pathLayer.Layers.Find(l => l.Name.Equals("path", StringComparison.InvariantCultureIgnoreCase));
            Entities.Object path = pathLayer.Objects.Find(p => p.Id == pathObjectId);
            id = path.Properties.GetPropertyInt("Id");
            return id;
        }


    }
}