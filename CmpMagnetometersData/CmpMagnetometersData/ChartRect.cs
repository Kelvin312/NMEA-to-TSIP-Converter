using System;
using System.Windows.Forms.DataVisualization.Charting;

namespace CmpMagnetometersData
{
    public class ChartRect
    {
        public double MinXTime;
        public double MaxXTime;
        public double MinYVal;
        public double MaxYVal;

        public ChartRect(ChartRect rect)
        {
            MinXTime = rect.MinXTime;
            MaxXTime = rect.MaxXTime;
            MinYVal = rect.MinYVal;
            MaxYVal = rect.MaxYVal;
        }

        public ChartRect(double minXTime, double maxXTime, double minYVal, double maxYVal)
        {
            MinXTime = minXTime;
            MaxXTime = Math.Max(maxXTime, minXTime);
            MinYVal = minYVal;
            MaxYVal = Math.Max(maxYVal, minYVal);
        }
        public ChartRect(ChartArea ca, bool isView = true)
        {
            if (isView)
            {
                MinXTime = ca.AxisX.ScaleView.ViewMinimum;
                MaxXTime = ca.AxisX.ScaleView.ViewMaximum;
                MinYVal = ca.AxisY.ScaleView.ViewMinimum;
                MaxYVal = ca.AxisY.ScaleView.ViewMaximum;
            }
            else
            {
                MinXTime = ca.AxisX.Minimum;
                MaxXTime = ca.AxisX.Maximum;
                MinYVal = ca.AxisY.Minimum;
                MaxYVal = ca.AxisY.Maximum;
            }
        }

        public double GetYSize()
        {
            return MaxYVal - MinYVal;
        }

        public double GetYMid()
        {
            return (MaxYVal + MinYVal)/2;
        }

        public bool IsXChange(ChartRect newRect)
        {
            return Math.Abs(MaxXTime - newRect.MaxXTime) 
                + Math.Abs(MinXTime - newRect.MinXTime) > 0.1 / 24 / 60 / 60;
        }

        public bool IsYZoomChange(ChartRect newRect)
        {
            return Math.Abs(GetYSize() - newRect.GetYSize()) > 0.1;
        }

        public void YResize(double newSize)
        {
            var yMid = GetYMid();
            var yDelta = newSize / 2;
            MaxYVal = yMid + yDelta;
            MinYVal = yMid - yDelta;
        }

        public void YMove(double newMid)
        {
            var yMid = newMid;
            var yDelta = GetYSize() / 2;
            MaxYVal = yMid + yDelta;
            MinYVal = yMid - yDelta;
        }

        public void Union(ChartRect rect)
        {
            MinXTime = Math.Min(MinXTime, rect.MinXTime);
            MaxXTime = Math.Max(MaxXTime, rect.MaxXTime);
            MinYVal = Math.Min(MinYVal, rect.MinYVal);
            MaxYVal = Math.Max(MaxYVal, rect.MaxYVal);
        }
        public void CutOff(ChartRect rect)
        {
            MinXTime = MinXTime < rect.MinXTime ? rect.MinXTime : MinXTime;
            MaxXTime = MaxXTime > rect.MaxXTime ? rect.MaxXTime : MaxXTime;
            MinYVal = MinYVal < rect.MinYVal ? rect.MinYVal : MinYVal;
            MaxYVal = MaxYVal > rect.MaxYVal ? rect.MaxYVal : MaxYVal;
        }
    }
}