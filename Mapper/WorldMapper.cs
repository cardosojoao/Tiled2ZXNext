using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2dot8.Extensions;
using Tiled2dot8.Entities;
using System.IO;
using Model = Tiled2dot8.Models;
using Entity = Tiled2dot8.Entities;
using System.Xml.Serialization;
using Tiled2dot8.Models;
using System.Reflection.Metadata;


namespace Tiled2dot8.Mapper
{
    public class WorldMapper
    {
        public static Entities.World Map(Model.World world,Options options)
        {
            Entity.World worldOutput = new();

            worldOutput.OnlyShowAdjacentMaps = world.OnlyShowAdjacentMaps;
            worldOutput.Type = world.Type;
            worldOutput.Maps = new List<Entity.Map>();
            foreach(Models.Map map in world.Maps)
            {
                Entity.Map mapOutput = new() { FileName=map.FileName, Height = map.Height, Width= map.Width, X=map.X, Y=map.Y };
                worldOutput.Maps.Add(mapOutput);
            }
            return worldOutput;
        }
    }
}
