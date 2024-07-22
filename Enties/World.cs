using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Tiled2ZXNext.Entities
{

    public class Map
    {
        public string FileName { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public NeighBours NeighBours { get; set; } = new NeighBours();
        public int Id { get; set; } = -1;
    }


    public class NeighBours
    {
        public int Left { get; set; } = 0;
        public int Right { get; set; } = 0;
        public int Top { get; set; } = 0;
        public int Bottom { get; set; } = 0;


    }

    public class World
    {
        public List<Map> Maps { get; set; }
        public bool OnlyShowAdjacentMaps { get; set; }
        public string Type { get; set; }

        private int[,] matrix;
        private int maxX;
        private int maxY;
        private int minX;
        private int minY;
        private int mapWidth;
        private int mapHeight;

        public void GetMatrix()
        {
            minX = int.MaxValue;
            minY = int.MaxValue;
            maxX = int.MinValue;
            maxY = int.MinValue;

            foreach (var map in Maps)
            {
                map.X /= map.Width;
                map.Y /= map.Height;

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
            matrix = new int[mapWidth, mapHeight];

            // allocate rooms
            foreach (var map in Maps)
            {
                map.X = map.X > 0 ?map.X-minX : map.X + Math.Abs(minX);
                map.Y = map.Y > 0 ? map.Y - minY : map.Y + Math.Abs(minY);

                // map.X += Math.Abs(minX);
                // map.Y += Math.Abs(minY);
                if (map.X >= 0 && map.Y >= 0)
                {
                    map.Id = int.Parse(map.FileName.Substring(map.FileName.Length - 8, 3));
                    matrix[map.X, map.Y] = map.Id;
                }
            }

            foreach (var map in Maps)
            {
                if (map.X >= 0 && map.Y >= 0)
                {
                    map.NeighBours = GetNeighbours(map.X, map.Y);
                }
            }
        }

        private NeighBours GetNeighbours(int x, int y)
        {
            NeighBours neigh = new NeighBours();
            if (x > 0)
            {
                neigh.Left = matrix[x - 1, y];
            }
            if (x < mapWidth - 1)
            {
                neigh.Right = matrix[x + 1, y];
            }

            if (y > 0)
            {
                neigh.Top = matrix[x, y - 1];
            }
            if (y < mapHeight - 1)
            {
                neigh.Bottom = matrix[x, y + 1];
            }
            return neigh;
        }
    }





}
