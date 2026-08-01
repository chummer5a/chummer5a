namespace Chummer
{
    partial class SelectReputationSpend
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
            this.lstSpends = new System.Windows.Forms.ListBox();
            this.tlpRight = new System.Windows.Forms.TableLayoutPanel();
            this.lblNotes = new System.Windows.Forms.Label();
            this.lblTarget = new System.Windows.Forms.Label();
            this.cboTarget = new System.Windows.Forms.ComboBox();
            this.txtFaction = new System.Windows.Forms.TextBox();
            this.lblStreetCred = new System.Windows.Forms.Label();
            this.nudStreetCred = new Chummer.NumericUpDownEx();
            this.lblKarma = new System.Windows.Forms.Label();
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.lblDiscount = new System.Windows.Forms.Label();
            this.nudDiscount = new Chummer.NumericUpDownEx();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.tlpMain.SuspendLayout();
            this.tlpRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStreetCred)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiscount)).BeginInit();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // tlpMain
            //
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpMain.Controls.Add(this.lstSpends, 0, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(566, 343);
            this.tlpMain.TabIndex = 0;
            //
            // lstSpends
            //
            this.lstSpends.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSpends.FormattingEnabled = true;
            this.lstSpends.Location = new System.Drawing.Point(3, 3);
            this.lstSpends.Name = "lstSpends";
            this.lstSpends.Size = new System.Drawing.Size(220, 337);
            this.lstSpends.TabIndex = 0;
            this.lstSpends.SelectedIndexChanged += new System.EventHandler(this.lstSpends_SelectedIndexChanged);
            //
            // tlpRight
            //
            this.tlpRight.ColumnCount = 2;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Controls.Add(this.lblNotes, 0, 0);
            this.tlpRight.Controls.Add(this.lblTarget, 0, 1);
            this.tlpRight.Controls.Add(this.cboTarget, 1, 1);
            this.tlpRight.Controls.Add(this.txtFaction, 1, 1);
            this.tlpRight.Controls.Add(this.lblStreetCred, 0, 2);
            this.tlpRight.Controls.Add(this.nudStreetCred, 1, 2);
            this.tlpRight.Controls.Add(this.lblKarma, 0, 3);
            this.tlpRight.Controls.Add(this.nudKarma, 1, 3);
            this.tlpRight.Controls.Add(this.lblDiscount, 0, 4);
            this.tlpRight.Controls.Add(this.nudDiscount, 1, 4);
            this.tlpRight.Controls.Add(this.tlpButtons, 1, 5);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(229, 3);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 6;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.Size = new System.Drawing.Size(334, 337);
            this.tlpRight.TabIndex = 1;
            this.tlpRight.SetColumnSpan(this.lblNotes, 2);
            //
            // lblNotes
            //
            this.lblNotes.AutoSize = true;
            this.lblNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNotes.Location = new System.Drawing.Point(3, 6);
            this.lblNotes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblNotes.Name = "lblNotes";
            this.lblNotes.Size = new System.Drawing.Size(328, 160);
            this.lblNotes.TabIndex = 0;
            //
            // lblTarget
            //
            this.lblTarget.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(3, 178);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(39, 13);
            this.lblTarget.TabIndex = 1;
            this.lblTarget.Text = "Target";
            this.lblTarget.Visible = false;
            //
            // cboTarget
            //
            this.cboTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTarget.FormattingEnabled = true;
            this.cboTarget.Location = new System.Drawing.Point(90, 174);
            this.cboTarget.Name = "cboTarget";
            this.cboTarget.Size = new System.Drawing.Size(241, 21);
            this.cboTarget.TabIndex = 2;
            this.cboTarget.Visible = false;
            this.cboTarget.SelectedIndexChanged += new System.EventHandler(this.cboTarget_SelectedIndexChanged);
            //
            // txtFaction
            //
            this.txtFaction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFaction.Location = new System.Drawing.Point(90, 174);
            this.txtFaction.Name = "txtFaction";
            this.txtFaction.Size = new System.Drawing.Size(241, 20);
            this.txtFaction.TabIndex = 3;
            this.txtFaction.Visible = false;
            this.txtFaction.TextChanged += new System.EventHandler(this.txtFaction_TextChanged);
            //
            // lblStreetCred
            //
            this.lblStreetCred.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStreetCred.AutoSize = true;
            this.lblStreetCred.Location = new System.Drawing.Point(3, 204);
            this.lblStreetCred.Name = "lblStreetCred";
            this.lblStreetCred.Size = new System.Drawing.Size(61, 13);
            this.lblStreetCred.TabIndex = 4;
            this.lblStreetCred.Tag = "String_StreetCred";
            this.lblStreetCred.Text = "Street Cred";
            //
            // nudStreetCred
            //
            this.nudStreetCred.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudStreetCred.AutoSize = true;
            this.nudStreetCred.Location = new System.Drawing.Point(90, 201);
            this.nudStreetCred.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudStreetCred.Name = "nudStreetCred";
            this.nudStreetCred.Size = new System.Drawing.Size(60, 20);
            this.nudStreetCred.TabIndex = 5;
            //
            // lblKarma
            //
            this.lblKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblKarma.AutoSize = true;
            this.lblKarma.Location = new System.Drawing.Point(3, 230);
            this.lblKarma.Name = "lblKarma";
            this.lblKarma.Size = new System.Drawing.Size(37, 13);
            this.lblKarma.TabIndex = 6;
            this.lblKarma.Tag = "String_Karma";
            this.lblKarma.Text = "Karma";
            //
            // nudKarma
            //
            this.nudKarma.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudKarma.AutoSize = true;
            this.nudKarma.Location = new System.Drawing.Point(90, 227);
            this.nudKarma.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(60, 20);
            this.nudKarma.TabIndex = 7;
            //
            // lblDiscount
            //
            this.lblDiscount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDiscount.AutoSize = true;
            this.lblDiscount.Location = new System.Drawing.Point(3, 256);
            this.lblDiscount.Name = "lblDiscount";
            this.lblDiscount.Size = new System.Drawing.Size(49, 13);
            this.lblDiscount.TabIndex = 8;
            this.lblDiscount.Tag = "String_ReputationSpend_Discount";
            this.lblDiscount.Text = "Discount ¥";
            this.lblDiscount.Visible = false;
            //
            // nudDiscount
            //
            this.nudDiscount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudDiscount.AutoSize = true;
            this.nudDiscount.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudDiscount.Location = new System.Drawing.Point(90, 253);
            this.nudDiscount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudDiscount.Name = "nudDiscount";
            this.nudDiscount.Size = new System.Drawing.Size(100, 20);
            this.nudDiscount.TabIndex = 9;
            this.nudDiscount.Visible = false;
            this.nudDiscount.ValueChanged += new System.EventHandler(this.nudDiscount_ValueChanged);
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
            this.tlpButtons.Controls.Add(this.cmdOK, 1, 0);
            this.tlpButtons.Location = new System.Drawing.Point(162, 308);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(172, 29);
            this.tlpButtons.TabIndex = 10;
            //
            // cmdCancel
            //
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(80, 23);
            this.cmdCancel.TabIndex = 1;
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
            this.cmdOK.Location = new System.Drawing.Point(89, 3);
            this.cmdOK.MinimumSize = new System.Drawing.Size(80, 0);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(80, 23);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            //
            // SelectReputationSpend
            //
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectReputationSpend";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectReputationSpend";
            this.Text = "Spend Street Cred / Karma";
            this.Load += new System.EventHandler(this.SelectReputationSpend_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStreetCred)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiscount)).EndInit();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.ListBox lstSpends;
        private System.Windows.Forms.TableLayoutPanel tlpRight;
        private System.Windows.Forms.Label lblNotes;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.ComboBox cboTarget;
        private System.Windows.Forms.TextBox txtFaction;
        private System.Windows.Forms.Label lblStreetCred;
        private Chummer.NumericUpDownEx nudStreetCred;
        private System.Windows.Forms.Label lblKarma;
        private Chummer.NumericUpDownEx nudKarma;
        private System.Windows.Forms.Label lblDiscount;
        private Chummer.NumericUpDownEx nudDiscount;
        private System.Windows.Forms.TableLayoutPanel tlpButtons;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
    }
}
