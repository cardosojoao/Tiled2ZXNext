using System;
using System.Text;
using Tiled2dot8.Entities;
using Tiled2dot8.Extensions;


namespace Tiled2dot8
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
            all.Append(_world.Name).AppendLine("_scenes:");

            _world.Maps.Sort((m1,m2) =>   m1.Id.CompareTo(m2.Id));  
            
            foreach (Map map in _world.Maps)
            {
                Console.WriteLine("Map " + map.FileName);
                all.Append("\t\t;").AppendLine(map.FileName);
                all.Append("\t\tdb $").Append(map.NeighBours.Left.Id.ToString("X2")).AppendLine("\t; (Left) id");
                all.Append("\t\tdw $").Append(map.NeighBours.Left.Xoffset.Int2Hex("X4")).Append(", $").Append(map.NeighBours.Left.Yoffset.Int2Hex("X4")).AppendLine("\t; (Left) Xoffset, Yoffset");
                all.Append("\t\tdb $").Append(map.NeighBours.Right.Id.ToString("X2")).AppendLine("\t; (Right) id");
                all.Append("\t\tdw $").Append(map.NeighBours.Right.Xoffset.Int2Hex("X4")).Append(", $").Append(map.NeighBours.Right.Yoffset.Int2Hex("X4")).AppendLine("\t; (Right) Xoffset, Yoffset");
                all.Append("\t\tdb $").Append(map.NeighBours.Top.Id.ToString("X2")).AppendLine("\t; (Top) id");
                all.Append("\t\tdw $").Append(map.NeighBours.Top.Xoffset.Int2Hex("X4")).Append(", $").Append(map.NeighBours.Top.Yoffset.Int2Hex("X4")).AppendLine("\t; (Top) Xoffset, Yoffset");
                all.Append("\t\tdb $").Append(map.NeighBours.Bottom.Id.ToString("X2")).AppendLine("\t; (Bottom) id");
                all.Append("\t\tdw $").Append(map.NeighBours.Bottom.Xoffset.Int2Hex("X4")).Append(", $").Append(map.NeighBours.Bottom.Yoffset.Int2Hex("X4")).AppendLine("\t; (Bottom) Xoffset, Yoffset ");
            }
            return all;
        }
    }
}