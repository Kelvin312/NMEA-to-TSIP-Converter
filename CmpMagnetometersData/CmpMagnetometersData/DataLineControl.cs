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
        }

        public event EventHandler<SendEventArgs> SendEvent;


        public void ReadEvent(object sender, SendEventArgs e)
        {
            if (e.TypeEvent == SendEventArgs.TypeEventE.RbtnClick)
            {
                rbFirstSelect.Checked = false;

            }
        }

        public List<DataPixel> DataPixels = new List<DataPixel>();
        private ChartControl _chartControl = new ChartControl();

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

        public void UpdateChart(TableLayoutPanel tlb, ref int hSize)
        {
            var contains = tlb.Controls.Contains(_chartControl);
            if (contains && !cbVisible.Checked)
            {
                tlb.Controls.Remove(_chartControl);
                tlb.RowStyles.RemoveAt(tlb.RowStyles.Count-1);
            }
            if (!contains && cbVisible.Checked)
            {
                _chartControl._ptrSeries.Points.Clear();
                foreach (var dp in DataPixels)
                {
                    _chartControl._ptrSeries.Points.Add(dp.GetDataPoint());
                }
                _chartControl.Dock = DockStyle.Fill;
                tlb.Controls.Add(_chartControl);
                tlb.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            }
            if(cbVisible.Checked) hSize += 285;
        }

        private void rbFirstSelect_CheckedChanged(object sender, EventArgs e)
        {
            if(rbFirstSelect.Checked)
           SendEvent?.Invoke(this, new SendEventArgs() {Args = 0, TypeEvent = SendEventArgs.TypeEventE.RbtnClick});
        }
    }


    public class SendEventArgs
    {
        public enum TypeEventE
        {
            RbtnClick
        }

        public TypeEventE TypeEvent;
        public int Args;

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
