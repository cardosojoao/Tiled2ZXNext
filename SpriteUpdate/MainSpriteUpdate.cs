using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Model = TiledIO.Models;

namespace Tiled2dot8
{
    public class SpriteUpdate
    {
        public void Run(SpritetOptions args)
        {
            List<string> spriteInfo = LoadSpritesInfo(args.Input);
            string projectRaw = File.ReadAllText(args.Project);
            Model.Project projectData = JsonSerializer.Deserialize<Model.Project>(projectRaw);
            Model.EnumPropertyType propData =  (Model.EnumPropertyType)projectData.PropertyTypes.Find(x => string.Equals(x.Name, args.PropertyType, StringComparison.OrdinalIgnoreCase));
            propData.Values = spriteInfo;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(projectData, options);
            File.WriteAllText(args.Project, json);
        }


        public static List<string> LoadSpritesInfo(string fileName)
        {
            var result = new List<string>();

            foreach (string line in File.ReadLines(fileName))
            {
                string text = line.Trim();

                // Ignore empty lines and comments
                if (string.IsNullOrEmpty(text) || text.StartsWith(";"))
                    continue;

                int endIndex = text.IndexOfAny(new char[] { ' ', '\t' });
                if (endIndex > 0)
                {
                    text = endIndex >= 0 ? text.Substring(0, endIndex) : text;
                    if (text.EndsWith("_PATTERN_ID", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(text[..^11]);
                    }
                }
            }
            return result;
        }
    }
}