using System;
using System.Collections.Generic;
using System.Linq;
using Tiled2dot8.Models;

namespace Tiled2dot8.Extensions
{
    public static partial class PropertyExtensions
    {


        /// <summary>
        /// get the sprite sheet id from tileset
        /// </summary>
        /// <param name="properties">collection of properties</param>
        /// <param name="name">property name</param>
        /// <returns>property value or empty if not found</returns>
        public static string GetProperty(this TilesetTileProperty[] properties, string name)
        {
            //if (tileSet.properties == null) throw new ArgumentNullException($"tileSet: missing [properties] of {tileSet.name}");
            TilesetTileProperty prop = properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop?.value ?? "";
        }

        public static int GetPropertyInt(this TilesetTileProperty[] properties, string name)
        {
            try
            {
                if (properties == null) throw new ArgumentNullException(nameof(properties));
                TilesetTileProperty? prop = properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                //return prop?.value ?? "";
                return prop == null ? throw new KeyNotFoundException(name) : int.Parse(prop.value);

            }
            catch (Exception ex)
            {
                throw new Exception("missing property " + name, ex);
            }
        }

        public static bool GetPropertyBool(this TilesetTileProperty[] properties, string name, bool value = false)
        {
            try
            {
                if (properties == null) throw new ArgumentNullException(nameof(properties));
                TilesetTileProperty? prop = properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                //return prop?.value ?? "";
                return prop == null ? value : bool.Parse(prop.value.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("missing property " + name, ex);
            }
        }





        public static bool ExistProperty(this TilesetTileProperty[] properties, string name)
        {
            if (properties == null) throw new ArgumentNullException($"tileSet: missing [properties]");
            TilesetTileProperty prop = properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop == null ? false : true;
        }


    }
}
