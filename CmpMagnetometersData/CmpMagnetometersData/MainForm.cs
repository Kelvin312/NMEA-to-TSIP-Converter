using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            Size = Properties.Settings.Default.MainFormSize;
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
                        chartForm.MouseEnter += ChartForm_MouseEnter;

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

                btnDelete.Enabled = false;
                btnCalculate.Enabled = false;
            }
        }

        private void RefreshChartForms(bool isResetZoom = false)
        {
            Config.GlobalBorder = new ChartRect();
            foreach (ChartForm chartForm in tlbContent.Controls)
            {
                if (chartForm.IsValid)
                {
                    chartForm.RefreshData();
                    if (chartForm.IsValid && chartForm.IsEnable)
                    {
                        Config.GlobalBorder.Union(chartForm.Border);
                    }
                }
            }
            foreach (ChartForm chartForm in tlbContent.Controls)
            {
                chartForm.UpdateAxis(null, false, isResetZoom, true);
            }
        }

        private void btnMagneticField_Click(object sender, EventArgs e)
        {
            if (!btnMagneticField.Checked)
            {
                btnRmsDeviation.Checked = false;
                btnMagneticField.Checked = true;
                Config.IsMagneticField = btnMagneticField.Checked;
                RefreshChartForms(true);
            }
        }

        private void btnRmsDeviation_Click(object sender, EventArgs e)
        {
            if (!btnRmsDeviation.Checked)
            {
                btnRmsDeviation.Checked = true;
                btnMagneticField.Checked = false;
                Config.IsMagneticField = btnMagneticField.Checked;
                RefreshChartForms(true);
            }
        }


        private SortedSet<KeyValueHolder<double, int>> _removeXlist = new SortedSet<KeyValueHolder<double, int>>();
        private void btnCheck_Click(object sender, EventArgs e)
        {
            if(tlbContent.Controls.Count < 2) return;
            
            SortedSet<KeyValueHolder<double, int>> intersectXlist = null;
            var unionXlist = new SortedSet<KeyValueHolder<double, int>>();
            int enableCount = 0;

            foreach (ChartForm chartForm in tlbContent.Controls)
            {
                if (chartForm.IsValid && chartForm.IsEnable)
                {
                    enableCount++;
                    if (intersectXlist == null)
                        intersectXlist = new SortedSet<KeyValueHolder<double, int>>(chartForm.GetXList());
                    else
                        intersectXlist.IntersectWith(chartForm.GetXList());
                    unionXlist.UnionWith(chartForm.GetXList());
                }
            }
            if(enableCount < 2) return;

            unionXlist.ExceptWith(intersectXlist);
            lblValues.Text = "Проверено";
            txtValues.Text = "";
            if (unionXlist.Count > 0)
            {
                _removeXlist = unionXlist;
                lblValues.Text = "Не совпадают";
                btnDelete.Enabled = true;
                StringBuilder text = new StringBuilder();
                foreach (var item in unionXlist)
                {
                    text.AppendFormat("{0:"+ Properties.Settings.Default.ViewTimeText+"}\r\n",
                        DateTime.FromOADate(item.Key));
                }
                txtValues.AppendText(text.ToString());
            }
            else
            {
                btnCalculate.Enabled = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            btnDelete.Enabled = false;

            foreach (ChartForm chartForm in tlbContent.Controls)
            {
                chartForm.RemovePoints(_removeXlist);
            }
            RefreshChartForms();

            btnCalculate.Enabled = true;
            lblValues.Text = "Удалено";
            txtValues.Text = "";
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            btnCalculate.Enabled = false;
            lblValues.Text = "коэффициент корреляции";
            txtValues.Text = "";
            var formCount = tlbContent.Controls.Count;
            StringBuilder text = new StringBuilder();

            for (int fi = 0; fi < formCount; fi++)
            {
                var firsForm = tlbContent.Controls[fi] as ChartForm;
                if (firsForm.IsValid && firsForm.IsEnable)
                    for (int si = fi+1; si < formCount; si++)
                    {
                        var secondForm = tlbContent.Controls[si] as ChartForm;
                        if (secondForm.IsValid && secondForm.IsEnable)
                        {
                            double sumA = 0, sumB = 0, sumAB = 0, sumAA = 0, sumBB = 0;
                            long a, b, n = firsForm.GetCount();
                            for (int i = 0; i < n; i++)
                            {
                                a = firsForm.GetValue(i);
                                b = secondForm.GetValue(i);
                                sumA += a;
                                sumB += b;
                                sumAB += a * b;
                                sumAA += a * a;
                                sumBB += b * b;
                            }
                            double res = (n * sumAB - sumA * sumB) /
                                         Math.Sqrt((n * sumAA - sumA * sumA) * (n * sumBB - sumB * sumB));
                            text.AppendFormat("{0} и {1} = {2:F}\r\n\r\n",
                                firsForm.FileName,secondForm.FileName, res);
                        }
                    }
            }
            txtValues.AppendText(text.ToString());
        }

        private void ChartForm_MouseEnter(object sender, EventArgs e)
        {
            panelContent.Focus();
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            var configForm = new ConfigForm();
            if (configForm.ShowDialog() == DialogResult.OK)
            {
                foreach (ChartForm chartForm in tlbContent.Controls)
                {
                    chartForm.SetTimeView();
                    if(configForm.IsColorChange) chartForm.RefreshData();
                }
                Properties.Settings.Default.Save();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.MainFormSize = Size;
            Properties.Settings.Default.Save();
        }
    }


    public class CustomPanel : Panel
    {
        protected override void OnMouseWheel(MouseEventArgs e)
        {
           if(ModifierKeys == Keys.None) base.OnMouseWheel(e);
           else if (ModifierKeys == Keys.Control)
           {
               Point cursorPos = e.Location;
               Control child = this;
               while ((child = child.GetChildAtPoint(cursorPos)) != null)
               {
                   cursorPos.X -= child.Location.X;
                   cursorPos.Y -= child.Location.Y;
                   if (child.Name == "chartControl")
                   {
                       (child.Parent as ChartForm)?.ChartControl_MouseWheel(e.Delta);
                       break;
                   }
               }
           }
        }
    }
}
