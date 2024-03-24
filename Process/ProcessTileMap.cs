﻿using System.Collections.Generic;
using System.Text;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.Entities;

namespace Tiled2ZXNext
{
    public class ProcessTileMap : IProcess
    {
        private readonly Layer _rootLayer;
        private readonly Scene _tileData;
        private readonly List<Tileset> _tileSets = new();
        
        /// <summary>
        /// process TileMap layers by merging all the layers into a single one
        /// </summary>
        /// <param name="layer">parent layer</param>
        /// <param name="scene">scene</param>
        public ProcessTileMap(Layer layer, Scene scene)
        {
            _rootLayer = layer;
            _tileData = scene;
        }


        public StringBuilder Execute()
        {
            StringBuilder tileMapCode = new();
            if (_rootLayer.Layers.Count > 0)
            {
                // create tileMap buffer
                TileMap tileMap = new(_rootLayer.Layers[0].Height, _rootLayer.Layers[0].Width);
                foreach (Layer layer in _rootLayer.Layers)
                {
                    if (layer.Visible)
                    {
                        MergeLayerTileMap(layer, tileMap);
                    }
                }
                TileSetUpdate();
                MapTileMap(tileMap);
                tileMapCode = WriteTiledLayer(tileMap);
            }
            return tileMapCode;
        }

        /// <summary>
        /// process a tile layer using a pre existing buffer, each tile layer write on top of previous layers tiles
        /// The final result is the merge of all layers (using overwrite)
        /// </summary>
        /// <param name="layer">tile layer to process</param>
        /// <param name="tileMap">buffer</param>
        /// <returns>true is was a valid layer of false is not</returns>
        public bool MergeLayerTileMap(Layer layer, TileMap tileMap)
        {
            if (layer.Type == "tilelayer")
            {
                tileMap.Type = layer.Properties.GetPropertyInt( "Type");
                int index = 0;
                for (int row = 0; row < layer.Height; row++)
                {
                    for (int col = 0; col < layer.Width; col++)
                    {
                        uint tileId = (uint)layer.Data[index];
                        if (tileId > 0)
                        {
                            const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;  // bit 3: mirror X    //  1000    8 /2    = 4
                            const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;    // bit 2: mirror Y    //  0100    4 /2    = 2
                            const uint FLIPPED_ROTATE90_FLAG = 0x20000000;      // bit 1: Rotate      //  0010    10/2    = 5
                            // 1010 0 00 00 00
                            const uint MASK = 0xe0000000;

                            uint extend = tileId & (MASK);
                            // 0b11100000_00000000_00000000_00000000;
                            // 0b00000000_00000000_00000000_00001110;
                            extend >>= 28;                     // bit 31 -> bit 3
                            if ((extend & (uint)2) > 0)
                            {
                                extend ^= (uint)8;
                            }
                            tileId &= 0xffff;
                            var gidData = _tileData.GetParsedGid((int)tileId);     // tile index is 0 based
                            uint paletteIndex = (uint)gidData.tileSheet.PaletteIndex << 4;
                            extend |= paletteIndex;                 // add palette index
                            tileId = (uint)gidData.gid - 1;         // the gid is always +1, we need to ensure that the ranges are from 0..63 for the first block and 64..127 for the second block
                            tileMap.Tiles[index] = (new Tile() { Settings = extend, TileID = tileId });
                            if (_tileSets.FindIndex(t => t.TileSheetID == gidData.tileSheet.TileSheetID) == -1)
                            {
                                _tileSets.Add(gidData.tileSheet);
                            }
                        }
                        index++;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private StringBuilder WriteTiledLayer(TileMap tilemap)
        {
            int lengthData = 0;
            StringBuilder data = new(1024);
            StringBuilder headerType = new(200);
            StringBuilder header = new(100);

            headerType.Append("\t\tdb $");
            headerType.Append(tilemap.Type.ToString("X2"));
            headerType.Append("\t\t; data type\r\n");

            // raw format
            int index = 0;
            for (int row = 0; row < tilemap.Height; row++)
            {
                data.Append("\t\t");
                data.Append("db $");
                for (int col = 0; col < tilemap.Width; col++)
                {
                    if (col > 0)
                    {   // not first col add separator
                        data.Append(",$");
                    }
                    Tile tile = tilemap.Tiles[index];

                    data.Append(tile.TileID.ToString("X2"));
                    data.Append(",$");
                    data.Append(tile.Settings.ToString("X2"));

                    lengthData += 2;
                    index++;
                }
                data.Append("\r\n");
            }
            lengthData += 4;        // add the 2 tilesheets bytes

            headerType.Append("\t\tdw $");
            headerType.Append(lengthData.ToString("X4"));
            headerType.Append("\t\t; Block size\r\n");
            // select tilesheet id
            int tileSheet0 = _tileSets[0].TileSheetID;
            int tileSheet1 = _tileSets.Count > 1 ? _tileSets[1].TileSheetID : 255;
            int tileSheet2 = _tileSets.Count > 2 ? _tileSets[2].TileSheetID : 255;
            int tileSheet3 = _tileSets.Count > 3 ? _tileSets[3].TileSheetID : 255;
            // always show two tilesheets
            headerType.Append("\t\tdb $");
            headerType.Append(tileSheet0.ToString("X2"));
            headerType.Append(", $");
            headerType.Append(tileSheet1.ToString("X2"));
            headerType.Append(", $");
            headerType.Append(tileSheet2.ToString("X2"));
            headerType.Append(", $");
            headerType.Append(tileSheet3.ToString("X2"));
            headerType.Append("\t\t; Tile sheet Id  00..fe=valid, ff=not defined\r\n");
            // insert header at begin
            header.Insert(0, headerType);
            header.Append(data);
            return header;
        }


        private void MapTileMap(TileMap layer)
        {
            foreach (Tile tile in layer.Tiles)
            {
                tile.TileID = MapTile(tile.TileID);
            }
        }
        

        /// <summary>
        /// re-assign the 
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        private uint MapTile(uint gid)
        {
            uint gidMap = 0;
            foreach (Tileset tileset in _tileSets)
            {
                if (gid >= tileset.Firstgid && gid <= tileset.Lastgid)
                {
                    gidMap = (uint)(gid - tileset.Firstgid + tileset.FirstgidMap+1);
                    break;
                }
            }
            return gidMap;
        }

        private void TileSetUpdate()
        {
            // sort tile sets by gid number, to remap the gid in the layers
            _tileSets.Sort(CompareTileset);
            int firstGid = 0;
            foreach (Tileset tileset in _tileSets)
            {
                tileset.FirstgidMap = firstGid;
                firstGid += 64;
            }
        }

        /// <summary>
        /// sort tileset by firstgid
        /// </summary>
        /// <param name="tileset1"></param>
        /// <param name="Tileset2"></param>
        /// <returns></returns>
        private static int CompareTileset(Tileset tileset1, Tileset Tileset2)
        {
            if (tileset1.Firstgid == Tileset2.Firstgid)
            {
                return 0;
            }
            else if (tileset1.Firstgid < Tileset2.Firstgid)
            {
                return -1;
            }
            return 1;
        }
    }
}