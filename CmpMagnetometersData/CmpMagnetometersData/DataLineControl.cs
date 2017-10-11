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

        public void UpdateChart(TableLayoutPanel tlb)
        {
            var row = tlb.Controls.Contains(_chartControl);
            if ( !cbVisible.Checked)
            {
                tlb.Controls.Remove(_chartControl);
            }
            if ( cbVisible.Checked)
            {
                _chartControl._ptrSeries.Points.Clear();
                foreach (var dp in DataPixels)
                {
                    _chartControl._ptrSeries.Points.Add(dp.GetDataPoint());
                }
                tlb.Controls.Add(_chartControl);
            }
        }
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
