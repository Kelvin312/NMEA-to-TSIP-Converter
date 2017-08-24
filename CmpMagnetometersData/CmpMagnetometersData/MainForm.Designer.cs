namespace CmpMagnetometersData
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnAdd = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnMagneticField = new System.Windows.Forms.ToolStripButton();
            this.btnRmsDeviation = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnConfig = new System.Windows.Forms.ToolStripButton();
            this.ofdAddFile = new System.Windows.Forms.OpenFileDialog();
            this.tlbContent = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAdd,
            this.toolStripSeparator1,
            this.btnMagneticField,
            this.btnRmsDeviation,
            this.toolStripSeparator2,
            this.btnConfig});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1002, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnAdd
            // 
            this.btnAdd.Image = ((System.Drawing.Image)(resources.GetObject("btnAdd.Image")));
            this.btnAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(79, 22);
            this.btnAdd.Text = "Добавить";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnMagneticField
            // 
            this.btnMagneticField.Checked = true;
            this.btnMagneticField.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnMagneticField.Image = ((System.Drawing.Image)(resources.GetObject("btnMagneticField.Image")));
            this.btnMagneticField.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMagneticField.Name = "btnMagneticField";
            this.btnMagneticField.Size = new System.Drawing.Size(118, 22);
            this.btnMagneticField.Text = "Магнитное поле";
            this.btnMagneticField.Click += new System.EventHandler(this.btnMagneticField_Click);
            // 
            // btnRmsDeviation
            // 
            this.btnRmsDeviation.Image = ((System.Drawing.Image)(resources.GetObject("btnRmsDeviation.Image")));
            this.btnRmsDeviation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRmsDeviation.Name = "btnRmsDeviation";
            this.btnRmsDeviation.Size = new System.Drawing.Size(51, 22);
            this.btnRmsDeviation.Text = "СКО";
            this.btnRmsDeviation.Click += new System.EventHandler(this.btnRmsDeviation_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnConfig
            // 
            this.btnConfig.Image = ((System.Drawing.Image)(resources.GetObject("btnConfig.Image")));
            this.btnConfig.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(87, 22);
            this.btnConfig.Text = "Настройки";
            // 
            // ofdAddFile
            // 
            this.ofdAddFile.Filter = "txt|*.txt";
            this.ofdAddFile.Multiselect = true;
            this.ofdAddFile.Title = "Добавить файл";
            // 
            // tlbContent
            // 
            this.tlbContent.ColumnCount = 1;
            this.tlbContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlbContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlbContent.Location = new System.Drawing.Point(0, 25);
            this.tlbContent.Name = "tlbContent";
            this.tlbContent.RowCount = 1;
            this.tlbContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlbContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlbContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlbContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlbContent.Size = new System.Drawing.Size(1002, 436);
            this.tlbContent.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1002, 461);
            this.Controls.Add(this.tlbContent);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Сравнение данных магнитометров";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnAdd;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnMagneticField;
        private System.Windows.Forms.ToolStripButton btnRmsDeviation;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnConfig;
        private System.Windows.Forms.OpenFileDialog ofdAddFile;
        private System.Windows.Forms.TableLayoutPanel tlbContent;
    }
}

