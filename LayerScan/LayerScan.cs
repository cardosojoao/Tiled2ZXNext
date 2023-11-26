using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.Entities;
using System.Reflection;

namespace Tiled2ZXNext
{
    public class LayerScan
    {
        private readonly LayerAreas _layerAreas;
        private readonly Layer _layer;
        private readonly int _tileSize;

        public LayerScan(Layer layer, int tileSize)
        {
            _layer = layer;
            _layerAreas = new() { Name = layer.Name };
            _tileSize = tileSize;
        }

        /// <summary>
        /// Areas are always rectangles
        /// </summary>
        /// <param name="grouplayer"></param>
        public LayerAreas ScanAreas()
        {
            //List<int> spriteSheets = new();
            //int GroupType = _layer.Properties.GetPropertyInt("Type");
            int index = 0;

            for (int row = 0; row < _layer.Height; row++)
            {
                for (int col = 0; col < _layer.Width; col++)
                {
                    uint tileId = (uint)_layer.Data[index];
                    Cell cell = new Cell() { TileID = tileId, X = col, Y = (_tileSize == 16 ? row - 1 : row) };  // tile 16 the left bottom corner is the coord from tiled
                    if (tileId != 0 && !_layerAreas.Included(cell))
                    {
                        Area area = _layer.ScanArea(cell, _tileSize);
                        area.SortHoriz();
                        _layerAreas.Areas.Add(area);
                    }
                    index++;
                }
            }
            return _layerAreas;
        }


        /// <summary>
        /// try to split areas in smaller areas  with a bigger fill percentage (fill percentage is the number of cells filled in the area)
        /// </summary>
        /// <returns></returns>
        public LayerAreas SplitAreas()
        {
            LayerAreas newAreas = new();
            foreach (Area area in _layerAreas.Areas)
            {
                if (area.Fill() != 100)
                {
                    newAreas.Areas.AddRange(area.SplitHoriz());
                    if (area.Cells.Count > 0)
                    {
                        if (area.Fill() != 100)
                        {
                            area.SortVert();
                            newAreas.Areas.AddRange(area.SplitVert());
                            if (area.Cells.Count > 0)
                            {
                                newAreas.Areas.Add(area.Explode());
                            }
                        }
                        else
                        {
                            newAreas.Areas.Add(area);
                        }
                    }
                }
                else
                {
                    newAreas.Areas.Add(area);
                }
            }
            return newAreas;
        }



        public LayerAreas ScanLayer()
        {
            for (int row = 0; row < _layer.Height; row += 4)
            {
                for (int col = 0; col < _layer.Width; col += 8)
                {
                    Area area = GetArea(row, col);
                    int fill =  area.Fill();
                    if( fill > 0)
                    {
                        _layerAreas.Areas.Add(area.Explode());

                    }

                    //uint tileId = (uint)_layer.Data[index];
                    //Cell cell = new Cell() { TileID = tileId, X = col, Y = (_tileSize == 16 ? row - 1 : row) };  // tile 16 the left bottom corner is the coord from tiled
                    //if (tileId != 0 && !_layerAreas.Included(cell))
                    //{
                    //    Area area = _layer.ScanArea(cell, _tileSize);
                    //    area.SortHoriz();
                    //    _layerAreas.Areas.Add(area);
                    //}
                    //index++;
                }
            }
            return _layerAreas;
        }


        private Area GetArea(int y, int x)
        {
            List<Cell> cells = new();
            for (int row = 0; row < 4; row++)
            {
                int index = (y + row) * _layer.Width + x;
                for (int col = 0; col < 8; col++)
                {
                    uint tileId = (uint)_layer.Data[index + col];
                    if (tileId > 0)
                    {
                        cells.Add(new Cell() { TileID = tileId, X = col+x, Y = row+y });
                    }
                }
            }
            return new Area(cells, _tileSize);
        }
    }
}
