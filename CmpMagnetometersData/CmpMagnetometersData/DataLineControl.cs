using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.VisualStyles;

namespace CmpMagnetometersData
{
    public partial class DataLineControl : UserControl
    {
        public DataLineControl()
        {
            InitializeComponent();
            _chartControl.ViewChanged += _chartControl_ViewChanged;
        }

        protected virtual void _chartControl_ViewChanged(object sender, ChartArea e)
        {
            if (e == null)
            {
                SendEvent?.Invoke(this, new SendEventArgs() { TypeEvent = SendEventArgs.TypeEventE.ResetZoom });
            }
            else
            {
                SendEvent?.Invoke(this, new SendEventArgs() {Rect = new ChartRect(e), TypeEvent = SendEventArgs.TypeEventE.ChangeZoom });
            }
        }

        public event EventHandler<SendEventArgs> SendEvent;


        public virtual void ReadEvent(object sender, SendEventArgs e)
        {
            switch (e.TypeEvent)
            {
                case SendEventArgs.TypeEventE.RbtnClick:
                    rbFirstSelect.Checked = false;
                    break;
                case SendEventArgs.TypeEventE.ResetZoom:
                    if(_isVisibleChart) _chartControl.UpdateAxis(null, false, true);
                    break;
                case SendEventArgs.TypeEventE.ChangeZoom:
                    if (_isVisibleChart) _chartControl.UpdateAxis(e.Rect);
                    break;
                case SendEventArgs.TypeEventE.ChangeZoomWithY:
                    if (_isVisibleChart) _chartControl.UpdateAxis(e.Rect, true);
                    break;
                case SendEventArgs.TypeEventE.UpdateBorder:
                    _chartControl.GlobalBorder = e.Rect;
                    if (_isVisibleChart) _chartControl.UpdateAxis(null, false, false, true);
                    break;
            }
        }

        private bool _isVisibleChart = false;

        public List<DataPixel> DataPixels = new List<DataPixel>();
        protected ChartControl _chartControl = new ChartControl();

        public string ReloadPath { get; set; }
        public int ReloadIndex { get; set; }

        public string LineName
        {
            get
            {
                return _lineName;
            }

            set
            {
                _lineName = value;
                txtDataLineName.Text = value;
                _chartControl.lblName.Text = value;
            }
        }

        private string _lineName;

        private void txtDataLineName_TextChanged(object sender, EventArgs e)
        {
            _lineName = txtDataLineName.Text;
            _chartControl.lblName.Text = txtDataLineName.Text;
        }

        public virtual void UpdateChart(TableLayoutPanel tlb, ref int hSize)
        {
            _isVisibleChart = tlb.Controls.Contains(_chartControl);
            if (_isVisibleChart && !cbVisible.Checked)
            {
                tlb.Controls.Remove(_chartControl);
                tlb.RowStyles.RemoveAt(tlb.RowStyles.Count-1);
                _isVisibleChart = false;
            }
            if (!_isVisibleChart && cbVisible.Checked)
            {
                _chartControl._ptrSeries.Points.Clear();
                foreach (var dp in DataPixels)
                {
                    _chartControl._ptrSeries.Points.Add(dp.GetDataPoint());
                }
                _chartControl.Dock = DockStyle.Fill;
                tlb.Controls.Add(_chartControl);
                tlb.RowStyles.Add(new RowStyle(SizeType.Percent,100));
                _isVisibleChart = true;
            }
            if(_isVisibleChart) hSize += 285;
        }

        private void rbFirstSelect_CheckedChanged(object sender, EventArgs e)
        {
            if(rbFirstSelect.Checked)
           SendEvent?.Invoke(this, new SendEventArgs() {TypeEvent = SendEventArgs.TypeEventE.RbtnClick});
        }
    }


    public class SendEventArgs
    {
        public enum TypeEventE
        {
            RbtnClick,
            ResetZoom,
            ChangeZoom,
            ChangeZoomWithY,
            UpdateBorder
        }

        public TypeEventE TypeEvent;
        public ChartRect Rect = null;

    }

    public class DataPixel
    {
        public int Val;
        public DateTime Time;
        public Color Color;

        public DataPoint GetDataPoint()
        {
            var res = new DataPoint(Time.ToOADate(),Val);
            res.Color = Color;
            return res;
        }
    }
}
