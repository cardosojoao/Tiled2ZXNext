using System.Collections.Generic;
using Entity = Tiled2ZXNext.Entities;
using Model = Tiled2ZXNext.Models;

namespace Tiled2ZXNext.Mapper
{
    public static class PropertyMapper
    {
        public static List<Entity.Property> Map(List<Model.Property> propertiesRaw)
        {
            List<Entity.Property> properties = new();
            foreach (Model.Property propertyRaw in propertiesRaw)
            {
                Entity.Property property = new()
                {
                    Name = propertyRaw.Name,
                    Type = propertyRaw.Type.ToString(),
                    Value = propertyRaw.Value.ToString()
                };
                properties.Add(property);
            }
            return properties;
        }


        public static List<Entity.Property> Map(List<Model.XML.Property> propertiesRaw)
        {
            List<Entity.Property> properties = new();
            foreach (Model.XML.Property propertyRaw in propertiesRaw)
            {
                Entity.Property property = new()
                {
                    Name = propertyRaw.Name,
                    Type = propertyRaw.Type.ToString(),
                    Value = propertyRaw.Value.ToString()
                };
                properties.Add(property);
            }
            return properties;
        }
    }
}
