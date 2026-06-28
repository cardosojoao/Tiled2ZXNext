using System.IO;
using Entities = TiledIO.Entities;

namespace Tiled2dot8
{
    public class WorldUpdate
    {
        public void Run(WorldOptions args)
        {
            Entities.World world = TiledIO.Tiled.LoadWorld(args.Input);
            //string worldRaw = File.ReadAllText(args.Input);
            //Models.World worldData = JsonSerializer.Deserialize<Models.World>(worldRaw);
            //Entities.World world = WorldMapper.Map(worldData);
            world.Name = Path.GetFileNameWithoutExtension(args.Input);
            world.GetMatrix();
            ProcessWorld proc = new ProcessWorld(world);
            var worldProc = proc.Execute();
            string worldName = Path.GetFileName(args.Input);
            File.WriteAllText(args.Output, worldProc.ToString());
        }
    }
}