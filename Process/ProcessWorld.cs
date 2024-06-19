using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;


namespace Tiled2ZXNext
{
    /// <summary>
    /// Process Environment Element Interaction
    /// </summary>
    public class ProcessWorld : IProcess
    {
        private readonly World _world;
        public ProcessWorld(World world)
        {
            _world = world;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group World");
            StringBuilder all = new();
            all.AppendLine("map_rooms:");

            _world.Maps.Sort((m1,m2) =>   m1.Id.CompareTo(m2.Id));  
            
            foreach (Map map in _world.Maps)
            {
                Console.WriteLine("Map " + map.FileName);
                all.Append("\t\t;").AppendLine(map.FileName);
                all.Append("\t\tdb $").Append(map.NeighBours.Left.ToString("X2")).AppendLine("\t; Left");
                all.Append("\t\tdb $").Append(map.NeighBours.Right.ToString("X2")).AppendLine("\t; Right");
                all.Append("\t\tdb $").Append(map.NeighBours.Top.ToString("X2")).AppendLine("\t; Top");
                all.Append("\t\tdb $").Append(map.NeighBours.Bottom.ToString("X2")).AppendLine("\t; Bottom\n");
            }

            return all;
        }
    }
}