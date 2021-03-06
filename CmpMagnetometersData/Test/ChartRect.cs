﻿using System;
using System.Windows.Forms.DataVisualization.Charting;

namespace Test
{
    public struct AxisSize
    {
        public double Min;
        public double Max;

        public double Size
        {
            get { return Max - Min; }
            set
            {
                var mid = Middle;
                Min = mid - value / 2.0;
                Max = mid + value / 2.0;
            }
        }

        public double Middle
        {
            get { return (Max + Min) / 2.0; }
            set
            {
                var delta = Size / 2.0;
                Min = value - delta;
                Max = value + delta;
            }
        }

        public void Shift(double step)
        {
            Min += step;
            Max += step;
        }

        public bool Check(AxisSize old, double minSize, AxisSize border)
        {
            if (Size + minSize * 0.1 > border.Size || minSize * 3 > border.Size)
            {
                this = border;
                return true;
            }
            if (Size < minSize) Size = minSize * 3;
            if (Min < border.Min)
            {
                Shift(border.Min - Min);
                return true;
            }
            if (Max > border.Max)
            {
                Shift(border.Max - Max);
                return true;
            }
            return Math.Abs(old.Min - Min) + Math.Abs(old.Max - Max) > minSize * 0.1;
        }

        public AxisSize(double min, double max)
        {
            Min = min;
            Max = max;
        }
        public AxisSize(Axis axis)
        {
            Min = axis.Minimum;
            Max = axis.Maximum;
        }
        public AxisSize(AxisScaleView view)
        {
            Min = view.ViewMinimum;
            Max = view.ViewMaximum;
        }
    }

    public class ChartRect
    {
        public AxisSize X;
        public AxisSize Y;

        public ChartRect()
        {
            X = Y = new AxisSize(double.PositiveInfinity, double.NegativeInfinity);
        }

        public ChartRect(ChartRect other)
        {
            X = other.X;
            Y = other.Y;
        }

        public ChartRect(double xMin, double xMax, double yMin, double yMax)
        {
            X = new AxisSize(xMin, xMax);
            Y = new AxisSize(yMin, yMax);
        }
        public ChartRect(ChartArea ca, bool isBorder = false)
        {
            if (isBorder)
            {
                X = new AxisSize(ca.AxisX);
                Y = new AxisSize(ca.AxisY);
            }
            else
            {
                X = new AxisSize(ca.AxisX.ScaleView);
                Y = new AxisSize(ca.AxisY.ScaleView);
            }
        }

        public void Union(ChartRect rect)
        {
            X.Min = Math.Min(X.Min, rect.X.Min);
            X.Max = Math.Max(X.Max, rect.X.Max);
            Y.Min = Math.Min(Y.Min, rect.Y.Min);
            Y.Max = Math.Max(Y.Max, rect.Y.Max);
        }

        public void Union(double x, double y)
        {
            X.Min = Math.Min(X.Min, x);
            X.Max = Math.Max(X.Max, x);
            Y.Min = Math.Min(Y.Min, y);
            Y.Max = Math.Max(Y.Max, y);
        }

        public void CutOff(ChartRect rect)
        {
            X.Min = Math.Max(X.Min, rect.X.Min);
            X.Max = Math.Min(X.Max, rect.X.Max);
            Y.Min = Math.Max(Y.Min, rect.Y.Min);
            Y.Max = Math.Min(Y.Max, rect.Y.Max);
        }
    }
}
