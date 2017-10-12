namespace CmpMagnetometersData
{
    partial class DataLineControl
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbVisible = new System.Windows.Forms.CheckBox();
            this.rbFirstSelect = new System.Windows.Forms.RadioButton();
            this.cbSecondSelect = new System.Windows.Forms.CheckBox();
            this.txtDataLineName = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbVisible
            // 
            this.cbVisible.AutoSize = true;
            this.cbVisible.Location = new System.Drawing.Point(3, 4);
            this.cbVisible.Name = "cbVisible";
            this.cbVisible.Size = new System.Drawing.Size(15, 14);
            this.cbVisible.TabIndex = 0;
            this.cbVisible.UseVisualStyleBackColor = true;
            // 
            // rbFirstSelect
            // 
            this.rbFirstSelect.AutoSize = true;
            this.rbFirstSelect.Location = new System.Drawing.Point(33, 4);
            this.rbFirstSelect.Name = "rbFirstSelect";
            this.rbFirstSelect.Size = new System.Drawing.Size(14, 13);
            this.rbFirstSelect.TabIndex = 1;
            this.rbFirstSelect.UseVisualStyleBackColor = true;
            this.rbFirstSelect.CheckedChanged += new System.EventHandler(this.rbFirstSelect_CheckedChanged);
            // 
            // cbSecondSelect
            // 
            this.cbSecondSelect.AutoSize = true;
            this.cbSecondSelect.Location = new System.Drawing.Point(53, 4);
            this.cbSecondSelect.Name = "cbSecondSelect";
            this.cbSecondSelect.Size = new System.Drawing.Size(15, 14);
            this.cbSecondSelect.TabIndex = 2;
            this.cbSecondSelect.UseVisualStyleBackColor = true;
            // 
            // txtDataLineName
            // 
            this.txtDataLineName.Location = new System.Drawing.Point(74, 1);
            this.txtDataLineName.Name = "txtDataLineName";
            this.txtDataLineName.Size = new System.Drawing.Size(178, 20);
            this.txtDataLineName.TabIndex = 3;
            this.txtDataLineName.TextChanged += new System.EventHandler(this.txtDataLineName_TextChanged);
            // 
            // btnSave
            // 
            this.btnSave.Image = global::CmpMagnetometersData.Properties.Resources.save;
            this.btnSave.Location = new System.Drawing.Point(258, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(30, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // btnReload
            // 
            this.btnReload.Image = global::CmpMagnetometersData.Properties.Resources.reload;
            this.btnReload.Location = new System.Drawing.Point(294, 0);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(30, 23);
            this.btnReload.TabIndex = 5;
            this.btnReload.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnReload.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Image = global::CmpMagnetometersData.Properties.Resources.remove;
            this.btnDelete.Location = new System.Drawing.Point(330, 0);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(30, 23);
            this.btnDelete.TabIndex = 6;
            this.btnDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // DataLineControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtDataLineName);
            this.Controls.Add(this.cbSecondSelect);
            this.Controls.Add(this.rbFirstSelect);
            this.Controls.Add(this.cbVisible);
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "DataLineControl";
            this.Size = new System.Drawing.Size(365, 22);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbVisible;
        private System.Windows.Forms.TextBox txtDataLineName;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Button btnDelete;
        public System.Windows.Forms.RadioButton rbFirstSelect;
        public System.Windows.Forms.CheckBox cbSecondSelect;
    }
}
