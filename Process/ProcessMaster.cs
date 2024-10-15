using System;
using System.Collections.Generic;
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
    public abstract class ProcessMaster : IProcess
    {
        protected readonly Layer _layer;
        protected readonly Scene _scene;
        protected readonly List<Property> _properties;
        protected int lengthData = 0;
        protected StringBuilder all = new();
        protected StringBuilder headerType = new(512);
        protected StringBuilder header = new(512);
        protected StringBuilder data = new(1024);
        protected int blockType = 0;
        protected bool validatorItemActive = false;
        public ProcessMaster(Layer layer, Scene scene, List<Property> properties)
        {
            _layer = layer;
            _scene = scene;
            _properties = properties;
        }

        public virtual StringBuilder Execute()
        {
            Console.WriteLine("Group " + _layer.Name);
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
        protected virtual StringBuilder WriteObjectsLayer(Layer layer)
        {
            return new StringBuilder();
        }

        protected void CheckValidator()
        {
            blockType = _layer.Properties.GetPropertyInt("Type");        // this type will be used by the engine to map the parser

            StringBuilder validator = Validator.ProcessLayerValidator(_layer.Properties);

            if (validator.Length > 0)
            {
                int prevBlockType = blockType;
                blockType += 128;
                headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine($"\t\t; data block type {prevBlockType} with validator.");
                headerType.Append(validator);
            }
            else
            {
                // validator at item level
                StringBuilder validatorItem = Validator.ProcessItemValidator(_layer.Properties);
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
        }
    }
}