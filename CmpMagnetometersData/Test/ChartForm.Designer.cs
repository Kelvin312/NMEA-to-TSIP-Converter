namespace Test
{
    partial class ChartForm
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chartControl = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cbEnable = new System.Windows.Forms.CheckBox();
            this.lblChartName = new System.Windows.Forms.Label();
            this.lblChartNameHide = new System.Windows.Forms.Label();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnReopen = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.dtpDate = new System.Windows.Forms.DateTimePicker();
            this.dtpTime = new System.Windows.Forms.DateTimePicker();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl)).BeginInit();
            this.SuspendLayout();
            // 
            // chartControl
            // 
            this.chartControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea6.Name = "main";
            this.chartControl.ChartAreas.Add(chartArea6);
            this.chartControl.Location = new System.Drawing.Point(99, 3);
            this.chartControl.Name = "chartControl";
            series6.ChartArea = "main";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series6.Name = "main";
            this.chartControl.Series.Add(series6);
            this.chartControl.Size = new System.Drawing.Size(498, 244);
            this.chartControl.TabIndex = 1;
            // 
            // cbEnable
            // 
            this.cbEnable.AutoSize = true;
            this.cbEnable.Checked = true;
            this.cbEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnable.Location = new System.Drawing.Point(3, 85);
            this.cbEnable.Name = "cbEnable";
            this.cbEnable.Size = new System.Drawing.Size(75, 17);
            this.cbEnable.TabIndex = 2;
            this.cbEnable.Text = "Включить";
            this.cbEnable.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.cbEnable.UseVisualStyleBackColor = true;
            this.cbEnable.CheckedChanged += new System.EventHandler(this.cbEnable_CheckedChanged);
            // 
            // lblChartName
            // 
            this.lblChartName.AutoEllipsis = true;
            this.lblChartName.Location = new System.Drawing.Point(3, 29);
            this.lblChartName.Name = "lblChartName";
            this.lblChartName.Size = new System.Drawing.Size(93, 53);
            this.lblChartName.TabIndex = 3;
            this.lblChartName.Text = "NoName123456789012345678901234567890123456789";
            // 
            // lblChartNameHide
            // 
            this.lblChartNameHide.AutoSize = true;
            this.lblChartNameHide.Location = new System.Drawing.Point(99, 8);
            this.lblChartNameHide.Name = "lblChartNameHide";
            this.lblChartNameHide.Size = new System.Drawing.Size(21, 13);
            this.lblChartNameHide.TabIndex = 4;
            this.lblChartNameHide.Text = "hid";
            this.lblChartNameHide.Visible = false;
            // 
            // btnCreate
            // 
            this.btnCreate.Image = global::Test.Properties.Resources.create;
            this.btnCreate.Location = new System.Drawing.Point(3, 160);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(90, 23);
            this.btnCreate.TabIndex = 6;
            this.btnCreate.Text = "Создать";
            this.btnCreate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCreate.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Image = global::Test.Properties.Resources.save;
            this.btnSave.Location = new System.Drawing.Point(3, 189);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 23);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Сохранить";
            this.btnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // btnReopen
            // 
            this.btnReopen.Image = global::Test.Properties.Resources.reload;
            this.btnReopen.Location = new System.Drawing.Point(3, 218);
            this.btnReopen.Name = "btnReopen";
            this.btnReopen.Size = new System.Drawing.Size(90, 23);
            this.btnReopen.TabIndex = 5;
            this.btnReopen.Text = "Переоткр.";
            this.btnReopen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnReopen.UseVisualStyleBackColor = true;
            // 
            // btnMinimize
            // 
            this.btnMinimize.Image = global::Test.Properties.Resources.minimize;
            this.btnMinimize.Location = new System.Drawing.Point(3, 3);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(90, 23);
            this.btnMinimize.TabIndex = 0;
            this.btnMinimize.Text = "Свернуть";
            this.btnMinimize.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // dtpDate
            // 
            this.dtpDate.CustomFormat = "dd/MM/yy hh:mm:ss";
            this.dtpDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDate.Location = new System.Drawing.Point(3, 108);
            this.dtpDate.Name = "dtpDate";
            this.dtpDate.ShowUpDown = true;
            this.dtpDate.Size = new System.Drawing.Size(90, 20);
            this.dtpDate.TabIndex = 8;
            this.dtpDate.ValueChanged += new System.EventHandler(this.dtp_ValueChanged);
            this.dtpDate.Leave += new System.EventHandler(this.dtp_Leave);
            // 
            // dtpTime
            // 
            this.dtpTime.CustomFormat = "dd/MM/yy HH:mm:ss";
            this.dtpTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpTime.Location = new System.Drawing.Point(3, 134);
            this.dtpTime.Name = "dtpTime";
            this.dtpTime.ShowUpDown = true;
            this.dtpTime.Size = new System.Drawing.Size(90, 20);
            this.dtpTime.TabIndex = 9;
            this.dtpTime.ValueChanged += new System.EventHandler(this.dtp_ValueChanged);
            this.dtpTime.Leave += new System.EventHandler(this.dtp_Leave);
            // 
            // ChartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dtpTime);
            this.Controls.Add(this.dtpDate);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.btnReopen);
            this.Controls.Add(this.lblChartNameHide);
            this.Controls.Add(this.lblChartName);
            this.Controls.Add(this.cbEnable);
            this.Controls.Add(this.chartControl);
            this.Controls.Add(this.btnMinimize);
            this.MinimumSize = new System.Drawing.Size(600, 250);
            this.Name = "ChartForm";
            this.Size = new System.Drawing.Size(600, 250);
            ((System.ComponentModel.ISupportInitialize)(this.chartControl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblChartNameHide;
        protected System.Windows.Forms.Button btnMinimize;
        protected System.Windows.Forms.DataVisualization.Charting.Chart chartControl;
        protected System.Windows.Forms.CheckBox cbEnable;
        protected System.Windows.Forms.Label lblChartName;
        protected System.Windows.Forms.Button btnReopen;
        protected System.Windows.Forms.Button btnCreate;
        protected System.Windows.Forms.Button btnSave;
        protected System.Windows.Forms.DateTimePicker dtpDate;
        protected System.Windows.Forms.DateTimePicker dtpTime;
    }
}
