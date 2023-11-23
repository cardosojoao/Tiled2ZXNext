using System.Collections.Generic;
using Entity = Tiled2ZXNext.Entities;
using Model = Tiled2ZXNext.Models;

namespace Tiled2ZXNext.Mapper
{
    public static class PropertyMapper
    {
        public static List<Entity.Property> Map(List<Model.Property> propertiesRaw)
        {
            List<Entity.Property> properties = new List<Entity.Property>();
            foreach (Model.Property propertyRaw in propertiesRaw)
            {
                Entity.Property property = new Entity.Property();
                property.Name = propertyRaw.Name;
                property.Type = propertyRaw.Type;
                property.Value = propertyRaw.Value;
                properties.Add(property);
            }
            return properties;
        }
    }
}
