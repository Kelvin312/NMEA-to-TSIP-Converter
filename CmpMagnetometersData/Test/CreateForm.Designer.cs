namespace CmpMagnetometersData
{
    partial class CreateForm
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
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbB = new System.Windows.Forms.ComboBox();
            this.txtX = new System.Windows.Forms.TextBox();
            this.txtA = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(95, 120);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ок";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(176, 120);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "Распределение",
            "A - B",
            "A * x"});
            this.cmbType.Location = new System.Drawing.Point(88, 12);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(163, 21);
            this.cmbType.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Тип графика";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "A";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "B";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(12, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "x";
            // 
            // cmbB
            // 
            this.cmbB.FormattingEnabled = true;
            this.cmbB.Location = new System.Drawing.Point(88, 65);
            this.cmbB.Name = "cmbB";
            this.cmbB.Size = new System.Drawing.Size(163, 21);
            this.cmbB.TabIndex = 8;
            // 
            // txtX
            // 
            this.txtX.Location = new System.Drawing.Point(88, 92);
            this.txtX.Name = "txtX";
            this.txtX.Size = new System.Drawing.Size(163, 20);
            this.txtX.TabIndex = 9;
            // 
            // txtA
            // 
            this.txtA.Enabled = false;
            this.txtA.Location = new System.Drawing.Point(88, 39);
            this.txtA.Name = "txtA";
            this.txtA.Size = new System.Drawing.Size(163, 20);
            this.txtA.TabIndex = 10;
            // 
            // CreateForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(263, 155);
            this.Controls.Add(this.txtA);
            this.Controls.Add(this.txtX);
            this.Controls.Add(this.cmbB);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CreateForm";
            this.Text = "CreateForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.ComboBox cmbType;
        public System.Windows.Forms.ComboBox cmbB;
        public System.Windows.Forms.TextBox txtX;
        public System.Windows.Forms.TextBox txtA;
    }
}