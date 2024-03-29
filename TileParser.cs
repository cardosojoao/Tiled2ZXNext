﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tiled2ZXNext
{
    public class TiledParser : TiledRoot
    {
        List<int> spriteSheets;

        public TiledParser()
        {
            spriteSheets = new();
        }

        /// <summary>
        /// based on layer type call the correct constructor
        /// </summary>
        /// <param name="layer">current layer</param>
        /// <param name="compress">compress or not value</param>
        /// <returns>string build with layer contents</returns>
        //public StringBuilder WriteLayer(Layer layer)
        //{
        //    StringBuilder result;

        //    switch (layer.Type)
        //    {
        //        case "tilelayer":
        //            result = WriteTiledLayer(layer);
        //            break;
        //        case "objectgroup":
        //            //result = WriteObjectsLayer(layer);
        //            break;
        //        case "group":
        //            if(layer.Name.Equals("layer2", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                //new GroupScan().Scan(layer);
        //            }
        //            result = new StringBuilder();
        //            break;
        //        default:
        //            {
        //                result = new StringBuilder(50);
        //                result.Append("Invalid layer type [");
        //                result.Append(layer.Type);
        //                result.Append(']');
        //            }
        //            break;
        //    }
        //    return result;
        //}


        /// <summary>
        /// get header and data from tile writters and append everyrhing
        /// </summary>
        /// <param name="layer">current layer</param>
        /// <param name="compress">compress or not value</param>
        /// <returns>main string build with header and data appended</returns>
        //public StringBuilder WriteTiledLayer(Layer layer)
        //{
        //    StringBuilder output = new(5000);
        //    List<StringBuilder> data;

        //    data = WriteTiledLayerRaw(layer);
        //    output.Append(data[0]);
        //    output.Append(data[1]);

        //    return output;
        //}

        //public StringBuilder WriteTiledLayer(TileMap tileMap)
        //{
        //    StringBuilder output = new(5000);
        //    List<StringBuilder> data;

        //    data = WriteTiledLayerRaw(tileMap);
        //    output.Append(data[0]);
        //    output.Append(data[1]);

        //    return output;
        //}


        /// <summary>
        /// Write tilemap layer raw without any type of compression
        /// it does include the X mirror bit in the tile id (bit 7) that allow up 127 differen tiles
        /// </summary>
        /// <param name="layer">tilemap layer</param>
        /// <returns>string builder collection that includes header and data</returns>
        //private StringBuilder WriteTiledLayerRaw(Layer layer)
        //{
        //    int lengthData = 0;
        //    StringBuilder data = new(1024);
        //    StringBuilder headerType = new(200);
        //    StringBuilder header = new(100);
        //    //List<StringBuilder> output = new() { header, data };
        //    List<int> spriteSheets = new();
        //    int GroupType = GetPropertyInt(layer.Properties, "Type");
        //    //int GroupType = GroupTypeConvert(GetPropertyInt(layer.Properties, "Type"), false);

        //    headerType.Append("\t\tdb $");
        //    headerType.Append(GroupType.ToString("X2"));
        //    headerType.Append("\t\t; data type\r\n");

        //    // raw format
        //    int index = 0;
        //    for (int row = 0; row < layer.Height; row++)
        //    {
        //        data.Append("\t\t");
        //        data.Append("db $");
        //        for (int col = 0; col < layer.Width; col++)
        //        {
        //            if (col > 0)
        //            {   // not first col add separator
        //                data.Append(", $");
        //            }

        //            uint tileId = (uint)layer.Data[index];
        //            uint extend = 0;
        //            if (tileId == 0)
        //            {
        //                tileId = 63;  // means empty tile and must exist in tile patterns
        //            }
        //            else
        //            {
        //                const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        //                const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;

        //                extend = tileId & (FLIPPED_HORIZONTALLY_FLAG + FLIPPED_VERTICALLY_FLAG);
        //                // 0b11000000_00000000_00000000_00000000;
        //                // 0b00000000_00000000_00001100_00000000;

        //                extend >>= 28;                     // bit 31 -> bit 11

        //                tileId &= 0b11111111;
        //                var gidData = GetParsedGid((int)tileId);     // tile index is 0 based
        //                SpriteSheetsBlock(spriteSheets, gidData.tileSheet.TileSheetID);

        //                tileId = (uint)gidData.gid - 1;
        //                //tileId = (uint)CalcGid8((int)tileId);  no longer used we match the tiled id with tilemap tile ids

        //                //tileId &= 0b01111111;
        //                //tileId |= mirror;              // add mirror flag
        //            }


        //            data.Append(tileId.ToString("X2"));
        //            data.Append(", $");
        //            data.Append(extend.ToString("X2"));


        //            lengthData += 2;
        //            index++;
        //        }
        //        data.Append("\r\n");
        //    }
        //    lengthData++;

        //    headerType.Append("\t\tdw $");
        //    headerType.Append(lengthData.ToString("X4"));
        //    headerType.Append("\t\t; Block size\r\n");

        //    // get first sprite sheet code
        //    int spriteSheetID = spriteSheets[0];

        //    headerType.Append("\t\tdb $");
        //    headerType.Append(spriteSheetID.ToString("X2"));
        //    headerType.Append("\t\t; Sprite Sheet ID\r\n");

        //    if (spriteSheets.Count > 1)
        //    {
        //        Console.Error.WriteLine("Multiple sprite sheets in block " + layer.Name);
        //    }

        //    // insert header at begin
        //    header.Insert(0, headerType);
        //    header.Append(data);

        //    return header; ;
        //}


        /// <summary>
        /// write objects layer
        /// </summary>
        /// <param name="layer">layer</param>
        /// <param name="compress">compress or raw</param>
        /// <returns></returns>
        //public StringBuilder WriteObjectsLayer(Layer layer)
        //{
        //    StringBuilder output = new(1024);
        //    List<StringBuilder> data;

        //    int GroupType = GetPropertyInt(layer.Properties, "Type");

        //    data = WriteObjectsLayerCompress(layer);

        //    output.Append(data[0]);
        //    output.Append(data[1]);

        //    return output;
        //}


        /// <summary>
        /// write objects layer compressed, there is an header with the shared properties and then each line contain the object property
        /// </summary>
        /// <param name="layer">layer</param>
        /// <returns>string builder collection with header and data</returns>
        //public List<StringBuilder> WriteObjectsLayerCompress(Layer layer)
        //{
        //    int lengthData = 0;
        //    StringBuilder data = new(1024);
        //    StringBuilder headerType = new(200);
        //    StringBuilder header = new(200);
        //    List<StringBuilder> output = new() { header, data };
        //    List<int> spriteSheets = new();

        //    bool haveError = false;
        //    StringBuilder error = new();
        //    foreach (Object obj in layer.Objects)
        //    {
        //        if (obj.Visible)
        //        {
        //            if (!CheckObject(obj))
        //            {
        //                string objID = obj.Id + "-" + (obj.Name.Length > 0 ? obj.Name : "<na>");
        //                error.Clear();
        //                error.Append("Object ");
        //                error.Append(objID);
        //                error.Append($" Incorrect Width {obj.Width} or Height {obj.Height}, must be an even value\r\n");
        //                Console.WriteLine(error.ToString());
        //                haveError = true;
        //            }
        //        }
        //    }
        //    if (haveError)
        //    {
        //        throw new Exception("Incorrect height or Width must be even.");
        //    }


        //    int layerMask = GetPropertyInt(layer.Properties, "LayerMask");
        //    int layerId = GetPropertyInt(layer.Properties, "Layer");
        //    string tagName = GetProperty(layer.Properties, "Tag");            
        //    //int eventsID = GetPropertyInt(layer.Properties, "EventsConfig");
        //    int blockType = GetPropertyInt(layer.Properties, "Type");
        //    string bodyType = GetProperty(layer.Properties, "BodyType").ToLower();
        //    string eventName = GetProperty(layer.Properties, "EventName");

        //    int eventIndex = Controller.Tables["EventName"].Items.FindIndex(r => r.Equals(eventName, StringComparison.CurrentCultureIgnoreCase));
        //    int tagIndex = Controller.Tables["TagName"].Items.FindIndex(r => r.Equals(tagName, StringComparison.CurrentCultureIgnoreCase));

        //    //int GroupType = GroupTypeConvert(GetPropertyInt(layer.Properties, "Type"), true);

        //    headerType.Append("\t\tdb $");
        //    headerType.Append(blockType.ToString("X2"));
        //    headerType.Append("\t\t; data block type\r\n");

        //    layerMask *= 16;
        //    layerMask += layerId;
        //    header.Append("\t\tdb $");

        //    header.Append(layer.Objects.Count(c => c.Visible).ToString("X2"));  // only visible objects count
        //    header.Append("\t\t; Objects count\r\n");
        //    header.Append("\t\tdb $");
        //    header.Append(tagIndex.ToString("X2"));
        //    header.Append("\t\t; Tag [");
        //    header.Append(tagName);
        //    header.Append("]\r\n");
        //    header.Append("\t\tdb $");
        //    header.Append(layerMask.ToString("X2"));
        //    header.Append("\t\t; Layer\r\n");
        //    header.Append("\t\tdb $");
        //    header.Append(BodyTypeInt(bodyType).ToString("X2"));
        //    header.Append("\t\t; Body Type 0=trigger , 4=rigid\r\n"); // to be removed
        //    header.Append("\t\tdb $");
        //    header.Append(eventIndex.ToString("X2"));
        //    header.Append("\t\t; Event ID [");
        //    header.Append(eventName);
        //    header.Append("]\r\n");
            


        //    lengthData += 5;

        //    if (layer.Name.ToLower() == "collision")
        //    {
        //        data.Append("\t\t; X, Y, Width, Height\r\n");
        //    }
        //    else
        //    {
        //        data.Append("\t\t; X, Y, Width, Height, Gid\r\n");
        //    }
        //    foreach (Object obj in layer.Objects)
        //    {

        //        if (obj.Visible)
        //        {

        //            data.Append("\t\tdb $");
        //            data.Append(Double2Hex(obj.X));
        //            data.Append(",$");
        //            data.Append(Double2Hex(obj.Y));
        //            data.Append(",$");
        //            data.Append(Double2Hex(obj.Width));
        //            data.Append(",$");
        //            data.Append(Double2Hex(obj.Height));
        //            lengthData += 4;
        //            if (obj.Gid != null)
        //            {
        //                var gidData = GetParsedGid((int)obj.Gid);
        //                SpriteSheetsBlock(spriteSheets, gidData.tileSheet.TileSheetID);

        //                data.Append(",$");
        //                data.Append(Double2Hex(gidData.gid));
        //                lengthData++;
        //            }
        //            data.Append("\r\n");
        //        }
        //    }

        //    if (spriteSheets.Count > 0)
        //    {
        //        lengthData++;
        //    }

        //    headerType.Append("\t\tdw $");
        //    headerType.Append(lengthData.ToString("X4"));           // size must be 2B long (map is over 256 Bytes)
        //    headerType.Append("\t\t; Block size\r\n");

        //    if (spriteSheets.Count > 0)
        //    {
        //        // get first sprite sheet code
        //        int spriteSheetID = spriteSheets[0];

        //        headerType.Append("\t\tdw $");
        //        headerType.Append(spriteSheetID.ToString("X4"));
        //        headerType.Append("\t\t; Sprite Sheet ID\r\n");

        //        if (spriteSheets.Count > 1)
        //        {
        //            Console.Error.WriteLine("Multiple sprite sheets in block " + layer.Name);
        //        }
        //    }
        //    // insert header at begin
        //    header.Insert(0, headerType);
        //    return output;
        //}



        //private bool CheckObject(Object obj)
        //{
        //    bool result = true;


        //    if (obj.Height % 2 == 0)
        //    {
        //        obj.Y += (obj.Height / 2);
        //    }
        //    else
        //    {
        //        result = false;
        //    }
        //    if (obj.Width % 2 == 0)
        //    {
        //        obj.X += (obj.Width / 2);
        //    }
        //    else
        //    {
        //        result = false;
        //    }
        //    return result;
        //}
        ///// <summary>
        ///// process a tile layer using a pre existing buffer, each tile layer write on top of previous layers tiles
        ///// The finall result is the merge of all layers (using overwrite)
        ///// </summary>
        ///// <param name="layer">tile layer to process</param>
        ///// <param name="tileMap">buffer</param>
        ///// <returns>true is was a valid layer of false is not</returns>
        //public bool ParseLayer(Layer layer, TileMap tileMap)
        //{
        //    if (layer.Type == "tilelayer")
        //    {
        //        tileMap.Type = GetPropertyInt(layer.Properties, "Type");
        //        int index = 0;
        //        for (int row = 0; row < layer.Height; row++)
        //        {
        //            for (int col = 0; col < layer.Width; col++)
        //            {
        //                uint tileId = (uint)layer.Data[index];
        //                if (tileId > 0)
        //                {
        //                    const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        //                    const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;

        //                    uint extend = tileId & (FLIPPED_HORIZONTALLY_FLAG + FLIPPED_VERTICALLY_FLAG);
        //                    // 0b11000000_00000000_00000000_00000000;
        //                    // 0b00000000_00000000_00001100_00000000;
        //                    extend >>= 28;                     // bit 31 -> bit 11
        //                    tileId &= 0b11111111;
        //                    var gidData = GetParsedGid((int)tileId);     // tile index is 0 based
        //                    uint paletteIndex = (uint)gidData.tileSheet.PaletteIndex << 4;
        //                    extend |= paletteIndex;     // add pallete index
        //                    tileId = (uint)gidData.gid - 1;         // the gid is always +1, we need to ensure that the ranges are from 0..63 for the first block and 64..127 for the second block
        //                    tileMap.Tiles[index] = (new Tile() { Settings = extend, TileID = tileId });
        //                }
        //                index++;
        //            }
        //        }
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //}

        /// <summary>
        /// get property value as string
        /// </summary>
        /// <param name="properties">collection of properties</param>
        /// <param name="name">property name</param>
        /// <returns>propery value</returns>
        public static string GetProperty(List<Property> properties, string name)
        {
            string value = properties.Find(p => p.Name.ToLower() == name.ToLower()).Value;
            return value;
        }

        /// <summary>
        /// get property value as int
        /// </summary>
        /// <param name="properties">properties</param>
        /// <param name="name">property name</param>
        /// <returns>value as int in case error 255</returns>
        public static int GetPropertyInt(List<Property> properties, string name)
        {
            string value;
            try
            {
                if (properties == null)
                {
                    value = "255";
                }
                else
                {
                    value = properties.Find(p => p.Name.ToLower() == name.ToLower()).Value;
                }
            }
            catch
            {
                value = "255";
            }
            return int.Parse(value);
        }

        /// <summary>
        /// convert double to hexadecimal value
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>hexadecimal value</returns>
        public static string Double2Hex(double value)
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

        /// <summary>
        /// convert group type by cross checking compress parameter
        /// </summary>
        /// <param name="groupType">original group type</param>
        /// <param name="compress">compress flag</param>
        /// <returns>new group type</returns>

        /// <summary>
        /// convert the tileid to always have index 0 for the tileset used
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        //private static double ParentChildoffset(double parent, double child)
        //{
        //    double offset;

        //    if (parent > child)
        //    {
        //        offset = child - parent;
        //    }
        //    else
        //    {
        //        offset = parent - child;
        //    }
        //    return offset;
        //}

        /// <summary>
        /// based on width and height return the size code
        /// currently only accepts same width and height and return that value (1,2,4,8,16)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>value from width or height</returns>
        //private static double SpriteSize(double width, double height)
        //{
        //    double size = 0;
        //    if (width == 1 && height == 1)
        //    {
        //        size = 1;
        //    }
        //    else if (width == 2 && height == 2)
        //    {
        //        size = 2;
        //    }
        //    else if (width == 4 && height == 4)
        //    {
        //        size = 4;
        //    }
        //    else if (width == 8 && height == 8)
        //    {
        //        size = 8;
        //    }
        //    else if (width == 16 && height == 16)
        //    {
        //        size = 16;
        //    }
        //    return size;
        //}

        /// <summary>
        /// resolve GID by set gid to index of specific sprite sheet
        /// </summary>
        /// <param name="gid">Tiled sprite GID</param>
        /// <returns>tupple with gid and sprite sheet converted</returns>
        public  (int gid, Tileset tileSheet) GetParsedGid(int gid)
        {
            Tileset tileSheet = null;
            foreach (Tileset tileSet in this.Tilesets)
            {
                if (gid >= tileSet.Firstgid && gid <= tileSet.Lastgid)
                {
                    // we are going to load the two tilesheets in memory sequential the number can be sequential //  gid -= tileSet.Parsedgid;
                    tileSheet = tileSet;
                    break;
                }
            }
            return (gid, tileSheet);
        }

        /// <summary>
        /// keep track of all different Sprite sheets where used in the current block
        /// </summary>
        /// <param name="spriteSheets">collection of used sprite sheets</param>
        /// <param name="spriteSheetID">current sprite sheet ID</param>
        private static void SpriteSheetsBlock(List<int> spriteSheets, int spriteSheetID)
        {
            if (!spriteSheets.Exists(i => i == spriteSheetID))
            {
                spriteSheets.Add(spriteSheetID);
            }
        }
    }


    public class TileMap
    {
        public int Type { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public List<Tile> Tiles { get; set; }

        public TileMap(int height, int width)
        {
            Height = height;
            Width = width;
            Tiles = new List<Tile>(height * width);
            for (int i = 0; i < (height * width); i++)
            {
                Tiles.Add(new Tile() { Settings = 0, TileID = 0 });
            }
        }


    }
    public class Tile
    {
        public uint Settings;
        public uint TileID;
    }
}
