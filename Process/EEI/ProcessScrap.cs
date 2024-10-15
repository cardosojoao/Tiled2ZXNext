using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.ProcessLayers;


namespace Tiled2ZXNext.Process.EEI
{
    /// <summary>
    /// Process Environment Element Interaction scrap
    /// </summary>
    public class ProcessScrap : ProcessMaster
    {
        //private readonly Layer _layer;
        //private readonly Scene _scene;
        //private readonly List<Property> _properties;
        public ProcessScrap(Layer layer, Scene scene, List<Property> properties) : base(layer,scene,properties)
        {
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + _layer.Name);
            StringBuilder all = new();
            if (_layer.Visible)
            {
                string fileName = _scene.Properties.GetProperty("FileName");
                all.Append('.').Append(fileName).Append('_').Append(_layer.Name).Append('_').Append(_layer.Id).AppendLine(":");
                all.Append(WriteObjectsLayer(_layer));
            }
            return all;
        }

        /// <summary>
        /// write objects layer , there is an header with the shared properties and then each line contain the object properties
        /// </summary>
        /// <param name="layer">layer</param>
        /// <returns>string builder collection with header and data</returns>
        private StringBuilder WriteObjectsLayer(Layer layer)
        {
            //bool validatorItemActive = false;
            //int lengthData = 0;
            //StringBuilder data = new(1024);
            //StringBuilder headerType = new(200);
            //StringBuilder header = new(200);
            ////List<StringBuilder> output = new() { header, data };
            layer.Properties.Merge(_properties);

            CheckValidator();

            //int blockType = layer.Properties.GetPropertyInt("Type");        // this type will be used by the engine to map the parser
            int layerMask = layer.Properties.GetPropertyInt("LayerMask");
            int layerId = layer.Properties.GetPropertyInt("Layer");
            string eventName = layer.Properties.GetProperty("EventName");

            int eventIndex = Project.Instance.Tables["EventName"].Items.FindIndex(r => r.Equals(eventName, StringComparison.CurrentCultureIgnoreCase));

            
            // validator at layer level
            //StringBuilder validatorLayer = Validator.ProcessLayerValidator(layer.Properties);
            //if (validatorLayer.Length > 0)
            //{
            //    int prevBlockType = blockType;
            //    blockType += 128;
            //    headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine($"\t\t; data block type {prevBlockType} with Validator layer.");
            //    headerType.Append(validatorLayer);
            //}
            //else
            //{
            //    // validator at item level
            //    StringBuilder validatorItem = Validator.ProcessItemValidator(layer.Properties);
            //    validatorItemActive = validatorItem.Length > 0;
            //    if (validatorItemActive)
            //    {
            //        int prevBlockType = blockType;
            //        blockType += 64;
            //        headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine($"\t\t; data block type {prevBlockType} with Validator item.");
            //        //headerType.Append(validatorItem);
            //    }
            //    else
            //    {
            //        headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine("\t\t; data block type");
            //    }
            //}

            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible).ToString("X2")).AppendLine("\t\t; Objects count.");
            lengthData++;

            // merge layer mask with layerID in a single byte
            layerMask *= 16;
            layerMask += layerId;
            header.Append("\t\tdb $").Append(layerMask.ToString("X2")).AppendLine("\t\t; Layer");
            header.Append("\t\tdb $").Append(eventIndex.ToString("X2")).Append("\t\t; Event ID [").Append(eventName).AppendLine("]");
            lengthData += 2;
            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible)
                {
                    int x = (int)obj.X + 8 + (int)Controller.Config.Offset.x;    // middle of first sprite, left start with 0 + 32 pixels sprites
                    int y = (int)obj.Y - 8 + (int)Controller.Config.Offset.y;    // middle of first sprite, top starts with 16 + 32 pixels sprites

                    if (validatorItemActive)
                    {

                        int flagId = obj.Properties.GetPropertyInt("FlagId");
                        int enableValue = obj.Properties.GetPropertyInt("EnableValue");

                        data.Append("\t\tdb $").Append(flagId.Int2Hex("X2")).AppendLine("\t\t; Flag Id.");
                        data.Append("\t\tdb $").Append(enableValue.Int2Hex("X2")).AppendLine("\t\t; enable value.");
                        lengthData += 2;
                    }

                    data.Append("\t\tdw $").Append(x.Int2Hex("X4")).Append(", $").Append(y.Int2Hex("X4")).AppendLine("\t\t; x, y.");
                    lengthData += 4;


                }
            }
            // size must be 2Bytes long (map is over 256 Bytes)
            headerType.Append("\t\tdw $").Append(lengthData.ToString("X4")).Append("\t\t; Block size\r\n");
            // insert header at begin
            header.Insert(0, headerType);
            header.Append(data);
            return header;
        }
    }
}