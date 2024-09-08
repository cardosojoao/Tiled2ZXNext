using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
//using Tiled2ZXNext.Models;
using Tiled2ZXNext.ProcessLayers;


namespace Tiled2ZXNext.Process.EEI
{
    /// <summary>
    /// Process Environment Element Interaction scrap
    /// </summary>
    public class ProcessSprite : ProcessMaster
    {
        public ProcessSprite(Layer layer, Scene scene, List<Property> properties) : base(layer, scene, properties)
        {
        }

        /// <summary>
        /// write objects layer , there is an header with the shared properties and then each line contain the object properties
        /// </summary>
        /// <param name="layer">layer</param>
        /// <returns>string builder collection with header and data</returns>
        protected override StringBuilder WriteObjectsLayer(Layer layer)
        {
            blockType = layer.Properties.GetPropertyInt("Type");        // this type will be used by the engine to map the parser
            layer.Properties.Merge(_properties);
            // size must be 2Bytes long (map is over 256 Bytes)
            switch (blockType)
            {
                case 8:
                    {
                        SpriteDynamic(layer);
                        break;
                    }
                case 9:
                    {
                        Machine(layer);
                        break;
                    }
                default:
                    break;
            }


            headerType.Append("\t\tdw $").Append(lengthData.ToString("X4")).Append("\t\t; Block size\r\n");
            // insert header at begin
            header.Insert(0, headerType);
            header.Append(data);
            return header;
        }

        private void SpriteDynamic(Layer layer)
        {
            // validator at layer level
            StringBuilder validatorLayer = Validator.ProcessLayerValidator(layer.Properties);
            if (validatorLayer.Length > 0)
            {
                int prevBlockType = blockType;
                blockType += 128;
                headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine($"\t\t; data block type {prevBlockType} with Validator layer.");
                headerType.Append(validatorLayer);
            }
            else
            {
                // validator at item level
                StringBuilder validatorItem = Validator.ProcessItemValidator(layer.Properties);
                validatorItemActive = validatorItem.Length > 0;
                if (validatorItemActive)
                {
                    int prevBlockType = blockType;
                    blockType += 64;
                    headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine($"\t\t; data block type {prevBlockType} with Validator item.");
                    //headerType.Append(validatorItem);
                }
                else
                {
                    headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine("\t\t; data block type");
                }
            }
            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible).ToString("X2")).AppendLine("\t\t; Objects count.");
            lengthData++;

            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    int x = (int)obj.X + (int)(obj.Width / 2) + (int)Controller.Config.Offset.x;    // middle of first sprite, left start with 0 
                    int y = (int)obj.Y - (int)(obj.Height / 2) + (int)Controller.Config.Offset.y;    // middle of first sprite, top starts with 16
                    if (validatorItemActive)
                    {
                        int flagId = obj.Properties.GetPropertyInt("FlagId");
                        int enableValue = obj.Properties.GetPropertyInt("EnableValue");
                        data.Append("\t\tdb $").Append(flagId.Int2Hex("X2")).AppendLine("\t\t; Flag Id.");
                        data.Append("\t\tdb $").Append(enableValue.Int2Hex("X2")).AppendLine("\t\t; enable value.");
                        lengthData += 2;
                    }
                    int spriteIndex = obj.Properties.GetPropertyInt("SpriteIndex");
                    int spritePatternId = obj.Properties.GetPropertyInt("SpriteName");
                    data.Append("\t\tdw $").Append(x.Int2Hex("X4")).Append(", $").Append(y.Int2Hex("X4")).AppendLine("\t\t; x, y.");
                    data.Append("\t\tdb $").Append(spritePatternId.Int2Hex("X2")).Append(", $").Append(spriteIndex.Int2Hex("X2")).AppendLine("\t\t; spritePattern, spriteIndex.");
                    lengthData += 6;
                }
            }

        }

        private void Machine(Layer layer)
        {
            headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine("\t\t; data block type");
            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible).ToString("X2")).AppendLine("\t\t; Objects count.");
            lengthData++;
            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    int x = (int)obj.X + (int)(obj.Width / 2) + (int)Controller.Config.Offset.x;    // middle of first sprite, left start with 0 
                    int y = (int)obj.Y - (int)(obj.Height / 2) + (int)Controller.Config.Offset.y;    // middle of first sprite, top starts with 16
                    int spritePatternId = obj.Properties.GetPropertyInt("SpriteName");
                    int flagId = obj.Properties.GetPropertyInt("FlagId");
                    data.Append("\t\tdw $").Append(x.Int2Hex("X4")).Append(", $").Append(y.Int2Hex("X4")).AppendLine("\t\t; x, y.");
                    data.Append("\t\tdb $").Append(spritePatternId.Int2Hex("X2")).AppendLine("\t\t; Sprite Name.");
                    data.Append("\t\tdb $").Append(flagId.Int2Hex("X2")).AppendLine("\t\t; Flag Id.");
                    lengthData += 6;
                }
            }
        }
    }
}