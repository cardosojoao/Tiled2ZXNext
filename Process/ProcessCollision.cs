using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
using Model = Tiled2ZXNext.Models;

namespace Tiled2ZXNext
{
    public class ProcessCollision : IProcess
    {
        private readonly Model.Layer _groupLayer;
        private readonly Model.Scene _tiledData;
        public ProcessCollision(Model.Layer layer, Model.Scene tiledData)
        {
            _groupLayer = layer;
            _tiledData = tiledData;
        }

        public StringBuilder Execute()
        {
            StringBuilder collisionCode = new();
            string fileName = _tiledData.Properties.GetProperty("FileName");
            foreach (Model.Layer layer in _groupLayer.Layers)
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
        private static StringBuilder WriteObjectsLayer(Model.Layer layer)
        {
            int lengthData = 0;
            StringBuilder data = new(1024);
            StringBuilder headerType = new(200);
            StringBuilder header = new(200);
            List<StringBuilder> output = new() { header, data };

            bool haveError = false;
            StringBuilder error = new();
            foreach (Model.Object obj in layer.Objects)
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

            headerType.Append("\t\tdb $");
            headerType.Append(blockType.ToString("X2"));
            headerType.Append("\t\t; data block type\r\n");

            layerMask *= 16;
            layerMask += layerId;
            header.Append("\t\tdb $");

            header.Append(layer.Objects.Count(c => c.Visible).ToString("X2"));  // only visible objects count
            header.Append("\t\t; Objects count\r\n");
            header.Append("\t\tdb $");
            header.Append(tagIndex.ToString("X2"));
            header.Append("\t\t; Tag [");
            header.Append(tagName);
            header.Append("]\r\n");
            header.Append("\t\tdb $");
            header.Append(layerMask.ToString("X2"));
            header.Append("\t\t; Layer\r\n");
            header.Append("\t\tdb $");
            header.Append(BodyTypeInt(bodyType).ToString("X2"));
            header.Append("\t\t; Body Type 0=trigger , 4=rigid\r\n");   // to be removed
            header.Append("\t\tdb $");
            header.Append(eventIndex.ToString("X2"));
            header.Append("\t\t; Event ID [");
            header.Append(eventName);
            header.Append("]\r\n");

            lengthData += 5;

            if (layer.Name.ToLower() == "collision")
            {
                data.Append("\t\t; X, Y, Width, Height\r\n");
            }
            else
            {
                data.Append("\t\t; X, Y, Width, Height, Gid\r\n");
            }
            foreach (Model.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    data.Append("\t\tdw $");
                    data.Append((obj.X + Controller.Config.Offset.x).Double2Hex( "X4"));
                    data.Append(",$");
                    data.Append((obj.Y + Controller.Config.Offset.y).Double2Hex( "X4"));
                    // data.Append("\r\n");
                    //data.Append("\t\tdb $");
                    data.Append(",$");
                    data.Append(obj.Width.Double2Hex());
                    data.Append(",$");
                    data.Append(obj.Height.Double2Hex());
                    lengthData += 6;
                    data.Append("\r\n");
                }
            }

            headerType.Append("\t\tdw $");
            headerType.Append(lengthData.ToString("X4"));               // size must be 2B long (map is over 256 Bytes)
            headerType.Append("\t\t; Block size\r\n");
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
        private static bool CheckObject(Model.Object obj)
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
