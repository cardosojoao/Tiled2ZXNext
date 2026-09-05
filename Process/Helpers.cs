using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Entity = TiledIO.Entities;



namespace Tiled2dot8.Process
{
    internal class Helpers
    {

        public static List<Entity.Property> GetPropertiesSpriteTileSheet(Entity.Object obj, Entity.Scene scene)
        {
            uint gid = (uint)obj.Gid ;
            if (gid >= 0)
            {
                gid &= 0xffff;
                var gidData = scene.GetParsedGid((int)gid);     // tile index is 0 based
                int tileIndex = (int)gid - gidData.tileSheet.Firstgid;
                var props = gidData.tileSheet.Tiles.Count > 0 ? gidData.tileSheet.Tiles[tileIndex].Properties : new List<Entity.Property>();
            } else if (obj.Template != null)
            {


            }

            return new List<Entity.Property>();

        }
    }
}
