using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CmpMagnetometersData
{
    public class DistributionLineControl:DataLineControl
    {

        public DistributionLineControl() : base()
        {
            rbFirstSelect.Enabled = false;
            cbSecondSelect.Enabled = false;
            _chartControl.ch.Series.Add(new Series());
            _ptrSeries2 = _chartControl.ch.Series[1];
            _ptrSeries2.ChartType = SeriesChartType.Column;
            _chartControl._ptrSeries.Color = Color.DarkGreen;
            _ptrSeries2.Color = Color.DarkViolet;

            _chartControl._ptrSeries.XValueType = ChartValueType.Double;
            _chartControl._ptrChartArea.CursorX.IntervalType = DateTimeIntervalType.Auto;
            _ptrSeries2.XValueType = ChartValueType.Double;
            _chartControl._ptrAxisX.LabelStyle.Format = "#";
            _chartControl._ptrAxisY.LabelStyle.Format = "F4";
            _chartControl._ptrChartArea.CursorY.Interval = 1e-6;

        }

        private Series _ptrSeries2 = null;

        public override void ReadEvent(object sender, SendEventArgs e)
        {
            
        }

        private List<DistPixel> _distList = new List<DistPixel>();

        public void CreateDistribution(List<DataPixel> dpList, int steps)
        {
            int minVal = int.MaxValue, maxVal = int.MinValue;
            int countValues = 0;
            double meanValues = 0;
            double dispersionValues = 0;
            foreach (var dp in dpList)
            {
                minVal = Math.Min(minVal, dp.Val);
                maxVal = Math.Max(maxVal, dp.Val);
                ++countValues;
                meanValues += (dp.Val - meanValues) / countValues;
            }
            countValues = 0;
            foreach (var dp in dpList)
            {
                ++countValues;
                dispersionValues += (Math.Pow(dp.Val - meanValues, 2.0) - dispersionValues) / countValues;
            }
            dispersionValues *= 2;
            var stepSize = (maxVal - minVal) / steps + 1;
            _distList.Clear();
            for (int i = 0; i < steps; i++)
            {
                var v = minVal + stepSize / 2.0 + stepSize * i;
                var normal = Math.Exp(-Math.Pow(v - meanValues, 2.0) / dispersionValues) /
                                              Math.Sqrt(Math.PI * dispersionValues);
                _distList.Add(new DistPixel() {Count = 0, Normal = normal, ValX = v});
            }
            foreach (var dp in dpList)
            {
                var i = (dp.Val - minVal) / stepSize;
                _distList[i].Count++;
            }
            foreach (var dl in _distList)
            {
                dl.Count /= dpList.Count;
            }
            
        }

        public override void UpdateChart(TableLayoutPanel tlb, ref int hSize)
        {
            var contains = tlb.Controls.Contains(_chartControl);
            if (contains && !cbVisible.Checked)
            {
                tlb.Controls.Remove(_chartControl);
                tlb.RowStyles.RemoveAt(tlb.RowStyles.Count - 1);
            }
            if (!contains && cbVisible.Checked)
            {
                _chartControl._ptrSeries.Points.Clear();
                _ptrSeries2.Points.Clear();

                foreach (var dist in _distList)
                {
                    _chartControl._ptrSeries.Points.Add(new DataPoint(dist.ValX,dist.Normal));
                    _ptrSeries2.Points.Add(new DataPoint(dist.ValX, dist.Count));
                }

                _chartControl.Dock = DockStyle.Fill;
                tlb.Controls.Add(_chartControl);
                tlb.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            }
            if (cbVisible.Checked) hSize += 285;
        }
    }

    public class DistPixel
    {
        public double ValX;
        public double Count;
        public double Normal;
    }
}
