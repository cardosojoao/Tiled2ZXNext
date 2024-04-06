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
                if (layer.Visible)
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

            int blockType = layer.Properties.GetPropertyInt("Type");

            headerType.Append("\t\tdb $").Append(blockType.ToString("X2")).Append("\t\t; data block type\r\n");
            header.Append("\t\tdb $").Append(layer.Objects.Count(c => c.Visible && c.Type.Equals("path", StringComparison.InvariantCultureIgnoreCase)).ToString("X2")).Append("\t\t; Objects count\r\n");
            lengthData += 1;


            foreach (Entities.Object obj in layer.Objects)
            {
                if (obj.Visible && obj.Type.Equals("path", StringComparison.InvariantCultureIgnoreCase))
                {
                    int blockLength = 0;
                    Entities.Polygon polygon = obj.Polygon;
                    int id = polygon.Properties.GetPropertyInt("ID");
                    string timer = polygon.Properties.GetProperty("time");
                    List<Double> timeIntervals = GetTimers(timer);
                    int i = 0;
                    int x0 = polygon[i].X + (int)obj.X; // root point x
                    int y0 = polygon[i].Y + (int)obj.Y; // root point y
                    int x1 = x0;                        // first point x = root x
                    int y1 = y0;                        // first point y = root y
                    double time = timeIntervals[i];     // time to run the distance bettween 2 points x
                    int x2;
                    int y2;
                    data.Append("\t\tdb $").Append(id.Int2Hex("X2"));
                    data.Append("\t\t; ID\r\n");
                    lengthData++;                   // only counts to data type length not block
                    List<(int frames, int stepX, int stepY)> moves = new();
                    for (; i < polygon.Count - 1; i++)
                    {
                        x2 = polygon[i + 1].X + (int)obj.X; // next point x
                        y2 = polygon[i + 1].Y + (int)obj.Y; // next point y
                        time = timeIntervals[i];   // time to run the distance bettween 2 points x
                        var step = CalcStep(time, x1, y1, x2, y2);
                        moves.Add(step);
                        // data.Append("\t\tdb $").Append(step.frames.Int2Hex("X2")).Append(", $").Append(step.stepX.Int2Hex("X2")).Append(", $").Append(step.stepY.Int2Hex("X2"));
                        // data.Append("\t\t; frames, StepX, StepY\r\n");
                        // lengthData += 3;
                        x1 = x2;
                        y1 = y2;
                    }
                    time = timeIntervals[i];
                    x2 = x0;
                    y2 = y0;
                    var stepf = CalcStep(time, x1, y1, x2, y2);
                    moves.Add(stepf);

                    StringBuilder blockData = new(1024);
                    // set initial position
                    x0 += (int)Controller.Config.Offset.x;
                    y0 += (int)Controller.Config.Offset.y;

                    blockData.Append("\t\tdb $").Append(x0.Int2Hex("X4")).Append(", $").Append(y0.Int2Hex("X4"));
                    blockData.Append("\t\t; X, Y\r\n");
                    blockLength += 2;
                    blockData.Append("\t\tdb $").Append(moves.Count.Int2Hex("X2"));
                    blockData.Append("\t\t; Number of steps\r\n");
                    blockLength++;
                    blockData.Append("\t\tdb $").Append(255.Int2Hex("X2"));
                    blockData.Append("\t\t; No key frame\r\n");
                    blockLength++;
                    blockData.Append("\t\tdb $").Append(1.Int2Hex("X2"));
                    blockData.Append("\t\t; flags  bit0 - 1=Loop, 0=Once\r\n");
                    blockLength++;
                    foreach (var step in moves)
                    {
                        blockData.Append("\t\tdb $").Append(step.stepX.Int2Hex("X2")).Append(", $").Append(step.stepY.Int2Hex("X2")).Append(", $").Append(step.frames.Int2Hex("X2"));
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

        private static List<double> GetTimers(string timer)
        {
            List<double> timers = new();

            string[] intervals = timer.Split(',');
            foreach (string timeInterval in intervals)
            {
                timers.Add(double.Parse(timeInterval));
            }
            return timers;
        }

        private static (int frames, int stepX, int stepY) CalcStep(double time, double x1, double y1, double x2, double y2)
        {
            double distanceX = x2 - x1; //  distance  x bettween 2 points 
            double distanceY = y2 - y1; //  distance  y bettween 2 points 

            int stepx = (int)((distanceX / (time * 50)) * 50);  // distance divide by time multiple by 50 (frames) and multiple by 50 (frames)
            int stepy = (int)((distanceY / (time * 50)) * 50);  // distance divide by time multiple by 50 (frames) and multiple by 50 (frames)
            int frames = (int)time * 50;                        // the number of frames must be the same    
            Console.WriteLine($"Frames={frames} DistanceX={distanceX} stepX={stepx} ${stepx.Int2Hex("X2")}, DistanceY={distanceY} StepY={stepy} ${stepy.Int2Hex("X2")}");
            return (frames, stepx, stepy);
        }
    }
}