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

                    newControl.DataPixels.Clear();

                    int i = 0, j = 0;
                    while (true)
                    {
                        var fTime = fc.DataPixels[i].Time;
                        var fVal = fc.DataPixels[i].Val;
                        var sTime = sc.DataPixels[j].Time;
                        var sVal = sc.DataPixels[j].Val;
                        if (fTime < sTime)
                        {
                            ++i;
                        }
                        else if (fTime > sTime)
                        {
                            ++j;
                        }
                        else
                        {
                            ++i; ++j;
                            newControl.DataPixels.Add(new DataPixel() { Color = Color.DarkGreen, Time = fTime, Val = fVal - sVal });
                        }
                        if (i >= fc.DataPixels.Count || j >= sc.DataPixels.Count) break;
                    }

                    if (newControl.DataPixels.Count > 1)
                    {
                        tableLayoutPanel3.Controls.Add(newControl);
                        newControl.SendEvent += DataControl_SendEvent;
                    }

                    //    bool isMaxI = false, isMaxJ = false;
                    //for (int i = 0, j=0;;)
                    //    {
                    //        var res = 0;
                    //        bool isUseFt = true;
                    //        var fTime = fc.DataPixels[i].Time;
                    //        var fVal = fc.DataPixels[i].Val;
                    //        var sTime = sc.DataPixels[j].Time;
                    //        var sVal = sc.DataPixels[j].Val;

                    //        if (isMaxJ || !isMaxI && fTime < sTime)
                    //        {
                    //            isUseFt = true;
                    //            res = fVal;
                    //            i++;
                    //        }
                    //        else if (isMaxI || !isMaxJ && fTime > sTime)
                    //        {
                    //            isUseFt = false;
                    //            res = -sVal;
                    //            j++;
                    //        }
                    //        else if (fTime == sTime)
                    //        {
                    //            res = fVal - sVal;
                    //            ++i;
                    //            ++j;
                    //        }
                    //        if (i >= fc.DataPixels.Count)
                    //        {
                    //            if (isMaxJ) break;
                    //            i = fc.DataPixels.Count - 1;
                    //            isMaxI = true;
                    //        }
                    //        if (j >= sc.DataPixels.Count)
                    //        {
                    //            if (isMaxI) break;
                    //            j = sc.DataPixels.Count - 1;
                    //            isMaxJ = true;
                    //        }

                    //        newControl.DataPixels.Add(new DataPixel() { Color = Color.DarkGreen, Time = isUseFt ? fTime: sTime, Val = res });
                    //    }

                }
            }
        }

        private void btnDistribution_Click(object sender, EventArgs e)
        {
            DataLineControl fc = null;
            foreach (DataLineControl c in tableLayoutPanel3.Controls)
            {
                if (c.rbFirstSelect.Checked) fc = c;
            }
            int steps;
            if (fc == null || !int.TryParse(txtDistribution.Text, out steps) || steps < 2) return;

            DistributionLineControl dlc = new DistributionLineControl();
            dlc.CreateDistribution(fc.DataPixels,steps);

            dlc.LineName = fc.LineName + " Распр.";

            tableLayoutPanel3.Controls.Add(dlc);
            //dlc.SendEvent += DataControl_SendEvent;
        }

        private void btnCorrelation_Click(object sender, EventArgs e)
        {
            DataLineControl fc = null;
            foreach (DataLineControl c in tableLayoutPanel3.Controls)
            {
                if (c.rbFirstSelect.Checked) fc = c;
            }
            if (fc == null) return;
            StringBuilder resList = new StringBuilder();

            foreach (DataLineControl sc in tableLayoutPanel3.Controls)
            {
                if (sc.cbSecondSelect.Checked)
                {
                    int i = 0, j = 0;
                    int count = 0;
                    double fsum = 0, ssum = 0, mulsum = 0, fsqsum = 0, ssqsum = 0;


                    while (true)
                    {
                        var fTime = fc.DataPixels[i].Time;
                        var fVal = fc.DataPixels[i].Val;
                        var sTime = sc.DataPixels[j].Time;
                        var sVal = sc.DataPixels[j].Val;
                        if (fTime < sTime)
                        {
                           ++i;
                        }
                        else if (fTime > sTime)
                        {
                            ++j;
                        }
                        else
                        {
                            ++i; ++j;
                            count++;
                            fsum += fVal;
                            ssum += sVal;
                            mulsum += fVal * 1L * sVal;
                            fsqsum += fVal * 1L * fVal;
                            ssqsum += sVal * 1L * sVal;
                        }

                        if(i >= fc.DataPixels.Count || j >= sc.DataPixels.Count) break;
                    }
                    if (count > 1)
                    {
                        var res = (count * mulsum - fsum * ssum) /
                                  Math.Sqrt((count * fsqsum - fsum * fsum) * (count * ssqsum - ssum * ssum));
                        resList.AppendFormat("{0:F}\r\n", res);
                    }

                }
            }

            MessageBox.Show(resList.ToString());
        }
    }
}
