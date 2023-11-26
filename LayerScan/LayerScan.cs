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

        public LayerAreas ScanLayer()
        {
            for (int row = 0; row < _layer.Height; row += 4)
            {
                for (int col = 0; col < _layer.Width; col += 4)
                {
                    Area area = GetArea(row, col);
                    int fill =  area.Fill();
                    if( fill > 0)
                    {
                        _layerAreas.Areas.Add(area.Explode());

                    }
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
                for (int col = 0; col < 4; col++)
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
