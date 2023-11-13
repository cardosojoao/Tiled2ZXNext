using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

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
        /// 
        /// </summary>
        /// <param name="grouplayer"></param>
        public LayerAreas ScanAreas()
        {
            List<int> spriteSheets = new();
            int GroupType = TiledParser.GetPropertyInt(_layer.Properties, "Type");
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

    }
}
