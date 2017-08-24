using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CmpMagnetometersData
{
    public partial class ChartForm : UserControl
    {
        private List<FileDataPoint> _filePoints = new List<FileDataPoint>();
        private SortedSet<KeyValueHolder<double, int>> _sortedXlist = new SortedSet<KeyValueHolder<double, int>>();
        private readonly Chart _chartControl;
        private readonly Series _ptrSeries;
        private readonly ChartArea _ptrChartArea;
        private readonly Axis _ptrAxisX;
        private readonly Axis _ptrAxisY;
        private ChartRect _maxRect;

        public bool IsReady = false;

        public ChartForm(string fileName)
        {
            InitializeComponent();
            lblFileName.Text = Path.GetFileNameWithoutExtension(fileName);
            _chartControl = chartControl;
            _ptrSeries = _chartControl.Series[0];
            _ptrChartArea = _chartControl.ChartAreas[0];
            _ptrAxisX = _ptrChartArea.AxisX;
            _ptrAxisY = _ptrChartArea.AxisY;
            ChartControlInit();
            ReadFile(fileName);
            if (_filePoints.Count < 2) return;
            IsReady = true;
        }

        private void ChartForm_Load(object sender, EventArgs e)
        {
            dtpStartX.Value = _filePoints.First().Time;
            _isDtpValueChanged = false;
        }

        //Зум выделением - синхронно, автоподгонка по вертикали
        //Зум роликом - синхронно, автоподгонка по вертикали
        //Перемещение - синхронно по горизонтали

        public void ScaleViewResize(ChartRect resize, bool isSetY = false)
        {
            ChartRect current = new ChartRect(_ptrChartArea);

            bool isXChange = current.IsXChange(resize);
            bool isYZoomChange = current.IsYZoomChange(resize);
            if (!(isXChange || isYZoomChange || isSetY)) return;

            if (isSetY)
            {
                _ptrAxisX.Maximum = resize.MaxXTime;
                _ptrAxisX.Minimum = resize.MinXTime;
                _ptrAxisY.Maximum = resize.MaxYVal + 10;
            }
            if (isXChange)
            {
                _ptrAxisX.ScaleView.Zoom(resize.MinXTime, resize.MaxXTime);
            }
            if (isSetY)
            {
                _ptrAxisY.ScaleView.Zoom(resize.MinYVal, resize.MaxYVal);
                return;
            }
            if (isYZoomChange) current.YResize(resize.GetYSize());
            var isYMove = YScrollCorrect(current);
            if (isYZoomChange || isYMove) _ptrAxisY.ScaleView.Zoom(current.MinYVal, current.MaxYVal);
        }

        public bool YScrollCorrect(ChartRect rect)
        {
            var bet = _sortedXlist.GetViewBetween(
                new KeyValueHolder<double, int>(rect.MinXTime),
                new KeyValueHolder<double, int>(rect.MaxXTime));
            if (bet.Count == 0) return false;
            var yf = _ptrSeries.Points[bet.First().Value].YValues.FirstOrDefault();
            var yl = _ptrSeries.Points[bet.Last().Value].YValues.FirstOrDefault();
            bool isYMove = rect.MinYVal > Math.Max(yf, yl) || rect.MaxYVal < Math.Min(yf, yl);
            if (isYMove) rect.YMove((yf + yl) / 2);
            return isYMove;
        }

        public ChartRect GetRect()
        {
            return _maxRect;

        }

        private void ChartControlInit()
        {
            _chartControl.Legends.Clear();
            _ptrSeries.ChartType = SeriesChartType.Spline;

            //Настраиваем формат данных и вид меток
            _ptrSeries.XValueType = ChartValueType.DateTime;
            _ptrAxisX.LabelStyle.Format = Config.ViewTimeFormat;
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
        }


        public event EventHandler<ChartRect> ScaleViewChanged;

        private void ChartControl_AxisViewChanged(object sender, ViewEventArgs e)
        {
            ScaleViewChanged?.Invoke(this, new ChartRect(e.ChartArea));
        }

        public delegate void ResetZoomEventHandler();

        public event ResetZoomEventHandler ResetZoom;

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
                ChartRect newZoom = new ChartRect(_ptrChartArea);

                ScaleViewZoom(e.Delta, ref newZoom.MinXTime, ref newZoom.MaxXTime, Config.XMinZoom);
                ScaleViewZoom(e.Delta, ref newZoom.MinYVal, ref newZoom.MaxYVal, Config.YMinZoom);
                newZoom.CutOff(new ChartRect(_ptrChartArea, false));
                ScaleViewResize(newZoom);
                ScaleViewChanged?.Invoke(this, newZoom);
            }
        }

        private void ScaleViewZoom(int delta, ref double start, ref double end, double minDeltaPos)
        {
            var deltaPos = (end - start) * Config.ZoomSpeed;
            if (delta < 0) deltaPos = -deltaPos;
            else if (deltaPos <= minDeltaPos) return;
            start += deltaPos;
            end -= deltaPos;
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

                _ptrAxisX.ScaleView.Scroll(newX);
                _ptrAxisY.ScaleView.Scroll(newY);
                _chartControl.Refresh();
                ScaleViewChanged?.Invoke(this, new ChartRect(_ptrChartArea));
            }
        }

        #endregion

        private void ReadFile(string fileName)
        {
            _filePoints.Clear();
            int pointIndex = -3;
            using (StreamReader sr = new StreamReader(fileName))
            {
                foreach (var str in sr.ReadToEnd()
                    .Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (++pointIndex < 0) continue;
                    try
                    {
                        var p = new FileDataPoint(str);
                        _filePoints.Add(p);
                    }
                    catch (Exception)
                    {
                        // throw;
                    }
                }
            }
            RefreshData();
        }

        public void RefreshData(DateTime? newTime = null)
        {
            if (_filePoints.Count < 2) return;
            _ptrSeries.Points.Clear();
            _sortedXlist.Clear();
            _maxRect = new ChartRect(double.PositiveInfinity, double.NegativeInfinity,
                double.PositiveInfinity, double.NegativeInfinity);

            TimeSpan deltaTime = new TimeSpan();
            _ptrAxisX.LabelStyle.Format = Config.ViewTimeFormat;
            dtpStartX.CustomFormat = Config.ViewTimeFormat;
            if (newTime != null)
            {
                deltaTime = newTime.Value.Subtract(_filePoints.First().Time);
            }
            for (int i = 0; i < _filePoints.Count; i++)
            {
                var fPoint = _filePoints[i];
                if (newTime != null)
                {
                    fPoint.Time = fPoint.Time.Add(deltaTime);
                }
                var pix = fPoint.GetPixel();
                _ptrSeries.Points.Add(pix);
                _sortedXlist.Add(new KeyValueHolder<double, int>(pix.XValue, i));
                _maxRect.Union(new ChartRect(pix.XValue, pix.XValue,
                    pix.YValues.First(), pix.YValues.First()));
            }
        }

        private bool _isDtpValueChanged = false;
        private void dtpStartX_ValueChanged(object sender, EventArgs e)
        {
            _isDtpValueChanged = true;
        }

        private void dtpStartX_Leave(object sender, EventArgs e)
        {
            if (!_isDtpValueChanged) return;
            RefreshData(dtpStartX.Value);
            _isDtpValueChanged = false;

            var current = new ChartRect(_ptrChartArea);
            _ptrAxisX.Minimum = Math.Min(_ptrAxisX.Minimum, _maxRect.MinXTime);
            _ptrAxisX.Maximum = Math.Max(_ptrAxisX.Maximum, _maxRect.MaxXTime);
            current.MinXTime = _maxRect.MinXTime;
            current.MaxXTime = _maxRect.MaxXTime;
            ScaleViewResize(current);
        }
    }
}
