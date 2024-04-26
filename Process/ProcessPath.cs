using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;


namespace Tiled2ZXNext
{
    public class ProcessPaths : IProcess
    {
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        public ProcessPaths(Layer layer, Scene scene)
        {
            _rootLayer = layer;
            _scene = scene;
        }

        public StringBuilder Execute()
        {
            StringBuilder locationsCode = new();
            string fileName = _scene.Properties.GetProperty("FileName");
            foreach (Layer layer in _rootLayer.Layers)
            {
                if (layer.Visible && layer.Name.Equals("path", StringComparison.InvariantCultureIgnoreCase))
                {
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
        private static StringBuilder WriteObjectsLayer(Layer layer)
        {
            int lengthData = 0;
            StringBuilder data = new(1024);
            StringBuilder headerType = new(200);
            StringBuilder header = new(200);
            List<StringBuilder> output = new() { header, data };

            StringBuilder error = new();

            int blockType = layer.Properties.GetPropertyInt("Type");        // this type will be used by the engine to map the parser

            headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).Append("\t\t; data block type\r\n");

            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible && c.Type.Equals("path", StringComparison.InvariantCultureIgnoreCase)).ToString("X")).Append("\t\t; Objects count\r\n");
            lengthData += 1;


            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible && obj.Type.Equals("path", StringComparison.InvariantCultureIgnoreCase))
                {
                    int blockLength = 0;
                    Polygon polygon = obj.Polygon;
                    int id = polygon.Properties.GetPropertyInt("ID");

                    //string timer = polygon.Properties.GetProperty("time");      // time in hunder
                    string speed = polygon.Properties.GetProperty("speed");

                    //List<Double> timeIntervals = GetTimers(timer);

                    List<int> speedIntervals = GetSpeed(speed, polygon.Count);
                    if (speedIntervals.Count != polygon.Count)
                    {
                        Console.WriteLine("Polygn {0} Sp property have number of elements different from nodes.", obj.Id);
                        break;
                    }
                    int i = 0;
                    //int x0 = polygon[i].X + (int)obj.X; // root point x
                    //int y0 = polygon[i].Y + (int)obj.Y; // root point y
                    //int x1 = x0;                        // first point x = root x
                    //int y1 = y0;                        // first point y = root y
                    //int  speedStep = speedIntervals[i];     // time to run the distance bettween 2 points x
                    //int x2;
                    //int y2;
                    data.Append("\t\tdb $").Append(id.Int2Hex("X2"));
                    data.Append("\t\t; ID\r\n");
                    lengthData++;                   // only counts to data type length not block
                    List<(int speed, int X, int Y)> steps = new();

                    for (; i < polygon.Count; i++)
                    {
                        int x = polygon[i ].X + (int)obj.X; // next point x
                        int y = polygon[i ].Y + (int)obj.Y; // next point y
                        int speedStep = speedIntervals[i];   // time to run the distance bettween 2 points x

                        //var step = CalcStep(speedStep, x1, y1, x2, y2);
                        steps.Add((speedStep, x, y));
                    }
                    //speedStep = speedIntervals[i];
                    //x2 = x0;
                    //y2 = y0;
                    //var stepf = CalcStep(speedStep, x1, y1, x2, y2);
                    //steps.Add(stepf);

                    StringBuilder blockData = new(1024);
                    // set initial position
                    //x0 += (int)Controller.Config.Offset.x;
                    //y0 += (int)Controller.Config.Offset.y;

                    //blockData.Append("\t\tdw $").Append(x0.Int2Hex("X4")).Append(", $").Append(y0.Int2Hex("X4"));
                    //blockData.Append("\t\t; X, Y\r\n");
                    //blockLength += 4;
                    blockData.Append("\t\tdb $").Append(steps.Count.Int2Hex("X2"));
                    blockData.Append("\t\t; Number of steps\r\n");
                    blockLength++;
                    //blockData.Append("\t\tdb $").Append(255.Int2Hex("X2"));
                    //blockData.Append("\t\t; No key frame\r\n");
                    //blockLength++;
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


        //private static List<double> GetTimers(string timer)
        //{
        //    List<double> timers = new();

        //    string[] intervals = timer.Split(',');
        //    foreach (string timeInterval in intervals)
        //    {
        //        timers.Add(double.Parse(timeInterval));
        //    }
        //    return timers;
        //}





        //private static (int frames, int stepX, int stepY) CalcStep(double time, double x1, double y1, double x2, double y2)
        //{
        //    double distanceX = x2 - x1; //  distance  x bettween 2 points 
        //    double distanceY = y2 - y1; //  distance  y bettween 2 points 
        //    int stepx = (int)((distanceX / (time * 50)) * 50);  // distance divide by time multiple by 50 (frames) and multiple by 50 (frames)
        //    int stepy = (int)((distanceY / (time * 50)) * 50);  // distance divide by time multiple by 50 (frames) and multiple by 50 (frames)
        //    double x = (distanceX * 50) / stepx;
        //    x = Math.Round(x + .4999999f, MidpointRounding.AwayFromZero);
        //    double y = (distanceY * 50) / stepy;
        //    y = Math.Round(y + .4999999f, MidpointRounding.AwayFromZero);
        //    if (x.Equals(double.NaN))
        //    {
        //        x = y;
        //    }
        //    if (y.Equals(double.NaN))
        //    {
        //        y = x;
        //    }
        //    if (x != y)
        //    {
        //        Console.Error.WriteLine("Number of frames x={0},y={1} is different.", x, y);
        //    }
        //    // int frames = (int)time * 50;                        // the number of frames must be the same    
        //    int frames = (int)y;
        //    Console.WriteLine($"Frames={frames} DistanceX={distanceX} stepX={stepx} ${stepx.Int2Hex("X2")}, DistanceY={distanceY} StepY={stepy} ${stepy.Int2Hex("X2")}");
        //    return (frames, stepx, stepy);
        //}
    }
}