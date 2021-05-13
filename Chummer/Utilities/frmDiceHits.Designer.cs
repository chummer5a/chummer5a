namespace Chummer
{
    partial class frmDiceHits
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
            this.nudDiceResult = new Chummer.NumericUpDownEx();
            this.lblDice = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.cmdCancel = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.cmdRoll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudDiceResult
            // 
            this.nudDiceResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudDiceResult.Location = new System.Drawing.Point(153, 89);
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
            this.nudDiceResult.Size = new System.Drawing.Size(50, 20);
            this.nudDiceResult.TabIndex = 2;
            // 
            // lblDice
            // 
            this.lblDice.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDice.AutoSize = true;
            this.lblDice.Location = new System.Drawing.Point(51, 92);
            this.lblDice.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDice.Name = "lblDice";
            this.lblDice.Size = new System.Drawing.Size(66, 13);
            this.lblDice.TabIndex = 1;
            this.lblDice.Text = "Hits on 4D6:";
            this.lblDice.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.lblDescription, 4);
            this.lblDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescription.Location = new System.Drawing.Point(3, 6);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(320, 72);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Text = "[Text Goes Here]";
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
            this.tlpMain.Controls.Add(this.nudDiceResult, 2, 1);
            this.tlpMain.Controls.Add(this.lblDescription, 0, 0);
            this.tlpMain.Controls.Add(this.lblDice, 0, 1);
            this.tlpMain.Controls.Add(this.tlpButtons, 3, 2);
            this.tlpMain.Controls.Add(this.cmdRoll, 1, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(326, 143);
            this.tlpMain.TabIndex = 5;
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 2;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.button1, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(214, 114);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Size = new System.Drawing.Size(112, 29);
            this.tlpButtons.TabIndex = 122;
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
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.Location = new System.Drawing.Point(59, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(50, 23);
            this.button1.TabIndex = 17;
            this.button1.Tag = "String_OK";
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdRoll
            // 
            this.cmdRoll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRoll.AutoSize = true;
            this.cmdRoll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRoll.Image = global::Chummer.Properties.Resources.die;
            this.cmdRoll.Location = new System.Drawing.Point(123, 87);
            this.cmdRoll.Name = "cmdRoll";
            this.cmdRoll.Padding = new System.Windows.Forms.Padding(1);
            this.cmdRoll.Size = new System.Drawing.Size(24, 24);
            this.cmdRoll.TabIndex = 123;
            this.cmdRoll.UseVisualStyleBackColor = true;
            this.cmdRoll.Click += new System.EventHandler(this.cmdRoll_Click);
            // 
            // frmDiceHits
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(344, 161);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDiceHits";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_DiceHits";
            this.Text = "Dice Hits";
            this.Load += new System.EventHandler(this.frmDiceHits_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudDiceResult)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Chummer.NumericUpDownEx nudDiceResult;
        private System.Windows.Forms.Label lblDice;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private BufferedTableLayoutPanel tlpButtons;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button cmdRoll;
    }
}