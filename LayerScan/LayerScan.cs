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
        //public List<LayerAreas> LayerAreas { get; set; } = new List<LayerAreas>();
        private readonly LayerAreas _layerAreas;
        private readonly Layer _layer;

        public LayerScan(Layer layer)
        {
            _layer = layer;
            _layerAreas = new() { Name = layer.Name };
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
                    Cell cell = new Cell() { TileID = tileId, X = col, Y = row };
                    if (tileId != 0 && !_layerAreas.Included(cell))
                    {
                        Area area = _layer.ScanArea(cell);
                        area.SortHoriz();
                        _layerAreas.Areas.Add(area);
                        //Area area = new();
                        //layerAreas.Areas.Add(area);
                        //area.ScanArea(cell, _layer);
                        //area.SortHoriz();
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



        ///// <summary>
        ///// try to split the area in small chunks with 100% filled
        ///// check row by row (horizontal)
        ///// </summary>
        ///// <param name="area">are to split in smaller areas</param>
        ///// <returns>collection smaller areas</returns>
        //private static List<Area> SplitHoriz(Area area)
        //{
        //    List<Area> areas = new List<Area>();        // list of new areas
        //    int i = 0;                                  // index of area cell
        //    Area newArea = new();                       // new area object

        //    while (i < area.Cells.Count)
        //    {
        //        // if area is equal or smaller than 4x4 just stop and add the area
        //        (int width, int height) areaCurrentSize = area.GetSize();
        //        if (areaCurrentSize.width <= 4 && areaCurrentSize.width <= 4)
        //        {
        //            areas.Add(area.Explode());
        //            break;
        //        }

        //        List<Cell> row = GetRow(area, i);        // get next row
        //        (int width, int height) rowSize = Area.GetSize(row);
        //        (int width, int height) areaSize = Area.GetSize(newArea.Cells);

        //        // check if we have 100% of row fill
        //        bool rowOK = Area.GetSize(row).width == row.Count;
        //        // check if width of new area and row is the same, if first row area is always good
        //        bool areaOK = newArea.Cells.Count == 0 ? true : Area.GetSize(newArea.Cells).width == row.Count;

        //        // if row is 100% and area+row are 100% add row to area
        //        if (rowOK && areaOK)
        //        {
        //            newArea.Cells.AddRange(row);
        //            area.Cells.RemoveRange(i, row.Count);
        //        }
        //        else
        //        {
        //            // if row isn´t 100% or doesn't have the same width of new area
        //            // if exist a new area save it and reset new area
        //            if (newArea.Cells.Count > 0)
        //            {
        //                areas.Add(newArea);
        //                newArea = new Area();
        //            }
        //            else
        //            {
        //                // if we skip the row
        //                i += row.Count;
        //            }
        //        }
        //    }
        //    if (newArea.Cells.Count > 0)
        //    {
        //        areas.Add(newArea);
        //    }
        //    return areas;
        //}





    }

}
