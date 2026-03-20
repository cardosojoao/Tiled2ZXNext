using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Text.Json;
using Tiled2dot8.Entities;
using Tiled2dot8.Mapper;
using Tiled2dot8.Models;

namespace Tiled2dot8
{
    public class WorldUpdate
    {
        public void Run(WorldOptions args)
        {
            if (Directory.Exists(args.Input))
            {
                string[] worlds = Directory.GetFiles(Path.GetFullPath(args.Input), "*.world");
                foreach (string worldFile in worlds)
                {
                    string worldRaw = File.ReadAllText(worldFile);
                    Models.World worldData = JsonSerializer.Deserialize<Models.World>(worldRaw);
                    Entities.World world = WorldMapper.Map(worldData);
                    world.Name = Path.GetFileNameWithoutExtension(worldFile);

                    world.GetMatrix();
                    ProcessWorld proc = new ProcessWorld(world);
                    var worldProc = proc.Execute();
                    string worldName = Path.GetFileName(worldFile);
                    File.WriteAllText(Path.Combine(args.Output, Path.GetFileNameWithoutExtension(worldName) + "_worldMap.inc"), worldProc.ToString());
                }
            }
            else
            {
                Console.WriteLine($"Path {args.Input} does not exist.");
            }
        }
    }
}