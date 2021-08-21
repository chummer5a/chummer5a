namespace Chummer
{
    public sealed partial class InitiativeUserControl
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
            if (disposing)
            {
                components?.Dispose();
                foreach (Character objCharacter in _lstCharacters)
                    objCharacter.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.btnNext = new System.Windows.Forms.Button();
            this.btnSort = new System.Windows.Forms.Button();
            this.btnDelay = new System.Windows.Forms.Button();
            this.chkBoxChummer = new System.Windows.Forms.CheckedListBox();
            this.tlpLeft = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblRound = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lblInitiativeManeuvers = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnMinusInit1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnAdd1Init = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnMinus5Init = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.btnAdd5Init = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnMinus10Init = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.btnAdd10Init = new System.Windows.Forms.Button();
            this.cboManeuvers = new Chummer.ElasticComboBox();
            this.btnApplyInterrupt = new System.Windows.Forms.Button();
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpMain.SuspendLayout();
            this.tlpLeft.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.tlpLeft, 0, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(420, 265);
            this.tlpMain.TabIndex = 1;
            // 
            // btnNext
            // 
            this.btnNext.AutoSize = true;
            this.btnNext.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNext.Location = new System.Drawing.Point(3, 239);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(60, 23);
            this.btnNext.TabIndex = 0;
            this.btnNext.Tag = "Button_Next";
            this.btnNext.Text = "{Next}";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnSort
            // 
            this.btnSort.AutoSize = true;
            this.btnSort.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSort.Location = new System.Drawing.Point(69, 239);
            this.btnSort.Name = "btnSort";
            this.btnSort.Size = new System.Drawing.Size(60, 23);
            this.btnSort.TabIndex = 1;
            this.btnSort.Tag = "Button_Sort";
            this.btnSort.Text = "{Sort}";
            this.btnSort.UseVisualStyleBackColor = true;
            this.btnSort.Click += new System.EventHandler(this.btnSort_Click);
            // 
            // btnDelay
            // 
            this.btnDelay.AutoSize = true;
            this.btnDelay.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDelay.Location = new System.Drawing.Point(135, 239);
            this.btnDelay.Name = "btnDelay";
            this.btnDelay.Size = new System.Drawing.Size(62, 23);
            this.btnDelay.TabIndex = 2;
            this.btnDelay.Tag = "Button_Delay";
            this.btnDelay.Text = "{Delay}";
            this.btnDelay.UseVisualStyleBackColor = true;
            this.btnDelay.Click += new System.EventHandler(this.btnDelay_Click);
            // 
            // chkBoxChummer
            // 
            this.chkBoxChummer.BackColor = System.Drawing.SystemColors.Window;
            this.tlpLeft.SetColumnSpan(this.chkBoxChummer, 3);
            this.chkBoxChummer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkBoxChummer.FormattingEnabled = true;
            this.chkBoxChummer.Location = new System.Drawing.Point(3, 3);
            this.chkBoxChummer.Name = "chkBoxChummer";
            this.chkBoxChummer.Size = new System.Drawing.Size(194, 230);
            this.chkBoxChummer.TabIndex = 6;
            this.chkBoxChummer.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chkBoxChummer_MouseClick);
            this.chkBoxChummer.SelectedIndexChanged += new System.EventHandler(this.listBoxChummers_SelectedIndexChanged);
            // 
            // tlpLeft
            // 
            this.tlpLeft.ColumnCount = 3;
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpLeft.Controls.Add(this.btnDelay, 2, 1);
            this.tlpLeft.Controls.Add(this.btnSort, 1, 1);
            this.tlpLeft.Controls.Add(this.chkBoxChummer, 0, 0);
            this.tlpLeft.Controls.Add(this.btnNext, 0, 1);
            this.tlpLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLeft.Location = new System.Drawing.Point(0, 0);
            this.tlpLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLeft.Name = "tlpLeft";
            this.tlpLeft.RowCount = 2;
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLeft.Size = new System.Drawing.Size(200, 265);
            this.tlpLeft.TabIndex = 7;
            // 
            // lblRound
            // 
            this.lblRound.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblRound.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblRound, 2);
            this.lblRound.Location = new System.Drawing.Point(28, 244);
            this.lblRound.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRound.Name = "lblRound";
            this.lblRound.Size = new System.Drawing.Size(54, 13);
            this.lblRound.TabIndex = 0;
            this.lblRound.Tag = "Label_Round";
            this.lblRound.Text = "{Round#}";
            this.lblRound.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnReset
            // 
            this.btnReset.AutoSize = true;
            this.btnReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.btnReset, 2);
            this.btnReset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReset.Location = new System.Drawing.Point(116, 239);
            this.btnReset.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(101, 23);
            this.btnReset.TabIndex = 1;
            this.btnReset.Tag = "Button_Reset";
            this.btnReset.Text = "{Reset}";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.AutoSize = true;
            this.btnAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.btnAdd, 2);
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAdd.Location = new System.Drawing.Point(3, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(104, 23);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Tag = "Button_Add";
            this.btnAdd.Text = "{Add}";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.AutoSize = true;
            this.btnRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.btnRemove, 2);
            this.btnRemove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRemove.Location = new System.Drawing.Point(113, 3);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(104, 23);
            this.btnRemove.TabIndex = 1;
            this.btnRemove.Tag = "Button_Remove";
            this.btnRemove.Text = "{Remove}";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // lblInitiativeManeuvers
            // 
            this.lblInitiativeManeuvers.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblInitiativeManeuvers.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblInitiativeManeuvers, 4);
            this.lblInitiativeManeuvers.Location = new System.Drawing.Point(55, 35);
            this.lblInitiativeManeuvers.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblInitiativeManeuvers.Name = "lblInitiativeManeuvers";
            this.lblInitiativeManeuvers.Size = new System.Drawing.Size(110, 13);
            this.lblInitiativeManeuvers.TabIndex = 4;
            this.lblInitiativeManeuvers.Tag = "Label_InitiativeManeuvers";
            this.lblInitiativeManeuvers.Text = "{Initiative Maneuvers}";
            this.lblInitiativeManeuvers.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 62);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(16, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "-1";
            // 
            // btnMinusInit1
            // 
            this.btnMinusInit1.AutoSize = true;
            this.btnMinusInit1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnMinusInit1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMinusInit1.Image = global::Chummer.Properties.Resources.delete;
            this.btnMinusInit1.Location = new System.Drawing.Point(58, 57);
            this.btnMinusInit1.Name = "btnMinusInit1";
            this.btnMinusInit1.Padding = new System.Windows.Forms.Padding(1);
            this.btnMinusInit1.Size = new System.Drawing.Size(24, 24);
            this.btnMinusInit1.TabIndex = 3;
            this.btnMinusInit1.UseVisualStyleBackColor = true;
            this.btnMinusInit1.Click += new System.EventHandler(this.btnMinusInit1_Click);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(143, 62);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(19, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "+1";
            // 
            // btnAdd1Init
            // 
            this.btnAdd1Init.AutoSize = true;
            this.btnAdd1Init.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAdd1Init.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAdd1Init.Image = global::Chummer.Properties.Resources.add;
            this.btnAdd1Init.Location = new System.Drawing.Point(168, 57);
            this.btnAdd1Init.Name = "btnAdd1Init";
            this.btnAdd1Init.Padding = new System.Windows.Forms.Padding(1);
            this.btnAdd1Init.Size = new System.Drawing.Size(24, 24);
            this.btnAdd1Init.TabIndex = 6;
            this.btnAdd1Init.UseVisualStyleBackColor = true;
            this.btnAdd1Init.Click += new System.EventHandler(this.btnAdd1Init_Click);
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(36, 92);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "-5";
            // 
            // btnMinus5Init
            // 
            this.btnMinus5Init.AutoSize = true;
            this.btnMinus5Init.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnMinus5Init.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMinus5Init.Image = global::Chummer.Properties.Resources.delete;
            this.btnMinus5Init.Location = new System.Drawing.Point(58, 87);
            this.btnMinus5Init.Name = "btnMinus5Init";
            this.btnMinus5Init.Padding = new System.Windows.Forms.Padding(1);
            this.btnMinus5Init.Size = new System.Drawing.Size(24, 24);
            this.btnMinus5Init.TabIndex = 8;
            this.btnMinus5Init.UseVisualStyleBackColor = true;
            this.btnMinus5Init.Click += new System.EventHandler(this.btnMinus5Init_Click);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(143, 92);
            this.label5.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(19, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "+5";
            // 
            // btnAdd5Init
            // 
            this.btnAdd5Init.AutoSize = true;
            this.btnAdd5Init.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAdd5Init.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAdd5Init.Image = global::Chummer.Properties.Resources.add;
            this.btnAdd5Init.Location = new System.Drawing.Point(168, 87);
            this.btnAdd5Init.Name = "btnAdd5Init";
            this.btnAdd5Init.Padding = new System.Windows.Forms.Padding(1);
            this.btnAdd5Init.Size = new System.Drawing.Size(24, 24);
            this.btnAdd5Init.TabIndex = 10;
            this.btnAdd5Init.UseVisualStyleBackColor = true;
            this.btnAdd5Init.Click += new System.EventHandler(this.btnAdd5Init_Click);
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(30, 122);
            this.label6.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(22, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "-10";
            // 
            // btnMinus10Init
            // 
            this.btnMinus10Init.AutoSize = true;
            this.btnMinus10Init.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnMinus10Init.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMinus10Init.Image = global::Chummer.Properties.Resources.delete;
            this.btnMinus10Init.Location = new System.Drawing.Point(58, 117);
            this.btnMinus10Init.Name = "btnMinus10Init";
            this.btnMinus10Init.Padding = new System.Windows.Forms.Padding(1);
            this.btnMinus10Init.Size = new System.Drawing.Size(24, 24);
            this.btnMinus10Init.TabIndex = 12;
            this.btnMinus10Init.UseVisualStyleBackColor = true;
            this.btnMinus10Init.Click += new System.EventHandler(this.btnMinus10Init_Click);
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(137, 122);
            this.label7.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(25, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "+10";
            // 
            // btnAdd10Init
            // 
            this.btnAdd10Init.AutoSize = true;
            this.btnAdd10Init.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAdd10Init.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAdd10Init.Image = global::Chummer.Properties.Resources.add;
            this.btnAdd10Init.Location = new System.Drawing.Point(168, 117);
            this.btnAdd10Init.Name = "btnAdd10Init";
            this.btnAdd10Init.Padding = new System.Windows.Forms.Padding(1);
            this.btnAdd10Init.Size = new System.Drawing.Size(24, 24);
            this.btnAdd10Init.TabIndex = 14;
            this.btnAdd10Init.UseVisualStyleBackColor = true;
            this.btnAdd10Init.Click += new System.EventHandler(this.btnAdd10Init_Click);
            // 
            // cboManeuvers
            // 
            this.cboManeuvers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpRight.SetColumnSpan(this.cboManeuvers, 4);
            this.cboManeuvers.Enabled = false;
            this.cboManeuvers.FormattingEnabled = true;
            this.cboManeuvers.Location = new System.Drawing.Point(3, 147);
            this.cboManeuvers.Name = "cboManeuvers";
            this.cboManeuvers.Size = new System.Drawing.Size(214, 21);
            this.cboManeuvers.TabIndex = 15;
            this.cboManeuvers.TooltipText = "";
            // 
            // btnApplyInterrupt
            // 
            this.btnApplyInterrupt.AutoSize = true;
            this.btnApplyInterrupt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.btnApplyInterrupt, 4);
            this.btnApplyInterrupt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApplyInterrupt.Enabled = false;
            this.btnApplyInterrupt.Location = new System.Drawing.Point(3, 174);
            this.btnApplyInterrupt.Name = "btnApplyInterrupt";
            this.btnApplyInterrupt.Size = new System.Drawing.Size(214, 23);
            this.btnApplyInterrupt.TabIndex = 16;
            this.btnApplyInterrupt.Tag = "Button_ApplyManeuver";
            this.btnApplyInterrupt.Text = "{Apply Maneuver}";
            this.btnApplyInterrupt.UseVisualStyleBackColor = true;
            this.btnApplyInterrupt.Click += new System.EventHandler(this.btnApplyInterrupt_Click);
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 4;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpRight.Controls.Add(this.btnApplyInterrupt, 0, 6);
            this.tlpRight.Controls.Add(this.cboManeuvers, 0, 5);
            this.tlpRight.Controls.Add(this.btnAdd10Init, 3, 4);
            this.tlpRight.Controls.Add(this.label7, 2, 4);
            this.tlpRight.Controls.Add(this.btnMinus10Init, 1, 4);
            this.tlpRight.Controls.Add(this.label6, 0, 4);
            this.tlpRight.Controls.Add(this.btnAdd5Init, 3, 3);
            this.tlpRight.Controls.Add(this.label5, 2, 3);
            this.tlpRight.Controls.Add(this.btnMinus5Init, 1, 3);
            this.tlpRight.Controls.Add(this.label4, 0, 3);
            this.tlpRight.Controls.Add(this.btnAdd1Init, 3, 2);
            this.tlpRight.Controls.Add(this.label3, 2, 2);
            this.tlpRight.Controls.Add(this.btnMinusInit1, 1, 2);
            this.tlpRight.Controls.Add(this.label1, 0, 2);
            this.tlpRight.Controls.Add(this.lblInitiativeManeuvers, 0, 1);
            this.tlpRight.Controls.Add(this.btnRemove, 2, 0);
            this.tlpRight.Controls.Add(this.btnAdd, 0, 0);
            this.tlpRight.Controls.Add(this.btnReset, 2, 8);
            this.tlpRight.Controls.Add(this.lblRound, 0, 8);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(200, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 9;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.Size = new System.Drawing.Size(220, 265);
            this.tlpRight.TabIndex = 8;
            // 
            // InitiativeUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.tlpMain);
            this.Name = "InitiativeUserControl";
            this.Size = new System.Drawing.Size(420, 265);
            this.Tag = "";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpLeft.ResumeLayout(false);
            this.tlpLeft.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnSort;
        private System.Windows.Forms.Button btnDelay;
        private System.Windows.Forms.CheckedListBox chkBoxChummer;
        private BufferedTableLayoutPanel tlpLeft;
        private BufferedTableLayoutPanel tlpRight;
        private System.Windows.Forms.Button btnApplyInterrupt;
        private ElasticComboBox cboManeuvers;
        private System.Windows.Forms.Button btnAdd10Init;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnMinus10Init;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnAdd5Init;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnMinus5Init;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnAdd1Init;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnMinusInit1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblInitiativeManeuvers;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Label lblRound;
    }
}
