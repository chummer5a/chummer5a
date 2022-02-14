namespace ChummerHub.Client.UI
{
    partial class frmSINnerVisibility
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
            this.bOk = new System.Windows.Forms.Button();
            this.ucSINnerVisibility1 = new ChummerHub.Client.UI.ucSINnerVisibility();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.ucSINnerVisibility1, 0, 0);
            this.tlpMain.Controls.Add(this.bOk, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(304, 281);
            this.tlpMain.TabIndex = 0;
            // 
            // bOk
            // 
            this.bOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.bOk.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.bOk, 2);
            this.bOk.Location = new System.Drawing.Point(114, 255);
            this.bOk.Name = "bOk";
            this.bOk.Size = new System.Drawing.Size(75, 23);
            this.bOk.TabIndex = 2;
            this.bOk.Text = "OK";
            this.bOk.UseVisualStyleBackColor = true;
            this.bOk.Click += new System.EventHandler(this.BOk_Click);
            // 
            // ucSINnerVisibility1
            // 
            this.ucSINnerVisibility1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.ucSINnerVisibility1, 2);
            this.ucSINnerVisibility1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucSINnerVisibility1.Location = new System.Drawing.Point(3, 3);
            this.ucSINnerVisibility1.MyVisibility = null;
            this.ucSINnerVisibility1.Name = "ucSINnerVisibility1";
            this.ucSINnerVisibility1.Size = new System.Drawing.Size(298, 246);
            this.ucSINnerVisibility1.TabIndex = 1;
            // 
            // frmSINnerVisibility
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 281);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Name = "frmSINnerVisibility";
            this.Text = "frmSINnerVisibility";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private ucSINnerVisibility ucSINnerVisibility1;
        private System.Windows.Forms.Button bOk;
    }
}
