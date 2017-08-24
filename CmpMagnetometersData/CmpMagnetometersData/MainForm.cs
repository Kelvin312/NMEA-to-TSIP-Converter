using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CmpMagnetometersData
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            tlbContent.Controls.Clear();
            tlbContent.RowStyles.Clear();
            tlbContent.RowCount = 0;
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
                            chartForm.TurnChange += ChartForm_TurnChange;
                            chartForm.DeleteForm += ChartForm_DeleteForm;

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

        private void ChartForm_DeleteForm(object sender, EventArgs e)
        {
            RemoveChartForm();
        }

        private void RemoveChartForm()
        {
            _chartForms.RemoveAll(f => !f.IsReady);
            tlbContent.Controls.Clear();
            tlbContent.RowStyles.Clear();
            tlbContent.RowCount = 0;
            foreach (var f in _chartForms)
            {
                tlbContent.RowCount++;
                tlbContent.RowStyles.Add(!f.IsMinimize ? new RowStyle(SizeType.Percent, 100F) 
                    : new RowStyle(SizeType.Absolute, 30F));
                tlbContent.Controls.Add(f, 0, tlbContent.RowCount - 1);
            }
        }

        private void ChartForm_TurnChange(object sender, bool e)
        {
            var index = tlbContent.GetRow(sender as ChartForm);
            tlbContent.RowStyles[index] = e?new RowStyle(SizeType.Percent, 100F): 
                new RowStyle(SizeType.Absolute, 30F);
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


        private SortedSet<KeyValueHolder<double, int>> _removeXlist = new SortedSet<KeyValueHolder<double, int>>();
        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (_chartForms.Count < 2) return;
            var intersectXlist = new SortedSet<KeyValueHolder<double, int>>(_chartForms[0].GetXList());
            var unionXlist = new SortedSet<KeyValueHolder<double, int>>(_chartForms[0].GetXList());
            for (int si = 1; si < _chartForms.Count; si++)
            {
                var secondXlist = _chartForms[si].GetXList();
                intersectXlist.IntersectWith(secondXlist);
                unionXlist.UnionWith(secondXlist);
            }
            unionXlist.ExceptWith(intersectXlist);
            lblValues.Text = "Проверено";
            txtValues.Text = "";
            if (unionXlist.Count > 0)
            {
                _removeXlist = unionXlist;
                lblValues.Text = "Не совпадают";
                btnDelete.Enabled = true;
                foreach (var item in unionXlist)
                {
                    txtValues.AppendText(DateTime.FromOADate(item.Key).ToString(Config.ViewTimeFormat) + "\r\n");
                }
            }
            else
            {
                btnCalculate.Enabled = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            btnDelete.Enabled = false;

            foreach (var form in _chartForms)
            {
                form.GetPoints().RemoveAll(p => _removeXlist.Contains(
                    new KeyValueHolder<double, int>(p.Time.ToOADate())));
                form.RefreshData();
            }
            RemoveChartForm();

            btnCalculate.Enabled = true;
            lblValues.Text = "Удалено";
            txtValues.Text = "";
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            btnCalculate.Enabled = false;
            lblValues.Text = "коэффициент корреляции";
            txtValues.Text = "";

            for (int fi = 0; fi < _chartForms.Count; fi++)
            {
                var fp = _chartForms[fi].GetPoints();
                for (int si = fi+1; si < _chartForms.Count; si++)
                {
                    var sp = _chartForms[si].GetPoints();
                    double sumA = 0, sumB = 0, sumAB = 0, sumAA = 0, sumBB = 0;
                    long a, b, n = fp.Count;
                    for (int i = 0; i < n; i++)
                    {
                        a = (Config.IsMagneticField ? fp[i].MagneticField : fp[i].RmsDeviation);
                        b = (Config.IsMagneticField ? sp[i].MagneticField : sp[i].RmsDeviation);
                        sumA += a;
                        sumB += b;
                        sumAB += a * b;
                        sumAA += a * a;
                        sumBB += b * b;
                    }
                    double res = (n * sumAB - sumA * sumB) /
                                 Math.Sqrt((n * sumAA - sumA * sumA) * (n * sumBB - sumB * sumB));
                    txtValues.AppendText(
                        _chartForms[fi].GetFileName() 
                        + " и " 
                        + _chartForms[si].GetFileName() 
                        + " = " 
                        + res.ToString("F") 
                        + "\r\n\r\n");
                }
            }
        }
    }
}
