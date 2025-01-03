using System;
using System.Collections.Generic;

namespace Tiled2ZXNext.Entities
{
    public class Area
    {
        private readonly int _tileSize;
        public List<Cell> Cells { get; private set; } = new List<Cell>();
        public int X { get; set; }
        public int Y { get; set; }

        public Area(int tileSize)
        {
            _tileSize = tileSize;
        }

        /// <summary>
        /// set the size of area and populates with default values
        /// </summary>
        /// <param name="width">width of area</param>
        /// <param name="height">height of area</param>
        public Area(int width, int height, int tileSize)
        {
            _tileSize = tileSize;
            int step = tileSize / 8;
            Cells = new List<Cell>(width * height);

            for (int r = 0; r < height; r += step)
            {
                for (int c = 0; c < width; c += step)
                {
                    Cell cell = new() { X = c, Y = r, TileID = 0, Source = 0 };
                    Cells.Add(cell);
                }
            }
        }

        /// <summary>
        /// add range of cells to area
        /// </summary>
        /// <param name="range"></param>
        public Area(List<Cell> range, int tileSize)
        {
            Cells.AddRange(range);
            _tileSize = tileSize;
        }

        /// <summary>
        /// calculates width and height of area
        /// </summary>
        /// <returns>width and height</returns>
        public (int x, int y, int width, int height) GetSize()
        {
            return GetSize(0, Cells.Count);
        }

        /// <summary>
        /// calculate the fill % of area from 0 to 100
        /// </summary>
        /// <returns></returns>
        public int Fill()
        {
            if (Cells.Count == 0) return 0;
            (int x, int y, int width, int height) size = GetSize();
            int fill = (int)(Cells.Count / (float)(size.width * size.height) * 100);
            return fill;
        }

        /// <summary>
        /// get width and height of area
        /// </summary>
        /// <param name="index">initial cell to start counting</param>
        /// <param name="length">number of cells to count</param>
        /// <returns>width and height</returns>
        public (int x, int y, int width, int height) GetSize(int index, int length)
        {
            int step = _tileSize / 8;
            int lx = int.MaxValue;
            int rx = int.MinValue;
            int ty = int.MaxValue;
            int by = int.MinValue;

            for (int i = index, l = 0; l < length; i++, l++)
            {
                Cell cell = Cells[i];
                lx = Math.Min(cell.X, lx);
                rx = Math.Max(cell.X, rx);
                ty = Math.Min(cell.Y, ty);
                by = Math.Max(cell.Y, by);
            }
            // size will report number of cells horiz and vertical
            return (lx, ty, (rx - lx + step) / step, (by - ty + step) / step);
        }

        /// <summary>
        /// get width and height of collections of cells
        /// </summary>
        /// <param name="cells">collection of cells</param>
        /// <returns>width and height</returns>
        public static (int x, int y, int width, int height) GetSize(List<Cell> cells, int tileSize)
        {
            int step = tileSize / 8;
            int lx = int.MaxValue;
            int rx = int.MinValue;
            int ty = int.MaxValue;
            int by = int.MinValue;

            foreach (Cell cell in cells)
            {
                lx = Math.Min(cell.X, lx);
                rx = Math.Max(cell.X, rx);
                ty = Math.Min(cell.Y, ty);
                by = Math.Max(cell.Y, by);
            }
            //return (rx - lx + 1, by - ty + 1);
            // size will report number of cells horiz and vertical
            return (rx, ty, (rx - lx + step) / step, (by - ty + step) / step);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Area Explode()
        {
            (int x, int y, int width, int height) size = GetSize();

            int step = _tileSize / 8;
            Area area = new Area(size.width, size.height, 8);
            // get the offset of area

            area.X = size.x;
            area.Y = size.y;

            int x = size.x;
            int y = size.y;

            foreach (Cell cell in Cells)
            {
                // we need to ensure that we use the offset to have the correct position in exploded area
                int xa = (cell.X - x) / step;
                int ya = (cell.Y - y) / step;

                int index = ya * size.width + xa;
                area.Cells[index].TileID = cell.TileID;
                area.Cells[index].X = xa;
                area.Cells[index].Y = ya;
            }
            return area;
        }
    }
}
