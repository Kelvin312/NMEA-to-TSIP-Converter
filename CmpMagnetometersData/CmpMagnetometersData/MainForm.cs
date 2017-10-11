using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
                    if (fp.DataControl != null) tableLayoutPanel3.Controls.Add(fp.DataControl);
                  
                    if (fp.DeviationControl != null) tableLayoutPanel3.Controls.Add(fp.DeviationControl);
                }
                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (DataLineControl c in tableLayoutPanel3.Controls)
            {
                c.UpdateChart(tableLayoutPanel2);
            }
        }
    }
}
