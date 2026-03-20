using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace Tiled2dot8.enums
{

    public class TiledProjectEnums
    {

        public static List<TiledEnum> ReadEnumsFromTiledProject(string filePath)
        {
            string json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            TiledProject project = JsonSerializer.Deserialize<TiledProject>(json, options);

            if (project?.PropertyTypes == null)
                return new List<TiledEnum>();

            return project.PropertyTypes
                .Where(p => p.Type == "enum")
                .ToList();
        }

        public static void UpdatePropertyTypes(string filePath, JsonArray newPropertyTypes)
        {
            var json = File.ReadAllText(filePath);

            JsonNode root = JsonNode.Parse(json);

            root["propertyTypes"] = newPropertyTypes;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            File.WriteAllText(filePath, root.ToJsonString(options));
        }
    }
}
