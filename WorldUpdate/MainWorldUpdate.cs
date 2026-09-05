using System;
using System.Collections.Generic;
using System.IO;
using TiledIO.Entities;
using Entities = TiledIO.Entities;

namespace Tiled2dot8
{
    public class WorldUpdate
    {
        private int maxX;
        private int maxY;
        private int minX;
        private int minY;
        private int mapWidth;
        private int mapHeight;

        public void Run(WorldOptions args)
        {
            Entities.World world = TiledIO.Tiled.LoadWorld(args.Input);
            //string worldRaw = File.ReadAllText(args.Input);
            //Models.World worldData = JsonSerializer.Deserialize<Models.World>(worldRaw);
            //Entities.World world = WorldMapper.Map(worldData);
            world.Name = Path.GetFileNameWithoutExtension(args.Input);
            GetMatrix(world.Maps);
            ProcessWorld proc = new ProcessWorld(world);
            var worldProc = proc.Execute();
            string worldName = Path.GetFileName(args.Input);
            File.WriteAllText(args.Output, worldProc.ToString());
        }


        public void GetMatrix(List<Entities.Map> maps)
        {
            minX = int.MaxValue;
            minY = int.MaxValue;
            maxX = int.MinValue;
            maxY = int.MinValue;

            foreach (var map in maps)
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

            int offsetX = 0;
            int offsetY = 0;
            if (minX < 0)
            {
                offsetX = Math.Abs(minX);
            }

            if (minY < 0)
            {
                offsetY = Math.Abs(minY);
            }

            maxX += offsetX;
            minX += offsetX;
            maxY += offsetY;
            minY += offsetY;


            mapWidth = (maxX - minX) + 1;
            mapHeight = (maxY - minY) + 1;
            maxX++;
            maxY++;


            // allocate rooms
            foreach (var map in maps)
            {
                if (true)
                {
                    map.X += offsetX;
                    map.X1 += map.X;
                    map.Y += offsetY;
                    map.Y1 += map.Y;
                    map.Id = int.Parse(map.FileName.Substring(map.FileName.Length - 8, 3));
                }
                else
                {
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
            }

            foreach (var map in maps)
            {
                if (map.X >= 0 && map.Y >= 0)
                {
                    map.NeighBours = GetNeighbours(maps, map);
                }
            }
        }


        private static NeighBours GetNeighbours(List<Entities.Map> world, Entities.Map currentMap)
        {
            int x1, y1, x2, y2;

            x1 = currentMap.X1;
            x2 = currentMap.X2;
            y1 = currentMap.Y1;
            y2 = currentMap.Y2;


            NeighBours neigh = new NeighBours();

            foreach (Entities.Map map in world)
            {

                if (map.Id == currentMap.Id)
                {
                    continue;
                }
                if (map.X1 == x2)
                {
                    if (map.Y1 < y2 && map.Y2 > y1)
                    //if ((y1 < map.Y1 && y2 > map.Y2) ||  // 
                    //    (y1 > map.Y1 && y2 < map.Y2) || 
                    //    (map.Y1 > y1 && map.Y1 < y2) || 
                    //    (map.Y2 > y1 && map.Y2 < y2) ||
                    //        (map.Y1 == y1 && map.Y2==y2))
                    {
                        // join map on right
                        neigh.Right.Id = map.Id;
                        neigh.Right.Yoffset = currentMap.Y1 - map.Y1;
                    }
                }
                else if (map.X2 == x1)
                {
                    //if ((y1 < map.Y1 && y2 > map.Y2) || 
                    //    (y1 > map.Y1 && y2 < map.Y2) || 
                    //    (map.Y1 > y1 && map.Y1 < y2) || 
                    //    (map.Y2 > y1 && map.Y2 < y2) ||
                    //       (map.Y1 == y1 && map.Y2 == y2))
                    if (map.Y1 < y2 && map.Y2 > y1)
                    {
                        // join map on left
                        neigh.Left.Id = map.Id;
                        neigh.Left.Yoffset = currentMap.Y1 - map.Y1;
                    }
                }
                else if (map.Y1 == y2)
                {
                    if (x1 < map.X2 && x2 > map.X1)
                    //if ((x1 < map.X1 && x2 > map.X2) || 
                    //    (x1 > map.X1 && x2 < map.X2) || 
                    //    (map.X1 >= x1 && map.X1 <= x2) || 
                    //    (map.X2 >= x1 && map.X2 <= x2) ||
                    //    (map.X1 == x1 && map.X2 == x2))
                    {
                        // join map on bottom
                        neigh.Bottom.Id = map.Id;
                        neigh.Bottom.Xoffset = currentMap.X1 - map.X1;
                    }
                }
                else if (map.Y2 == y1)
                {
                    if (x1 < map.X2 && x2 > map.X1)
                    //if ((x1 < map.X1 && x2 > map.X2) || 
                    //    (x1 > map.X1 && x2 < map.X2) || 
                    //    (map.X1 >= x1 && map.X1 <= x2) || 
                    //    (map.X2 >= x1 && map.X2 <= x2) ||
                    //    (map.X1 == x1 && map.X2 == x2))
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