using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext
{
    public class Cell
    {
        public Cell()
        {
            TileID = 0;
        }

        /// <summary>
        /// direction path from parent
        /// </summary>
        public Direction Source { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public uint TileID { get; set; }
        public uint Settings { get; set; }

        public static int SortHoriz(Cell cella, Cell cellb)
        {
            if(cella.Y<cellb.Y)
            {
                return -1;
            }
            else if(cella.Y>cellb.Y)
            {
                return 1;
            }
            else
            {
                if(cella.X < cellb.X)
                {
                    return -1;
                }
                else if (cella.X > cellb.X)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }


        public static int SortVert(Cell cella, Cell cellb)
        {
            if (cella.X < cellb.X)
            {
                return -1;
            }
            else if (cella.X < cellb.X)
            {   
                return 1;
            }
            else
            {
                if (cella.Y < cellb.Y)
                {
                    return -1;
                }
                else if (cella.Y > cellb.Y)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }


    }

    public enum Direction : int
    {
        Top = 0,
        Right = 1,
        Bottom = 2,
        Left = 3
    }
}
