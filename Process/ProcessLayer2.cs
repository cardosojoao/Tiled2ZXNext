using System.Collections.Generic;
using System.Text;

namespace Tiled2ZXNext
{
    public class ProcessLayer2 : IProcess
    {
        public List<LayerAreas> LayersArea { get; private set; }
        private readonly Layer _groupLayer;
        private readonly TiledParser _tileData;
        private readonly List<Tileset> _tileSets = new();
        private int _type;
        private int _size;
        public ProcessLayer2(Layer layer, TiledParser tiledData)
        {
            _groupLayer = layer;
            LayersArea = new List<LayerAreas>();
            _tileData = tiledData;
            
            
        }

        public StringBuilder Execute()
        {
            
            LayersArea.Clear();
            foreach (Layer layer in _groupLayer.Layers)
            {
                if (layer.Visible)
                {
                    _type = TiledParser.GetPropertyInt(layer.Properties, "Type");
                    LayerScan layerScan = new LayerScan(layer);
                    layerScan.ScanAreas();
                    LayerAreas layerAreas = layerScan.SplitAreas();
                    layerAreas.Name = layer.Name;
                    LayerConvertCells(layerAreas);
                    TileSetUpdate();
                    MapLayer(layerAreas);
                    LayersArea.Add(layerAreas);
                }
            }
            StringBuilder layer2Code = new();
            foreach (LayerAreas layer in LayersArea)
            {
                _size = 0;
                layer2Code.Append("\t\tdb $");
                layer2Code.Append(_type.ToString("X2"));
                layer2Code.Append($"\t\t; data type Layer2-{layer.Name}\r\n");
                StringBuilder body = WriteLayer2Area(layer);
                layer2Code.Append("\t\tdw $");
                layer2Code.Append(_size.ToString("X4"));
                layer2Code.Append($"\t\t; Size of block\r\n");
                layer2Code.Append(body);
            }
            return layer2Code;
        }


        private StringBuilder WriteLayer2Area(LayerAreas layer)
        {
            StringBuilder data = new(1024);
            //data.Append("\t\tdb $");
            //data.Append(_type.ToString("X2")); _size++;
            //data.Append($"\t\t; data type Layer2-{layer.Name}\r\n");
            data.Append(WriteLayer2TileSets());
            data.Append("\t\tdb $");
            data.Append(layer.Areas.Count.ToString("X2")); _size++;
            data.Append("\t\t; areas count\r\n");
            int areaIndex = 0;
            foreach (Area area in layer.Areas)
            {
                data.Append($"\t\t; area {areaIndex}\r\n");
                data.Append(AreaParseCode(area));
                areaIndex++;
            }
            return data;
        }

        private StringBuilder WriteLayer2TileSets()
        {
            StringBuilder code = new();
            // select tilesheet id
            int tileSheet0 = _tileSets[0].TileSheetID;
            int tileSheet1 = _tileSets.Count > 1 ? _tileSets[1].TileSheetID : 255;
            // always show two tilesheets
            code.Append("\t\tdb $");
            code.Append(tileSheet0.ToString("X2")); _size++;
            code.Append(",$");
            code.Append(tileSheet1.ToString("X2")); _size++;
            code.Append("\t\t; Tile sheet Id  00..fe=valid, ff=not defined\r\n");
            return code;
        }
        private StringBuilder AreaParseCode(Area area)
        {
            StringBuilder areaCode = new();
            int x = area.Cells[0].X;
            int y = area.Cells[0].Y;

            (int width, int height) = area.GetSize();


            areaCode.Append("\t\tdb $");
            areaCode.Append(x.ToString("X2")); _size++;

            areaCode.Append(",$");
            areaCode.Append(y.ToString("X2")); _size++;

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
                areaCode.Append(cell.TileID.ToString("X2")); _size++;
                areaCode.Append(",$");
                areaCode.Append(cell.Settings.ToString("X2")); _size++;
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
                const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
                const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
                uint extend = tileId & (FLIPPED_HORIZONTALLY_FLAG + FLIPPED_VERTICALLY_FLAG);
                // 0b11000000_00000000_00000000_00000000;
                // 0b00000000_00000000_00001100_00000000;
                extend >>= 28;                     // bit 31 -> bit 11
                tileId &= 0xffff;
                var gidData = _tileData.GetParsedGid((int)tileId);     // tile index is 0 based
                uint paletteIndex = (uint)gidData.tileSheet.PaletteIndex << 4;
                extend |= paletteIndex;                 // add pallete index
                tileId = (uint)gidData.gid - 1;         // the gid is always +1, we need to ensure that the ranges are from 0..63 for the first block and 64..127 for the second block
                // if we don't have the tileset just add it
                if (_tileSets.FindIndex(t => t.TileSheetID == gidData.tileSheet.TileSheetID) == -1)
                {
                    _tileSets.Add(gidData.tileSheet);
                }
                cell.TileID = tileId;
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
                firstGid += 64;
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
