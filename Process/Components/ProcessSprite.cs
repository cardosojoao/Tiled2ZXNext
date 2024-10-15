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
    public class ProcessSpriteComponents : ProcessMaster
    {
        public enum ComponentFlags
        {
            Sprite = 1,
            SpriteFlag = 2,
            Body = 4
        }

        public class GameObject
        {
            public int X { get; set; }
            public int Y { get; set; }
            public bool Set { get; private set; }

            public GameObject()
            {
                Set = false;
            }
            public void Setup(double x, double y)
            {
                X = (int)x;
                Y = (int)y;
                Set = true;
            }
        }
        public ProcessSpriteComponents(Layer layer, Scene scene, List<Property> properties) : base(layer, scene, properties)
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
                //case 8:
                //    {
                //        SpriteDynamicOld(layer);
                //        break;
                //    }
                case 9:
                    {
                        Machine(layer);
                        break;
                    }
                case 12:
                    {
                        SpriteDynamic(layer);
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
            CheckValidator();
            header.Append("\t\tdb $").Append(layer.Layers.Count(c => c.Visible).ToString("X2")).AppendLine("\t\t; Objects count.");
            lengthData ++;
            foreach (Entities.Layer layerComponents in layer.Layers)
            {
                int components = layerComponents.Properties.GetPropertyInt("Components");
                GameObject gameObject = new();
                StringBuilder componentsData = new StringBuilder();

                if ((components & ((int)ComponentFlags.Sprite)) == ((int)ComponentFlags.Sprite))
                {
                    Entities.Object obj = layerComponents.Objects.Find(c => c.Name.Equals("sprite", StringComparison.InvariantCultureIgnoreCase));
                    componentsData.AppendLine("\t\t;\tSprite Component");
                    componentsData.Append("\t\tdb ").Append(obj.Properties.GetProperty("SpriteName").ToUpper()).Append("_PATTERN_ID").AppendLine("\t\t; PatternId");
                    componentsData.Append("\t\tdb $").Append(obj.Properties.GetPropertyInt("SpriteIndex").Int2Hex("X2")).AppendLine("\t\t; Pattern Index");
                    lengthData += 2;
                    if (!gameObject.Set)
                    {
                        gameObject.Setup(obj.X, obj.Y);
                    }
                }
                if ((components & ((int)ComponentFlags.SpriteFlag)) == ((int)ComponentFlags.SpriteFlag))
                {
                    Entities.Object obj = layerComponents.Objects.Find(c => c.Name.Equals("spriteflag", StringComparison.InvariantCultureIgnoreCase));
                    componentsData.AppendLine("\t\t;\tSprite Flag Component");
                    componentsData.Append("\t\tdb ").Append(obj.Properties.GetProperty("SpriteName").ToUpper()).Append("_PATTERN_ID").AppendLine("\t\t; PatternId");
                    componentsData.Append("\t\tdb $").Append(obj.Properties.GetPropertyInt("FlagId").Int2Hex("X2")).AppendLine("\t\t; Flag Patterm Index");
                    lengthData += 2;
                    if (!gameObject.Set)
                    {
                        gameObject.Setup(obj.X, obj.Y);
                    }
                }
                if ((components & ((int)ComponentFlags.Body)) == ((int)ComponentFlags.Body))
                {
                    Entities.Object obj = layerComponents.Objects.Find(c => c.Name.Equals("body", StringComparison.InvariantCultureIgnoreCase));

                    int layerMask = obj.Properties.GetPropertyInt("LayerMask");
                    int layerId = obj.Properties.GetPropertyInt("Layer");
                    string bodyType = obj.Properties.GetProperty("BodyType").ToLower();
                    string eventName = obj.Properties.GetProperty("EventName");

                    int eventIndex = Project.Instance.Tables["EventName"].Items.FindIndex(r => r.Equals(eventName, StringComparison.CurrentCultureIgnoreCase));
                    // merge layer mask with layerID in a single byte
                    layerMask *= 16;
                    layerMask += layerId;

                    byte offsetX = 0;
                    byte offsetY = 0;
                    if (gameObject.Set)
                    {
                        int x = (int)obj.X + (int)(obj.Width / 2);
                        int y = (int)obj.Y + (int)(obj.Height/ 2);
                        offsetX = (byte)(x - gameObject.X);
                        offsetY = (byte)(y - gameObject.Y);
                    }
                    componentsData.AppendLine("\t\t;\tBody Component");
                    componentsData.Append("\t\tdb $").Append(offsetX.ToString("X2")).Append(", $").Append(offsetY.ToString("X2")).AppendLine("\t\t; offset X,Y");
                    componentsData.Append("\t\tdb $").Append(((int)obj.Width).ToString("X2")).Append(", $").Append(((int)obj.Height).ToString("X2")).AppendLine("\t\t; width, height");
                    componentsData.Append("\t\tdb $").Append(layerMask.ToString("X2")).Append("\t\t; Layer\r\n");
                    componentsData.Append("\t\tdb $").Append(ProcessCollision.BodyTypeInt(bodyType) > 0 ? 128 : 0.ToString("X2")).Append(" + $").Append(eventIndex.ToString("X2")).Append("\t\t; Body Type 0=trigger , 128=rigid + EventName = ").AppendLine(eventName);   // to be removed
                    lengthData += 6;
                    if (!gameObject.Set)
                    {
                        gameObject.Setup(obj.X, obj.Y);
                    }
                }
                StringBuilder gameObjectHeader = new StringBuilder();
                gameObjectHeader.AppendLine("\t\t; GameObject Components Flags");
                gameObjectHeader.Append("\t\tdb $").AppendLine(layerComponents.Properties.GetPropertyInt("Components").Int2Hex("X2"));
                gameObjectHeader.Append("\t\tdw $").Append(gameObject.X.Int2Hex("X4")).Append(", $").Append(gameObject.Y.Int2Hex("X4")).AppendLine("\t\t; x, y.");
                lengthData += 5;

                data.Append(gameObjectHeader);
                data.Append(componentsData);
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
                    //int x = (int)obj.X + (int)(obj.Width / 2) + (int)Controller.Config.Offset.x;    // middle of first sprite, left start with 0 
                    //int y = (int)obj.Y - (int)(obj.Height / 2) + (int)Controller.Config.Offset.y;    // middle of first sprite, top starts with 16

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

        private void SpriteDynamicComponent(Layer layer)
        {
            headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine("\t\t; data block type");
            //header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible).ToString("X2")).AppendLine("\t\t; Objects count.");
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