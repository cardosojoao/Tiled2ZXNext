using System.Collections.Generic;
using Entity = Tiled2ZXNext.Entities;
using Model = Tiled2ZXNext.Models;

namespace Tiled2ZXNext.Mapper
{
    public static class TemplateMapper
    {
        public static Entity.Template Map(Model.XML.Template TemplateRaw)
        {
            Entity.Template template= new Entity.Template();
            foreach (Model.XML.Property propertyRaw in TemplateRaw.Object.Properties.Property)
            {
                Entity.Property property = new Entity.Property();
                property.Name = propertyRaw.Name;
                property.Type = propertyRaw.Type.ToString();
                property.Value = propertyRaw.Value.ToString();
                template.Properties.Add(property);
            }
            return template;
        }
    }
}
