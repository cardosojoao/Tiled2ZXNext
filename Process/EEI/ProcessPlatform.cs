using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Tiled2dot8.Entities;
using Tiled2dot8.Extensions;
using Tiled2dot8.ProcessLayers;

namespace Tiled2dot8.Process.EEI
{
    /// <summary>
    /// Process Environment Element Interaction
    /// </summary>
    public class ProcessPlatform : ProcessMaster // IProcess
    {
        //private readonly Layer _rootLayer;
        //private readonly Scene _scene;
        //private readonly List<Property> _properties;
        public ProcessPlatform(Layer layer, Scene scene, List<Property> properties): base(layer,scene,properties)
        {
            //_rootLayer = layer;
            //_scene = scene;
            //_properties = properties;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + base._layer.Name);
            StringBuilder locationsCode = new();
            string fileName = _scene.Properties.GetProperty("FileName");
            if (_layer.Visible)
            {
                Console.WriteLine("Layer " + _layer.Name);
                locationsCode.Append('.');
                locationsCode.Append(fileName);
                locationsCode.Append('_');
                locationsCode.Append(_layer.Name);
                locationsCode.Append('_');
                locationsCode.Append(_layer.Id);
                locationsCode.AppendLine(":");
                locationsCode.Append(WriteObjectsLayer(_layer));
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

            layer.Properties.Merge(_properties);
            CheckValidator();

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