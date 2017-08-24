namespace CmpMagnetometersData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartForm));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpStartX = new System.Windows.Forms.DateTimePicker();
            this.chartControl = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblFileName = new System.Windows.Forms.Label();
            this.btnTurn = new System.Windows.Forms.Button();
            this.lblFileNameHid = new System.Windows.Forms.Label();
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
            // 
            // btnDelete
            // 
            this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
            this.btnDelete.Location = new System.Drawing.Point(3, 118);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(85, 23);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "Убрать";
            this.btnDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
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
            this.dtpStartX.Checked = false;
            this.dtpStartX.CustomFormat = "yy.MM.dd/HH:mm:ss";
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
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series1.Name = "Series1";
            this.chartControl.Series.Add(series1);
            this.chartControl.Size = new System.Drawing.Size(465, 139);
            this.chartControl.TabIndex = 6;
            this.chartControl.Text = "chart1";
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
            // ChartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblFileNameHid);
            this.Controls.Add(this.btnTurn);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.chartControl);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtpStartX);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ChartForm";
            this.Size = new System.Drawing.Size(600, 145);
            this.Load += new System.EventHandler(this.ChartForm_Load);
            this.Resize += new System.EventHandler(this.ChartForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.chartControl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpStartX;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartControl;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Button btnTurn;
        private System.Windows.Forms.Label lblFileNameHid;
    }
}
