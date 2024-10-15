using System.Collections.Generic;
using Entity = Tiled2ZXNext.Entities;
using Model = Tiled2ZXNext.Models;

namespace Tiled2ZXNext.Mapper
{
    public static class PropertyTypeMapper
    {
        public static List<Entity.PropertyType> Map(List<Model.PropertyType> propertiesRaw)
        {
            List<Entity.PropertyType> properties = new();
            foreach (Model.PropertyType propertyRaw in propertiesRaw)
            {
                Entity.PropertyType property = new()
                {
                    Id = propertyRaw.Id,
                    Name = propertyRaw.Name,
                    StorageType = propertyRaw.StorageType,
                    Type = propertyRaw.Type,
                    Values = new List<string>(propertyRaw.Values),
                    ValuesAsFlags = propertyRaw.ValuesAsFlags,
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
