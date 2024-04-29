using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Tiled2ZXNext.Models.XML
{
    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(Template));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (Template)serializer.Deserialize(reader);
    // }

    //[XmlRoot(ElementName = "tileset")]
    //public class Tileset
    //{

    //    [XmlAttribute(AttributeName = "firstgid")]
    //    public int Firstgid { get; set; }

    //    [XmlAttribute(AttributeName = "source")]
    //    public string Source { get; set; }
    //}

    [XmlRoot(ElementName = "properties")]
    public class Properties
    {

        [XmlElement(ElementName = "property")]
        public List<Property> Property { get; set; }

    }

    [XmlRoot(ElementName = "property")]
    public class Property
    {

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }



    [XmlRoot(ElementName = "object")]
    public class Object
    {

        [XmlElement(ElementName = "properties")]
        public Properties Properties { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "gid")]
        public int Gid { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public int Width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public int Height { get; set; }

        [XmlAttribute(AttributeName = "rotation")]
        public int Rotation { get; set; }
    }

    [XmlRoot(ElementName = "template")]
    public class Template
    {

        //[XmlElement(ElementName = "tileset")]
        //public Tileset Tileset { get; set; }

        [XmlElement(ElementName = "object")]
        public Object Object { get; set; }
    }

}
