using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled2dot8.Entities;
using Tiled2dot8.Extensions;

namespace Tiled2dot8.Process.Components
{
    /// <summary>
    /// Process the dynamic parameters of sprites
    /// </summary>
    internal class SpriteModules
    {
        public static StringBuilder WriteObjectsLayer(Entities.Object obj, ref int lengdata)
        {
            StringBuilder data = new(1024);

            byte bitFlags = 0;
            StringBuilder bitFlagsComment = new(256);

            //            string AnimationName = obj.Properties.GetProperty("AnimationName");
            int offSetX = obj.Properties.GetPropertyInt("OffSetX", 0);
            int offSetY = obj.Properties.GetPropertyInt("OffSetY", 0);

            int shiftVertical = obj.Properties.GetPropertyInt("ShiftVertical", 0);
            int shiftHorizontal = obj.Properties.GetPropertyInt("ShiftHorizontal", 0);

            int shiftVerticalDirection = obj.Properties.GetPropertyInt("ShiftVerticalDirection", 0);
            int shiftHorizontalDirection = obj.Properties.GetPropertyInt("ShiftHorizontalDirection", 0);

            string flagName = obj.Properties.GetProperty("FlagName");
            int spriteIndex = obj.Properties.GetPropertyInt("SpriteIndex", -1);

            bool DynamicShift = obj.Properties.GetPropertyBool("DynamicShift", false);


            // sprite index or flag name
            if (spriteIndex == -1 && flagName.Length == 0)
            {
                throw new Exception("SpriteIndex or flag name or Animation not found");
            }
            else
            {
                lengdata++;

                if (flagName.Length > 0)
                {
                    bitFlags |= 0x80;

                    if (int.TryParse(flagName, out int flagNumber))
                    {
                        data.Append("\t\tdb $").Append(flagNumber.Int2Hex("X2")).AppendLine("\t\t; Flag Number.");
                        bitFlagsComment.Append("\t\t; flag bits - $80 Flag number");
                    }
                    else
                    {
                        data.Append("\t\tdb ").Append(flagName).AppendLine("\t\t; Flag Name.");
                        bitFlagsComment.Append("\t\t; flag bits - $80 Flag Name");
                    }
                }
                else if ((bitFlags & 0x01) == 0)
                {
                    bitFlags |= 0x40;
                    data.Append("\t\tdb $").Append(spriteIndex.Int2Hex("X2")).AppendLine("\t\t; sprite index.");
                    bitFlagsComment.Append("\t\t; flag bits - $40 Sprite index");
                }
            }

            // offset x, y
            if (offSetX != 0 && offSetY != 0)
            {
                lengdata += 2;
                bitFlags |= 0x20;
                bitFlagsComment.Append(", $20 Offset");
                data.Append("\t\tdb $").Append(offSetX.Byte2Hex("X2")).AppendLine("\t\t; offset X.");
                data.Append("\t\tdb $").Append(offSetY.Byte2Hex("X2")).AppendLine("\t\t; offset Y.");
            }

            // shiftHorizontal vertical
            if (shiftVertical != 0 || shiftVerticalDirection != 0)
            {
                lengdata += 1;
                bitFlags |= 0x10;
                bitFlagsComment.Append(", $10 shift vertical");
                if(shiftVerticalDirection > 0)
                {
                    shiftVertical |= (shiftVerticalDirection == 1 ? 0 : 128);       // 0= up, 128 = down  // just set 7 bit 
                }
                data.Append("\t\tdb $").Append(shiftVertical.Byte2Hex("X2")).AppendLine("\t\t; shiftVertical");
            }
            // shiftHorizontal horizontal
            if (shiftHorizontal != 0 || shiftHorizontalDirection != 0)
            {
                lengdata += 1;
                bitFlags |= 0x08;
                bitFlagsComment.Append(", $08 shift horizontal");
                if (shiftHorizontalDirection > 0)
                {
                    shiftHorizontal |= (shiftHorizontalDirection == 1 ? 0 : 128);   // just set 7 bit 
                }
                data.Append("\t\tdb $").Append(shiftHorizontal.Byte2Hex("X2")).AppendLine("\t\t; shiftHorizontal");
            }

            if (DynamicShift)
            {
                bitFlags |= 0x04;
                bitFlagsComment.Append(", $04 shift dynamic");
            }

            lengdata++;    // bit flags
            bitFlagsComment.Insert(0, $"\t\tdb ${((int)bitFlags).Int2Hex("X2")}").AppendLine();
            data.Insert(0, bitFlagsComment);
            return data;
        }
    }
}
