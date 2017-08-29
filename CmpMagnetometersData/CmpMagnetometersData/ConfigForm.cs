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
            txtNormalColor.BackColor = Config.NormalColor;
            txtWarningColor.BackColor = Config.WarningColor;
            txtErrorColor.BackColor = Config.ErrorColor;
            txtViewTimeDtp.Text = Config.ViewTimeDtp;
            txtViewTimeChart.Text = Config.ViewTimeChart;
            txtViewTimeText.Text = Config.ViewTimeText;
        }

        public bool isColorChange = false;
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (Config.NormalColor != txtNormalColor.BackColor)
            {
                Config.NormalColor = txtNormalColor.BackColor;
                isColorChange |= true;
            }
            if (Config.WarningColor != txtWarningColor.BackColor)
            {
                Config.WarningColor = txtWarningColor.BackColor;
                isColorChange |= true;
            }
            if (Config.ErrorColor != txtErrorColor.BackColor)
            {
                Config.ErrorColor = txtErrorColor.BackColor;
                isColorChange |= true;
            }
            Config.ViewTimeDtp = txtViewTimeDtp.Text;
            Config.ViewTimeChart = txtViewTimeChart.Text;
            Config.ViewTimeText = txtViewTimeText.Text;

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
