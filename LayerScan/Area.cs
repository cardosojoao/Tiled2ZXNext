using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext
{
    public class Area
    {
        public List<Cell> Cells { get; private set; } = new List<Cell>();

        public Area() { }

        /// <summary>
        /// set the size of area and populates with default values
        /// </summary>
        /// <param name="width">width of area</param>
        /// <param name="height">height of area</param>
        public Area(int width, int height)
        {
            Cells = new List<Cell>(width * height);

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    Cell cell = new() { X = c, Y = r, TileID = 0, Source = 0 };
                    Cells.Add(cell);
                }
            }
        }

        public Area(List<Cell> range)
        {
            Cells.AddRange(range);
        }
        /// <summary>
        /// is cell included in area
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool Included(Cell cell)
        {
            return Cells.FindIndex(c => c.X == cell.X && c.Y == cell.Y) > -1;
        }

        /// <summary>
        /// Sort cells by x,y
        /// </summary>
        public void SortHoriz()
        {
            Cells.Sort(Cell.SortHoriz);
        }

        /// <summary>
        /// sort cells by y,x
        /// </summary>
        public void SortVert()
        {
            Cells.Sort(Cell.SortVert);
        }

        /// <summary>
        /// calculates width and height of area
        /// </summary>
        /// <returns>width and height</returns>
        public (int width, int height) GetSize()
        {
            return GetSize(0, Cells.Count);
        }

        /// <summary>
        /// calculate the fill % of area from 0 to 100
        /// </summary>
        /// <returns></returns>
        public int Fill()
        {
            (int width, int height) size = GetSize();
            int fill = (int)((((float)Cells.Count) / ((float)(size.width * size.height))) * 100);
            return fill;
        }

        /// <summary>
        /// get width and height of area
        /// </summary>
        /// <param name="index">initial cell to start counting</param>
        /// <param name="length">number of cells to count</param>
        /// <returns>width and height</returns>
        public  (int width, int height) GetSize(int index, int length)
        {
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
            return (rx - lx + 1, by - ty + 1);
        }

        /// <summary>
        /// get width and height of collections of cells
        /// </summary>
        /// <param name="cells">collection of cells</param>
        /// <returns>width and height</returns>
        public static  (int width, int height) GetSize(List<Cell> cells)
        {
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
            return (rx - lx + 1, by - ty + 1);
        }

        /// <summary>
        /// get a new area using the index and length
        /// remove from existing area
        /// </summary>
        /// <param name="index">first cell position</param>
        /// <param name="length">number of cells</param>
        /// <returns>new area</returns>
        public Area Get(int index, int length)
        {
            Area area = new Area(Cells.GetRange(index, length));
            Cells.RemoveRange(index, length);
            return area;
        }

        public Area Explode()
        {
            (int width, int height) size = GetSize();
            Area area = new Area(size.width, size.height);

            // get the offset of area
            int x = Cells[0].X;
            int y = Cells[0].Y;

            foreach (Cell cell in Cells)
            {
                // we need to ensure that we use the offset to have the correct position in exploded area
                int xa = cell.X - x; 
                int ya = cell.Y - y;

                int index = ya * size.width + xa;
                area.Cells[index].TileID = cell.TileID;
                area.Cells[index].X = xa;
                area.Cells[index].Y = ya;
            }

            return area;
        }

        /// <summary>
        /// try to split the area in small chunks with 100% filled
        /// check row by row (horizontal)
        /// </summary>
        /// <param name="area">are to split in smaller areas</param>
        /// <returns>collection smaller areas</returns>
        public List<Area> SplitHoriz()
        {
            List<Area> areas = new List<Area>();        // list of new areas
            int i = 0;                                  // index of area cell
            Area newArea = new();                       // new area object

            while (i < Cells.Count)
            {
                // if area is equal or smaller than 4x4 just stop and add the area
                (int width, int height) areaCurrentSize = GetSize();
                if (areaCurrentSize.width <= 4 && areaCurrentSize.width <= 4)
                {
                    areas.Add(Explode());
                    break;
                }

                List<Cell> row = GetRow(i);        // get next row
                (int width, int height) rowSize = Area.GetSize(row);
                (int width, int height) areaSize = Area.GetSize(newArea.Cells);

                // check if we have 100% of row fill
                bool rowOK = Area.GetSize(row).width == row.Count;
                // check if width of new area and row is the same, if first row area is always good
                bool areaOK = newArea.Cells.Count == 0 ? true : Area.GetSize(newArea.Cells).width == row.Count;

                // if row is 100% and area+row are 100% add row to area
                if (rowOK && areaOK)
                {
                    newArea.Cells.AddRange(row);
                    Cells.RemoveRange(i, row.Count);
                }
                else
                {
                    // if row isn´t 100% or doesn't have the same width of new area
                    // if exist a new area save it and reset new area
                    if (newArea.Cells.Count > 0)
                    {
                        areas.Add(newArea);
                        newArea = new Area();
                    }
                    else
                    {
                        // if we skip the row
                        i += row.Count;
                    }
                }
            }
            if (newArea.Cells.Count > 0)
            {
                areas.Add(newArea);
            }
            return areas;
        }

        /// <summary>
        /// try to split the area in smaller chunks with 100% filled
        /// check col by col (vertical)
        /// </summary>
        /// <param name="area">area to be splited</param>
        /// <returns>collection of smaller areas</returns>
        public  List<Area> SplitVert()
        {
            int i = 0;                                  // index of area cell
            List<Area> areas = new List<Area>();        // list of new areas
            Area openArea = new();                       // current open area object

            while (i < Cells.Count)
            {
                // if area is equal or smaller than 4x4, add the area and exit
                (int width, int height) parentAreaSize = GetSize();
                if (parentAreaSize.width <= 4 && parentAreaSize.width <= 4 && Fill() != 100)
                {
                    areas.Add(Explode());
                    break;
                }
                List<Cell> col = GetCol(i);                                // get next row
                (int width, int height) colSize = Area.GetSize(col);             // size of column
                (int width, int height) areaSize = Area.GetSize(openArea.Cells);  // size of current new area


                // check if we have 100% of row fill
                bool colOK = colSize.height == col.Count;
                // check if width of new area and row is the same, if first row area is always good
                bool areaOK = openArea.Cells.Count == 0 ? true : areaSize.height == col.Count;
                // if last col of new area is previous col of new col (they are side by side)
                bool isNextCol = openArea.Cells.Count == 0 ? true : openArea.Cells[openArea.Cells.Count - 1].X == col[0].X + 1;


                // if row is 100% and area+row are 100% add row to area
                if (colOK && areaOK && isNextCol)
                {
                    openArea.Cells.AddRange(col);
                    Cells.RemoveRange(i, col.Count);   // remove from parent area
                }
                else
                {
                    // if row isn´t 100% or doesn't have the same width of new area
                    // if exist a new area save it and reset new area
                    if (openArea.Cells.Count > 0)
                    {
                        areas.Add(openArea);
                        openArea = new Area();
                    }
                    else
                    {
                        // if we skip the row
                        i += col.Count;
                    }
                }
            }
            if (openArea.Cells.Count > 0)
            {
                areas.Add(openArea);
            }
            return areas;
        }

        /// <summary>
        /// get single row from area
        /// </summary>
        /// <param name="area">parent area</param>
        /// <param name="index">where to we want to start selecting row</param>
        /// <returns>collections of cells in the same row</returns>
        private List<Cell> GetRow(int index)
        {
            List<Cell> row = new();
            Cell cell = Cells[index];
            int rowNumber = cell.Y;             // first cell with define the row value
            while (index < Cells.Count)
            {
                cell = Cells[index];
                if (cell.Y == rowNumber)
                {
                    row.Add(cell);
                }
                else
                {
                    break;
                }
                index++;
            }
            return row;
        }

        /// <summary>
        /// get a single columns from area
        /// </summary>
        /// <param name="area">parent area</param>
        /// <param name="index">where do we want to start selection the column</param>
        /// <returns>collection of cells in the same column</returns>
        private List<Cell> GetCol(int index)
        {
            List<Cell> col = new();
            Cell cell = Cells[index];
            int colNumber = cell.X;
            while (index < Cells.Count)
            {
                cell = Cells[index];
                if (cell.X == colNumber)
                {
                    col.Add(cell);
                }
                else
                {
                    break;
                }
                index++;
            }
            return col;
        }
    }
}
