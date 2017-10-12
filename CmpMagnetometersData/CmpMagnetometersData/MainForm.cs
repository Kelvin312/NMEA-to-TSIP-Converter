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

namespace CmpMagnetometersData
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        
        private void btnAddDataLines_Click(object sender, EventArgs e)
        {
            if (ofdDataLines.ShowDialog() == DialogResult.OK)
            {
                foreach (var filePath in ofdDataLines.FileNames)
                {
                    FileParser fp = new FileParser(filePath);
                    if (fp.DataControl != null)
                    {
                        tableLayoutPanel3.Controls.Add(fp.DataControl);
                        fp.DataControl.SendEvent += DataControl_SendEvent;
                    }

                    if (fp.DeviationControl != null)
                    {
                        tableLayoutPanel3.Controls.Add(fp.DeviationControl);
                        fp.DeviationControl.SendEvent += DataControl_SendEvent;


                    }
                }
                
            }
        }

        private void DataControl_SendEvent(object sender, SendEventArgs e)
        {
            foreach (DataLineControl c in tableLayoutPanel3.Controls)
            {
                if (!c.Equals(sender as DataLineControl))
                {
                    c.ReadEvent(sender, e);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int hSize = 0;
            foreach (DataLineControl c in tableLayoutPanel3.Controls)
            {
                c.UpdateChart(tableLayoutPanel2, ref hSize);
            }
     
            panel1.AutoScrollMinSize = new Size(400, hSize);
        }

        private void btnSub_Click(object sender, EventArgs e)
        {
            DataLineControl fc = null;
            foreach (DataLineControl c in tableLayoutPanel3.Controls)
            {
                if (c.rbFirstSelect.Checked) fc = c;
            }
            if(fc == null)return;
            foreach (DataLineControl sc in tableLayoutPanel3.Controls)
            {
                if (sc.cbSecondSelect.Checked)
                {
                    DataLineControl newControl = new DataLineControl();
                    newControl.LineName = fc.LineName + " - " + sc.LineName;

                    tableLayoutPanel3.Controls.Add(newControl);
                    newControl.SendEvent += DataControl_SendEvent;

                    newControl.DataPixels.Clear();
                    bool isMaxI = false, isMaxJ = false;
                for (int i = 0, j=0;;)
                    {
                        var res = 0;
                        bool isUseFt = true;
                        var fTime = fc.DataPixels[i].Time;
                        var fVal = fc.DataPixels[i].Val;
                        var sTime = sc.DataPixels[j].Time;
                        var sVal = sc.DataPixels[j].Val;

                        if (isMaxJ || !isMaxI && fTime < sTime)
                        {
                            isUseFt = true;
                            res = fVal;
                            i++;
                        }
                        else if (isMaxI || !isMaxJ && fTime > sTime)
                        {
                            isUseFt = false;
                            res = -sVal;
                            j++;
                        }
                        else if (fTime == sTime)
                        {
                            res = fVal - sVal;
                            ++i;
                            ++j;
                        }
                        if (i >= fc.DataPixels.Count)
                        {
                            if (isMaxJ) break;
                            i = fc.DataPixels.Count - 1;
                            isMaxI = true;
                        }
                        if (j >= sc.DataPixels.Count)
                        {
                            if (isMaxI) break;
                            j = sc.DataPixels.Count - 1;
                            isMaxJ = true;
                        }

                        newControl.DataPixels.Add(new DataPixel() { Color = Color.Black, Time = isUseFt ? fTime: sTime, Val = res });
                    }

                }
            }
        }
    }
}
