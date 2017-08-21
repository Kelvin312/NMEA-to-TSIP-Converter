using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CmpMagnetometersData
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            ChartConfig();
        }


        private void ChartConfig()
        {
            var ptrChartArea = chartControl.ChartAreas[0];
            chartControl.Series[0].ChartType = SeriesChartType.Spline;
            chartControl.Legends[0].Enabled = false;
            
            chartControl.Series[0].XValueType = ChartValueType.DateTime;
            ptrChartArea.AxisX.LabelStyle.Format = "yy.MM.dd-HH:mm:ss";
            ptrChartArea.AxisX.LabelStyle.IsEndLabelVisible = false;
            ptrChartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            ptrChartArea.CursorX.IsUserSelectionEnabled = true;
            ptrChartArea.CursorX.IntervalType = DateTimeIntervalType.Seconds;
            ptrChartArea.AxisX.ScrollBar.Enabled = false;

            chartControl.Series[0].YValueType = ChartValueType.Int32;
            ptrChartArea.AxisY.LabelStyle.Format = "#";
            ptrChartArea.AxisY.LabelStyle.IsEndLabelVisible = false;
            ptrChartArea.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            ptrChartArea.CursorY.IsUserSelectionEnabled = true;
            ptrChartArea.AxisY.ScrollBar.Enabled = false;

            chartControl.MouseWheel += ChartControl_MouseWheel;
            chartControl.MouseEnter += ChartControl_MouseEnter;
            chartControl.MouseLeave += ChartControl_MouseLeave;
            chartControl.MouseDown += ChartControl_MouseDown;
            chartControl.MouseMove += ChartControl_MouseMove;
            
        }


        private void ChartControl_MouseEnter(object sender, EventArgs e)
        {
            chartControl.Focus();
        }
        private void ChartControl_MouseLeave(object sender, EventArgs e)
        {
            chartControl.Parent.Focus();
        }
        private void ChartControl_MouseWheel(object sender, MouseEventArgs e)
        {
            var ptrChartArea = chartControl.ChartAreas[0];
            if (ModifierKeys == Keys.None)
            {
                ScaleViewZoom(ptrChartArea.AxisX, e.Delta);
                ScaleViewZoom(ptrChartArea.AxisY, e.Delta);
            }
        }
        private static void ScaleViewScroll(Axis ptrAxis, int delta)
        {
            AxisScaleView ptrView = ptrAxis.ScaleView;
            double deltaPos = (ptrView.ViewMaximum - ptrView.ViewMinimum) * 0.10;
            if (delta < 0) deltaPos = -deltaPos;
            ptrView.Scroll(ptrView.Position + deltaPos);
        }

        private static void ScaleViewZoom(Axis ptrAxis, int delta)
        {
            AxisScaleView ptrView = ptrAxis.ScaleView;
            double deltaPos = (ptrView.ViewMaximum - ptrView.ViewMinimum) * 0.25;
            if (delta < 0) deltaPos = -deltaPos;
            double newStart = ptrView.ViewMinimum + deltaPos;
            if (newStart < ptrAxis.Minimum) newStart = ptrAxis.Minimum;
            double newEnd = ptrView.ViewMaximum - deltaPos;
            if (newEnd > ptrAxis.Maximum) newEnd = ptrAxis.Maximum;
            ptrView.Zoom(newStart, newEnd);
        }

        private bool _mouseDowned;
        private double _xStart, _yStart;

        private void ChartControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _mouseDowned = true;
                _xStart = _yStart = 0;
                try
                {
                    var ptrChartArea = chartControl.ChartAreas[0];
                    _xStart = ptrChartArea.AxisX.PixelPositionToValue(e.Location.X);
                    _yStart = ptrChartArea.AxisY.PixelPositionToValue(e.Location.Y);
                }
                catch (Exception)
                {
                    
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                var ptrChartArea = chartControl.ChartAreas[0];
                ptrChartArea.AxisX.ScaleView.Zoom(ptrChartArea.AxisX.Minimum, ptrChartArea.AxisX.Maximum);
                ptrChartArea.AxisY.ScaleView.Zoom(ptrChartArea.AxisY.Minimum, ptrChartArea.AxisY.Maximum);
            }
        }

        private void ChartControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(_mouseDowned && e.Button == MouseButtons.Middle))
            {
                _mouseDowned = false;
                return;
            }
            var ptrChartArea = chartControl.ChartAreas[0];
            double selX, selY;
            try
            {
                selX = ptrChartArea.AxisX.PixelPositionToValue(e.Location.X);
                selY = ptrChartArea.AxisY.PixelPositionToValue(e.Location.Y);
            }
            catch (Exception)
            {
                return;
            }
            if (ptrChartArea.AxisX.ScaleView.IsZoomed ||
                ptrChartArea.AxisY.ScaleView.IsZoomed)
            {
                double dx = -selX + _xStart;
                double dy = -selY + _yStart;

                double newX = ptrChartArea.AxisX.ScaleView.Position + dx;
                double newY = ptrChartArea.AxisY.ScaleView.Position + dy;
               
                ptrChartArea.AxisX.ScaleView.Scroll(newX);
                ptrChartArea.AxisY.ScaleView.Scroll(newY);
            }
        }

       

        InputData inputData = new InputData();

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ofdAddFile.ShowDialog() == DialogResult.OK)
            {
                inputData.Read(ofdAddFile.FileName);
                chartControl.Series[0].Points.Clear();
                foreach (var p in inputData.GetPoints())
                {
                    chartControl.Series[0].Points.Add(p);
                }

            }

        }
    }
}
