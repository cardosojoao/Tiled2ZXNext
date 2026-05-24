using System;
using System.Collections.Generic;
using System.Text;
using Tiled2dot8.Entities;
using Tiled2dot8.Extensions;

namespace Tiled2dot8
{
    /// <summary>
    /// Process Environment Element Interaction
    /// </summary>
    public class ProcessStatic : IProcess
    {
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        private readonly List<Entities.Property> _properties;
        public ProcessStatic(Layer layer, Scene scene, List<Property> properties)
        {
            _rootLayer = layer;
            _scene = scene;
            _properties = properties;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + _rootLayer.Name);
            StringBuilder all = new();
            foreach (Entities.Layer staticSprite in _rootLayer.Layers)
            {
                if (staticSprite.Visible)
                {
                    Console.WriteLine($"\t Component sprite software {staticSprite.Id}");
                    TechHeader.Add("spritestatic", 1);
                    all.AppendLine($"\t\t;\tSprite Modular Component - {staticSprite.Id}");
                    all.Append("\t\tdw ").Append(staticSprite.X.ToString("X4")).Append(", ").AppendLine(staticSprite.Y.ToString("X4"));
                    all.Append("\t\tdb ").Append(staticSprite.Properties.GetProperty("SpriteName").ToUpper()).Append("_PATTERN_ID").AppendLine("\t\t; PatternId");

                    int index = staticSprite.Properties.GetPropertyInt("SpriteIndex");

                    
                    all.Append("\t\tdb ").Append(staticSprite.Properties.GetPropertyInt("SpriteIndex").ToString("X2"));
                }
            }
            return all; 
        }
    }
}