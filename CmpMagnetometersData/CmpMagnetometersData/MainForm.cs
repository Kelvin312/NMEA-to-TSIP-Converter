using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ofdAddFile.ShowDialog() == DialogResult.OK)
            {
                btnDelete.Enabled = false;
                btnCalculate.Enabled = false;

                var resetZoomList = new List<ChartForm>();
                foreach (var fPath in ofdAddFile.FileNames)
                {
                        var chartForm = new ChartForm(fPath);
                        if (chartForm.IsValid)
                        {
                            resetZoomList.Add(chartForm);
                            Config.GlobalBorder.Union(chartForm.Border);
                            chartForm.ScaleViewChanged += ChartForm_ScaleViewChanged;
                            chartForm.OtherEvent += ChartForm_OtherEvent;

                            chartForm.Dock = DockStyle.Fill;
                            tlbContent.RowCount++;
                            tlbContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                            tlbContent.Controls.Add(chartForm, 0, tlbContent.RowCount - 1);
                        }
                }
                var scrollMinSize = 0;
                foreach (ChartForm chartForm in tlbContent.Controls)
                {
                    if (chartForm.IsValid && chartForm.IsEnable)
                    {
                        chartForm.UpdateAxis(null, false, resetZoomList.Contains(chartForm), true);
                    }
                    scrollMinSize += chartForm.MinimumSize.Height;
                }
                panelContent.AutoScrollMinSize = new Size(0, scrollMinSize);
                resetZoomList.Clear();
            }
        }

        private void ChartForm_ScaleViewChanged(object sender, ChartRect e)
        {
            if (!((ChartForm) sender).IsValid || !((ChartForm) sender).IsEnable) return;
            foreach (ChartForm chartForm in tlbContent.Controls)
            {
                if (!sender.Equals(chartForm)) chartForm.UpdateAxis(e);
            }
        }

        private void ChartForm_OtherEvent(object sender, bool e)
        {
            if (e)
            {
                foreach (ChartForm chartForm in tlbContent.Controls)
                {
                    if (chartForm.IsValid)
                    {
                        chartForm.UpdateAxis(null, false, true);
                    }
                }
            }
            else
            {
                var senderCf = (ChartForm) sender;
                tlbContent.RowStyles[tlbContent.GetRow(senderCf)] =
                    senderCf.IsMinimize ? 
                    new RowStyle(SizeType.Absolute, senderCf.MinimumSize.Height) : 
                    new RowStyle(SizeType.Percent, 100F);

                Config.GlobalBorder = new ChartRect();
                foreach (ChartForm chartForm in tlbContent.Controls)
                {
                    if (chartForm.IsValid && chartForm.IsEnable)
                    {
                        Config.GlobalBorder.Union(chartForm.Border);
                    }
                }
                var scrollMinSize = 0;
                foreach (ChartForm chartForm in tlbContent.Controls)
                {
                    if (chartForm.IsValid && chartForm.IsEnable)
                    {
                        chartForm.UpdateAxis(null, false, false, true);
                    }
                    scrollMinSize += chartForm.MinimumSize.Height;
                }
                panelContent.AutoScrollMinSize = new Size(0, scrollMinSize);
            }
        }

        private void MagneticFieldChanged()
        {
            Config.GlobalBorder = new ChartRect();
            foreach (ChartForm chartForm in tlbContent.Controls)
            {
                if (chartForm.IsValid)
                {
                    chartForm.RefreshData();
                    if (chartForm.IsEnable)
                    {
                        Config.GlobalBorder.Union(chartForm.Border);
                    }
                }
            }
            foreach (ChartForm chartForm in tlbContent.Controls)
            {
                chartForm.UpdateAxis(null, false, true, true);
            }
        }

        private void btnMagneticField_Click(object sender, EventArgs e)
        {
            if (!btnMagneticField.Checked)
            {
                btnRmsDeviation.Checked = false;
                btnMagneticField.Checked = true;
                Config.IsMagneticField = btnMagneticField.Checked;
                MagneticFieldChanged();
            }
        }

        private void btnRmsDeviation_Click(object sender, EventArgs e)
        {
            if (!btnRmsDeviation.Checked)
            {
                btnRmsDeviation.Checked = true;
                btnMagneticField.Checked = false;
                Config.IsMagneticField = btnMagneticField.Checked;
                MagneticFieldChanged();
            }
        }


        private SortedSet<KeyValueHolder<double, int>> _removeXlist = new SortedSet<KeyValueHolder<double, int>>();
        private void btnCheck_Click(object sender, EventArgs e)
        {
            //if(tlbContent.Controls.Count < 2) return;
            //var intersectXlist = new SortedSet<KeyValueHolder<double, int>>(_chartForms[0].GetXList());
            //var unionXlist = new SortedSet<KeyValueHolder<double, int>>();

            //for (int si = 1; si < _chartForms.Count; si++)
            //{
            //    var secondXlist = _chartForms[si].GetXList();
            //    intersectXlist.IntersectWith(secondXlist);
            //    unionXlist.UnionWith(secondXlist);
            //}
            //unionXlist.ExceptWith(intersectXlist);
            //lblValues.Text = "Проверено";
            //txtValues.Text = "";
            //if (unionXlist.Count > 0)
            //{
            //    _removeXlist = unionXlist;
            //    lblValues.Text = "Не совпадают";
            //    btnDelete.Enabled = true;
            //    foreach (var item in unionXlist)
            //    {
            //        txtValues.AppendText(DateTime.FromOADate(item.Key).ToString(Config.ViewTimeFormat) + "\r\n");
            //    }
            //}
            //else
            //{
            //    btnCalculate.Enabled = true;
            //}
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            //btnDelete.Enabled = false;

            //foreach (var form in _chartForms)
            //{
            //    form.GetPoints().RemoveAll(p => _removeXlist.Contains(
            //        new KeyValueHolder<double, int>(p.Time.ToOADate())));
            //    form.RefreshData();
            //}
            //RemoveChartForm();

            //btnCalculate.Enabled = true;
            //lblValues.Text = "Удалено";
            //txtValues.Text = "";
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            //btnCalculate.Enabled = false;
            //lblValues.Text = "коэффициент корреляции";
            //txtValues.Text = "";

            //for (int fi = 0; fi < _chartForms.Count; fi++)
            //{
            //    var fp = _chartForms[fi].GetPoints();
            //    for (int si = fi+1; si < _chartForms.Count; si++)
            //    {
            //        var sp = _chartForms[si].GetPoints();
            //        double sumA = 0, sumB = 0, sumAB = 0, sumAA = 0, sumBB = 0;
            //        long a, b, n = fp.Count;
            //        for (int i = 0; i < n; i++)
            //        {
            //            a = (Config.IsMagneticField ? fp[i].MagneticField : fp[i].RmsDeviation);
            //            b = (Config.IsMagneticField ? sp[i].MagneticField : sp[i].RmsDeviation);
            //            sumA += a;
            //            sumB += b;
            //            sumAB += a * b;
            //            sumAA += a * a;
            //            sumBB += b * b;
            //        }
            //        double res = (n * sumAB - sumA * sumB) /
            //                     Math.Sqrt((n * sumAA - sumA * sumA) * (n * sumBB - sumB * sumB));
            //        txtValues.AppendText(
            //            _chartForms[fi].GetFileName() 
            //            + " и " 
            //            + _chartForms[si].GetFileName() 
            //            + " = " 
            //            + res.ToString("F") 
            //            + "\r\n\r\n");
            //    }
            //}
        }
    }


    public class CustomPanel : Panel
    {
        public CustomPanel()
        {
            this.MouseEnter += CustomPanel_MouseEnter;
        }

        private void CustomPanel_MouseEnter(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
           if(ModifierKeys == Keys.None) base.OnMouseWheel(e);
        }
    }
}
