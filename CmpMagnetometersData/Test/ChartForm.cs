using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Test.Properties;

namespace Test
{
    public partial class ChartForm : UserControl
    {
        public ChartForm(string chartName)
        {
            InitializeComponent();
            IsMinimize = false;
            IsEnable = true;
            ChartName = chartName;
            _ptrSeries = chartControl.Series[0];
            _ptrChartArea = chartControl.ChartAreas[0];
            _ptrAxisX = _ptrChartArea.AxisX;
            _ptrAxisY = _ptrChartArea.AxisY;
            ChartControlInit();
            UpdateControls();
        }
        public bool IsMinimize { get; protected set; }
        public bool IsEnable { get; protected set; }
        public string ChartName { get; }

        public int MinimumHeight { get; protected set; }

        public ChartRect Border { get; protected set; }

        public event EventHandler<ChartRect> ScaleViewChanged;

        public event EventHandler<OtherEventType> OtherEvent;

        protected void OnScaleViewChanged(ChartRect e = null)
        {
            if (e == null) e = new ChartRect(_ptrChartArea);
            ScaleViewChanged?.Invoke(this, e);
        }

        public virtual void OnScaleViewChanged(object sender, ChartRect e)
        {
            
        }

        protected void OnOtherEvent(OtherEventType e)
        {
            OtherEvent?.Invoke(this, e);
        }

        public virtual void OnOtherEvent(object sender, OtherEventType e)
        {

        }

        protected double XMinZoom, YMinZoom;

        protected void UpdateAxis(ChartRect newView = null, bool isUpdateY = false, bool isResetZoom = false, bool isUpdateBorder = false)
        {
            var curView = new ChartRect(_ptrChartArea);
            var globalBorder = new ChartRect(_ptrChartArea, true);

            if (isUpdateBorder)
            {
                globalBorder = new ChartRect(Config.GlobalBorder);
                globalBorder.Y.Min -= 2;
                globalBorder.Y.Max += YMinZoom * 10.0;
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
                }
            }
            var oldView = new ChartRect(_ptrChartArea);
            if (curView.X.Check(oldView.X, XMinZoom, globalBorder.X))
            {
                _ptrAxisX.ScaleView.Zoom(curView.X.Min, curView.X.Max);
            }
            if (curView.Y.Check(oldView.Y, YMinZoom, globalBorder.Y))
            {
                _ptrAxisY.ScaleView.Zoom(curView.Y.Min, curView.Y.Max);
            }
        }

        protected readonly Series _ptrSeries;
        protected readonly ChartArea _ptrChartArea;
        protected readonly Axis _ptrAxisX;
        protected readonly Axis _ptrAxisY;


        public virtual void UpdateControls()
        {
            if (IsMinimize)
            {
                btnMinimize.Text = "Развернуть";
                btnMinimize.Image = Resources.maximize;
                MinimumHeight = 29;
            }
            else
            {
                btnMinimize.Text = "Свернуть";
                btnMinimize.Image = Resources.minimize;
                MinimumHeight = 250;
            }
            chartControl.Visible = !IsMinimize;
            lblChartNameHide.Visible = IsMinimize;
            cbEnable.Checked = IsEnable;
            lblChartName.Text = ChartName;
            lblChartNameHide.Text = ChartName;
        }

        protected void ChartControlInit()
        {
            //X
            _ptrAxisX.LabelStyle.IsEndLabelVisible = false;
            _ptrAxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            _ptrChartArea.CursorX.IsUserSelectionEnabled = true;
            _ptrAxisX.ScrollBar.Enabled = false;
            //Y
            _ptrAxisY.LabelStyle.IsEndLabelVisible = false;
            _ptrAxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            _ptrChartArea.CursorY.IsUserSelectionEnabled = true;
            _ptrAxisY.ScrollBar.Enabled = false;
            //Mouse
            chartControl.MouseEnter += ChartControl_MouseEnter;
            chartControl.MouseDown += ChartControl_MouseDown;
            chartControl.MouseMove += ChartControl_MouseMove;
            chartControl.AxisViewChanged += ChartControl_AxisViewChanged;
        }

        public void ChartControl_MouseWheel(int delta)
        {
            ChartRect newZoom = new ChartRect(_ptrChartArea);
            ScaleViewZoom(delta, ref newZoom.X);
            ScaleViewZoom(delta, ref newZoom.Y);
            UpdateAxis(newZoom, true);
            ViewChanged();
        }

        private void ScaleViewZoom(int delta, ref AxisSize axis)
        {
            var deltaPos = axis.Size * Settings.Default.ZoomSpeed;
            if (delta > 0) deltaPos = -deltaPos;
            axis.Size = axis.Size + deltaPos;
        }

        private void ChartControl_MouseEnter(object sender, EventArgs e)
        {
            this.OnMouseEnter(e);
        }

        private bool _mouseDowned;
        private double _xStart, _yStart;

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
                    UpdateAxis(null, false, true);
                    ViewChanged(true);
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
                ViewChanged();
            }
        }

        private void ChartControl_AxisViewChanged(object sender, ViewEventArgs e)
        {
            ViewChanged();
        }

        protected virtual void ViewChanged(bool isResetZoom = false)
        {
            
        }




        protected virtual void btnMinimize_Click(object sender, EventArgs e)
        {
            IsMinimize ^= true;
            UpdateControls();
            OnOtherEvent(OtherEventType.MinimizeChanged);
        }

        protected virtual void cbEnable_CheckedChanged(object sender, EventArgs e)
        {
            IsEnable ^= true;
            UpdateControls();
        }


        private bool _isDtpValueChanged = false;

        private void dtp_ValueChanged(object sender, EventArgs e)
        {
            _isDtpValueChanged = true;
        }

        private void dtp_Leave(object sender, EventArgs e)
        {
            if (_isDtpValueChanged)
            {
                OnDtpValueChanged((sender as DateTimePicker).Value);
                _isDtpValueChanged = false;
            }
        }

        protected virtual void OnDtpValueChanged(DateTime newTime)
        {
            
        }

    }
}
