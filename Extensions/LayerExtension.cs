using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext
{
    public partial class Layer
    {
        private int _tileSize;
        public Area ScanArea(Cell cell, int tileSize)
        {
            _tileSize = tileSize;
            Area area = new(_tileSize);
            cell.Source = Direction.Left;
            area.Cells.Add(cell);
            FindNext(cell, area );
            return area;
        }

        private void FindNext(Cell cell, Area area)
        {
            int stepY = _tileSize / 8;
            int stepX = _tileSize / 8;
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
                            y -= stepY;
                            sourcePath = Direction.Bottom;
                            break;
                        case Direction.Right:
                            x += stepX;
                            sourcePath = Direction.Left;
                            break;
                        case Direction.Bottom:
                            y += stepY;
                            sourcePath = Direction.Top;
                            break;
                        case Direction.Left:
                            x -= stepX;
                            sourcePath = Direction.Right;
                            break;
                        default:
                            break;
                    }
                    // since we are using the top/left as coord and Tiled uses the Bottom/Left we need to ensure we look into the correct char pos
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
                cell.TileID = Data[(y + ((_tileSize / 8) - 1)) * Width + x];
            }
            return cell;
        }
    }
}
