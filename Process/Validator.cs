using System.Collections.Generic;
using System.Text;
using Tiled2ZXNext.Extensions;

namespace Tiled2ZXNext.ProcessLayers
{
    internal static class Validator
    {
        public static StringBuilder ProcessValidator(List<Entities.Property> properties)
        {
            StringBuilder builder = new StringBuilder();
            if (properties != null && properties.ExistProperty("FlagId") && properties.ExistProperty("EnableValue"))
            {
                builder.Append("\t\tdb $");
                builder.Append(properties.GetPropertyInt("FlagId").ToString("X2"));
                builder.AppendLine($"\t\t; Flag ID");
                builder.Append("\t\tdb $");
                builder.Append(properties.GetPropertyInt("EnableValue").ToString("X2"));
                builder.AppendLine($"\t\t; Enable Value");
            }
            return builder;
        }
    }
}
