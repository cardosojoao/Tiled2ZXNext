using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tiled2ZXNext.Models;

namespace Tiled2ZXNext.Extensions
{
    public static class PropertyExtensions
    {

        public static bool ExistProperty(this List<Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException($"missing [properties]");
            Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop != null;
        }


        public static string GetProperty(this List<Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException($"missing [properties]");
            Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop?.Value ?? "";
        }

        /// <summary>
        /// get the sprite sheet id from tileset
        /// </summary>
        /// <param name="properties">collection of properties</param>
        /// <param name="name">property name</param>
        /// <returns>property value or empty if not found</returns>
        public static string GetProperty(this TileSetXMl tileSet, string name)
        {
            if (tileSet.properties == null) throw new ArgumentNullException($"tileSet: missing [properties] of {tileSet.name}");
            TilesetTileProperty prop = tileSet.properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop?.value ?? "";
        }

        public static int GetPropertyInt(this TileSetXMl tileSet, string name)
        {
            if (tileSet.properties == null) throw new ArgumentNullException($"tileSet: missing [properties] of {tileSet.name}");
            TilesetTileProperty prop = tileSet.properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            //return prop?.value ?? "";
            return prop == null ? throw new KeyNotFoundException(name) : int.Parse(prop.value);
        }

                    


        public static bool ExistProperty(this TileSetXMl tileSet, string name)
        {
            if (tileSet.properties == null) throw new ArgumentNullException($"tileSet: missing [properties] of {tileSet.name}");
            TilesetTileProperty prop = tileSet.properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop == null ? false : true;
        }

        /// <summary>
        /// get property value as int
        /// </summary>
        /// <param name="properties">properties</param>
        /// <param name="name">property name</param>
        /// <returns>value as int in case error 255</returns>
        public static int GetPropertyInt(this List<Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException("properties");
            Property? prop = properties.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop == null ? throw new KeyNotFoundException(name) : int.Parse(prop.Value);
        }

        public static string GetProperty(this TiledParser tiledData, string name)
        {
            if (tiledData.Properties == null) throw new ArgumentNullException($"tileData.Properties {tiledData.Type}");
            return tiledData.Properties.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
        }

        /// <summary>
        /// get property value as string
        /// </summary>
        /// <param name="properties">collection of properties</param>
        /// <param name="name">property name</param>
        /// <returns>propery value</returns>
        //public static string GetProperty(List<Property> properties, string name)
        //{
        //    return properties.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Value;
        //}

    }


    public static class DictionaryExtension
    {
        public static void Append<K, V>(this Dictionary<K, V> first, Dictionary<K, V> second) where V : class
        {
            List<KeyValuePair<K, V>> pairs = second.ToList();
            pairs.ForEach(pair => first.Add(pair.Key, pair.Value));
        }
    }
}
