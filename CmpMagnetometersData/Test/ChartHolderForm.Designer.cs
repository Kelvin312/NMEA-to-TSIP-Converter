namespace Test
{
    partial class ChartHolderForm
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
            this.tlbContent = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // tlbContent
            // 
            this.tlbContent.AutoSize = true;
            this.tlbContent.ColumnCount = 1;
            this.tlbContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlbContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlbContent.Location = new System.Drawing.Point(0, 0);
            this.tlbContent.Margin = new System.Windows.Forms.Padding(0);
            this.tlbContent.Name = "tlbContent";
            this.tlbContent.RowCount = 1;
            this.tlbContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlbContent.Size = new System.Drawing.Size(679, 293);
            this.tlbContent.TabIndex = 0;
            // 
            // ChartHolderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tlbContent);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ChartHolderForm";
            this.Size = new System.Drawing.Size(679, 293);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlbContent;
    }
}
