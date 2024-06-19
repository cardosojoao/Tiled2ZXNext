using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.Entities;
using System.IO;
using Model = Tiled2ZXNext.Models;
using Entity = Tiled2ZXNext.Entities;
using System.Xml.Serialization;
using Tiled2ZXNext.Models;
using System.Reflection.Metadata;


namespace Tiled2ZXNext.Mapper
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
