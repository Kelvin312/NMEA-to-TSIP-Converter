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

        public ChartRect Substract(ChartRect rect)
        {
            ChartRect res = new ChartRect(this);
            res.MinXTime -= rect.MinXTime;
            res.MaxXTime -= rect.MaxXTime;
            res.MinYVal -= rect.MinYVal;
            res.MaxYVal -= rect.MaxYVal;
            return res;
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