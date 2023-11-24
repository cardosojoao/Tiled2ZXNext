using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;

namespace Tiled2ZXNext
{
    public class ProcessCollision : IProcess
    {
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        public ProcessCollision(Layer layer, Scene scene)
        {
            _rootLayer = layer;
            _scene = scene;
        }

        public StringBuilder Execute()
        {
            StringBuilder collisionCode = new();
            string fileName = _scene.Properties.GetProperty("FileName");
            foreach (Layer layer in _rootLayer.Layers)
            {
                if (layer.Visible)
                {
                    collisionCode.Append(fileName);
                    collisionCode.Append('_');
                    collisionCode.Append(layer.Name);
                    collisionCode.Append('_');
                    collisionCode.Append(layer.Id);
                    collisionCode.Append(":\r\n");
                    collisionCode.Append(WriteObjectsLayer(layer));
                }
            }
            return collisionCode;
        }

        /// <summary>
        /// write objects layer compressed, there is an header with the shared properties and then each line contain the object property
        /// </summary>
        /// <param name="layer">layer</param>
        /// <returns>string builder collection with header and data</returns>
        private static StringBuilder WriteObjectsLayer(Layer layer)
        {
            int lengthData = 0;
            StringBuilder data = new(1024);
            StringBuilder headerType = new(200);
            StringBuilder header = new(200);
            List<StringBuilder> output = new() { header, data };

            bool haveError = false;
            StringBuilder error = new();
            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    if (!CheckObject(obj))
                    {
                        string objID = obj.Id + "-" + (obj.Name.Length > 0 ? obj.Name : "<na>");
                        error.Clear();
                        error.Append("Object ");
                        error.Append(objID);
                        error.Append($" Incorrect Width {obj.Width} or Height {obj.Height}, must be an even value\r\n");
                        Console.WriteLine(error.ToString());
                        haveError = true;
                    }
                }
            }
            if (haveError)
            {
                throw new Exception("Incorrect height or Width must be even.");
            }


            int layerMask = layer.Properties.GetPropertyInt("LayerMask");
            int layerId = layer.Properties.GetPropertyInt("Layer");
            string tagName = layer.Properties.GetProperty("Tag");
            int blockType = layer.Properties.GetPropertyInt("Type");
            string bodyType = layer.Properties.GetProperty("BodyType").ToLower();
            string eventName = layer.Properties.GetProperty("EventName");

            int eventIndex = Scene.Instance.Tables["EventName"].Items.FindIndex(r => r.Equals(eventName, StringComparison.CurrentCultureIgnoreCase));
            int tagIndex = Scene.Instance.Tables["TagName"].Items.FindIndex(r => r.Equals(tagName, StringComparison.CurrentCultureIgnoreCase));

            headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).Append("\t\t; data block type\r\n");

            // merge layer mask with lasyID in a single byte
            layerMask *= 16;
            layerMask += layerId;
            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible).ToString("X2")).Append("\t\t; Objects count\r\n");
            header.Append("\t\tdb $").Append(tagIndex.ToString("X2")).Append("\t\t; Tag [").Append(tagName).Append("]\r\n");
            header.Append("\t\tdb $").Append(layerMask.ToString("X2")).Append("\t\t; Layer\r\n");
            header.Append("\t\tdb $").Append(BodyTypeInt(bodyType).ToString("X2")).Append("\t\t; Body Type 0=trigger , 4=rigid\r\n");   // to be removed
            header.Append("\t\tdb $").Append(eventIndex.ToString("X2")).Append("\t\t; Event ID [").Append(eventName).Append("]\r\n");
            lengthData += 5;

            if (layer.Name.Equals("collision", StringComparison.InvariantCultureIgnoreCase))
            {
                data.Append("\t\t; X, Y, Width, Height\r\n");
            }
            else
            {
                data.Append("\t\t; X, Y, Width, Height, Gid\r\n");
            }
            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    data.Append("\t\tdw $").Append((obj.X + Controller.Config.Offset.x).Double2Hex( "X4"));
                    data.Append(",$").Append((obj.Y + Controller.Config.Offset.y).Double2Hex( "X4"));
                    data.Append(",$").Append(obj.Width.Double2Hex());
                    data.Append(",$").Append(obj.Height.Double2Hex());
                    data.Append("\r\n");
                    lengthData += 6;
                }
            }

            // size must be 2B long (map is over 256 Bytes)
            headerType.Append("\t\tdw $").Append(lengthData.ToString("X4")).Append("\t\t; Block size\r\n");
            // insert header at begin
            header.Insert(0, headerType);
            header.Append(data);
            return header;
        }

        /// <summary>
        /// collision objects must be even in width and height
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool CheckObject(Entities.Object obj)
        {
            bool result = true;
            if (obj.Height % 2 == 0)
            {
                obj.Y += (obj.Height / 2);
            }
            else
            {
                result = false;
            }
            if (obj.Width % 2 == 0)
            {
                obj.X += (obj.Width / 2);
            }
            else
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// convert body type of int type
        /// </summary>
        /// <param name="bodyType">name of body</param>
        /// <returns>int type of body</returns>
        private static int BodyTypeInt(string bodyType)
        {
            switch (bodyType)
            {
                case "trigger":
                    return 0;
                case "rigid":
                    return 4;
                default:
                    return 4;
            }
        }
    }
}
