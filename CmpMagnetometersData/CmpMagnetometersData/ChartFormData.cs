using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CmpMagnetometersData
{
    public class ChartFormData : ChartForm
    {
        private List<FileDataPoint> _filePoints = new List<FileDataPoint>();
        private SortedSet<KeyValueHolder<double,int>> _sortedXlist = new SortedSet<KeyValueHolder<double, int>>();
        private readonly Chart _chartControl;
        private readonly Series _ptrSeries;
        private readonly ChartArea _ptrChartArea;
        private readonly Axis _ptrAxisX;
        private readonly Axis _ptrAxisY;
        private bool _isEvent = false;

        public bool IsReady = false;
        public ChartFormData(string fileName)
        {
            lblFileName.Text = Path.GetFileNameWithoutExtension(fileName);
            _chartControl = chartControl;
            _ptrSeries = _chartControl.Series[0];
            _ptrChartArea = _chartControl.ChartAreas[0];
            _ptrAxisX = _ptrChartArea.AxisX;
            _ptrAxisY = _ptrChartArea.AxisY;
            ChartControlInit();
            ReadFile(fileName);
            if (_filePoints.Count > 1) IsReady = true;
        }

        //Зум выделением - синхронно, автоподгонка по вертикали
        //Зум роликом - синхронно, автоподгонка по вертикали
        //Перемещение - синхронно по горизонтали

        public void ScaleViewResize(ChartRect resize, bool isScrollY = false)
        {
            ChartRect current = new ChartRect(_ptrChartArea);

            bool isXChange = current.IsXChange(resize);
            bool isYZoomChange = current.IsYZoomChange(resize);
            if (!isXChange && !isYZoomChange) return;

            if (isScrollY)
            {
                if (resize.MaxXTime > _ptrAxisX.Maximum) _ptrAxisX.Maximum = resize.MaxXTime;
                if (resize.MinXTime < _ptrAxisX.Minimum) _ptrAxisX.Minimum = resize.MinXTime;
            }

            current.MinXTime = resize.MinXTime;
            current.MaxXTime = resize.MaxXTime;

            if (isXChange) _ptrAxisX.ScaleView.Zoom(current.MinXTime, current.MaxXTime);

            if (isYZoomChange) current.YResize(resize.GetYSize());
            if (isScrollY)
            {
                current.MinYVal = resize.MinYVal;
                current.MaxYVal = resize.MaxYVal;
            }

            var isYMove = YScrollCorrect(current);
            if (isYZoomChange || isYMove) _ptrAxisY.ScaleView.Zoom(current.MinYVal, current.MaxYVal);
        }

        public bool YScrollCorrect(ChartRect rect)
        {
            var bet = _sortedXlist.GetViewBetween(
                new KeyValueHolder<double, int>(rect.MinXTime),
                new KeyValueHolder<double, int>(rect.MaxXTime));
            if (bet.Count == 0) return false;
            var yf = _ptrSeries.Points[bet.FirstOrDefault().Value].YValues.FirstOrDefault();
            var yl = _ptrSeries.Points[bet.LastOrDefault().Value].YValues.FirstOrDefault();
            bool isYMove = rect.MinYVal > Math.Max(yf, yl) || rect.MaxYVal < Math.Min(yf, yl);
            if (isYMove) rect.YMove((yf + yl) / 2);
            return isYMove;
        }

        public ChartRect GetRect()
        {
            var cr = new ChartRect(double.PositiveInfinity, double.NegativeInfinity, 
                double.PositiveInfinity, double.NegativeInfinity);
            foreach (var p in _filePoints)
            {
                var pix = p.GetPixel();
                if (cr.MaxXTime < pix.XValue) cr.MaxXTime = pix.XValue;
                if (cr.MinXTime > pix.XValue) cr.MinXTime = pix.XValue;
                if (cr.MaxYVal < pix.YValues.FirstOrDefault()) cr.MaxYVal = pix.YValues.FirstOrDefault();
                if (cr.MinYVal > pix.YValues.FirstOrDefault()) cr.MinYVal = pix.YValues.FirstOrDefault();
            }

            return cr;
        }

        private void ChartControlInit()
        {
            _chartControl.Legends.Clear();
            _ptrSeries.ChartType = SeriesChartType.Spline;

            //Настраиваем формат данных и вид меток
            _ptrSeries.XValueType = ChartValueType.DateTime;
            _ptrAxisX.LabelStyle.Format = "yy.MM.dd-HH:mm:ss";
            _ptrAxisX.LabelStyle.IsEndLabelVisible = false;
            _ptrAxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            //Y
            _ptrSeries.YValueType = ChartValueType.Int32;
            _ptrAxisY.LabelStyle.Format = "#";
            _ptrAxisY.LabelStyle.IsEndLabelVisible = false;
            _ptrAxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            //Настраиваем масштабирование и скролл
            _ptrChartArea.CursorX.IsUserSelectionEnabled = true;
            _ptrChartArea.CursorX.IntervalType = DateTimeIntervalType.Seconds;
            _ptrAxisX.ScrollBar.Enabled = false;
            //Y
            _ptrChartArea.CursorY.IsUserSelectionEnabled = true;
            _ptrAxisY.ScrollBar.Enabled = false;
            //Mouse
            _chartControl.MouseEnter += ChartControl_MouseEnter;
            _chartControl.MouseLeave += ChartControl_MouseLeave;
            _chartControl.MouseWheel += ChartControl_MouseWheel;
            _chartControl.MouseDown += ChartControl_MouseDown;
            _chartControl.MouseMove += ChartControl_MouseMove;
            _chartControl.AxisViewChanged += ChartControl_AxisViewChanged;
            _chartControl.SelectionRangeChanging += ChartControl_SelectionRangeChanging;
        }

        
        public event EventHandler<ViewEventArgs> ScaleViewChanged;

        private void ChartControl_AxisViewChanged(object sender, ViewEventArgs e)
        {
            if (!_isEvent) return;
            ScaleViewChanged?.Invoke(this, e);
            _isEvent = false;
        }

        public delegate void ResetZoomEventHandler();

        public event ResetZoomEventHandler ResetZoom;

        private void ChartControl_SelectionRangeChanging(object sender, CursorEventArgs e)
        {
            _isEvent = true;
        }

        #region MouseEvents

        private bool _mouseDowned;
        private double _xStart, _yStart;

        private void ChartControl_MouseEnter(object sender, EventArgs e)
        {
            _chartControl.Focus();
        }
        private void ChartControl_MouseLeave(object sender, EventArgs e)
        {
            _mouseDowned = false;
            _chartControl.Parent.Focus();
        }
        private void ChartControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.None)
            {
                double xMin, xMax, yMin, yMax;
                ScaleViewZoom(_ptrAxisX, e.Delta, out xMin, out xMax);
                ScaleViewZoom(_ptrAxisY, e.Delta, out yMin, out yMax);
                ChartRect newZoom = new ChartRect(xMin,xMax,yMin,yMax);
                _isEvent = true;
                ScaleViewResize(newZoom);
            }
        }
        private void ScaleViewZoom(Axis ptrAxis, int delta, out double newStart, out double newEnd)
        {
            var ptrView = ptrAxis.ScaleView;
            var deltaPos = (ptrView.ViewMaximum - ptrView.ViewMinimum) * Config.ZoomSpeed;
            if (delta < 0) deltaPos = -deltaPos;
            newStart = ptrView.ViewMinimum + deltaPos;
            newEnd = ptrView.ViewMaximum - deltaPos;
        }
        private void ChartControl_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Middle:
                    _mouseDowned = true;
                    _xStart = _yStart = 0;
                    try
                    {
                        _xStart = _ptrAxisX.PixelPositionToValue(e.Location.X);
                        _yStart = _ptrAxisY.PixelPositionToValue(e.Location.Y);
                    }
                    catch (Exception)
                    {

                    }
                    break;
                case MouseButtons.Right:
                    ResetZoom?.Invoke();
                    break;
            }
        }

        private void ChartControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(_mouseDowned && e.Button == MouseButtons.Middle))
            {
                _mouseDowned = false;
                return;
            }

            double selX, selY;
            try
            {
                selX = _ptrAxisX.PixelPositionToValue(e.Location.X);
                selY = _ptrAxisY.PixelPositionToValue(e.Location.Y);
            }
            catch (Exception)
            {
                return;
            }
            if (_ptrAxisX.ScaleView.IsZoomed || _ptrAxisY.ScaleView.IsZoomed)
            {
                double dx = -selX + _xStart;
                double dy = -selY + _yStart;
                double newX = _ptrAxisX.ScaleView.Position + dx;
                double newY = _ptrAxisY.ScaleView.Position + dy;

                _isEvent = true;
                _ptrAxisX.ScaleView.Scroll(newX);
                _ptrAxisY.ScaleView.Scroll(newY);
            }
        }
        #endregion

        private void ReadFile(string fileName)
        {
            _filePoints.Clear();
            _ptrSeries.Points.Clear();
            _sortedXlist.Clear();
            int pointIndex = -3;
            using (StreamReader sr = new StreamReader(fileName))
            {
                foreach (var str in sr.ReadToEnd()
                    .Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (++pointIndex < 0) continue;
                    try
                    {
                        var fdp = new FileDataPoint(str);
                        var p = fdp.GetPixel();
                        _sortedXlist.Add(new KeyValueHolder<double, int>(p.XValue, _filePoints.Count));
                        _filePoints.Add(fdp);
                        _ptrSeries.Points.Add(p);
                    }
                    catch (Exception)
                    {
                        // throw;
                    }
                }
            }
        }

       
    }
}
