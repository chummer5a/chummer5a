namespace Chummer
{
    partial class frmLifestyleNuyen
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLifestyleNuyen));
            this.lblDescription = new System.Windows.Forms.Label();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblDice = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.nudDiceResult = new Chummer.NumericUpDownEx();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cboSelectLifestyle = new Chummer.ElasticComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDescription
            // 
            this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDescription.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblDescription, 3);
            this.lblDescription.Location = new System.Drawing.Point(3, 6);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(360, 26);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Tag = "Label_LifestyleNuyen_Description";
            this.lblDescription.Text = "Roll the number of dice shown below and enter the result to determine your charac" +
    "ter\'s starting Nuyen amount.";
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(162, 94);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(41, 23);
            this.cmdOK.TabIndex = 4;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblDice
            // 
            this.lblDice.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDice.AutoSize = true;
            this.lblDice.Location = new System.Drawing.Point(75, 71);
            this.lblDice.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDice.Name = "lblDice";
            this.lblDice.Size = new System.Drawing.Size(81, 13);
            this.lblDice.TabIndex = 1;
            this.lblDice.Tag = "Label_LifestyleNuyen_ResultOf";
            this.lblDice.Text = "Result of 4D6: (";
            this.lblDice.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblResult
            // 
            this.lblResult.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(209, 71);
            this.lblResult.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(37, 13);
            this.lblResult.TabIndex = 3;
            this.lblResult.Tag = "Label_LifestyleNuyen_ResultOf";
            this.lblResult.Text = "Result";
            // 
            // nudDiceResult
            // 
            this.nudDiceResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudDiceResult.AutoSize = true;
            this.nudDiceResult.Location = new System.Drawing.Point(162, 68);
            this.nudDiceResult.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudDiceResult.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudDiceResult.Name = "nudDiceResult";
            this.nudDiceResult.Size = new System.Drawing.Size(41, 20);
            this.nudDiceResult.TabIndex = 2;
            this.nudDiceResult.ValueChanged += new System.EventHandler(this.nudDiceResult_ValueChanged);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.lblResult, 2, 2);
            this.tlpMain.Controls.Add(this.nudDiceResult, 1, 2);
            this.tlpMain.Controls.Add(this.lblDescription, 0, 0);
            this.tlpMain.Controls.Add(this.lblDice, 0, 2);
            this.tlpMain.Controls.Add(this.cmdOK, 1, 3);
            this.tlpMain.Controls.Add(this.cboSelectLifestyle, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(366, 120);
            this.tlpMain.TabIndex = 5;
            // 
            // cboSelectLifestyle
            // 
            this.cboSelectLifestyle.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tlpMain.SetColumnSpan(this.cboSelectLifestyle, 3);
            this.cboSelectLifestyle.FormattingEnabled = true;
            this.cboSelectLifestyle.Location = new System.Drawing.Point(100, 41);
            this.cboSelectLifestyle.Name = "cboSelectLifestyle";
            this.cboSelectLifestyle.Size = new System.Drawing.Size(165, 21);
            this.cboSelectLifestyle.TabIndex = 5;
            this.cboSelectLifestyle.TooltipText = "";
            this.cboSelectLifestyle.SelectedIndexChanged += new System.EventHandler(this.cboSelectLifestyle_SelectionChanged);
            // 
            // frmLifestyleNuyen
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(384, 138);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLifestyleNuyen";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_LifestyleNuyen";
            this.Text = "Starting Nuyen";
            this.Load += new System.EventHandler(this.frmLifestyleNuyen_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label lblDice;
        private System.Windows.Forms.Label lblResult;
        private Chummer.NumericUpDownEx nudDiceResult;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private ElasticComboBox cboSelectLifestyle;
    }
}
