using System.Collections.Generic;
using System.Text.Json;

namespace Tiled2dot8.Entities
{
    public class Property
    {
        public string Name { get; set; }

        public string Type { get; set; }
        public string Propertytype { get; set; }

        //public object Value { get; set; }


        public JsonElement Value { get; set; }

        public object GetValue()
        {
            return Type switch
            {
                "int" => int.Parse(Value.ToString()),

                "string" => Value.GetString(),

                "class" => JsonSerializer.Deserialize<Dictionary<string, object>>(Value),
                _ => Value
            };
        }

    }
}

