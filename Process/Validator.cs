using System.Collections.Generic;
using System.Text;
using Tiled2dot8.Extensions;

namespace Tiled2dot8.ProcessLayers
{
    internal static class Validator
    {
        public static StringBuilder ProcessLayerValidator(List<Entities.Property> properties)
        {
            StringBuilder builder = new StringBuilder();
            if (properties != null && properties.ExistProperty("Validator.FlagId") && properties.ExistProperty("Validator.EnableValue"))
            {
                builder.Append("\t\tdb $");
                builder.Append(properties.GetPropertyInt("Validator.FlagId",-65535).ToString("X2"));
                builder.AppendLine($"\t\t; Flag ID");
                builder.Append("\t\tdb $");
                builder.Append(properties.GetPropertyInt("Validator.EnableValue",65535).ToString("X2"));
                builder.AppendLine($"\t\t; Enable Value");
            }
            return builder;
        }

        public static StringBuilder ProcessItemValidator(List<Entities.Property> properties)
        {
            StringBuilder builder = new StringBuilder();
            if (properties != null && properties.ExistProperty("Validator") && properties.GetPropertyBool("Validator"))
            {
                builder.AppendLine($"\t\t; Item Validator Enabled ");
            }
            return builder;
        }
    }
}
