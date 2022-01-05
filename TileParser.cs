using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Tiled2ZXNext
{
    public class TiledParser : TiledRoot
    {
        public List<tileset> TileSetData;



        public TiledParser()
        {
            TileSetData = new List<tileset>();
        }

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
            StringBuilder result;

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
                        result.Append(']');
                    }
                    break;
            }
            return result;
        }



        public StringBuilder WriteTiledLayer(Layer layer, bool compress = false)
        {
            StringBuilder output = new(5000);
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

        private List<StringBuilder> WriteTiledLayerCompress(Layer layer)
        {
            int lengthData = 0;
            StringBuilder headerType = new(200);
            StringBuilder header = new(100);
            StringBuilder data = new(1024);
            List<StringBuilder> output = new() { header, data };
            List<int> spriteSheets = new();

            int colOut = 0;     // row output not the same row of tiled map
            int index = 0;      // index of tiled map 

            int GroupType = GroupTypeConvert(GetPropertyInt(layer.Properties, "Type"), true);

            //GroupType += 16;                                    // compress format

            headerType.Append("\t\tdb $");
            headerType.Append(GroupType.ToString("X2"));
            headerType.Append("\t\t; data type\r\n");



            for (int row = 0; row < layer.Height; row++)        // loop all rows of tiled map
            {
                for (int col = 0; col < layer.Width; col++)     // loop all columns of row
                {
                    if (col == 0)
                    {
                        header.Append("\t\tdb %");
                        lengthData++;
                    }

                    if (col % 8 == 0 && col > 0)
                    {
                        header.Append(",%");
                        lengthData++;
                    }

                    uint tileId = (uint)layer.Data[index];             // get tileId
                    index++;
                    if (tileId > 0)                             // if  tileId > 0 is a valid tile
                    {   // tile active
                        // Sprites are 0 based

                        uint mirrorH = tileId & 0b10000000_00000000_00000000_00000000;
                        mirrorH >>= 24;                     // bit 31 -> bit 7

                        tileId &= 0b11111111;

                        var gidData = GetParsedGid((int)tileId);
                        SpriteSheetsBlock(spriteSheets, gidData.spriteSheet);

                        tileId = (uint)gidData.gid-1;
                        tileId = (uint)CalcGid8((int)tileId);
                        
                        tileId &= 0b01111111;
                        tileId |= mirrorH;              // add mirror flag
                        
                        header.Append('1');                     // set tile active in header

                        if (colOut == 0)
                        {   // first element must initiate
                            data.Append("\t\tdb $");
                            data.Append(tileId.ToString("X2"));
                            lengthData++;
                        }
                        else
                        {   // column separator and value
                            data.Append(",$");
                            data.Append(tileId.ToString("X2"));
                            lengthData++;
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

            // increase sprite data size  by 1 to include the sprite sheet code
            lengthData++;

            headerType.Append("\t\tdw $");
            headerType.Append(lengthData.ToString("X4"));
            headerType.Append("\t\t; Block size\r\n");

            // get first sprite sheet code
            int spriteSheetID = spriteSheets[0];

            headerType.Append("\t\tdb $");
            headerType.Append(spriteSheetID.ToString("X2"));
            headerType.Append("\t\t; Sprite Sheet ID\r\n");

            if (spriteSheets.Count > 1)
            {
                Console.Error.WriteLine("Multiple sprite sheets in block " + layer.Name);
            }

            // insert header at begin
            header.Insert(0, headerType);
            return output;
        }

        private List<StringBuilder> WriteTiledLayerRaw(Layer layer)
        {
            int lengthData = 0;
            StringBuilder data = new(1024);
            StringBuilder headerType = new(200);
            StringBuilder header = new(100);
            List<StringBuilder> output = new() { header, data };
            List<int> spriteSheets = new();

            int GroupType = GroupTypeConvert(GetPropertyInt(layer.Properties, "Type"), false);

            headerType.Append("\t\tdb $");
            headerType.Append(GroupType.ToString("X2"));
            headerType.Append("\t\t; data type\r\n");

            // raw format
            int index = 0;
            for (int row = 0; row < layer.Height; row++)
            {
                data.Append("\t\tdb ");
                for (int col = 0; col < layer.Width; col++)
                {
                    if (col > 0)
                    {   // not first col add separator
                        data.Append(',');
                    }
                    data.Append('$');
                    uint tileId = (uint)layer.Data[index];
                    if (tileId == 0)
                    {
                        tileId = 255;  // means empty tile
                    }
                    else
                    {
                        uint mirrorH = tileId & 0b10000000_00000000_00000000_00000000;
                        mirrorH >>= 24;                     // bit 31 -> bit 7

                        tileId &= 0b11111111;
                        var gidData = GetParsedGid((int)tileId);     // tile index is 0 based
                        SpriteSheetsBlock(spriteSheets, gidData.spriteSheet);

                        tileId = (uint)gidData.gid - 1;
                        tileId = (uint)CalcGid8((int)tileId);

                        tileId &= 0b01111111;
                        tileId |= mirrorH;              // add mirror flag
                    }

                    data.Append(tileId.ToString("X2"));
                    lengthData++;
                    index++;
                }
                data.Append("\r\n");
            }
            lengthData++;

            headerType.Append("\t\tdw $");
            headerType.Append(lengthData.ToString("X4"));
            headerType.Append("\t\t; Block size\r\n");

            // get first sprite sheet code
            int spriteSheetID = spriteSheets[0];

            headerType.Append("\t\tdb $");
            headerType.Append(spriteSheetID.ToString("X2"));
            headerType.Append("\t\t; Sprite Sheet ID\r\n");

            if (spriteSheets.Count > 1)
            {
                Console.Error.WriteLine("Multiple sprite sheets in block " + layer.Name);
            }

            // insert header at begin
            header.Insert(0, headerType);
            return output;
        }

        public StringBuilder WriteObjectsLayer(Layer layer, bool compress)
        {
            StringBuilder output = new(1024);
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
            int lengthData = 0;
            StringBuilder data = new(500);
            StringBuilder headerType = new(500);
            StringBuilder header = new(500);
            List<StringBuilder> output = new() { header, data };

            // int GroupType = GetPropertyInt(layer.Properties, "Type");
            int GroupType = GroupTypeConvert(GetPropertyInt(layer.Properties, "Type"), false);

            headerType.Append("\t\tdb $");
            headerType.Append(GroupType.ToString("X2"));
            headerType.Append("\t\t; data type\r\n");

            header.Append("\t\tdb $");
            header.Append(layer.Objects.Count.ToString("X2"));
            header.Append("\t\t; Objects count\r\n");
            lengthData++;

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
                lengthData += 7;
            }
            headerType.Append("\t\tdw $");
            headerType.Append(lengthData.ToString("X4"));
            headerType.Append("\t\t; Block size\r\n");
            // insert header at begin
            header.Insert(0, headerType);
            return output;
        }

        public List<StringBuilder> WriteObjectsLayerCompress(Layer layer)
        {
            int lengthData = 0;
            StringBuilder data = new(1024);
            StringBuilder headerType = new(200);
            StringBuilder header = new(200);
            List<StringBuilder> output = new() { header, data };
            List<int> spriteSheets = new();

            int layerMask = GetPropertyInt(layer.Properties, "LayerMask");
            int layerId = GetPropertyInt(layer.Properties, "Layer");
            int ObjectType = GetPropertyInt(layer.Properties, "ObjectType");
            int EventsID = GetPropertyInt(layer.Properties, "EventsConfig");
            //int GroupType = GetPropertyInt(layer.Properties, "Type");


            int GroupType = GroupTypeConvert(GetPropertyInt(layer.Properties, "Type"), true);

            headerType.Append("\t\tdb $");
            headerType.Append(GroupType.ToString("X2"));
            headerType.Append("\t\t; data type\r\n");

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

            lengthData += 4;

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
                lengthData += 4;
                if (obj.Gid != null)
                {
                    var gidData = GetParsedGid((int)obj.Gid);
                    SpriteSheetsBlock(spriteSheets, gidData.spriteSheet);

                    data.Append(",$");
                    data.Append(Double2Hex(gidData.gid));
                    lengthData++;
                }
                data.Append("\r\n");
            }

            if (spriteSheets.Count > 0)
            {
                lengthData++;
            }

            headerType.Append("\t\tdb $");
            headerType.Append(lengthData.ToString("X2"));
            headerType.Append("\t\t; Block size\r\n");

            if (spriteSheets.Count > 0)
            {
                // get first sprite sheet code
                int spriteSheetID = spriteSheets[0];

                headerType.Append("\t\tdw $");
                headerType.Append(spriteSheetID.ToString("X4"));
                headerType.Append("\t\t; Sprite Sheet ID\r\n");

                if (spriteSheets.Count > 1)
                {
                    Console.Error.WriteLine("Multiple sprite sheets in block " + layer.Name);
                }
            }
            // insert header at begin
            header.Insert(0, headerType);
            return output;
        }

        /// <summary>
        /// Object collection with image
        /// first object and then image
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public List<StringBuilder> WriteObjectsLayerCompress2(Layer layer)
        {
            int lengthData = 0;
            StringBuilder data = new(1024);
            StringBuilder headerType = new(200);
            StringBuilder header = new(200);
            List<StringBuilder> output = new() { header, data };
            List<int> spriteSheets = new();

            int layerMask = GetPropertyInt(layer.Properties, "LayerMask");
            int layerId = GetPropertyInt(layer.Properties, "Layer");
            int ObjectType = GetPropertyInt(layer.Properties, "ObjectType");
            int EventsID = GetPropertyInt(layer.Properties, "EventsConfig");
            //int GroupType = GetPropertyInt(layer.Properties, "Type");

            int GroupType = GroupTypeConvert(GetPropertyInt(layer.Properties, "Type"), true);

            headerType.Append("\t\tdb $");
            headerType.Append(GroupType.ToString("X2"));
            headerType.Append("\t\t; data type\r\n");

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
            lengthData += 4;
            if (layer.Name == "Collision")
            {
                data.Append("\t\t; X, Y, Width, Height\r\n");
            }
            else
            {
                data.Append("\t\t; X, Y, Width, Height, offsetX, offsetY, size, Gid\r\n");
            }

            int objType = 0; // object
            Object cacheObject = null;

            //; foreach (Object obj in layer.Objects)
            for (int index = layer.Objects.Count - 1; index > -1; index--)
            {
                Object obj = layer.Objects[index];
                if (objType == 0)
                {
                    // resolve Object
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
                    lengthData += 4;
                }
                else
                {
                    // resolve Sprite
                    int size = (int)SpriteSize(obj.Width, obj.Height);
                    (int gid, int spriteSheet) gidData;
                    //if (size == 8)
                    //{
                    //    gidData = GetParsedGid((int)obj.Gid);
                    //    gidData.gid--;
                    //    gidData.gid = CalcGid8x8(gidData.gid);
                    //}
                    //else
                    {
                        gidData = GetParsedGid((int)obj.Gid);
                    }
                    SpriteSheetsBlock(spriteSheets, gidData.spriteSheet);

                    data.Append(",$");
                    data.Append(Double2Hex(ParentChildoffset(cacheObject.X, obj.X)));
                    data.Append(",$");
                    data.Append(Double2Hex(ParentChildoffset(cacheObject.Y, obj.Y)));
                    data.Append(",$");
                    data.Append(Double2Hex(SpriteSize(obj.Width, obj.Height)));
                    data.Append(",$");
                    data.Append(Double2Hex(gidData.gid));
                    data.Append("\r\n");
                    objType = 0;        // object
                    lengthData += 4;
                }

            }

            lengthData++;

            headerType.Append("\t\tdw $");
            headerType.Append(lengthData.ToString("X4"));
            headerType.Append("\t\t; Block size\r\n");

            // get first sprite sheet code
            int spriteSheetID = spriteSheets[0];

            headerType.Append("\t\tdb $");
            headerType.Append(spriteSheetID.ToString("X2"));
            headerType.Append("\t\t; Sprite Sheet ID\r\n");

            if (spriteSheets.Count > 1)
            {
                Console.Error.WriteLine("Multiple sprite sheets in block " + layer.Name);
            }


            // insert header at begin
            header.Insert(0, headerType);

            return output;
        }

        public static StringBuilder WriteHeader(Layer layer, bool compress)
        {
            StringBuilder output = new(200);

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
            string result;
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
                type &= mask;
                groupType = (int)type;
            }

            return groupType;
        }

        private static double ParentChildoffset(double parent, double child)
        {
            double offset;

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
                size = 1;
            }
            else if (width == 2 && height == 2)
            {
                size = 2;
            }
            else if (width == 4 && height == 4)
            {
                size = 4;
            }
            else if (width == 8 && height == 8)
            {
                size = 8;
            }
            else if (width == 16 && height == 16)
            {
                size = 16;
            }
            return size;
        }

        /// <summary>
        /// resolve GID by set gid to index of specific sprite sheet
        /// </summary>
        /// <param name="gid">Tiled sprite GID</param>
        /// <returns>tupple with gid and sprite sheet converted</returns>
        private (int gid, int spriteSheet) GetParsedGid(int gid)
        {
            int spriteSheet = 0;
            foreach (Tileset tileSet in this.Tilesets)
            {
                if (gid >= tileSet.Firstgid && gid <= tileSet.Lastgid)
                {
                    gid -= tileSet.Parsedgid;
                    spriteSheet = tileSet.SpriteSheetID;
                    break;
                }
            }
            return (gid, spriteSheet);
        }

        /// <summary>
        /// keep track of all different Sprite sheets where used in the current block
        /// </summary>
        /// <param name="spriteSheets">collection of used sprite sheets</param>
        /// <param name="spriteSheetID">current sprite sheet ID</param>
        private void SpriteSheetsBlock(List<int> spriteSheets, int spriteSheetID)
        {
            if (!spriteSheets.Exists(i => i == spriteSheetID))
            {
                spriteSheets.Add(spriteSheetID);
            }
        }

        public int CalcGid8x8(int gidSize8)
        {
            int gid = gidSize8 % 16;
            int row = gidSize8 >> 5;
            gid >>= 1;
            gid = row * 8 + gid;

            int cella = gidSize8 & 0b00000001;      // col
            cella <<= 6;
            int cellb = gidSize8 & 0b00010000;      // row
            cellb <<= 3;
            cella |= cellb;
            gid |= cella;
            return gid;
        }

        public static int CalcGid8(int gidSize8)
        {
            int p1 = (gidSize8 / 16);
            int p2 = gidSize8 % 8;
            int p3 = p2 / 2;
            int p4 = (gidSize8>>3) & 0b1;

            int gid = (p1 * 16) + p2 + (p3 * 2) + (p4 * 2);
            
            return gid;
        }
    }
}
