using System;
using System.Collections.Generic;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.ProcessLayers;

namespace Tiled2ZXNext
{
    public class ProcessLayer2 : IProcess
    {
        public List<LayerAreas> LayersArea { get; private set; }
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        private readonly List<Property> _properties;
        private List<Tileset> _tileSets;
        private int _blockType;
        private int _size;
        private int _tileSize;
        public ProcessLayer2(Layer layer, Scene scene,  List<Property> properties )
        {
            _rootLayer = layer;
            LayersArea = new List<LayerAreas>();
            _scene = scene;
            _properties = properties;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + _rootLayer.Name);
            LayersArea.Clear();
            foreach (Layer layer in _rootLayer.Layers)
            {
                if (layer.Visible)
                {
                    layer.Properties.Merge(_properties);        // add parent extended properties

                    _tileSets = new();
                    Console.WriteLine("Layer " + layer.Name);
                    _blockType = layer.Properties.GetPropertyInt("Type");
                    _tileSize = layer.Properties.GetPropertyInt("Size");
                    LayerScan layerScan = new LayerScan(layer, _tileSize);
                    LayerAreas layerAreas = layerScan.ScanLayer();
                    layerAreas.Name = layer.Name;
                    layerAreas.Id = layer.Id;
                    layerAreas.Properties = layer.Properties;
                    LayerConvertCells(layerAreas);
                    layerAreas.TileSet = _tileSets;
                    // we don't include empty areas, the engine is prepared for that scenario and will crash!
                    if (layerAreas.Areas.Count > 0)
                    {
                        LayersArea.Add(layerAreas);
                    }


                }
            }

            StringBuilder layer2Code = new();
            foreach (LayerAreas layer in LayersArea)
            {
                layer2Code.Append('.');
                layer2Code.Append(_scene.FileName);
                layer2Code.Append('_');
                layer2Code.Append(layer.Name);
                layer2Code.Append('_');
                layer2Code.Append(layer.Id);
                layer2Code.AppendLine(":");

                StringBuilder validator = Validator.ProcessLayerValidator(layer.Properties);
                if(validator.Length>0)
                {
                    int prevBlockType = _blockType;
                    _blockType += 128;  // block with layer validator

                    layer2Code.Append("\t\tdb $").Append(_blockType.ToString("X2"));
                    layer2Code.Append($"\t\t; data type {prevBlockType} with validator, Layer2({_tileSize}x{_tileSize}) - {layer.Name}\r\n");
                    layer2Code.Append(validator);

                }
                else
                {
                    layer2Code.Append("\t\tdb $").Append(_blockType.ToString("X2"));
                    layer2Code.Append($"\t\t; data type Layer2({_tileSize}x{_tileSize}) - {layer.Name}\r\n");
                }
                _size = 0;
                
                StringBuilder body = WriteLayer2Area(layer);
                layer2Code.Append("\t\tdw $").Append(_size.ToString("X4")).Append($"\t\t; Size of block\r\n");
                layer2Code.Append(body);
            }
            return layer2Code;
        }


        private StringBuilder WriteLayer2Area(LayerAreas layer)
        {
            StringBuilder data = new(1024);
            data.Append(WriteLayer2TileSets(layer.TileSet));
            data.Append("\t\tdb $").Append(layer.Areas.Count.ToString("X2")).Append("\t\t; areas count\r\n");
            _size++;

            int areaIndex = 0;
            foreach (Area area in layer.Areas)
            {
                data.Append($"\t\t; area {areaIndex}\r\n");
                data.Append(AreaParseCode(area));
                areaIndex++;
            }
            return data;
        }

        private StringBuilder WriteLayer2TileSets(List<Tileset> tileSets)
        {
            StringBuilder code = new();
            // select tilesheet id
            int tileSheet0 = tileSets[0].TileSheetID;
            int tileSheet1 = tileSets.Count > 1 ? tileSets[1].TileSheetID : 255;
            int tileSheet2 = tileSets.Count > 2 ? tileSets[2].TileSheetID : 255;
            int tileSheet3 = tileSets.Count > 3 ? tileSets[3].TileSheetID : 255;
            // always show two tilesheets
            code.Append("\t\tdb $").Append(tileSheet0.ToString("X2"))
            .Append(",$").Append(tileSheet1.ToString("X2"))
            .Append(",$").Append(tileSheet2.ToString("X2"))
            .Append(",$").Append(tileSheet3.ToString("X2"))
            .Append("\t\t; Tile sheet Id  00..fe=valid, ff=not defined\r\n");
            _size += 4;
            return code;
        }
        private StringBuilder AreaParseCode(Area area)
        {
            (int x, int y, int width, int height) = area.GetSize();
            StringBuilder areaCode = new();
            areaCode.Append("\t\tdb $");
            areaCode.Append(area.X.ToString("X2")); _size++;
            areaCode.Append(",$");
            areaCode.Append(area.Y.ToString("X2")); _size++;
            areaCode.Append(",$");
            areaCode.Append(width.ToString("X2")); _size++;
            areaCode.Append(",$");
            areaCode.Append(height.ToString("X2")); _size++;
            areaCode.Append("\t\t; x, y, width, height\r\n");

            int rowCellCount = 0;
            foreach (Cell cell in area.Cells)
            {
                if (rowCellCount > 0)
                {   // not first col add separator
                    areaCode.Append(",$");
                }
                else
                {
                    areaCode.Append("\t\t");
                    areaCode.Append("db $");
                }
                areaCode.Append(cell.Settings.ToString("X2")); _size++;
                areaCode.Append(",$");
                areaCode.Append(cell.TileID.ToString("X2")); _size++;
                rowCellCount++;
                if (rowCellCount > 7)
                {
                    if (rowCellCount < area.Cells.Count)
                    {
                        rowCellCount = 0;
                        areaCode.Append("\r\n");
                    }
                }
            }
            areaCode.Append("\r\n");
            return areaCode;
        }

        /// <summary>
        /// update the layer tile settings by decoupling the setting from tileid
        /// and update the list of tile sets
        /// </summary>
        /// <param name="layer"></param>
        private void LayerConvertCells(LayerAreas layer)
        {
            foreach (Area area in layer.Areas)
            {
                foreach (Cell cell in area.Cells)
                {
                    CellConvert(cell);
                }
            }
        }


        /// <summary>
        /// split the TileId by settings and tileId
        /// </summary>
        /// <param name="cell">cell to be processed</param>
        private void CellConvert(Cell cell)
        {
            if (cell.TileID > 0)
            {
                uint tileId = cell.TileID;
                const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;  // bit 3: mirror X    //  1000    8 /2    = 4
                const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;    // bit 2: mirror Y    //  0100    4 /2    = 2
                const uint FLIPPED_ROTATE90_FLAG = 0x20000000;      // bit 1: Rotate      //  0010    10/2    = 5
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
                var gidData = _scene.GetParsedGid((int)tileId);     // tile index is 0 based
                uint paletteIndex = (uint)gidData.tileSheet.PaletteIndex << 4;
                //extend |= paletteIndex;                 // add pallete index
                tileId = (uint)gidData.gid - 1;         // the gid is always +1, we need to ensure that the ranges are from 0..63 for the first block and 64..127 for the second block
                                                        // if we don't have the tileset just add it

                int tileSheetIndex = _tileSets.FindIndex(t => t.TileSheetID == gidData.tileSheet.TileSheetID);
                if (tileSheetIndex == -1)
                {
                    _tileSets.Add(gidData.tileSheet);
                    tileSheetIndex = _tileSets.Count - 1;
                }
                // convert gid to tileSheetPosition
                cell.TileID = (uint)((tileId - _tileSets[tileSheetIndex].Parsedgid) + (tileSheetIndex * 64));
                cell.Settings = extend;
            }
        }

        /// <summary>
        /// sort tileset by gid number
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

        /// <summary>
        /// update the tile sets used to calculate the order of gid starting by 0
        /// </summary>
        private void TileSetUpdate()
        {
            // sort tile sets by gid number, to remap the gid in the layers
            _tileSets.Sort(CompareTileset);
            int firstGid = 0;
            foreach (Tileset tileset in _tileSets)
            {
                tileset.FirstgidMap = firstGid;
                firstGid += 64;                     // Todo: this depends on the tilesheet tile size 
            }
        }

        private void MapLayer(LayerAreas layerAreas)
        {
            foreach (Area area in layerAreas.Areas)
            {
                foreach (Cell cell in area.Cells)
                {
                    cell.TileID = MapCell(cell.TileID);
                }
            }
        }

        private uint MapCell(uint gid)
        {
            uint gidMap = 0;
            foreach (Tileset tileset in _tileSets)
            {
                if (gid >= tileset.Firstgid && gid <= tileset.Lastgid)
                {
                    gidMap = (uint)(gid - tileset.Firstgid + tileset.FirstgidMap + 1);
                    break;
                }
            }
            return gidMap;
        }
    }
}
