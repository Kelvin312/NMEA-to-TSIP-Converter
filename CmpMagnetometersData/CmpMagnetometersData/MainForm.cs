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
                        using (StreamReader sr = new StreamReader(fName))
                        {
                            var chartForm = new ChartFormData(sr);
                            if (chartForm.IsReady)
                            {
                                _chartForms.Add(chartForm);
                                chartForm.Dock = DockStyle.Fill;
                                tlbContent.RowCount++;
                                tlbContent.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
                                tlbContent.Controls.Add(chartForm, 0, tlbContent.RowCount-1);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(fName + "\r\n"+ex.Message, "Ошибка открытия файла", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw;
                    }
                }
                
                //inputData.Read(ofdAddFile.FileName);
                //chartControl.Series[0].Points.Clear();
                //foreach (var p in inputData.GetPoints())
                //{
                //    chartControl.Series[0].Points.Add(p);
                //}

            }

        }
    }
}
