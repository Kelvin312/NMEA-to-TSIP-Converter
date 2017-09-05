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
                tlbContent.RowStyles[tlbContent.GetRow(senderCf)] =
                    senderCf.IsMinimize ?
                        new RowStyle(SizeType.Absolute, senderCf.MinimumHeight) :
                        new RowStyle(SizeType.Percent, 100F);
                UpdateScrollSize();
            }
        }


        private void UpdateScrollSize()
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
