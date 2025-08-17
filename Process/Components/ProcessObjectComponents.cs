using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2dot8.Entities;
using Tiled2dot8.Extensions;
using Tiled2dot8.Process.Components;


namespace Tiled2dot8.Process.EEI
{
    /// <summary>
    /// Process Environment Element Interaction scrap
    /// </summary>
    public class ProcessObjectComponents : ProcessMaster
    {
        public enum ComponentFlags
        {
            Parameter = 1,  // Generic value (literal of from flag) to be used by other components
            Sprite = 2,     // sprite component
            SpriteFlag = 4, // replace by sprite modular
            Collider = 8,       // body component
            Animation = 16,  // animation component
            Force = 32  // force component
        }

        public class ComponentsType
        {
            public const string Animation = "AnimationComponent";
            public const string Sprite = "SpriteComponent";
            public const string Parameter = "ParameterComponent";
            public const string Body = "BodyComponent";
            public const string Force = "ForceComponent";
        }

        public class GameObject
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Components { get; set; }
            public bool Set { get; private set; }
            public int ParameterType { get; set; }
            public int ParameterValue { get; set; }

            public GameObject()
            {
                Set = false;
                Components = 0;
            }
            public void Setup(double x, double y)
            {
                X = (int)x;
                Y = (int)y;
                Set = true;

            }
        }
        public ProcessObjectComponents(Layer layer, Scene scene, List<Property> properties) : base(layer, scene, properties)
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
                        throw new Exception("Machine is not supported, it should use [spritemodular]");
                        Machine(layer);
                        break;
                    }
                case 17:
                    {
                        GameObjectCompnents(layer);
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

        private void GameObjectCompnents(Layer layer)
        {
            CheckValidator();
            header.Append("\t\tdb $").Append(layer.Layers.Count(c => c.Visible).ToString("X2")).AppendLine("\t\t; Objects count.");
            lengthData++;

            foreach (Entities.Layer entityGameObject in layer.Layers)
            {
                if (entityGameObject.Visible)
                {
                    GameObject gameObject = new();
                    StringBuilder componentsData = new StringBuilder();
                    {
                        //
                        // parameter component - set a literal value of flag value to be consumed by other components
                        //
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Parameter, StringComparison.InvariantCultureIgnoreCase));
                        if (obj != null && obj.Visible)
                        {
                            gameObject.Components |= ((int)ComponentFlags.Parameter);
                            componentsData.AppendLine($"\t\t;\tParameter Component - {obj.Id}");
                            if (obj.Properties.ExistProperty("type") && obj.Properties.ExistProperty("value"))
                            {
                                componentsData.Append("\t\tdb $").Append(obj.Properties.GetPropertyInt("type").Int2Hex("X2")).AppendLine("\t\t; Parameter Type");
                                componentsData.Append("\t\tdb $").Append(obj.Properties.GetPropertyInt("value").Int2Hex("X2")).AppendLine("\t\t; Parameter Value");
                            }
                            lengthData += 2;
                        }
                    }

                    {
                        //
                        //  Sprite Modular Component - setup a sprite with modular configuration
                        //
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Sprite, StringComparison.InvariantCultureIgnoreCase));
                        if (obj != null && obj.Visible)
                        {
                            TechHeader.Add("sprite", 1);
                            gameObject.Components |= ((int)ComponentFlags.Sprite);
                            componentsData.AppendLine($"\t\t;\tSprite Modular Component - {obj.Id}");
                            componentsData.Append("\t\tdb ").Append(obj.Properties.GetProperty("SpriteName").ToUpper()).Append("_PATTERN_ID").AppendLine("\t\t; PatternId");
                            // componentsData.Append("\t\tdb $").Append(obj.Properties.GetPropertyInt("Attributes",0).Int2Hex("X2")).AppendLine("\t\t; Sprite Attributes");
                            componentsData.Append(SpriteModules.WriteObjectsLayer(obj, ref lengthData));

                            lengthData += 1;
                            if (!gameObject.Set)
                            {
                                gameObject.Setup(obj.X, obj.Y);
                            }
                        }
                    }


                    {
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Name.Equals("spriteflag", StringComparison.InvariantCultureIgnoreCase));
                        if (obj != null && obj.Visible)
                        {
                            throw new Exception("[SpriteFlag] is not supported, it should use [spritemodular]");
                            //TechHeader.Add("sprite", 1);
                            //gameObject.Components |= ((int)ComponentFlags.SpriteFlag);
                            //componentsData.AppendLine("\t\t;\tSprite Flag Component - {obj.Id}");
                            //componentsData.Append("\t\tdb ").Append(obj.Properties.GetProperty("SpriteName").ToUpper()).Append("_PATTERN_ID").AppendLine("\t\t; PatternId");
                            //componentsData.Append("\t\tdb ").Append(obj.Properties.GetProperty("FlagName")).AppendLine("\t\t; Flag (Patterm Index)");
                            //componentsData.Append("\t\tdb $").Append(obj.Properties.GetPropertyInt("Attributes").Int2Hex("X2")).AppendLine("\t\t; Sprite Attributes");
                            //lengthData += 3;
                            //if (!gameObject.Set)
                            //{
                            //    gameObject.Setup(obj.X, obj.Y);
                            //}
                        }
                    }

                    //
                    // Force Component - setup a force
                    //
                    {
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Force, StringComparison.InvariantCultureIgnoreCase));
                        if (obj != null && obj.Visible)
                        {
                            TechHeader.Add("force", 1);
                            gameObject.Components |= ((int)ComponentFlags.Force);
                            int forceType = obj.Properties.GetPropertyInt("ForceType");
                            int forceX = obj.Properties.GetPropertyInt("ForceX", 0);
                            int forceY = obj.Properties.GetPropertyInt("ForceY", 0);
                            componentsData.AppendLine("\t\t;\tForce Component");
                            componentsData.Append("\t\tdw $").Append(forceX.Int2Hex("X4")).Append(", $").Append(forceY.Int2Hex("X4")).AppendLine("\t\t; vector2 X,Y");
                            componentsData.Append("\t\tdb $").Append(forceType.ToString("X2")).AppendLine("\t\t; force type");
                            lengthData += 5;
                        }
                    }

                    //
                    // Body Component - setup a body with collision and event
                    //
                    {
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Body, StringComparison.InvariantCultureIgnoreCase));
                        if (obj != null && obj.Visible)
                        {

                            TechHeader.Add("collider", 1);
                            gameObject.Components |= ((int)ComponentFlags.Collider);
                            int layerMask = obj.Properties.GetPropertyInt("LayerMask");
                            int layerId = obj.Properties.GetPropertyInt("Layer");
                            string bodyType = obj.Properties.GetProperty("ColliderType").ToLower();
                            string eventName = obj.Properties.GetProperty("EventName");
                            // merge layer mask with layerID in a single byte
                            layerMask *= 16;
                            layerMask += layerId;

                            byte offsetX = 0;
                            byte offsetY = 0;
                            int x = (int)obj.X + (int)(obj.Width / 2);
                            int y = (int)obj.Y + (int)(obj.Height / 2);

                            if (gameObject.Set)
                            {
                                offsetX = (byte)(x - gameObject.X);
                                offsetY = (byte)(y - gameObject.Y);
                            }
                            componentsData.AppendLine("\t\t; Collider Component");
                            componentsData.Append("\t\tdb $").Append(offsetX.ToString("X2")).Append(", $").Append(offsetY.ToString("X2")).AppendLine("\t\t; offset X,Y");
                            componentsData.Append("\t\tdb $").Append(((int)obj.Width).ToString("X2")).Append(", $").Append(((int)obj.Height).ToString("X2")).AppendLine("\t\t; width, height");
                            componentsData.Append("\t\tdb $").Append(layerMask.ToString("X2")).Append("\t\t; Layer\r\n");

                            componentsData.Append("\t\tdb $").Append(bodyType == "collider_Rigid" ? 128 : 0.ToString("X2")).Append(" + ").Append(eventName).AppendLine("\t\t; Collider Type 0=trigger , 128=rigid + EventName");
                            lengthData += 6;
                            if (!gameObject.Set)
                            {
                                gameObject.Setup(x, y);
                            }
                        }
                    }

                    //
                    // Animation Component - setup a sprite with animation
                    //
                    {
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Animation, StringComparison.InvariantCultureIgnoreCase));
                        if (obj != null && obj.Visible)
                        {
                            TechHeader.Add("animation", 1);
                            gameObject.Components |= ((int)ComponentFlags.Animation);
                            string flagsDescription = "bit 0 = flags Animation style 0 = Loop. 1 = Once";
                            componentsData.AppendLine($"\t\t;\tSprite Animation Component - {obj.Id}");
                            string animationName = obj.Properties.GetProperty("AnimationName");
                            int animationStyle = obj.Properties.GetPropertyInt("Style", 0);
                            int startDelay = obj.Properties.GetPropertyInt("StartDelay", 0);
                            int animationIndex = Project.Instance.Tables["Animation"].Items.FindIndex(r => r.Equals(animationName + "_CODE", StringComparison.CurrentCultureIgnoreCase));
                            int flags = animationStyle;
                            if (startDelay > 0)
                            {
                                lengthData += 1;
                                flags |= 0x80; // set start delay flag
                                flagsDescription += " , bit 7 = Start Delay in frames";
                            }
                            componentsData.Append("\t\tdb $").Append(flags.Int2Hex("X2")).Append("\t\t; ").AppendLine(flagsDescription);
                            if (startDelay > 0)
                            {
                                componentsData.Append("\t\tdb $").Append(startDelay.Int2Hex("X2")).AppendLine("\t\t; start delay in frames ");
                            }
                            componentsData.Append("\t\tdb ").AppendLine(animationName);
                            lengthData += 2;
                            if (!gameObject.Set)
                            {
                                gameObject.Setup(obj.X, obj.Y);
                            }
                        }
                    }
                    //
                    // GameObject 
                    //
                    TechHeader.Add("object", 1);
                    StringBuilder gameObjectHeader = new StringBuilder();
                    gameObjectHeader.Append("\t\t; GameObject: ").AppendLine(entityGameObject.Name);
                    gameObjectHeader.Append("\t\tdb $").Append(gameObject.Components.Int2Hex("X2")).AppendLine("\t\t; GameObject Components Flags");
                    int GameObjectX = gameObject.X + ((int)Controller.Config.Offset.x);
                    int GameObjectY = gameObject.Y + ((int)Controller.Config.Offset.y);
                    gameObjectHeader.Append("\t\tdw $").Append(GameObjectX.Int2Hex("X4")).Append(", $").Append(GameObjectY.Int2Hex("X4")).AppendLine("\t\t; x, y.");
                    lengthData += 5;
                    data.Append(gameObjectHeader);
                    data.Append(componentsData);
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
    }
}