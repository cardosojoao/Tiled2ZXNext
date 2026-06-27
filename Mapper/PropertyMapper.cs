using System.Collections.Generic;
using System.Text.Json;
using Entity = Tiled2dot8.Entities;
using Model = Tiled2dot8.Models;

namespace Tiled2dot8.Mapper
{
    public static class PropertyMapper
    {
        public static List<Entity.Property> Map(List<Model.Property> propertiesRaw)
        {
            List<Entity.Property> properties = new();
            if (propertiesRaw != null)
            {
                foreach (Model.Property propertyRaw in propertiesRaw)
                {
                    Entity.Property property = new()
                    {
                        Name = propertyRaw.Name,
                        Type = propertyRaw.Type.ToString(),
                        Value = propertyRaw.Value,
                        Propertytype = propertyRaw.Propertytype
                    };
                    properties.Add(property);
                }
            }
            return properties;
        }


        public static List<Entity.Property> Map(List<Models.TilesetTileProperty> propertiesRaw)
        {
            List<Entity.Property> properties = new();
            foreach (Models.TilesetTileProperty propertyRaw in propertiesRaw)
            {


                Entity.Property property = new()
                {
                    Name = propertyRaw.name,
                    Type = "string", // propertyRaw.type.ToString()
                    Value = JsonDocument.Parse($"\"{propertyRaw.value}\"").RootElement
                };
                properties.Add(property);
            }
            return properties;
        }
    }
}
