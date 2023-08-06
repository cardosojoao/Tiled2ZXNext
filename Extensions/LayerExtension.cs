using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext
{
    public partial class Layer
    {
        public Area ScanArea(Cell cell)
        {
            Area area = new();
            cell.Source = Direction.Left;
            area.Cells.Add(cell);
            FindNext(cell, area);
            return area;
        }

        private void FindNext(Cell cell, Area area)
        {
            //Cell cellNext = new();
            for (Direction path = Direction.Top; path <= Direction.Left; path++)
            {
                if (path != cell.Source)
                {
                    int x = cell.X;
                    int y = cell.Y;
                    Direction sourcePath = path;
                    switch (path)
                    {
                        case Direction.Top:
                            y--;
                            sourcePath = Direction.Bottom;
                            break;
                        case Direction.Right:
                            x++;
                            sourcePath = Direction.Left;
                            break;
                        case Direction.Bottom:
                            y++;
                            sourcePath = Direction.Top;
                            break;
                        case Direction.Left:
                            x--;
                            sourcePath = Direction.Right;
                            break;
                        default:
                            break;
                    }
                    Cell cellNext = GetCell(x, y);
                    if (cellNext.TileID != 0 && !area.Included(cellNext))
                    {
                        cellNext.Source = sourcePath;
                        area.Cells.Add(cellNext);
                        FindNext(cellNext, area);
                    }
                }
            }
        }

        private Cell GetCell(int x, int y)
        {
            Cell cell = new();
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                cell.X = x;
                cell.Y = y;
                cell.TileID = Data[y * Width + x];
            }
            return cell;
        }
    }
}
