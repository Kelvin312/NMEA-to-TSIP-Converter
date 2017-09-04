using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Test.Properties;

namespace Test
{
    public class FileForm:ChartForm
    {
        public FileForm(string filePath):base(Path.GetFileNameWithoutExtension(filePath))
        {
            _filePath = filePath;
            _ptrSeries.XValueType = ChartValueType.DateTime;
            _ptrChartArea.CursorX.IntervalType = DateTimeIntervalType.Seconds;
            _ptrSeries.YValueType = ChartValueType.Int32;
            _ptrAxisY.LabelStyle.Format = "#";
            ReadFile(_filePath);
        }

        public void UpdateControls()
        {
            _ptrAxisX.LabelStyle.Format = Settings.Default.ViewTimeChart;
            if(!_isValid) return;
            dtpDate.Value = _pointsList[0].Time;
            dtpTime.Value = _pointsList[0].Time;
        }

        protected override void OnDtpValueChanged(DateTime newTime)
        {
            RefreshData(newTime);
            OtherEvent?.Invoke(this, false);
        }

        protected override void OnScaleViewChanged(ChartRect newZoom)
        {
            if (newZoom != null)
            {
                UpdateAxis(newZoom, true);
            }
            else
            {
                UpdateAxis();
            }
        }

        protected override void OnResetZoom()
        {
            OtherEvent?.Invoke(this, true);
        }

        private readonly string _filePath;
        private List<FilePoint> _pointsList = new List<FilePoint>();
        private SortedSet<KeyValueHolder<double, int>> _xList = new SortedSet<KeyValueHolder<double, int>>();

        public SortedSet<KeyValueHolder<double, int>> GetXList() => _xList;

        public int GetValue(int index)
        {
            return Config.IsMagneticField ? _pointsList[index].MagneticField : _pointsList[index].RmsDeviation;
        }
        public int GetCount() => _pointsList.Count;

        private bool _isValid = false;

        public ChartRect Border { get; private set; }

      
        public event EventHandler<ChartRect> ScaleViewChanged;

        public event EventHandler<bool> OtherEvent;

        public event EventHandler<EventArgs> CreateChart;


      

        

        public void RemovePoints(SortedSet<KeyValueHolder<double, int>> rlist)
        {
            for (int i = _pointsList.Count - 1; i > -1; i--)
            {
                var test = new KeyValueHolder<double, int>(_pointsList[i].GetRoundTime());
                if (rlist.Contains(test)) _pointsList.RemoveAt(i);
            }
        }


        private void ReadFile(string filePath)
        {
            _pointsList.Clear();
            var pointIndex = -3;
            try
            {
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
            }
            catch (Exception ex)
            {
                _isValid = false;
                MessageBox.Show(ChartName + "\r\n" + ex.Message,
                    "Ошибка открытия файла",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                throw;
            }
            RefreshData();
        }

        public void RefreshData(DateTime? newTime = null)
        {
            _isValid = _pointsList.Count > 1;
            _ptrSeries.Points.Clear();
            _xList.Clear();
            Border = new ChartRect();
            TimeSpan deltaTime = new TimeSpan();
            if (!_isValid)
            {
                IsEnable = false;
                base.UpdateBaseControls();
                return;
            }
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
                _xList.Add(new KeyValueHolder<double, int>(point.GetRoundTime(), i));
                Border.Union(pix.XValue, pix.YValues.First());
            }


        }

        public void UpdateAxis(ChartRect newView = null, bool isUpdateY = false, bool isResetZoom = false, bool isUpdateBorder = false)
        {
            var curView = new ChartRect(_ptrChartArea);
            var globalBorder = new ChartRect(_ptrChartArea, true);

            if (isUpdateBorder)
            {
                globalBorder = new ChartRect(Config.GlobalBorder);
                globalBorder.Y.Min -= 2;
                globalBorder.Y.Max += Config.YMinZoom * 10.0;
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
                if (newView != null)
                {
                    //X
                    curView.X = newView.X;
                    //Y
                    if (isUpdateY)
                    {
                        curView.Y = newView.Y;
                    }
                    else if (Settings.Default.IsYSyncZoom)
                    {
                        curView.Y.Size = newView.Y.Size;
                    }
                }
                //Y
                if (Settings.Default.IsYAutoScroll)
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
            var oldView = new ChartRect(_ptrChartArea);
            if (curView.X.Check(oldView.X, Config.XMinZoom, globalBorder.X))
            {
                _ptrAxisX.ScaleView.Zoom(curView.X.Min, curView.X.Max);
            }
            if (curView.Y.Check(oldView.Y, Config.YMinZoom, globalBorder.Y))
            {
                _ptrAxisY.ScaleView.Zoom(curView.Y.Min, curView.Y.Max);
            }
        }
    }
}
