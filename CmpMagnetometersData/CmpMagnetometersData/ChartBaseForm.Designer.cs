namespace CmpMagnetometersData
{
    partial class ChartBaseForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartBaseForm));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.btnSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpStartX = new System.Windows.Forms.DateTimePicker();
            this.chartControl = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblFileName = new System.Windows.Forms.Label();
            this.btnTurn = new System.Windows.Forms.Button();
            this.lblFileNameHid = new System.Windows.Forms.Label();
            this.btnReOpen = new System.Windows.Forms.Button();
            this.cbEnable = new System.Windows.Forms.CheckBox();
            this.btnCreate = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.Location = new System.Drawing.Point(3, 89);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(84, 23);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Сохранить";
            this.btnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Начало файла";
            // 
            // dtpStartX
            // 
            this.dtpStartX.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartX.Location = new System.Drawing.Point(3, 63);
            this.dtpStartX.Name = "dtpStartX";
            this.dtpStartX.ShowUpDown = true;
            this.dtpStartX.Size = new System.Drawing.Size(123, 20);
            this.dtpStartX.TabIndex = 0;
            this.dtpStartX.ValueChanged += new System.EventHandler(this.dtpStartX_ValueChanged);
            this.dtpStartX.Leave += new System.EventHandler(this.dtpStartX_Leave);
            // 
            // chartControl
            // 
            this.chartControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chartControl.ChartAreas.Add(chartArea1);
            this.chartControl.Location = new System.Drawing.Point(132, 3);
            this.chartControl.Name = "chartControl";
            this.chartControl.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Name = "Series1";
            this.chartControl.Series.Add(series1);
            this.chartControl.Size = new System.Drawing.Size(465, 194);
            this.chartControl.TabIndex = 6;
            this.chartControl.Text = "chart1";
            this.chartControl.MouseEnter += new System.EventHandler(this.chartControl_MouseEnter);
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(3, 29);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(51, 13);
            this.lblFileName.TabIndex = 7;
            this.lblFileName.Text = "FileName";
            // 
            // btnTurn
            // 
            this.btnTurn.Location = new System.Drawing.Point(4, 3);
            this.btnTurn.Name = "btnTurn";
            this.btnTurn.Size = new System.Drawing.Size(84, 23);
            this.btnTurn.TabIndex = 8;
            this.btnTurn.Text = "Свернуть";
            this.btnTurn.UseVisualStyleBackColor = true;
            this.btnTurn.Click += new System.EventHandler(this.btnTurn_Click);
            // 
            // lblFileNameHid
            // 
            this.lblFileNameHid.AutoSize = true;
            this.lblFileNameHid.Location = new System.Drawing.Point(94, 8);
            this.lblFileNameHid.Name = "lblFileNameHid";
            this.lblFileNameHid.Size = new System.Drawing.Size(21, 13);
            this.lblFileNameHid.TabIndex = 9;
            this.lblFileNameHid.Text = "hid";
            this.lblFileNameHid.Visible = false;
            // 
            // btnReOpen
            // 
            this.btnReOpen.Location = new System.Drawing.Point(3, 118);
            this.btnReOpen.Name = "btnReOpen";
            this.btnReOpen.Size = new System.Drawing.Size(83, 23);
            this.btnReOpen.TabIndex = 10;
            this.btnReOpen.Text = "Переоткрыть";
            this.btnReOpen.UseVisualStyleBackColor = true;
            this.btnReOpen.Click += new System.EventHandler(this.btnReOpen_Click);
            // 
            // cbEnable
            // 
            this.cbEnable.AutoSize = true;
            this.cbEnable.Checked = true;
            this.cbEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnable.Location = new System.Drawing.Point(4, 147);
            this.cbEnable.Name = "cbEnable";
            this.cbEnable.Size = new System.Drawing.Size(75, 17);
            this.cbEnable.TabIndex = 11;
            this.cbEnable.Text = "Включить";
            this.cbEnable.UseVisualStyleBackColor = true;
            this.cbEnable.CheckedChanged += new System.EventHandler(this.cbEnable_CheckedChanged);
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(3, 170);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(83, 23);
            this.btnCreate.TabIndex = 12;
            this.btnCreate.Text = "Создать";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // ChartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.cbEnable);
            this.Controls.Add(this.btnReOpen);
            this.Controls.Add(this.lblFileNameHid);
            this.Controls.Add(this.btnTurn);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.chartControl);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtpStartX);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(600, 200);
            this.Name = "ChartForm";
            this.Size = new System.Drawing.Size(600, 200);
            ((System.ComponentModel.ISupportInitialize)(this.chartControl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpStartX;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartControl;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Button btnTurn;
        private System.Windows.Forms.Label lblFileNameHid;
        private System.Windows.Forms.Button btnReOpen;
        private System.Windows.Forms.CheckBox cbEnable;
        private System.Windows.Forms.Button btnCreate;
    }
}
