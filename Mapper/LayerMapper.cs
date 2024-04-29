using System.Collections.Generic;
using System.IO;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Models;
using Entity = Tiled2ZXNext.Entities;
using Model = Tiled2ZXNext.Models;

namespace Tiled2ZXNext.Mapper
{
    public class LayerMapper
    {
        public static List<Entity.Layer> Map(List<Model.Layer> LayersRaw)
        {
            List<Entity.Layer> layers = new();
            foreach (var layerRaw in LayersRaw)
            {
                layers.Add(ParseLayer(layerRaw));
            }
            return layers;
        }

        private static Entity.Layer ParseLayer(Model.Layer layerRaw)
        {

            Entity.Layer layer = new()
            {
                Layers = new(),
                Name = layerRaw.Name,
                Type = layerRaw.Type,
                Width = layerRaw.Width,
                Height = layerRaw.Height,
                X = layerRaw.X,
                Y = layerRaw.Y,
                Id = layerRaw.Id,
                Visible = layerRaw.Visible,
                Data = layerRaw.Data
            };

            if (layerRaw.Objects != null)
            {
                layer.Objects = ParseObjects(layerRaw.Objects);
            }
            if (layerRaw.Properties != null)
            {
                layer.Properties = PropertyMapper.Map(layerRaw.Properties);
            }

            if (layerRaw.Layers != null)
            {
                foreach (Model.Layer layerRawChild in layerRaw.Layers)
                {
                    layer.Layers.Add(ParseLayer(layerRawChild));
                }
            }

            return layer;
        }

        //private static List<Entity.Property> ParseProperties(List<Model.Property> propertiesRaw)
        //{
        //    List<Entity.Property> properties = new List<Entity.Property>();
        //    foreach (Model.Property propertyRaw in propertiesRaw)
        //    {
        //        Entity.Property property = new Entity.Property();
        //        property.Name = propertyRaw.Name;
        //        property.Type = propertyRaw.Type;
        //        property.Value = propertyRaw.Value;
        //        properties.Add(property);
        //    }
        //    return properties;
        //}

        private static List<Entity.Object> ParseObjects(List<Model.Object> objectsRaw)
        {
            List<Entity.Object> objects = new();
            foreach (Model.Object objectRaw in objectsRaw)
            {
                Entity.Object obj = new()
                {
                    Name = objectRaw.Name,
                    Width = objectRaw.Width,
                    Height = objectRaw.Height,
                    Visible = objectRaw.Visible,
                    Id = objectRaw.Id,
                    X = objectRaw.X,
                    Y = objectRaw.Y,
                    Type = objectRaw.Type,
                    Template = objectRaw.Template
                };


                if (objectRaw.Properties != null)
                {
                    obj.Properties = PropertyMapper.Map(objectRaw.Properties);
                }

                if (obj.Template != null)
                {
                    obj.Properties ??= new List<Entities.Property>();
                    Entities.Template template;
                    if (!Entities.Scene.Instance.Templates.ContainsKey(obj.Template))
                    {
                        string templatePath = Path.Combine(Entities.Scene.Instance.RootFolder, obj.Template);
                        Models.XML.Template templateData = SceneMapper.ReadTemplate(templatePath);
                        template = TemplateMapper.Map(templateData);
                        Entities.Scene.Instance.Templates.Add(obj.Template, template);
                    }
                    template = Entities.Scene.Instance.Templates[obj.Template];
                    foreach (var property in template.Properties)
                    {
                        if (obj.Properties.Find(p => p.Name == property.Name) == null)
                        {
                            obj.Properties.Add(property);
                        }
                    }
                }

                if (objectRaw.Polygon != null)
                {
                    obj.Polygon = new();
                    if (objectRaw.Properties != null)
                    {
                        obj.Polygon.Properties = PropertyMapper.Map(objectRaw.Properties);
                    }
                    foreach (Model.PolygonPoint point in objectRaw.Polygon)
                    {
                        obj.Polygon.Add(new Entity.PolygonPoint() { X = point.x, Y = point.y });
                    }
                }
                objects.Add(obj);
            }
            return objects;
        }


    }
}
