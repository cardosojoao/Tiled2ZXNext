using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.ProcessLayers;


namespace Tiled2ZXNext
{
    public class ProcessPaths : IProcess
    {
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        private readonly List<Property> _properties;
        public ProcessPaths(Layer layer, Scene scene, List<Property> properties)
        {
            _rootLayer = layer;
            _scene = scene;
            _properties = properties;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + _rootLayer.Name);
            StringBuilder locationsCode = new();
            string fileName = _scene.Properties.GetProperty("FileName");
            foreach (Layer layer in _rootLayer.Layers)
            {
                if (layer.Visible && layer.Name.Equals("path", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Layer " + layer.Name);
                    locationsCode.Append('.');
                    locationsCode.Append(fileName);
                    locationsCode.Append('_');
                    locationsCode.Append(layer.Name);
                    locationsCode.Append('_');
                    locationsCode.Append(layer.Id);
                    locationsCode.Append(":\r\n");
                    locationsCode.Append(WriteObjectsLayer(layer));
                }
            }
            return locationsCode;
        }

        /// <summary>
        /// write objects layer compressed, there is an header with the shared properties and then each line contain the object property
        /// </summary>
        /// <param name="layer">layer</param>
        /// <returns>string builder collection with header and data</returns>
        private StringBuilder WriteObjectsLayer(Layer layer)
        {
            int lengthData = 0;
            StringBuilder data = new(1024);
            StringBuilder headerType = new(200);
            StringBuilder header = new(200);
            List<StringBuilder> output = new() { header, data };


            int blockType = layer.Properties.GetPropertyInt("Type");        // this type will be used by the engine to map the parser

            layer.Properties.Merge(_properties);
            StringBuilder validator = Validator.ProcessLayerValidator(layer.Properties);

            if (validator.Length > 0)
            {
                int prevBlockType = blockType;
                headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).AppendLine($"\t\t; data block type {prevBlockType} with validator.");
                headerType.Append(validator);
            }
            else
            {
                headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).Append("\t\t; data block type\r\n");
            }



            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible && c.Type.Equals("path", StringComparison.InvariantCultureIgnoreCase)).ToString("X")).Append("\t\t; Objects count\r\n");
            lengthData += 1;


            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible && obj.Type.Equals("path", StringComparison.InvariantCultureIgnoreCase))
                {
                    int blockLength = 0;
                    Polygon polygon = obj.Polygon;
                    int id = polygon.Properties.GetPropertyInt("ID");
                    string speed = polygon.Properties.GetProperty("speed");

                    List<int> speedIntervals = GetSpeed(speed, polygon.Count);
                    if (speedIntervals.Count != polygon.Count)
                    {
                        Console.WriteLine("Polygn {0} Sp property have number of elements different from nodes.", obj.Id);
                        break;
                    }
                    int i = 0;
                    data.Append("\t\tdb $").Append(id.Int2Hex("X2"));
                    data.Append("\t\t; ID\r\n");
                    lengthData++;                   // only counts to data type length not block
                    List<(int speed, int X, int Y)> steps = new();

                    for (; i < polygon.Count; i++)
                    {
                        int x = polygon[i ].X + (int)obj.X; // next point x
                        int y = polygon[i ].Y + (int)obj.Y; // next point y
                        int speedStep = speedIntervals[i];   // time to run the distance bettween 2 points x
                        steps.Add((speedStep, x, y));
                    }
                    StringBuilder blockData = new(1024);
                    blockData.Append("\t\tdb $").Append(steps.Count.Int2Hex("X2"));
                    blockData.Append("\t\t; Number of steps\r\n");
                    blockLength++;
                    blockData.Append("\t\tdb $").Append( SetFlags(steps).Int2Hex("X2"));
                    blockData.Append("\t\t; flags  bit0 - 1=Loop, 0=Once\r\n");
                    blockLength++;

                    foreach (var step in steps)
                    {
                        blockData.Append("\t\tdb $").Append(step.X.Int2Hex("X2")).Append(", $").Append(step.Y.Int2Hex("X2")).Append(", $").Append(step.speed.Int2Hex("X2"));
                        blockData.Append("\t\t; StepX, StepY, frames\r\n");
                        blockLength += 3;
                    }
                    data.Append("\t\tdb $").Append(blockLength.Int2Hex("X2"));
                    data.Append("\t\t; Block size\r\n");
                    blockLength++;
                    data.Append(blockData);
                    data.Append("\r\n");
                    lengthData += blockLength;
                }
            }
            // size must be 2B long (map is over 256 Bytes)
            headerType.Append("\t\tdw $").Append(lengthData.ToString("X4")).Append("\t\t; Block size\r\n");
            // insert header at begin
            header.Insert(0, headerType);
            header.Append(data);
            return header;
        }

        private static int SetFlags(List<(int speed, int X, int Y)> steps)
        {
            bool simple = true;
            int flags = 0;
            int x0 = steps[0].X;
            int y0 = steps[0].Y;
            for (int step = 1; step < steps.Count; step++)
            {
                int x1 = steps[step].X;
                int y1 = steps[step].Y;
                int lengthX = Math.Abs(x1 - x0);
                int lengthY = Math.Abs(y1 - y0);
                if (!(lengthX == 0 || lengthY == 0 || lengthX == lengthY))
                {
                    simple = false;
                }
            }
            if(simple)
            {
                flags |= 2;
            }
            return flags;
        }

        private static List<int> GetSpeed(string speedList, int count)
        {
            List<int> speed = new(count);
            string[] intervals = speedList.Split(',');
            for (int index = 0; index < count; index++)
            {
                int stepSpeed;
                if (index < intervals.Length)
                {
                    stepSpeed = int.Parse(intervals[index]);
                }
                else
                {
                    stepSpeed = int.Parse(intervals[^1]);
                }
                speed.Add(stepSpeed);
            }
            return speed;
        }
    }
}