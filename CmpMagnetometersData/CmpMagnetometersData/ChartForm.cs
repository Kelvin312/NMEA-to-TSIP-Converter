using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CmpMagnetometersData
{
    public partial class ChartForm : UserControl
    {
        private readonly Chart _chartControl;
        private readonly Series _ptrSeries;
        private readonly ChartArea _ptrChartArea;
        private readonly Axis _ptrAxisX;
        private readonly Axis _ptrAxisY;
        private readonly string _filePath;

        public ChartForm(string filePath)
        {
            InitializeComponent();

            IsMinimize = false;
            IsEnable = true;
            _filePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(_filePath);
            lblFileName.Text = FileName;
            lblFileNameHid.Text = FileName;

            _chartControl = chartControl;
            _ptrSeries = _chartControl.Series[0];
            _ptrChartArea = _chartControl.ChartAreas[0];
            _ptrAxisX = _ptrChartArea.AxisX;
            _ptrAxisY = _ptrChartArea.AxisY;
            ChartControlInit();

            ReadFile(_filePath);
        }

        private void ChartControlInit()
        {
            //Настраиваем формат данных и вид меток
            _ptrSeries.XValueType = ChartValueType.DateTime;
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
           // _chartControl.MouseEnter += ChartControl_MouseEnter;
           // _chartControl.MouseLeave += ChartControl_MouseLeave;
           // _chartControl.MouseWheel += ChartControl_MouseWheel;
            _chartControl.MouseDown += ChartControl_MouseDown;
            _chartControl.MouseMove += ChartControl_MouseMove;
            _chartControl.AxisViewChanged += ChartControl_AxisViewChanged;
        }

        private void ChartForm_MouseEnter(object sender, EventArgs e)
        {
            Focus();
        }
        private void ChartForm_MouseLeave(object sender, EventArgs e)
        {
            Parent.Focus();
        }

        private void ChartForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys != Keys.Control) return;
            var child = GetChildAtPoint(e.Location);
            if (!_chartControl.Equals(child)) return;

            ChartRect newZoom = new ChartRect(_ptrChartArea);
            ScaleViewZoom(e.Delta, ref newZoom.X, Config.XMinZoom);
            ScaleViewZoom(e.Delta, ref newZoom.Y, Config.YMinZoom);
            UpdateAxis(newZoom, true);
            ScaleViewChanged?.Invoke(this, newZoom);
        }


        #region ChartEvents

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

        public void ChartControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys != Keys.Control) return;
            var child = GetChildAtPoint(e.Location);
            if (!_chartControl.Equals(child)) return;
            ChartRect newZoom = new ChartRect(_ptrChartArea);

            ScaleViewZoom(e.Delta, ref newZoom.X, Config.XMinZoom);
            ScaleViewZoom(e.Delta, ref newZoom.Y, Config.YMinZoom);
            UpdateAxis(newZoom, true);
            ScaleViewChanged?.Invoke(this, newZoom);
        }

        private void ScaleViewZoom(int delta, ref AxisSize axis, double minZoom)
        {
            var deltaPos = axis.Size * Config.ZoomSpeed;
            if (delta > 0)
            {
                if (axis.Size <= minZoom) return;
                deltaPos = -deltaPos;
            }
            axis.Size = axis.Size + deltaPos;
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
                        // ignored
                    }
                    break;
                case MouseButtons.Right:
                    OtherEvent?.Invoke(this, true);
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
                UpdateAxis();
                _chartControl.Refresh();
                ScaleViewChanged?.Invoke(this, new ChartRect(_ptrChartArea));
            }
        }

        private void ChartControl_AxisViewChanged(object sender, ViewEventArgs e)
        {
            UpdateAxis();
            ScaleViewChanged?.Invoke(this, new ChartRect(_ptrChartArea));
        }

        #endregion

        private bool _isDtpValueChanged = false;
        private void dtpStartX_ValueChanged(object sender, EventArgs e)
        {
            _isDtpValueChanged = true;
        }

        private void dtpStartX_Leave(object sender, EventArgs e)
        {
            if (!_isDtpValueChanged) return;
            _isDtpValueChanged = false;
            RefreshData(dtpStartX.Value);
            OtherEvent?.Invoke(this, false);
        }

        private void cbEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsValid) cbEnable.Checked = false;
        }

        private void btnReOpen_Click(object sender, EventArgs e)
        {
            ReadFile(_filePath);
            OtherEvent?.Invoke(this, false);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btnTurn_Click(object sender, EventArgs e)
        {
            IsMinimize ^= true;
            foreach (Control c in this.Controls)
            {
                c.Visible = !IsMinimize;
            }
            btnTurn.Visible = true;
            lblFileNameHid.Visible = IsMinimize;
            btnTurn.Text = IsMinimize ? "Развернуть" : "Свернуть";
            MinimumSize = new Size(MinimumSize.Width, IsMinimize ? 30 : 170);
            OtherEvent?.Invoke(this, false);
        }
    }
}
