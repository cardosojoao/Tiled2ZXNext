using System;
using System.IO;
using System.Text.Json;
using Tiled2dot8.Mapper;

namespace Tiled2dot8
{
    public class WorldUpdate
    {
        public void Run(WorldOptions args)
        {
            string worldRaw = File.ReadAllText(args.Input);
            Models.World worldData = JsonSerializer.Deserialize<Models.World>(worldRaw);
            Entities.World world = WorldMapper.Map(worldData);
            world.Name = Path.GetFileNameWithoutExtension(args.Input);

            world.GetMatrix();
            ProcessWorld proc = new ProcessWorld(world);
            var worldProc = proc.Execute();
            string worldName = Path.GetFileName(args.Input);
            File.WriteAllText(args.Output, worldProc.ToString());

            //if (Directory.Exists(args.Input))
            //{
            //    string[] worlds = Directory.GetFiles(Path.GetFullPath(args.Input), "*.world");
            //    foreach (string worldFile in worlds)
            //    {
            //        string worldRaw = File.ReadAllText(worldFile);
            //        Models.World worldData = JsonSerializer.Deserialize<Models.World>(worldRaw);
            //        Entities.World world = WorldMapper.Map(worldData);
            //        world.Name = Path.GetFileNameWithoutExtension(worldFile);

            //        world.GetMatrix();
            //        ProcessWorld proc = new ProcessWorld(world);
            //        var worldProc = proc.Execute();
            //        string worldName = Path.GetFileName(worldFile);
            //        File.WriteAllText(Path.Combine(args.Output, Path.GetFileNameWithoutExtension(worldName) + "_worldMap.inc"), worldProc.ToString());
            //    }
            //}
            //else
            //{
            //    Console.WriteLine($"Path {args.Input} does not exist.");
            //}
        }
    }
}