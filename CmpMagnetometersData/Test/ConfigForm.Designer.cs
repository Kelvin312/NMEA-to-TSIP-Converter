namespace Test
{
    partial class ConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.txtNormalColor = new System.Windows.Forms.TextBox();
            this.txtWarningColor = new System.Windows.Forms.TextBox();
            this.txtErrorColor = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtViewTimeText = new System.Windows.Forms.TextBox();
            this.txtViewTimeChart = new System.Windows.Forms.TextBox();
            this.txtViewTimeDtp = new System.Windows.Forms.TextBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(378, 253);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ок";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(459, 253);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // txtNormalColor
            // 
            this.txtNormalColor.Location = new System.Drawing.Point(98, 12);
            this.txtNormalColor.Name = "txtNormalColor";
            this.txtNormalColor.ReadOnly = true;
            this.txtNormalColor.Size = new System.Drawing.Size(31, 20);
            this.txtNormalColor.TabIndex = 2;
            this.txtNormalColor.Click += new System.EventHandler(this.txtColor_Click);
            // 
            // txtWarningColor
            // 
            this.txtWarningColor.Location = new System.Drawing.Point(98, 38);
            this.txtWarningColor.Name = "txtWarningColor";
            this.txtWarningColor.ReadOnly = true;
            this.txtWarningColor.Size = new System.Drawing.Size(31, 20);
            this.txtWarningColor.TabIndex = 3;
            this.txtWarningColor.Click += new System.EventHandler(this.txtColor_Click);
            // 
            // txtErrorColor
            // 
            this.txtErrorColor.Location = new System.Drawing.Point(98, 64);
            this.txtErrorColor.Name = "txtErrorColor";
            this.txtErrorColor.ReadOnly = true;
            this.txtErrorColor.Size = new System.Drawing.Size(31, 20);
            this.txtErrorColor.TabIndex = 4;
            this.txtErrorColor.Click += new System.EventHandler(this.txtColor_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Изм. Верн.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Изм. Предупр.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Крит.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(203, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(169, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Формат времени начала файла";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(203, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(142, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Формат времени графика";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(203, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(179, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Формат времени неверных точек";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 93);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(189, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Формат ASCII сохраняемого файла";
            // 
            // txtViewTimeText
            // 
            this.txtViewTimeText.Location = new System.Drawing.Point(395, 64);
            this.txtViewTimeText.Name = "txtViewTimeText";
            this.txtViewTimeText.Size = new System.Drawing.Size(138, 20);
            this.txtViewTimeText.TabIndex = 12;
            // 
            // txtViewTimeChart
            // 
            this.txtViewTimeChart.Location = new System.Drawing.Point(395, 38);
            this.txtViewTimeChart.Name = "txtViewTimeChart";
            this.txtViewTimeChart.Size = new System.Drawing.Size(138, 20);
            this.txtViewTimeChart.TabIndex = 13;
            // 
            // txtViewTimeDtp
            // 
            this.txtViewTimeDtp.Location = new System.Drawing.Point(395, 12);
            this.txtViewTimeDtp.Name = "txtViewTimeDtp";
            this.txtViewTimeDtp.Size = new System.Drawing.Size(138, 20);
            this.txtViewTimeDtp.TabIndex = 14;
            // 
            // textBox7
            // 
            this.textBox7.Location = new System.Drawing.Point(207, 90);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(326, 20);
            this.textBox7.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(546, 7);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(439, 273);
            this.label8.TabIndex = 16;
            this.label8.Text = resources.GetString("label8.Text");
            // 
            // ConfigForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(987, 288);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.txtViewTimeDtp);
            this.Controls.Add(this.txtViewTimeChart);
            this.Controls.Add(this.txtViewTimeText);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtErrorColor);
            this.Controls.Add(this.txtWarningColor);
            this.Controls.Add(this.txtNormalColor);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ConfigForm";
            this.Text = "ConfigForm";
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.TextBox txtNormalColor;
        private System.Windows.Forms.TextBox txtWarningColor;
        private System.Windows.Forms.TextBox txtErrorColor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtViewTimeText;
        private System.Windows.Forms.TextBox txtViewTimeChart;
        private System.Windows.Forms.TextBox txtViewTimeDtp;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.Label label8;
    }
}