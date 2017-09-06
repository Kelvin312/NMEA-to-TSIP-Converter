using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public partial class ChartHolderForm : UserControl
    {
        public ChartHolderForm()
        {
            InitializeComponent();
            tlbContent.Controls.Clear();
            tlbContent.RowStyles.Clear();
            tlbContent.RowCount = 0;
        }

        public void AddChart(ChartForm chartForm)
        {
            chartForm.ScaleViewChanged += ChartForm_ScaleViewChanged;
            chartForm.OtherEvent += ChartForm_OtherEvent;
            chartForm.MouseEnter += ChartForm_MouseEnter;

            chartForm.Dock = DockStyle.Fill;
            tlbContent.RowCount++;
            tlbContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlbContent.Controls.Add(chartForm, 0, tlbContent.RowCount - 1);
        }


        private void ChartForm_ScaleViewChanged(object sender, ChartRect e)
        {
            if (!((ChartForm)sender).IsEnable) return;
            foreach (ChartForm chartForm in tlbContent.Controls)
            {
                chartForm.OnScaleViewChanged(sender, e);
            }
        }

        private void ChartForm_OtherEvent(object sender, OtherEventType e)
        {
            if (e == OtherEventType.ResetZoom)
            {
                if (!((ChartForm)sender).IsEnable) return;
                foreach (ChartForm chartForm in tlbContent.Controls)
                {
                    chartForm.OnOtherEvent(sender, e);
                }
            }
            if (e == OtherEventType.DataChanged)
            {
                if(sender!=null && !((ChartForm)sender).IsEnable) return;
                Config.GlobalBorder = new ChartRect();
                foreach (ChartForm chartForm in tlbContent.Controls)
                {
                    if (chartForm.IsEnable)
                    {
                        Config.GlobalBorder.Union(chartForm.Border);
                    }
                }
                foreach (ChartForm chartForm in tlbContent.Controls)
                {
                    chartForm.OnOtherEvent(sender, e);
                }
            }
            if (e == OtherEventType.MinimizeChanged)
            {
                var senderCf = (ChartForm)sender;
                tlbContent.RowStyles[tlbContent.GetRow((Control)sender)] =
                    senderCf.IsMinimize ?
                        new RowStyle(SizeType.Absolute, senderCf.MinimumHeight) :
                        new RowStyle(SizeType.Percent, 100F);
                UpdateScrollSize();
            }
            if (e == OtherEventType.CreateChart)
            {
                var form = new CreateForm
                {
                    cmbType = {SelectedIndex = 0},
                    txtA = {Text = (sender as ChartForm).ChartName}
                };
                foreach (ChartForm chartForm in tlbContent.Controls)
                {

                    if (chartForm is FileForm && chartForm.IsEnable && !chartForm.Equals(sender))
                    {
                        form.cmbB.Items.Add(new CmbItem(chartForm));
                    }

                }
                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (form.cmbType.SelectedIndex == 0)
                    {
                        var cnt = int.Parse(form.txtX.Text);
                        AddChart(new DistributionForm(sender as FileForm, cnt));
                        UpdateScrollSize();
                    }
                }
                

            }
        }


        public void UpdateScrollSize()
        {
            var scrollMinSize = 0;
            foreach (ChartForm chartForm in tlbContent.Controls)
            {
                scrollMinSize += chartForm.MinimumHeight;
            }
            this.AutoScrollMinSize = new Size(100, scrollMinSize);
        }


        private void ChartForm_MouseEnter(object sender, EventArgs e)
        {
            this.Focus();
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            return this.DisplayRectangle.Location;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            
            if (ModifierKeys == Keys.None) base.OnMouseWheel(e);
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
