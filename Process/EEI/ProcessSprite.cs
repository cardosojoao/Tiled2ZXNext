using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.Process.Components;


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
                case 12:
                    {
                        SpriteDynamicComponent(layer);
                        break;
                    }
                case 16:
                    {
                        // sprite static
                        SpriteDynamicComponent(layer);
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

        private void SpriteDynamicComponent(Layer layer)
        {
            CheckValidator();
            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible).ToString("X2")).AppendLine("\t\t; Objects count.");
            lengthData++;

            StringBuilder spriteBlock = new(1024);
            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    spriteBlock.Clear();
                    int blockLength = 0;

                    int x = (int)obj.X + (int)(obj.Width / 2) + (int)Controller.Config.Offset.x;    // middle of first sprite, left start with 0 
                    int y = (int)obj.Y - (int)(obj.Height / 2) + (int)Controller.Config.Offset.y;    // middle of first sprite, top starts with 16
                    if (validatorItemActive)
                    {
                        int flagId = obj.Properties.GetPropertyInt("FlagId");
                        int enableValue = obj.Properties.GetPropertyInt("EnableValue");
                        spriteBlock.Append("\t\tdb $").Append(flagId.Int2Hex("X2")).AppendLine("\t\t; Flag Id.");
                        spriteBlock.Append("\t\tdb $").Append(enableValue.Int2Hex("X2")).AppendLine("\t\t; enable value.");
                        blockLength += 2;
                    }
                    string spritePatternName = obj.Properties.GetProperty("SpriteName");
                    spriteBlock.Append("\t\tdw $").Append(x.Int2Hex("X4")).Append(", $").Append(y.Int2Hex("X4")).AppendLine("\t\t; x, y.");
                    spriteBlock.Append("\t\tdb ").Append(spritePatternName).Append(Controller.Config.SpriteSoftwareSuffix).AppendLine("\t\t; spritePatternName.");
                    blockLength += 5;
                    spriteBlock.Append(SpriteModules.WriteObjectsLayer(obj, ref blockLength));
                    lengthData += blockLength;

                    StringBuilder blockSize = new StringBuilder(256);
                    blockSize.Append("\t\tdb $").Append(blockLength.Int2Hex("X2")).AppendLine("\t\t; sprite block size");
                    lengthData++;
                    spriteBlock.Insert(0, blockSize);
                    data.Append(spriteBlock);
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
                    int x = (int)obj.X + (int)Controller.Config.Offset.x;    // middle of first sprite, left start with 0 
                    int y = (int)obj.Y + (int)Controller.Config.Offset.y;


                    int spritePatternId = obj.Properties.GetPropertyInt("SpriteName");
                    int flagId = obj.Properties.GetPropertyInt("FlagId");
                    data.Append("\t\tdw $").Append(x.Int2Hex("X4")).Append(", $").Append(y.Int2Hex("X4")).AppendLine("\t\t; x, y.");
                    data.Append("\t\tdb $").Append(spritePatternId.Int2Hex("X2")).AppendLine("\t\t; Sprite Name.");
                    data.Append("\t\tdb $").Append(flagId.Int2Hex("X2")).AppendLine("\t\t; Flag Id.");
                    lengthData += 6;
                }
            }
        }

        private void SpriteDynamic(Layer layer)
        {
            headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine("\t\t; data block type");
            lengthData++;
            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {

                    int x = (int)obj.X + (int)Controller.Config.Offset.x;    // middle of first sprite, left start with 0 
                    int y = (int)obj.Y + (int)Controller.Config.Offset.y;


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