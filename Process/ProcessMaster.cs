using System;
using System.Collections.Generic;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;


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
    }
}