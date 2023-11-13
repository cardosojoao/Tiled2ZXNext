using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tiled2ZXNext
{
    public class ProcessCollision : IProcess
    {
        private readonly Layer _groupLayer;
        private readonly TiledParser _tiledData;
        public ProcessCollision(Layer layer, TiledParser tiledData)
        {
            _groupLayer = layer;
            _tiledData = tiledData;
        }

        public StringBuilder Execute()
        {
            StringBuilder collisionCode = new();
            string fileName = TiledParser.GetProperty(_tiledData, "FileName");
            foreach (Layer layer in _groupLayer.Layers)
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
            foreach (Object obj in layer.Objects)
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


            int layerMask = TiledParser.GetPropertyInt(layer.Properties, "LayerMask");
            int layerId = TiledParser.GetPropertyInt(layer.Properties, "Layer");
            string tagName = TiledParser.GetProperty(layer.Properties, "Tag");
            int blockType = TiledParser.GetPropertyInt(layer.Properties, "Type");
            string bodyType = TiledParser.GetProperty(layer.Properties, "BodyType").ToLower();
            string eventName = TiledParser.GetProperty(layer.Properties, "EventName");

            int eventIndex = Controller.Tables["EventName"].Items.FindIndex(r => r.Equals(eventName, StringComparison.CurrentCultureIgnoreCase));
            int tagIndex = Controller.Tables["TagName"].Items.FindIndex(r => r.Equals(tagName, StringComparison.CurrentCultureIgnoreCase));

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
            foreach (Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    data.Append("\t\tdw $");
                    data.Append(TiledParser.Double2Hex(obj.X + Controller.Config.Offset.x,"X4"));
                    data.Append(",$");
                    data.Append(TiledParser.Double2Hex(obj.Y + Controller.Config.Offset.y,"X4"));
                    data.Append("\r\n");
                    data.Append("\t\tdb $");
                    data.Append(TiledParser.Double2Hex(obj.Width));
                    data.Append(",$");
                    data.Append(TiledParser.Double2Hex(obj.Height));
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
        private static bool CheckObject(Object obj)
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
