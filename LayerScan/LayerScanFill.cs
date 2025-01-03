using System.Collections.Generic;
using Tiled2ZXNext.Entities;

namespace Tiled2ZXNext
{
    public class LayerScanFill
    {
        private Layer _layer;
        int _layerHeight;
        int _layerWidth;
        
        public LayerScanFill(Layer layer)
        {
            _layer = layer;
            _layerWidth = _layer.Width;
        }

        public List<Rectangle> Scan()
        {
            List<Rectangle> fillRectangles= new();
            while (true)
            {
                Rectangle area = ScanSingle();
                if (area.Width == 0)
                    break;
                fillRectangles.Add(area);
            }
            return fillRectangles;
        }


        private  Rectangle ScanSingle()
        {
            int x1 = 0;
            int y1 = 0;
            bool exit = false;
            bool rootSet = false;
            _layerHeight = _layer.Height;

            int x;
            for (x = 0; x < _layerWidth; x++)
            {
                for (int y = y1; y < _layerHeight; y++)
                {
                    uint value = _layer.Data[x + y * _layerWidth];  // GetValue(x, y);
                    if (value == 0)
                    {
                        if (rootSet)
                        {
                            if (x == x1)
                            {
                                _layerHeight = y;
                                break;
                            }
                            else
                            {
                                exit = true;
                                break;
                            }
                        }
                    }
                    else if (value > 0)
                    {
                        if (!rootSet)
                        {
                            x1 = x;
                            y1 = y;
                            rootSet = true;
                        }
                    }

                }
                //    if (value > 0 && !rootSet)
                //    {
                //        x1 = x;
                //        y1 = y;
                //        height++;
                //        rootSet = true;
                //    }
                //    else if (rootSet)
                //    {
                //        if ((value == 0 && heightMax == -1) || (value == 0 && y == _layerHeight))
                //        {
                //            heightMax = y;
                //            _layerHeight = heightMax;
                //            exity = true;
                //        }
                //        else if (value == 0 && y <= heightMax)
                //        {
                //            exit = true;
                //        }
                //        else if (heightMax == -1)
                //        {
                //            height++;
                //        }
                //        if (exity || exit)
                //        {
                //            break;
                //        }
                //    }
                //}
                if (!exit && rootSet)
                {
                    for (int i = y1; i < _layerHeight; i++)
                    {
                        _layer.Data[x + i * _layerWidth] = 0;
                        //ResetValue(x, i);
                    }
                }
                else if (exit)
                {
                    break;
                }
            }
            if (rootSet)
            {
                return new Rectangle() { X = x1, Y = y1, Width = x - x1, Height = _layerHeight - y1 };
            }
            else
            {
                return new Rectangle() { X = x1, Y = y1, Width = 0, Height = 0 };
            }
        }
        //private uint GetValue(int x, int y)
        //{
        //    uint value = _layer.Data[x + y * _layerWidth];
        //    return value;
        //}

        //private void ResetValue(int x, int y)
        //{
        //    _layer.Data[x + y * _layerWidth] = 0;
        //}
    }
}
