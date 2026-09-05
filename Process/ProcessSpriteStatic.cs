using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledIO.Entities;
using TiledIO.Extensions;

namespace Tiled2dot8
{
    /// <summary>
    /// Process Environment Element Interaction
    /// </summary>
    public class ProcessSpriteStatic : IProcess
    {
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        private readonly List<Property> _properties;
        public ProcessSpriteStatic(Layer layer, Scene scene, List<Property> properties)
        {
            _rootLayer = layer;
            _scene = scene;
            _properties = properties;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + _rootLayer.Name);
            StringBuilder staticSprites = new();

            foreach (Layer staticSpriteGroup in _rootLayer.Layers)
            {
                if (staticSpriteGroup.Visible)
                {
                    Console.WriteLine("Layer " + staticSpriteGroup.Name);
                    staticSprites.Append('.');
                    staticSprites.Append(staticSpriteGroup.Name);
                    staticSprites.Append('_');
                    staticSprites.Append('_');
                    staticSprites.Append(staticSpriteGroup.Id);
                    staticSprites.AppendLine(":");
                    staticSprites.Append(WriteObjectsLayer(staticSpriteGroup));
                }
            }
            return staticSprites;
        }

        private StringBuilder WriteObjectsLayer(Layer staticSpriteGroup)
        {
            int lengthData = 0;
            Console.WriteLine($"\t Static Sprite Group {staticSpriteGroup.Id}");

            StringBuilder headerType = new(200);
            StringBuilder header = new(200);
            StringBuilder data = new();

            int blockType = staticSpriteGroup.Properties.GetPropertyInt("Type");

            StringBuilder validator = ProcessLayers.Validator.ProcessLayerValidator(staticSpriteGroup.Properties);

            if (validator.Length > 0)
            {
                int prevBlockType = blockType;
                blockType += 128;
                headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine($"\t\t; data block type {prevBlockType} with validator");
                headerType.Append(validator);
            }
            else
            {
                headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).Append("\t\t; data block type\r\n");
            }


            var sprites = GetVisibleSprites(staticSpriteGroup).ToList();
            header.Append("\t\tdb $").Append(sprites.Count.ToString("X2")).Append("\t\t; Objects count\r\n");
            lengthData += 1;


            WriteSprites();

            // size must be 2B long (map is over 256 Bytes)
            headerType.Append("\t\tdw $").Append(lengthData.ToString("X4")).Append("\t\t; Block size\r\n");
            // insert header at begin
            header.Insert(0, headerType);
            header.Append(data);
            return header;


            void WriteSprites()
            {
                foreach (var staticSprite in sprites)
                {
                    TechHeader.Add("spritestatic", 1);
                    if (staticSprite.Properties == null)
                    {
                        staticSprite.Properties = new List<Property>();
                    }

                    uint gid = (uint)staticSprite.Gid;
                    const uint MASK = 0xc0000000;
                    uint extend = gid & (MASK);
                    // 0b11000000_00000000_00000000_00000000;
                    // 0b00000000_00000000_00000000_11000000;
                    extend >>= 24;                     // bit 31 -> bit 7
                    gid &= 0xffff;
                    var gidData = _scene.GetParsedGid((int)gid);     // tile index is 0 based
                    if (gidData.tileSheet == null)
                    {
                        throw new InvalidOperationException($"Static sprite {staticSprite.Id} references unknown tile GID {gid}.");
                    }
                    int tileIndex = (int)gid - gidData.tileSheet.Firstgid;
                    var props = gidData.tileSheet.Tiles?.Find(tile => tile.ID == (uint)tileIndex)?.Properties ?? new List<Property>();
                    PropertyExtensions.Merge(staticSprite.Properties, props);
                    PropertyExtensions.Merge(staticSprite.Properties, gidData.tileSheet.Properties);

                    data.AppendLine($"\t\t;\tSprite Static - {staticSprite.Id}");
                    data.Append("\t\tdw $").Append(((int)staticSprite.X).Int2Hex("X4")).Append(", $").Append(((int)staticSprite.Y).Int2Hex("X4")).AppendLine("\t\t; X,Y");
                    data.Append("\t\tdb ").Append(staticSprite.Properties.GetProperty("Name").ToUpper()).Append("_PATTERN_ID").AppendLine("\t\t; PatternId");

                    tileIndex ^= (int)extend;     // apply extend to tile index, so we can use it in pattern table to flip the tile if needed
                    data.Append("\t\tdb $").Append(tileIndex.ToString("X2")).AppendLine("\t\t; bit 7,6 = flip horizontal/vertical bit 5..0 Pattern Index");
                    lengthData += 6;
                }



            }
        }

        private static IEnumerable<TiledIO.Entities.Object> GetVisibleSprites(Layer layer)
        {
            if (!layer.Visible)
            {
                yield break;
            }

            if (layer.Objects != null)
            {
                foreach (var obj in layer.Objects)
                {
                    if (obj.Visible && obj.Gid.HasValue && obj.Gid.Value != 0)
                    {
                        yield return obj;
                    }
                }
            }

            if (layer.Layers != null)
            {
                foreach (var child in layer.Layers)
                {
                    foreach (var sprite in GetVisibleSprites(child))
                    {
                        yield return sprite;
                    }
                }
            }
        }
    }
}
