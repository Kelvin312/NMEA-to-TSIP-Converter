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
    public class FileData
    {
        private List<FileDataPoint> _filePoints = new List<FileDataPoint>();
        private SortedSet<KeyValueHolder<double,int>> _sortedXlist = new SortedSet<KeyValueHolder<double, int>>();
        private readonly Chart _chartControl = new Chart();
        private readonly Series _ptrSeries;
        private readonly ChartArea _ptrChartArea;
        private readonly Axis _ptrAxisX;
        private readonly Axis _ptrAxisY;

        public FileData(StreamReader fileReader)
        {
            _ptrSeries = _chartControl.Series[0];
            _ptrChartArea = _chartControl.ChartAreas[0];
            _ptrAxisX = _ptrChartArea.AxisX;
            _ptrAxisY = _ptrChartArea.AxisY;
            ChartControlInit();
            ReadFile(fileReader);
        }

        //Зум выделением - синхронно, автоподгонка по вертикали
        //Зум роликом - синхронно, автоподгонка по вертикали
        //Перемещение - синхронно по горизонтали

        public void ScaleViewResize(ChartRect resize)
        {
            ChartRect current = new ChartRect(_ptrChartArea);

            bool isXChange = current.IsXChange(resize);
            bool isYZoomChange = current.IsYZoomChange(resize);
            if (!isXChange && !isYZoomChange) return;

            current.MinXTime = resize.MinXTime;
            current.MaxXTime = resize.MaxXTime;

            if (isXChange) _ptrAxisX.ScaleView.Zoom(current.MinXTime, current.MaxXTime);

            if (isYZoomChange) current.YResize(resize.GetYSize());

            var isYMove = YScrollCorrect(current);
            if (isYZoomChange || isYMove) _ptrAxisY.ScaleView.Zoom(current.MinYVal, current.MaxYVal);
        }

        public bool YScrollCorrect(ChartRect rect)
        {
            var bet = _sortedXlist.GetViewBetween(
                new KeyValueHolder<double, int>(rect.MinXTime),
                new KeyValueHolder<double, int>(rect.MaxXTime));
            var yf = _ptrSeries.Points[bet.FirstOrDefault().Value].YValues.FirstOrDefault();
            var yl = _ptrSeries.Points[bet.LastOrDefault().Value].YValues.FirstOrDefault();
            bool isYMove = rect.MinYVal > Math.Max(yf, yl) || rect.MaxYVal < Math.Min(yf, yl);
            if (isYMove) rect.YMove((yf + yl) / 2);
            return isYMove;
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
                //
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
                    //_ptrAxisX.ScaleView.Zoom(_ptrAxisX.Minimum, _ptrAxisX.Maximum);
                    //_ptrAxisY.ScaleView.Zoom(_ptrAxisY.Minimum, _ptrAxisY.Maximum);
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
            }
        }
        #endregion

        private void ReadFile(StreamReader fileReader)
        {
            _filePoints.Clear();
            _ptrSeries.Points.Clear();
            _sortedXlist.Clear();
            int pointIndex = -3;
            foreach (var str in fileReader.ReadToEnd()
                .Split(new [] { '\0' }, StringSplitOptions.RemoveEmptyEntries))
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
