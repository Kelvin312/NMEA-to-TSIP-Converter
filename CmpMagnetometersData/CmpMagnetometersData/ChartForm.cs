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
        private readonly Chart _chartControl;
        private readonly Series _ptrSeries;
        private readonly ChartArea _ptrChartArea;
        private readonly Axis _ptrAxisX;
        private readonly Axis _ptrAxisY;

        public ChartForm(string filePath)
        {
            InitializeComponent();

            IsMinimize = false;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            lblFileName.Text = FileName;
            lblFileNameHid.Text = FileName;

            _chartControl = chartControl;
            _ptrSeries = _chartControl.Series[0];
            _ptrChartArea = _chartControl.ChartAreas[0];
            _ptrAxisX = _ptrChartArea.AxisX;
            _ptrAxisY = _ptrChartArea.AxisY;
            ChartControlInit();

            ReadFile(filePath);
        }

        //private void ChartForm_Load(object sender, EventArgs e)
        //{
        //    dtpStartX.Value = _filePoints.First().Time;
        //    _isDtpValueChanged = false;
        //}

        private void ChartControlInit()
        {
            _chartControl.Legends.Clear();
            _ptrSeries.ChartType = SeriesChartType.Spline;

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
            _chartControl.MouseEnter += ChartControl_MouseEnter;
            _chartControl.MouseLeave += ChartControl_MouseLeave;
            _chartControl.MouseWheel += ChartControl_MouseWheel;
            _chartControl.MouseDown += ChartControl_MouseDown;
            _chartControl.MouseMove += ChartControl_MouseMove;
            _chartControl.AxisViewChanged += ChartControl_AxisViewChanged;
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

        private void ChartControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.None)
            {
                ChartRect newZoom = new ChartRect(_ptrChartArea);

                ScaleViewZoom(e.Delta, ref newZoom.X, Config.XMinZoom);
                ScaleViewZoom(e.Delta, ref newZoom.Y, Config.YMinZoom);
                UpdateAxis(newZoom, true);
                ScaleViewChanged?.Invoke(this, newZoom);
            }
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

        private void btnTurn_Click(object sender, EventArgs e)
        {
            IsMinimize ^= true;
            foreach (Control c in this.Controls)
            {
                c.Visible = !IsMinimize;
            }
                OtherEvent?.Invoke(this, false);
        }

       
        private void btnDelete_Click(object sender, EventArgs e)
        {
            
        }

        //private void ChartForm_Resize(object sender, EventArgs e)
        //{
        //    if(IsMinimize == Height < 145) return;
        //    IsMinimize = this.Height < 145;
        //        
        //    btnTurn.Visible = true;
        //    lblFileNameHid.Visible = IsMinimize;
        //    lblFileNameHid.Text = lblFileName.Text;
        //    btnTurn.Text = IsMinimize ? "Развернуть" : "Свернуть";
        //}

    }
}
