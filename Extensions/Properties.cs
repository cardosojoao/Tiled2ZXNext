using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tiled2dot8.Models;
using Tiled2dot8.Models.XML;

namespace Tiled2dot8.Extensions
{
    public static class PropertyExtensions
    {

        public static bool ExistProperty(this List<Models.Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Models.Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop != null;
        }

        public static bool ExistProperty(this List<Entities.Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Entities.Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop != null;
        }

        public static string GetProperty(this List<Models.Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Models.Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop?.Value.ToString() ?? "";
        }

        public static string GetProperty(this List<Entities.Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Entities.Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop?.Value ?? "";
        }

        public static string GetProperty(this List<Entities.Property> properties, string name, string value)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Entities.Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop?.Value ?? value;
        }

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

        /// <summary>
        /// get property value as int
        /// </summary>
        /// <param name="properties">properties</param>
        /// <param name="name">property name</param>
        /// <returns>value as int in case error 255</returns>
        public static int GetPropertyInt(this List<Entities.Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Entities.Property? prop = properties.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop == null ? throw new KeyNotFoundException(name) : int.Parse(prop.Value);
        }

        public static int GetPropertyInt(this List<Entities.Property> properties, string name, int value)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Entities.Property? prop = properties.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop == null ? value : int.Parse(prop.Value);
        }

        public static int GetPropertyInt(this List<Models.Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Models.Property? prop = properties.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop == null ? throw new KeyNotFoundException(name) : int.Parse(prop.Value.ToString());
        }

        public static bool GetPropertyBool(this List<Entities.Property> properties, string name, bool value = false)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Entities.Property? prop = properties.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop == null ? value : bool.Parse(prop.Value.ToString());
        }

        public static void Merge(this List<Entities.Property> properties, List<Entities.Property> mergeProperties)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if(mergeProperties != null)
            {
                properties.AddRange(mergeProperties);
            }
        }

        //public static string GetProperty(this TiledParser tiledData, string name)
        //{
        //    if (tiledData.Properties == null) throw new ArgumentNullException($"tileData.Properties {tiledData.Type}");
        //    return tiledData.Properties.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
        //}

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
