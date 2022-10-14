namespace Chummer.Forms
{
    sealed partial class ScrollableMessageBox
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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.imgIcon = new System.Windows.Forms.PictureBox();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.cmdButton3 = new System.Windows.Forms.Button();
            this.cmdButton2 = new System.Windows.Forms.Button();
            this.cmdButton1 = new System.Windows.Forms.Button();
            this.txtText = new System.Windows.Forms.TextBox();
            this.tlpTop = new System.Windows.Forms.TableLayoutPanel();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgIcon)).BeginInit();
            this.tlpButtons.SuspendLayout();
            this.tlpTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.tlpButtons, 0, 1);
            this.tlpMain.Controls.Add(this.tlpTop, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(464, 281);
            this.tlpMain.TabIndex = 0;
            // 
            // imgIcon
            // 
            this.imgIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.imgIcon.Location = new System.Drawing.Point(24, 33);
            this.imgIcon.Margin = new System.Windows.Forms.Padding(24, 33, 0, 9);
            this.imgIcon.Name = "imgIcon";
            this.imgIcon.Size = new System.Drawing.Size(100, 100);
            this.imgIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imgIcon.TabIndex = 2;
            this.imgIcon.TabStop = false;
            // 
            // tlpButtons
            // 
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 3;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.Controls.Add(this.cmdButton3, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdButton2, 1, 0);
            this.tlpButtons.Controls.Add(this.cmdButton1, 2, 0);
            this.tlpButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.tlpButtons.Location = new System.Drawing.Point(196, 241);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0, 6, 10, 11);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(258, 29);
            this.tlpButtons.TabIndex = 3;
            // 
            // cmdButton3
            // 
            this.cmdButton3.AutoSize = true;
            this.cmdButton3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdButton3.Location = new System.Drawing.Point(3, 3);
            this.cmdButton3.Name = "cmdButton3";
            this.cmdButton3.Size = new System.Drawing.Size(80, 23);
            this.cmdButton3.TabIndex = 2;
            this.cmdButton3.Text = "OK";
            this.cmdButton3.UseVisualStyleBackColor = true;
            this.cmdButton3.Click += new System.EventHandler(this.cmdButton3_Click);
            // 
            // cmdButton2
            // 
            this.cmdButton2.AutoSize = true;
            this.cmdButton2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdButton2.Location = new System.Drawing.Point(89, 3);
            this.cmdButton2.Name = "cmdButton2";
            this.cmdButton2.Size = new System.Drawing.Size(80, 23);
            this.cmdButton2.TabIndex = 1;
            this.cmdButton2.Text = "OK";
            this.cmdButton2.UseVisualStyleBackColor = true;
            this.cmdButton2.Click += new System.EventHandler(this.cmdButton2_Click);
            // 
            // cmdButton1
            // 
            this.cmdButton1.AutoSize = true;
            this.cmdButton1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdButton1.Location = new System.Drawing.Point(175, 3);
            this.cmdButton1.Name = "cmdButton1";
            this.cmdButton1.Size = new System.Drawing.Size(80, 23);
            this.cmdButton1.TabIndex = 0;
            this.cmdButton1.Text = "OK";
            this.cmdButton1.UseVisualStyleBackColor = true;
            this.cmdButton1.Click += new System.EventHandler(this.cmdButton1_Click);
            // 
            // txtText
            // 
            this.txtText.BackColor = System.Drawing.SystemColors.Window;
            this.txtText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtText.Location = new System.Drawing.Point(140, 33);
            this.txtText.Margin = new System.Windows.Forms.Padding(16, 33, 6, 9);
            this.txtText.MaxLength = 2147483647;
            this.txtText.Multiline = true;
            this.txtText.Name = "txtText";
            this.txtText.ReadOnly = true;
            this.txtText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtText.Size = new System.Drawing.Size(318, 193);
            this.txtText.TabIndex = 6;
            this.txtText.TabStop = false;
            // 
            // tlpTop
            // 
            this.tlpTop.AutoSize = true;
            this.tlpTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTop.BackColor = System.Drawing.SystemColors.Window;
            this.tlpTop.ColumnCount = 2;
            this.tlpTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTop.Controls.Add(this.imgIcon, 0, 0);
            this.tlpTop.Controls.Add(this.txtText, 1, 0);
            this.tlpTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTop.Location = new System.Drawing.Point(0, 0);
            this.tlpTop.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTop.Name = "tlpTop";
            this.tlpTop.RowCount = 1;
            this.tlpTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTop.Size = new System.Drawing.Size(464, 235);
            this.tlpTop.TabIndex = 7;
            // 
            // ScrollableMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScrollableMessageBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgIcon)).EndInit();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpTop.ResumeLayout(false);
            this.tlpTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.PictureBox imgIcon;
        private System.Windows.Forms.Button cmdButton1;
        private System.Windows.Forms.Button cmdButton2;
        private System.Windows.Forms.Button cmdButton3;
        private System.Windows.Forms.TableLayoutPanel tlpButtons;
        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.TableLayoutPanel tlpTop;
    }
}
