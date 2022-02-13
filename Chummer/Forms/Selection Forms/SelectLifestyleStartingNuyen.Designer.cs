namespace Chummer
{
    partial class SelectLifestyleStartingNuyen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectLifestyleStartingNuyen));
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblResult = new System.Windows.Forms.Label();
            this.lblDice = new System.Windows.Forms.Label();
            this.lblSelectLifestyle = new System.Windows.Forms.Label();
            this.cmdRoll = new System.Windows.Forms.Button();
            this.nudDiceResult = new Chummer.NumericUpDownEx();
            this.cboSelectLifestyle = new Chummer.ElasticComboBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).BeginInit();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 4;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.lblResult, 3, 2);
            this.tlpMain.Controls.Add(this.lblDice, 0, 2);
            this.tlpMain.Controls.Add(this.lblSelectLifestyle, 0, 1);
            this.tlpMain.Controls.Add(this.cmdRoll, 1, 2);
            this.tlpMain.Controls.Add(this.nudDiceResult, 2, 2);
            this.tlpMain.Controls.Add(this.cboSelectLifestyle, 1, 1);
            this.tlpMain.Controls.Add(this.lblDescription, 0, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 0, 3);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(326, 143);
            this.tlpMain.TabIndex = 5;
            // 
            // lblResult
            // 
            this.lblResult.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(204, 92);
            this.lblResult.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(37, 13);
            this.lblResult.TabIndex = 3;
            this.lblResult.Tag = "Label_LifestyleNuyen_ResultOf";
            this.lblResult.Text = "Result";
            // 
            // lblDice
            // 
            this.lblDice.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDice.AutoSize = true;
            this.lblDice.Location = new System.Drawing.Point(40, 92);
            this.lblDice.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDice.Name = "lblDice";
            this.lblDice.Size = new System.Drawing.Size(81, 13);
            this.lblDice.TabIndex = 1;
            this.lblDice.Tag = "Label_LifestyleNuyen_ResultOf";
            this.lblDice.Text = "Result of 4D6: (";
            // 
            // lblSelectLifestyle
            // 
            this.lblSelectLifestyle.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSelectLifestyle.AutoSize = true;
            this.lblSelectLifestyle.Location = new System.Drawing.Point(39, 64);
            this.lblSelectLifestyle.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSelectLifestyle.Name = "lblSelectLifestyle";
            this.lblSelectLifestyle.Size = new System.Drawing.Size(82, 13);
            this.lblSelectLifestyle.TabIndex = 6;
            this.lblSelectLifestyle.Tag = "Label_LifestyleNuyen_SelectLifestyle";
            this.lblSelectLifestyle.Text = "Lifestyle to Use:";
            // 
            // cmdRoll
            // 
            this.cmdRoll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRoll.AutoSize = true;
            this.cmdRoll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRoll.Image = global::Chummer.Properties.Resources.die;
            this.cmdRoll.Location = new System.Drawing.Point(127, 87);
            this.cmdRoll.Name = "cmdRoll";
            this.cmdRoll.Padding = new System.Windows.Forms.Padding(1);
            this.cmdRoll.Size = new System.Drawing.Size(24, 24);
            this.cmdRoll.TabIndex = 120;
            this.cmdRoll.UseVisualStyleBackColor = true;
            this.cmdRoll.Click += new System.EventHandler(this.cmdRoll_Click);
            // 
            // nudDiceResult
            // 
            this.nudDiceResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudDiceResult.AutoSize = true;
            this.nudDiceResult.Location = new System.Drawing.Point(157, 89);
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
            // cboSelectLifestyle
            // 
            this.cboSelectLifestyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.SetColumnSpan(this.cboSelectLifestyle, 3);
            this.cboSelectLifestyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSelectLifestyle.FormattingEnabled = true;
            this.cboSelectLifestyle.Location = new System.Drawing.Point(127, 60);
            this.cboSelectLifestyle.Name = "cboSelectLifestyle";
            this.cboSelectLifestyle.Size = new System.Drawing.Size(196, 21);
            this.cboSelectLifestyle.TabIndex = 5;
            this.cboSelectLifestyle.TooltipText = "";
            this.cboSelectLifestyle.SelectedIndexChanged += new System.EventHandler(this.cboSelectLifestyle_SelectionChanged);
            // 
            // lblDescription
            // 
            this.tlpMain.SetColumnSpan(this.lblDescription, 4);
            this.lblDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescription.Location = new System.Drawing.Point(3, 6);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(320, 45);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Tag = "Label_LifestyleNuyen_Description";
            this.lblDescription.Text = "Roll the number of dice shown below and enter the result to determine your charac" +
    "ter\'s starting Nuyen amount.";
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpMain.SetColumnSpan(this.tlpButtons, 4);
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(214, 114);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Size = new System.Drawing.Size(112, 29);
            this.tlpButtons.TabIndex = 121;
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(50, 23);
            this.cmdCancel.TabIndex = 18;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(59, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(50, 23);
            this.cmdOK.TabIndex = 17;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // frmLifestyleNuyen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(344, 161);
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
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).EndInit();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblDice;
        private System.Windows.Forms.Label lblResult;
        private Chummer.NumericUpDownEx nudDiceResult;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private ElasticComboBox cboSelectLifestyle;
        private System.Windows.Forms.Label lblSelectLifestyle;
        private System.Windows.Forms.Button cmdRoll;
        private BufferedTableLayoutPanel tlpButtons;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
    }
}
