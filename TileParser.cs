using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext
{
    public class TiledParser : TiledRoot
    {

        public bool ExistLayer(string layerName)
        {
            return Layers.Exists(l => l.Name.ToLower() == layerName.ToLower());
        }

        public Layer GetLayer(string layerName)
        {
            return Layers.Find(l => l.Name.ToLower() == layerName.ToLower());
        }


        public StringBuilder WriteLayer(Layer layer, bool compress = false)
        {
            StringBuilder result = null;

            switch (layer.Type)
            {
                case "tilelayer":
                    result = WriteTiledLayer(layer, compress);
                    break;
                case "objectgroup":
                    result = WriteObjectsLayer(layer, compress);
                    break;
                default:
                    {
                        result = new StringBuilder(50);
                        result.Append("Invalid layer type [");
                        result.Append(layer.Type);
                        result.Append("]");
                    }
                    break;
            }
            return result;
        }



        public static StringBuilder WriteTiledLayer(Layer layer, bool compress = false)
        {
            StringBuilder output = new StringBuilder(5000);
            List<StringBuilder> data;

            if (compress)
            {
                data = WriteTiledLayerCompress(layer);
                output.Append(data[0]);
                output.Append(data[1]);
            }
            else
            {
                data = WriteTiledLayerRaw(layer);
                output.Append(data[0]);
                output.Append(data[1]);
            }
            return output;
        }

        private static List<StringBuilder> WriteTiledLayerCompress(Layer layer)
        {
            StringBuilder header = new StringBuilder(100);
            StringBuilder data = new StringBuilder(1024);
            List<StringBuilder> output = new List<StringBuilder>() { header, data };

            int colOut = 0;     // row output not the same row of tiled map
            int index = 0;      // index of tiled map 

            int GroupType = GroupTypeConvert(GetPropertyInt(layer.Properties, "Type"), true);

            //GroupType += 16;                                    // compress format

            header.Append("\t\tdb $");
            header.Append(GroupType.ToString("X2"));
            header.Append("\t\t; data type\r\n");

            for (int row = 0; row < layer.Height; row++)        // loop all rows of tiled map
            {
                header.Append("\t\tdw %");

                for (int col = 0; col < layer.Width; col++)     // loop all columns of row
                {
                    int tileID = layer.Data[index];             // get tileId
                    index++;
                    if (tileID > 0)                             // if  tileId > 0 is a valid tile
                    {   // tile active
                        // Sprites are 0 based
                        tileID--;
                        header.Append('1');                     // set tile active in header
                        if (colOut == 0)
                        {   // first element must initiate
                            data.Append("\t\tdb $");
                            data.Append(tileID.ToString("X2"));
                        }
                        else
                        {   // column separator and value
                            data.Append(",$");
                            data.Append(tileID.ToString("X2"));
                        }
                        colOut++;
                        // max tiles per line 8
                        if (colOut > 7)
                        {
                            colOut = 0;
                            data.Append("\r\n");
                        }
                    }
                    else
                    {   // set tile inactive in header
                        header.Append('0');
                    }
                }
                header.Append("\r\n");
            }
            data.Append("\r\n");                                // last line of data, add new line
            return output;
        }

        private static List<StringBuilder> WriteTiledLayerRaw(Layer layer)
        {
            StringBuilder data = new StringBuilder(1024);
            StringBuilder header = new StringBuilder(100);
            List<StringBuilder> output = new List<StringBuilder>() { header, data };

            int GroupType = GroupTypeConvert(GetPropertyInt(layer.Properties, "Type"), false);

            header.Append("\t\tdb $");
            header.Append(GroupType.ToString("X2"));
            header.Append("\t\t; data type\r\n");

            // raw format
            int index = 0;
            for (int row = 0; row < layer.Height; row++)
            {
                data.Append("\t\tdb ");
                for (int col = 0; col < layer.Width; col++)
                {
                    if (col > 0)
                    {   // not first col add separator
                        data.Append(",");
                    }
                    data.Append("$");
                    int tileId = layer.Data[index];
                    if (tileId == 0)
                    {
                        tileId = 255;  // means empty tile
                    }
                    else
                    {
                        tileId--;     // tile index is 0 based
                    }

                    data.Append(tileId.ToString("X2"));
                    index++;
                }
                data.Append("\r\n");
            }
            return output;
        }

        public static StringBuilder WriteObjectsLayer(Layer layer, bool compress)
        {
            StringBuilder output = new StringBuilder(1024);
            List<StringBuilder> data;

            int GroupType = GroupTypeConvert(GetPropertyInt(layer.Properties, "Type"), compress);

            switch (GroupType)
            {
                case 16:
                    {
                        data = WriteObjectsLayerCompress(layer);
                        break;
                    }
                case 17:
                    {
                        data = WriteObjectsLayerCompress2(layer);
                        break;
                    }
                default:
                    {
                        data = WriteObjectsLayerRaw(layer);
                    }
                    break;
            }
            output.Append(data[0]);
            output.Append(data[1]);

            return output;
        }

        /// <summary>
        /// objects collection
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static List<StringBuilder> WriteObjectsLayerRaw(Layer layer)
        {
            StringBuilder data = new StringBuilder(500);
            StringBuilder header = new StringBuilder(500);
            List<StringBuilder> output = new List<StringBuilder>() { header, data };

            int GroupType = GetPropertyInt(layer.Properties, "Type");
            header.Append("\t\tdb $");
            header.Append(GroupType.ToString("X2"));
            header.Append("\t\t; data type\r\n");

            header.Append("\t\tdb $");
            header.Append(layer.Objects.Count.ToString("X2"));
            header.Append("\t\t; Objects count\r\n");


            data.Append("\t\t; X, Y, Width, Height, ObjectType, Layer, EventsID\r\n");
            foreach (Object obj in layer.Objects)
            {
                int layerMask = GetPropertyInt(obj.Properties, "LayerMask");
                int layerId = GetPropertyInt(obj.Properties, "Layer");
                int ObjectType = GetPropertyInt(obj.Properties, "ObjectType");
                int EventsID = GetPropertyInt(obj.Properties, "EventsConfig");
                layerMask *= 16 + layerId;


                data.Append("\t\tdb $");
                data.Append(Double2Hex(obj.X));
                data.Append(",$");
                data.Append(Double2Hex(obj.Y));
                data.Append(",$");
                data.Append(Double2Hex(obj.Width));
                data.Append(",$");
                data.Append(Double2Hex(obj.Height));
                data.Append(",$");
                data.Append(Double2Hex(ObjectType));
                data.Append(",$");
                data.Append(Double2Hex(layerMask));
                data.Append(",$");
                data.Append(Double2Hex(EventsID));
                data.Append("\r\n");

            }
            return output;
        }

        public static List<StringBuilder> WriteObjectsLayerCompress(Layer layer)
        {
            StringBuilder data = new StringBuilder(1024);
            StringBuilder header = new StringBuilder(200);
            List<StringBuilder> output = new List<StringBuilder>() { header, data };

            int layerMask = GetPropertyInt(layer.Properties, "LayerMask");
            int layerId = GetPropertyInt(layer.Properties, "Layer");
            int ObjectType = GetPropertyInt(layer.Properties, "ObjectType");
            int EventsID = GetPropertyInt(layer.Properties, "EventsConfig");
            int GroupType = GetPropertyInt(layer.Properties, "Type");


            header.Append("\t\tdb $");
            header.Append(GroupType.ToString("X2"));
            header.Append("\t\t; data type\r\n");

            layerMask *= 16 + layerId;
            header.Append("\t\tdb $");
            header.Append(ObjectType.ToString("X2"));
            header.Append("\t\t; Type\r\n");
            header.Append("\t\tdb $");
            header.Append(layerMask.ToString("X2"));
            header.Append("\t\t; Layer\r\n");
            header.Append("\t\tdb $");
            header.Append(EventsID.ToString("X2"));
            header.Append("\t\t; Events config\r\n");
            header.Append("\t\tdb $");
            header.Append(layer.Objects.Count.ToString("X2"));
            header.Append("\t\t; Objects count\r\n");

            if (layer.Name == "Collision")
            {
                data.Append("\t\t; X, Y, Width, Height\r\n");
            }
            else
            {
                data.Append("\t\t; X, Y, Width, Height, Gid\r\n");
            }
            foreach (Object obj in layer.Objects)
            {
                data.Append("\t\tdb $");
                data.Append(Double2Hex(obj.X));
                data.Append(",$");
                data.Append(Double2Hex(obj.Y));
                data.Append(",$");
                data.Append(Double2Hex(obj.Width));
                data.Append(",$");
                data.Append(Double2Hex(obj.Height));
                if (obj.Gid != null)
                {
                    data.Append(",$");
                    data.Append(Double2Hex((int)obj.Gid));
                }
                data.Append("\r\n");
            }
            return output;
        }

        /// <summary>
        /// Object collection with image
        /// first object and then image
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static List<StringBuilder> WriteObjectsLayerCompress2(Layer layer)
        {
            StringBuilder data = new StringBuilder(1024);
            StringBuilder header = new StringBuilder(200);
            List<StringBuilder> output = new List<StringBuilder>() { header, data };

            int layerMask = GetPropertyInt(layer.Properties, "LayerMask");
            int layerId = GetPropertyInt(layer.Properties, "Layer");
            int ObjectType = GetPropertyInt(layer.Properties, "ObjectType");
            int EventsID = GetPropertyInt(layer.Properties, "EventsConfig");
            int GroupType = GetPropertyInt(layer.Properties, "Type");


            header.Append("\t\tdb $");
            header.Append(GroupType.ToString("X2"));
            header.Append("\t\t; data type\r\n");

            layerMask *= 16 + layerId;
            header.Append("\t\tdb $");
            header.Append(ObjectType.ToString("X2"));
            header.Append("\t\t; Type\r\n");
            header.Append("\t\tdb $");
            header.Append(layerMask.ToString("X2"));
            header.Append("\t\t; Layer\r\n");
            header.Append("\t\tdb $");
            header.Append(EventsID.ToString("X2"));
            header.Append("\t\t; Events config\r\n");
            header.Append("\t\tdb $");
            header.Append((layer.Objects.Count / 2).ToString("X2"));
            header.Append("\t\t; Objects count\r\n");

            if (layer.Name == "Collision")
            {
                data.Append("\t\t; X, Y, Width, Height\r\n");
            }
            else
            {
                data.Append("\t\t; X, Y, Width, Height, Gid\r\n");
            }

            int objType = 0; // object
            Object cacheObject = null;

            //; foreach (Object obj in layer.Objects)
            for (int index = layer.Objects.Count - 1; index > -1; index--)
            {
                Object obj = layer.Objects[index];
                if (objType == 0)
                {
                    data.Append("\t\tdb $");
                    data.Append(Double2Hex(obj.X));
                    data.Append(",$");
                    data.Append(Double2Hex(obj.Y));
                    data.Append(",$");
                    data.Append(Double2Hex(obj.Width));
                    data.Append(",$");
                    data.Append(Double2Hex(obj.Height));
                    cacheObject = obj;
                    objType = 1;        // sprite
                }
                else
                {
                    data.Append(",$");
                    data.Append(Double2Hex(TiledParser.ParentChildoffset(cacheObject.X, obj.X)));
                    data.Append(",$");
                    data.Append(Double2Hex(TiledParser.ParentChildoffset(cacheObject.Y, obj.Y)));
                    data.Append(",$");
                    data.Append(Double2Hex(TiledParser.SpriteSize(obj.Width, obj.Height)));
                    data.Append(",$");
                    data.Append(Double2Hex((int)obj.Gid));
                    data.Append("\r\n");
                    objType = 0;        // object
                }

            }
            return output;
        }

        public static StringBuilder WriteHeader(Layer layer, bool compress)
        {
            StringBuilder output = new StringBuilder(200);

            output.Append(";\r\n");
            output.Append(";\r\n");
            output.Append("; Layer [");
            output.Append(layer.Name);
            output.Append("] X-");
            output.Append(layer.Width);
            output.Append(", Y-");
            output.Append(layer.Height);
            if (compress)
            {
                output.Append(" (Compressed)");
            }
            else
            {
                output.Append(" (Raw)");
            }

            output.Append("\r\n");
            output.Append(";\r\n");

            return output;
        }

        public static string GetProperty(List<Property> properties, string name)
        {
            string? value = properties.Find(p => p.Name.ToLower() == name.ToLower()).Value;
            return value;
        }

        public static int GetPropertyInt(List<Property> properties, string name)
        {
            string value;
            try
            {
                value = properties.Find(p => p.Name.ToLower() == name.ToLower()).Value;
            }
            catch
            {
                value = "255";
            }
            return int.Parse(value);
        }

        private static string Double2Hex(double value)
        {
            string result = null;
            if (value >= 0)
            {
                result = ((int)Math.Round(value, 0)).ToString("X2");
            }
            else
            {
                result = ((int)Math.Round(value, 0)).ToString("X2").Substring(6, 2);
            }
            return result;

        }

        private static int GroupTypeConvert(int groupType, bool compress)
        {
            if (groupType < 16)
            {
                if (compress)
                {
                    groupType += 16;
                }
            }
            else if (groupType >= 32)
            {
                uint type = (ushort)groupType;
                uint mask = 0b_0000_0000_0001_1111;
                type = type & mask;
                groupType = (int)type;
            }

            return groupType;
        }

        private static double ParentChildoffset(double parent, double child)
        {
            double offset = 0;

            if (parent > child)
            {
                offset = child - parent;
            }
            else
            {
                offset = parent - child;
            }
            return offset;
        }

        private static double SpriteSize(double width, double height)
        {
            double size = 0;
            if (width == 1 && height == 1)
            {
                size = 0;
            }
            else if (width == 2 && height == 2)
            {
                size = 1;
            }
            else if (width == 4 && height == 4)
            {
                size = 2;
            }
            else if (width == 8 && height == 8)
            {
                size = 3;
            }
            else if (width == 16 && height == 16)
            {
                size = 4;
            }
            return size;
        }

    }
}
