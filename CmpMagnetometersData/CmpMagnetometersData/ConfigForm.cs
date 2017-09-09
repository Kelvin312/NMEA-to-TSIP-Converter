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
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            txtNormalColor.BackColor = Properties.Settings.Default.NormalColor;
            txtWarningColor.BackColor = Properties.Settings.Default.WarningColor;
            txtErrorColor.BackColor = Properties.Settings.Default.ErrorColor;
            txtViewTimeDtp.Text = Properties.Settings.Default.ViewTimeDtp;
            txtViewTimeChart.Text = Properties.Settings.Default.ViewTimeChart;
            txtViewTimeText.Text = Properties.Settings.Default.ViewTimeText;
        }

        public bool IsColorChange = false;
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.NormalColor != txtNormalColor.BackColor)
            {
                Properties.Settings.Default.NormalColor = txtNormalColor.BackColor;
                IsColorChange = true;
            }
            if (Properties.Settings.Default.WarningColor != txtWarningColor.BackColor)
            {
                Properties.Settings.Default.WarningColor = txtWarningColor.BackColor;
                IsColorChange = true;
            }
            if (Properties.Settings.Default.ErrorColor != txtErrorColor.BackColor)
            {
                Properties.Settings.Default.ErrorColor = txtErrorColor.BackColor;
                IsColorChange = true;
            }
            Properties.Settings.Default.ViewTimeDtp = txtViewTimeDtp.Text;
            Properties.Settings.Default.ViewTimeChart = txtViewTimeChart.Text;
            Properties.Settings.Default.ViewTimeText = txtViewTimeText.Text;

            this.DialogResult = DialogResult.OK;
        }

        private void txtColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = (sender as TextBox).BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                (sender as TextBox).BackColor = colorDialog1.Color;
            }
        }
    }
}
