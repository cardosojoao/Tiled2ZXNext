using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
            Parameter = 1,      // Generic value (literal of from flag) to be used by other components
            Sprite = 2,         // sprite component
            SpriteHW = 4,       // sprite hardware component
            Collider = 8,       // body component
            Animation = 16,     // animation component
            Force = 32,         // force component
            Path = 64           // Path component
        }

        public class ComponentsType
        {
            public const string Animation = "AnimationComponent";
            public const string Sprite = "SpriteComponent";
            public const string SpriteHW = "SpriteHWComponent";
            public const string Parameter = "ParameterComponent";
            public const string Collider = "ColliderComponent";
            public const string Force = "ForceComponent";
            public const string Path = "PathComponent";
        }

        public class GameObject
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Components { get; set; }
            public bool Set { get; private set; }
            public int ParameterType { get; set; }
            public int ParameterValue { get; set; }
            public string Tag { get; set; }
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
                    Console.WriteLine($"GameObject {entityGameObject.Name} {entityGameObject.Id}");
                    GameObject gameObject = new();
                    gameObject.Tag = entityGameObject.Properties.GetProperty("tag", "tag_None");
                    StringBuilder componentsData = new StringBuilder();
                    {
                        //
                        // parameter component - set a literal value of flag value to be consumed by other components
                        //
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Parameter, StringComparison.InvariantCultureIgnoreCase) && c.Visible);
                        if (obj != null && obj.Visible)
                        {
                            Console.WriteLine($"\t Component parameter {obj.Id}");
                            gameObject.Components |= ((int)ComponentFlags.Parameter);
                            componentsData.AppendLine($"\t;\tParameter Component - {obj.Id}");
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
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Sprite, StringComparison.InvariantCultureIgnoreCase) && c.Visible);
                        if (obj != null && obj.Visible)
                        {
                            Console.WriteLine($"\t Component sprite software {obj.Id}");
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
                        //
                        //  Sprite hardware component
                        //
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.SpriteHW, StringComparison.InvariantCultureIgnoreCase) && c.Visible);
                        if (obj != null && obj.Visible)
                        {
                            Console.WriteLine($"\t Component sprite hardware {obj.Id}");
                            TechHeader.Add("spritehw", 1);
                            gameObject.Components |= ((int)ComponentFlags.SpriteHW);
                            componentsData.AppendLine($"\t\t;\tSpriteHW Component - {obj.Id}");
                            componentsData.Append("\t\tdb ").Append(obj.Properties.GetProperty("Pattern").ToUpper()).Append("_PATTERN_ID").AppendLine("\t\t; PatternId");
                            componentsData.Append("\t\tdb ").Append(obj.Properties.GetPropertyInt("PatternIndex").Int2Hex("X2")).AppendLine("\t\t; Patterm Index");

                            bool flipVertical = obj.Properties.GetPropertyBool("FlipVert");
                            bool flipHorizontal = obj.Properties.GetPropertyBool("FlipHoriz");
                            bool rotate = obj.Properties.GetPropertyBool("Rotate");

                            int pallete = (byte)obj.Properties.GetPropertyInt("Pallete") << 4;

                            pallete += ((flipHorizontal ? 1 : 0) << 3) + ((flipVertical ? 1 : 0) << 2) + ((rotate ? 1 : 0) << 1);

                            componentsData.Append("\t\tdb $").Append(pallete.Int2Hex("X2")).AppendLine("\t\t; bit7..bit40=Pallete Index , bit1=flipHoriz, bit0=flipVert");
                            int width = obj.Properties.GetPropertyInt("Width");
                            int heigth = obj.Properties.GetPropertyInt("Heigth");
                            int dimensions = ((width << 4) + heigth);
                            componentsData.Append("\t\tdb $").Append(dimensions.Int2Hex("X2")).AppendLine("\t\t; bit7..bit4=width , bit3..bit0=heigth");

                            lengthData += 4;
                            if (!gameObject.Set)
                            {
                                gameObject.Setup(obj.X, obj.Y);
                            }
                        }
                    }

                    {
                        //
                        // Body Component - setup a body with collision and event
                        //
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Collider, StringComparison.InvariantCultureIgnoreCase) && c.Visible);
                        if (obj != null)
                        {
                            Console.WriteLine($"\t Component collider {obj.Id}");
                            TechHeader.Add("collider", 1);
                            gameObject.Components |= ((int)ComponentFlags.Collider);
                            int layerMask = obj.Properties.GetPropertyInt("LayerMask");
                            int layerId = obj.Properties.GetPropertyInt("Layer");
                            string bodyType = obj.Properties.GetProperty("ColliderType").ToLower();
                            string eventName = obj.Properties.GetProperty("EventName");
                            // merge layer mask with layerID in a single byte
                            layerMask *= 16;
                            layerMask += layerId;
                            // offset bettween the object and collider
                            sbyte offsetX = 0;
                            sbyte offsetY = 0;
                            int x = (int)obj.X + (int)(obj.Width / 2);
                            int y = (int)obj.Y + (int)(obj.Height / 2);

                            if (gameObject.Set)
                            {
                                offsetX = (sbyte)(x - gameObject.X);
                                offsetY = (sbyte)(y - gameObject.Y);
                            }
                            componentsData.AppendLine($"\t\t;\tCollider Component - {obj.Id}");
                            componentsData.Append("\t\tdb $").Append(offsetX.ToString("X2")).Append(", $").Append(offsetY.ToString("X2")).AppendLine("\t\t; offset X,Y");
                            componentsData.Append("\t\tdb $").Append(((int)obj.Width).ToString("X2")).Append(", $").Append(((int)obj.Height).ToString("X2")).AppendLine("\t\t; width, height");
                            componentsData.Append("\t\tdb $").Append(layerMask.ToString("X2")).Append("\t\t; Layer\r\n");
                            componentsData.Append("\t\tdb $").Append(bodyType.Equals("collider_Rigid", StringComparison.InvariantCultureIgnoreCase) ? 128.ToString("X2") : 0.ToString("X2")).Append(" + ").Append(eventName).AppendLine("\t\t; Collider Type 0=trigger , 128=rigid + EventName");
                            lengthData += 6;
                            if (!gameObject.Set)
                            {
                                gameObject.Setup(x, y);
                            }
                        }
                    }

                    {
                        //
                        // Animation Component - setup a sprite with animation
                        //
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Animation, StringComparison.InvariantCultureIgnoreCase) && c.Visible);
                        if (obj != null)
                        {
                            /*
                             * The goal is to get the animation data and load into the heap and get the id 
                             */
                            Console.WriteLine($"\t Component animation {obj.Id}");
                            TechHeader.Add("animation", 1);
                            gameObject.Components |= ((int)ComponentFlags.Animation);



                            
                            componentsData.AppendLine($"\t\t;\tAnimation Component - {obj.Id}");

                            //string animationName = obj.Properties.GetProperty("AnimationName");
                            int animationStyle = obj.Properties.GetPropertyInt("Style", 0);
                            int startDelay = obj.Properties.GetPropertyInt("StartDelay", 0);
                            string stepsRaw = obj.Properties.GetProperty("Steps", "");
                            int Direction = obj.Properties.GetPropertyInt("V/H",0);

                            int[][] steps = stepsRaw
                                                .Split(';')
                                                .Select(part => part.Split(',').Select(int.Parse).ToArray())
                                                .ToArray();

                            int i = 0;
                            PolygonPoint current = new() { X = (float)obj.X + obj.Polygon[i].X, Y = (float)obj.Y + obj.Polygon[i].Y };
                            PolygonPoint next;

                            int[] linesX = new int[obj.Polygon.Count - 1];
                            int[] linesY = new int[obj.Polygon.Count - 1];
                            int lineX = 0;
                            int lineY = 0;
                            i++;
                            for (; i < obj.Polygon.Count; i++)
                            {
                                next = new() { X = (float)obj.X + obj.Polygon[i].X, Y = (float)obj.Y + obj.Polygon[i].Y };
                                lineX = (int)(next.X - current.X);
                                lineY = (int)(next.Y - current.Y);
                                current = next;
                                linesX[i-1] = lineX;
                                linesY[i-1] = lineY;
                            }

                            componentsData.Append("\t\tdb $").Append(linesY.Length.Byte2Hex("X2")).AppendLine("\t;\t\t Number of steps");
                            for (int lineIndex = 0; lineIndex < linesX.Length; lineIndex++)
                            {
                                lineX = linesX[lineIndex];
                                lineY = linesY[lineIndex];
                                componentsData.Append("\t\tdb $").Append(Math.Abs(lineY).Byte2Hex("X2"))
                                    .Append(", $").Append(steps[lineIndex][0].Byte2Hex("X2"))
                                    .Append(", $").Append((Math.Sign(lineY) * steps[lineIndex][1]).Byte2Hex("X2"))
                                    .AppendLine("\t\t; Length, frames per step, step increment");
                                lengthData += 3;
                            }

                            // int animationIndex = Project.Instance.Tables["Animation"].Items.FindIndex(r => r.Equals(animationName + "_CODE", StringComparison.CurrentCultureIgnoreCase));

                            string flagsDescription = "bit0 = flags Animation style 0 = Loop. 1 = Once , bit1 = Vertical = 0, Horizontal = 1, bit7 = Start Delay in frames";
                            int flags = animationStyle;         // bit 0  0 = Loop, 1 = Once
                            flags |= (Direction << 1);          // bit 1  0 = Vertical, 1 = Horizontal

                            if (startDelay > 0)
                            {
                                flags |= 0x80; // set start delay flag
                            }
                            componentsData.Append("\t\tdb $").Append(flags.Int2Hex("X2")).Append("\t\t; ").AppendLine(flagsDescription);
                            lengthData += 1;
                            if (startDelay > 0)
                            {
                                componentsData.Append("\t\tdb $").Append(startDelay.Int2Hex("X2")).AppendLine("\t\t; start delay in frames ");
                                lengthData += 1;
                            }
                            //componentsData.Append("\t\tdb ").AppendLine(animationName);
                            

                            if (!gameObject.Set)
                            {
                                gameObject.Setup(obj.X, obj.Y);
                            }
                        }
                    }

                    {
                        //
                        // Force Component - setup a force
                        //
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Force, StringComparison.InvariantCultureIgnoreCase));
                        if (obj != null && obj.Visible)
                        {
                            Console.WriteLine($"\t Component force {obj.Id}");
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

                    {
                        //
                        // Path Component - setup a path for moving objects
                        //
                        Entities.Object obj = entityGameObject.Objects.Find(c => c.Type.Equals(ComponentsType.Path, StringComparison.InvariantCultureIgnoreCase));
                        if (obj != null && obj.Visible)
                        {
                            Console.WriteLine($"\t Component path {obj.Id}");
                            if ((gameObject.Components & ((int)ComponentFlags.SpriteHW)) == 0)
                            {
                                throw new Exception($"Path component [{obj.Id}] doesn't have a Sprite Hardware component");
                            }
                            TechHeader.Add("path", 1);
                            gameObject.Components |= ((int)ComponentFlags.Path);
                            componentsData.AppendLine($"\t\t;\tPath Component - {obj.Id}");
                            int PathID = GetPathId(obj.Properties.GetPropertyInt("PathId"));
                            componentsData.Append("\t\tdb $").Append(PathID.Int2Hex("X2")).AppendLine("\t\t; Path Id");
                            lengthData += 1;
                            gameObject.Tag = "tag_Platform";
                        }
                    }

                    //
                    // GameObject 
                    //
                    TechHeader.Add("object", 1);
                    StringBuilder gameObjectHeader = new StringBuilder();
                    gameObjectHeader.Append("\t\t; GameObject: ").AppendLine(entityGameObject.Name);
                    gameObjectHeader.Append("\t\tdb $").Append(gameObject.Components.Int2Hex("X2")).AppendLine("\t\t; GameObject Components Flags");
                    gameObjectHeader.Append("\t\tdb ").Append(gameObject.Tag).AppendLine("\t\t; GameObject tag");
                    int GameObjectX = gameObject.X + ((int)Controller.Config.Offset.x);
                    int GameObjectY = gameObject.Y + ((int)Controller.Config.Offset.y);
                    gameObjectHeader.Append("\t\tdw $").Append(GameObjectX.Int2Hex("X4")).Append(", $").Append(GameObjectY.Int2Hex("X4")).AppendLine("\t\t; x, y.");
                    lengthData += 6;
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

        private int GetPathId(int pathObjectId)
        {
            int id = -1;
            Layer pathLayer = _scene.Layers.Find(l => l.Name.Equals("path", StringComparison.InvariantCultureIgnoreCase));
            pathLayer = pathLayer.Layers.Find(l => l.Name.Equals("path", StringComparison.InvariantCultureIgnoreCase));
            Entities.Object path = pathLayer.Objects.Find(p => p.Id == pathObjectId);
            id = path.Properties.GetPropertyInt("Id");
            return id;
        }
    }
}