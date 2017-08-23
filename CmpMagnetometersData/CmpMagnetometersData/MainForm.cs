using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            tlbContent.RowCount = 0;
            tlbContent.RowStyles.Clear();
        }

        private List<ChartFormData> _chartForms = new List<ChartFormData>();

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ofdAddFile.ShowDialog() == DialogResult.OK)
            {
                foreach (var fName in ofdAddFile.FileNames)
                {
                    try
                    {
                        var chartForm = new ChartFormData(fName);
                        if (chartForm.IsReady)
                        {
                            if (_chartForms.Count == 0)
                            {
                                Config.GlobalRect = chartForm.GetRect();
                            }
                            else
                            {
                                Config.GlobalRect.Union(chartForm.GetRect());
                            }
                            _chartForms.Add(chartForm);
                            chartForm.ScaleViewChanged += ChartForm_ScaleViewChanged;
                            chartForm.ResetZoom += ChartForm_ResetZoom;

                            chartForm.Dock = DockStyle.Fill;
                            tlbContent.RowCount++;
                            tlbContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                            tlbContent.Controls.Add(chartForm, 0, tlbContent.RowCount - 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(fName + "\r\n" + ex.Message, 
                            "Ошибка открытия файла", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
                        throw;
                    }
                }
            }
        }

        private void ChartForm_ScaleViewChanged(object sender, ViewEventArgs e)
        {
            var resize = new ChartRect(e.ChartArea);
            foreach (var chartForm in _chartForms)
            {
                if(!sender.Equals(chartForm)) chartForm.ScaleViewResize(resize);
            }
        }

        private void ChartForm_ResetZoom()
        {
            foreach (var chartForm in _chartForms)
            {
                chartForm.ScaleViewResize(Config.GlobalRect, true);
            }
        }


    }
}
