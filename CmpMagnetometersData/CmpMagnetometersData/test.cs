using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace CmpMagnetometersData
{
    public partial class ChartForm
    {
        public ChartRect Border { get; private set; }
        public bool IsMinimize { get; private set; }
        public bool IsValid { get; private set; }

        public readonly string FileName;

        private List<FilePoint> _pointsList = new List<FilePoint>();
        private SortedSet<KeyValueHolder<double, int>> _xList = new SortedSet<KeyValueHolder<double, int>>();

        public event EventHandler<ChartRect> ScaleViewChanged;

        public event EventHandler<bool> OtherEvent; 

        private void ReadFile(string filePath)
        {
            _pointsList.Clear();
            var pointIndex = -3;
            using (var sr = new StreamReader(filePath))
            {
                foreach (var str in sr.ReadToEnd()
                    .Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (++pointIndex < 0) continue;
                    try
                    {
                        var p = new FilePoint(str);
                        _pointsList.Add(p);
                    }
                    catch (Exception)
                    {
                        // throw;
                    }
                }
            }
            RefreshData();
        }

        private void RefreshData(DateTime? newTime = null)
        {
            IsValid = _pointsList.Count > 1;
            if(!IsValid) return;
            _ptrSeries.Points.Clear();
            _xList.Clear();
            Border = new ChartRect();
            TimeSpan deltaTime = new TimeSpan();
            
            if (newTime != null)
            {
                deltaTime = newTime.Value.Subtract(_pointsList[0].Time);
            }
            for (int i = 0; i < _pointsList.Count; i++)
            {
                var point = _pointsList[i];
                if (newTime != null)
                {
                    point.Time = point.Time.Add(deltaTime);
                }
                var pix = point.GetPixel();
                _ptrSeries.Points.Add(pix);
                _xList.Add(new KeyValueHolder<double, int>(pix.XValue, i));
                Border.Union(pix.XValue, pix.YValues.First());
            }

            _ptrAxisX.LabelStyle.Format = Config.ViewTimeFormat;
            dtpStartX.CustomFormat = Config.ViewTimeFormat;
            dtpStartX.Value = _pointsList[0].Time;
        }

        public void UpdateAxis(ChartRect newView = null, bool isUpdateY = false, bool isResetZoom = false, bool isUpdateBorder = false)
        {
            var curView = new ChartRect(_ptrChartArea);
            var globalBorder = new ChartRect(_ptrChartArea, true);
            if (newView == null) newView = curView;

            if (isUpdateBorder)
            {
                globalBorder = new ChartRect(Config.GlobalBorder);
                globalBorder.Y.Min -= 10;
                globalBorder.Y.Max += 10;
                _ptrAxisX.Minimum = globalBorder.X.Min;
                _ptrAxisX.Maximum = globalBorder.X.Max;
                _ptrAxisY.Minimum = globalBorder.Y.Min;
                _ptrAxisY.Maximum = globalBorder.Y.Max;
            }
            if (isResetZoom)
            {
                curView.X = globalBorder.X;
                curView.Y = Border.Y;
            }
            else
            {
                //X
                curView.X = newView.X;
                //Y
                if (isUpdateY)
                {
                    curView.Y = newView.Y;
                }
                else if (Config.IsYSyncZoom && !curView.Equals(newView))
                {
                    curView.Y.Size = newView.Y.Size;
                }
                if (Config.IsYAutoScroll)
                {
                    var bet = _xList.GetViewBetween(
                           new KeyValueHolder<double, int>(curView.X.Min),
                           new KeyValueHolder<double, int>(curView.X.Max));
                    if (bet.Count > 0)
                    {
                        var yf = _ptrSeries.Points[bet.First().Value].YValues.First();
                        var yl = _ptrSeries.Points[bet.Last().Value].YValues.First();
                        var isYMove = curView.Y.Min > Math.Max(yf, yl) 
                            || curView.Y.Max < Math.Min(yf, yl);
                        if (isYMove) curView.Y.Middle = (yf + yl) / 2;
                    }
                }
            }
            curView.X.Check(Config.XMinZoom, globalBorder.X);
            curView.Y.Check(Config.YMinZoom, globalBorder.Y);
            _ptrAxisX.ScaleView.Zoom(curView.X.Min, curView.X.Max);
            _ptrAxisY.ScaleView.Zoom(curView.Y.Min, curView.Y.Max);
        }
    }
}
