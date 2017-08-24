using System;
using System.Collections.Generic;
using System.Windows.Forms;

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

        private List<ChartForm> _chartForms = new List<ChartForm>();

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ofdAddFile.ShowDialog() == DialogResult.OK)
            {
                foreach (var fName in ofdAddFile.FileNames)
                {
                    try
                    {
                        var chartForm = new ChartForm(fName);
                        if (chartForm.IsReady)
                        {
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
                ChartForm_ResetZoom();
            }
        }

        private void ChartForm_ScaleViewChanged(object sender, ChartRect e)
        {
            foreach (var chartForm in _chartForms)
            {
                if(!sender.Equals(chartForm)) chartForm.ScaleViewResize(e);
            }
        }

        private void ChartForm_ResetZoom()
        {
            bool isFirst = true;
            foreach (var chartForm in _chartForms)
            {
                if (isFirst)
                {
                    Config.GlobalRect = chartForm.GetRect();
                    isFirst = false;
                }
                else
                {
                    Config.GlobalRect.Union(chartForm.GetRect());
                }
            }
            foreach (var chartForm in _chartForms)
            {
                chartForm.ScaleViewResize(Config.GlobalRect, true);
            }
        }

        private void btnMagneticField_Click(object sender, EventArgs e)
        {
            if (!btnMagneticField.Checked)
            {
                btnRmsDeviation.Checked = false;
                btnMagneticField.Checked = true;
                Config.IsMagneticField = btnMagneticField.Checked;
                foreach (var chartForm in _chartForms)
                {
                    chartForm.RefreshData();
                }

                ChartForm_ResetZoom();
            }
        }

        private void btnRmsDeviation_Click(object sender, EventArgs e)
        {
            if (!btnRmsDeviation.Checked)
            {
                btnRmsDeviation.Checked = true;
                btnMagneticField.Checked = false;
                Config.IsMagneticField = btnMagneticField.Checked;
                foreach (var chartForm in _chartForms)
                {
                    chartForm.RefreshData();
                }
                ChartForm_ResetZoom();
            }
        }
    }
}
