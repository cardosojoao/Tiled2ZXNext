using System.Collections.Generic;
using Entity = Tiled2dot8.Entities;
using Model = Tiled2dot8.Models;

namespace Tiled2dot8.Mapper
{
    public static class TemplateMapper
    {
        public static Entity.Template Map(Model.XML.Template templateRaw)
        {
            Entity.Template template= new Entity.Template();

            template.Gid = templateRaw.Object.Gid;
            template.Type = templateRaw.Object.Type;
            template.Height = templateRaw.Object.Height;
            template.Width = templateRaw.Object.Width;
            if (templateRaw.Object.Properties != null)
            {
                foreach (Model.XML.Property propertyRaw in templateRaw.Object.Properties.Property)
                {
                    Entity.Property property = new Entity.Property();
                    property.Name = propertyRaw.Name;
                    property.Type = propertyRaw.Type ?? "";
                    //property.Value = propertyRaw.Value;
                    template.Properties.Add(property);
                }
            }
            return template;
        }
    }
}
