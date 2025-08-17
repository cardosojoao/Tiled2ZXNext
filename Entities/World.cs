using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Threading.Tasks.Dataflow;

namespace Tiled2dot8.Entities
{

    public class Map
    {
        public string FileName { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get { return X1 + Width; } }
        public int Y2 { get { return Y1 + Height; } }



        public NeighBours NeighBours { get; set; } = new NeighBours();
        public int Id { get; set; } = -1;
    }


    public class NeighBours
    {
        public NeighBour Left { get; set; } = new NeighBour();
        public NeighBour Right { get; set; } = new NeighBour();
        public NeighBour Top { get; set; } = new NeighBour();
        public NeighBour Bottom { get; set; } = new NeighBour();


    }


    public class NeighBour
    {
        public int Id { get; set; } = 0;
        public int Xoffset { get; set; } = 0;
        public int Yoffset { get; set; } = 0;
    }


    public class World
    {
        public string Name { get; set; }
        public List<Map> Maps { get; set; }
        public bool OnlyShowAdjacentMaps { get; set; }
        public string Type { get; set; }

        //private int[,] matrix;
        private int maxX;
        private int maxY;
        private int minX;
        private int minY;
        private int mapWidth;
        private int mapHeight;

        //public void GetMatrix()
        //{
        //    minX = int.MaxValue;
        //    minY = int.MaxValue;
        //    maxX = int.MinValue;
        //    maxY = int.MinValue;

        //    foreach (var map in Maps)
        //    {
        //        map.PosX = map.X;
        //        map.PosY = map.Y;

        //        map.X /= map.Width;
        //        map.Y /= map.Height;

        //        if (maxX < map.X)
        //        {
        //            maxX = map.X;
        //        }

        //        if (maxY < map.Y)
        //        {
        //            maxY = map.Y;
        //        }

        //        if (minX > map.X)
        //        {
        //            minX = map.X;
        //        }

        //        if (minY > map.Y)
        //        {
        //            minY = map.Y;
        //        }
        //    }

        //    mapWidth = (maxX - minX) + 1;
        //    mapHeight = (maxY - minY) + 1;
        //    maxX++;
        //    maxY++;
        //    matrix = new int[mapWidth, mapHeight];

        //    // allocate rooms
        //    foreach (var map in Maps)
        //    {
        //        map.X = map.X > 0 ? map.X - minX : map.X + Math.Abs(minX);
        //        map.Y = map.Y > 0 ? map.Y - minY : map.Y + Math.Abs(minY);


        //        // map.X += Math.Abs(minX);
        //        // map.Y += Math.Abs(minY);
        //        if (map.X >= 0 && map.Y >= 0)
        //        {
        //            map.Id = int.Parse(map.FileName.Substring(map.FileName.Length - 8, 3));
        //            matrix[map.X, map.Y] = map.Id;
        //        }
        //    }

        //    foreach (var map in Maps)
        //    {
        //        if (map.X >= 0 && map.Y >= 0)
        //        {
        //            map.NeighBours = GetNeighbours(map.X, map.Y);
        //        }
        //    }
        //}


        public void GetMatrix()
        {
            minX = int.MaxValue;
            minY = int.MaxValue;
            maxX = int.MinValue;
            maxY = int.MinValue;

            foreach (var map in Maps)
            {
                if (maxX < map.X)
                {
                    maxX = map.X;
                }

                if (maxY < map.Y)
                {
                    maxY = map.Y;
                }

                if (minX > map.X)
                {
                    minX = map.X;
                }

                if (minY > map.Y)
                {
                    minY = map.Y;
                }
            }

            mapWidth = (maxX - minX) + 1;
            mapHeight = (maxY - minY) + 1;
            maxX++;
            maxY++;
            // allocate rooms
            foreach (var map in Maps)
            {
                //map.X1 = map.X;
                //map.Y1 = map.Y;
                if (minX < 0)
                {
                    map.X += Math.Abs(minX);
                }
                if (minY < 0)
                {
                    map.Y += Math.Abs(minY);
                }
                map.X1 = map.X;
                map.Y1 = map.Y;
                map.Id = int.Parse(map.FileName.Substring(map.FileName.Length - 8, 3));
            }

            foreach (var map in Maps)
            {
                if (map.X >= 0 && map.Y >= 0)
                {
                    map.NeighBours = GetNeighbours(Maps, map);
                }
            }
        }


        //private NeighBours GetNeighbours(int x, int y)
        //{
        //    NeighBours neigh = new NeighBours();
        //    if (x > 0)
        //    {
        //        neigh.Left = matrix[x - 1, y];
        //    }
        //    if (x < mapWidth - 1)
        //    {
        //        neigh.Right = matrix[x + 1, y];
        //    }

        //    if (y > 0)
        //    {
        //        neigh.Top = matrix[x, y - 1];
        //    }
        //    if (y < mapHeight - 1)
        //    {
        //        neigh.Bottom = matrix[x, y + 1];
        //    }
        //    return neigh;
        //}

        /// <summary>
        /// find adjacent scenes (neighbours) for current scene
        /// </summary>
        /// <param name="x1">current map top</param>
        /// <param name="y1">current map left</param>
        /// <param name="x2">current map right</param>
        /// <param name="y2">current map bottom</param>
        /// <returns>list of neighbours</returns>
        private NeighBours GetNeighbours(List<Map> world, Map currentMap)
        {
            int x1, y1, x2, y2;

            x1 = currentMap.X1;
            x2 = currentMap.X2;
            y1 = currentMap.Y1;
            y2 = currentMap.Y2;


            NeighBours neigh = new NeighBours();

            foreach (Map map in world)
            {
                if (map.X1 == x2)
                {
                    if ((y1 <= map.Y1 && y2 >= map.Y2) || (y1 >= map.Y1 && y2 <= map.Y2) || (map.Y1 >= y1 && map.Y1 <= y2) || (map.Y2 >= y1 && map.Y2 <= y2))
                    {
                        // join map on right
                        neigh.Right.Id = map.Id;
                        neigh.Right.Yoffset = currentMap.Y1 - map.Y1;
                    }
                }
                else if (map.X2 == x1)
                {
                    if ((y1 <= map.Y1 && y2 >= map.Y2) || (y1 >= map.Y1 && y2 <= map.Y2) || (map.Y1 >= y1 && map.Y1 <= y2) || (map.Y2 >= y1 && map.Y2 <= y2))
                    {
                        // join map on left
                        neigh.Left.Id = map.Id;
                        neigh.Left.Yoffset =  currentMap.Y1 - map.Y1;
                    }
                }
                else if (map.Y1 == y2)
                {
                    if ((x1 <= map.X1 && x2 >= map.X2) || (x1 >= map.X1 && x2 <= map.X2) || ( map.X1 >= x1 && map.X1 <= x2) || ( map.X2>= x1 && map.X2 <= x2))
                    {
                        // join map on bottom
                        neigh.Bottom.Id = map.Id;
                        neigh.Bottom.Xoffset = currentMap.X1 - map.X1;
                    }
                }
                else if (map.Y2 == y1)
                {
                    if ((x1 <= map.X1 && x2 >= map.X2) || (x1 >= map.X1 && x2 <= map.X2) || (map.X1 >= x1 && map.X1 <= x2) || (map.X2 >= x1 && map.X2 <= x2))
                    {
                        // join map on top
                        neigh.Top.Id = map.Id;
                        neigh.Top.Xoffset = currentMap.X1 - map.X1;
                    }
                }
            }
            return neigh;
        }
    }
}
