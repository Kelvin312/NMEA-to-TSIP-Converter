using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Test
{
    public class DistributionForm : ChartForm
    {
        private Series _ptrSecondSeries = null;
        

        public DistributionForm(FileForm ff, int columnCnt) : base("?")
        {
            
            XMinZoom = 1e-6;
            YMinZoom = 1e-6;

            _ptrChartArea.CursorY.IntervalType = DateTimeIntervalType.Number;
            _ptrChartArea.CursorY.Interval = YMinZoom;


            cbEnable.Visible = false;
            IsEnable = false;
            dtpDate.Visible = false;
            dtpTime.Visible = false;
            btnCreate.Visible = false;
            btnReopen.Text = "Удалить";
            chartControl.Series.Add(new Series("second"));
            _ptrSecondSeries = chartControl.Series[1];
            _ptrSeries.ChartType = SeriesChartType.Column;
            _ptrSecondSeries.ChartType = SeriesChartType.Spline;
            _ptrSecondSeries.Color = Color.MediumVioletRed;
            Create(ff, columnCnt);
        }

        private void Create(FileForm ff, int columnCnt)
        {
            var n = ff.GetCount();
            var buffer = new SortedDictionary<int, int>();
            double u = 0, q2 = 0;
            int valMin = int.MaxValue;
            int valMax = int.MinValue;
            for (int i = 0; i < n; i++)
            {
                int val = ff.GetValue(i);
                u += val;
                if (val < valMin) valMin = val;
                if (val > valMax) valMax = val;
            }
            u /= n;

            var columnSize = (valMax - valMin + columnCnt) / columnCnt;

            for (int i = 0; i < n; i++)
            {
                int val = ff.GetValue(i);
                q2 += Math.Pow(val - u, 2);

                var columnIndex = (val - valMin) / columnSize;
                if (buffer.ContainsKey(columnIndex))
                {
                    buffer[columnIndex]++;
                }
                else
                {
                    buffer.Add(columnIndex, 1);
                }
            }
            q2 *= 2;
            q2 /= n;
            foreach (var p in buffer)
            {
                var columnIndex = p.Key;
                var val = columnIndex * columnSize + valMin + columnSize / 2;
                var fres = p.Value / (double) n;
                var sres = Math.Exp(-Math.Pow(val - u, 2) / q2) / Math.Sqrt(q2 * Math.PI);
                _ptrSeries.Points.AddXY(val, fres);
                _ptrSecondSeries.Points.AddXY(val, sres);
            }
        }
    }
}
