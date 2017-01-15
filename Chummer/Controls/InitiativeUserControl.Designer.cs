namespace Chummer
{
    partial class InitiativeUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
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
            this.cboManeuvers = new System.Windows.Forms.ComboBox();
            this.btnApplyInterrupt = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnSort = new System.Windows.Forms.Button();
            this.btnDelay = new System.Windows.Forms.Button();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRound = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.chkBoxChummer = new System.Windows.Forms.CheckedListBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkBoxChummer, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(420, 265);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnAdd);
            this.flowLayoutPanel1.Controls.Add(this.btnRemove);
            this.flowLayoutPanel1.Controls.Add(this.lblInitiativeManeuvers);
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.btnMinusInit1);
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.btnAdd1Init);
            this.flowLayoutPanel1.Controls.Add(this.label4);
            this.flowLayoutPanel1.Controls.Add(this.btnMinus5Init);
            this.flowLayoutPanel1.Controls.Add(this.label5);
            this.flowLayoutPanel1.Controls.Add(this.btnAdd5Init);
            this.flowLayoutPanel1.Controls.Add(this.label6);
            this.flowLayoutPanel1.Controls.Add(this.btnMinus10Init);
            this.flowLayoutPanel1.Controls.Add(this.label7);
            this.flowLayoutPanel1.Controls.Add(this.btnAdd10Init);
            this.flowLayoutPanel1.Controls.Add(this.cboManeuvers);
            this.flowLayoutPanel1.Controls.Add(this.btnApplyInterrupt);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(253, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(164, 229);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(3, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Tag = "Button_Add";
            this.btnAdd.Text = "{Add}";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(84, 3);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 1;
            this.btnRemove.Tag = "Button_Remove";
            this.btnRemove.Text = "{Remove}";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // lblInitiativeManeuvers
            // 
            this.lblInitiativeManeuvers.AutoSize = true;
            this.lblInitiativeManeuvers.Location = new System.Drawing.Point(3, 29);
            this.lblInitiativeManeuvers.MinimumSize = new System.Drawing.Size(170, 13);
            this.lblInitiativeManeuvers.Name = "lblInitiativeManeuvers";
            this.lblInitiativeManeuvers.Size = new System.Drawing.Size(170, 13);
            this.lblInitiativeManeuvers.TabIndex = 4;
            this.lblInitiativeManeuvers.Tag = "Label_InitiativeManeuvers";
            this.lblInitiativeManeuvers.Text = "{Initiative Maneuvers}";
            this.lblInitiativeManeuvers.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 57);
            this.label1.Margin = new System.Windows.Forms.Padding(10, 15, 0, 10);
            this.label1.MinimumSize = new System.Drawing.Size(30, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "-1 ";
            // 
            // btnMinusInit1
            // 
            this.btnMinusInit1.BackgroundImage = global::Chummer.Properties.Resources.delete;
            this.btnMinusInit1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMinusInit1.Location = new System.Drawing.Point(40, 52);
            this.btnMinusInit1.Margin = new System.Windows.Forms.Padding(0, 10, 3, 3);
            this.btnMinusInit1.Name = "btnMinusInit1";
            this.btnMinusInit1.Size = new System.Drawing.Size(24, 24);
            this.btnMinusInit1.TabIndex = 3;
            this.btnMinusInit1.UseVisualStyleBackColor = true;
            this.btnMinusInit1.Click += new System.EventHandler(this.btnMinusInit1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(77, 57);
            this.label3.Margin = new System.Windows.Forms.Padding(10, 15, 0, 10);
            this.label3.MinimumSize = new System.Drawing.Size(30, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "+1 ";
            // 
            // btnAdd1Init
            // 
            this.btnAdd1Init.BackgroundImage = global::Chummer.Properties.Resources.add;
            this.btnAdd1Init.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAdd1Init.Location = new System.Drawing.Point(107, 52);
            this.btnAdd1Init.Margin = new System.Windows.Forms.Padding(0, 10, 3, 3);
            this.btnAdd1Init.Name = "btnAdd1Init";
            this.btnAdd1Init.Size = new System.Drawing.Size(24, 24);
            this.btnAdd1Init.TabIndex = 6;
            this.btnAdd1Init.UseVisualStyleBackColor = true;
            this.btnAdd1Init.Click += new System.EventHandler(this.btnAdd1Init_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 95);
            this.label4.Margin = new System.Windows.Forms.Padding(10, 15, 0, 10);
            this.label4.MinimumSize = new System.Drawing.Size(30, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "-5";
            // 
            // btnMinus5Init
            // 
            this.btnMinus5Init.BackgroundImage = global::Chummer.Properties.Resources.delete;
            this.btnMinus5Init.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMinus5Init.Location = new System.Drawing.Point(40, 90);
            this.btnMinus5Init.Margin = new System.Windows.Forms.Padding(0, 10, 3, 3);
            this.btnMinus5Init.Name = "btnMinus5Init";
            this.btnMinus5Init.Size = new System.Drawing.Size(24, 24);
            this.btnMinus5Init.TabIndex = 8;
            this.btnMinus5Init.UseVisualStyleBackColor = true;
            this.btnMinus5Init.Click += new System.EventHandler(this.btnMinus5Init_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(77, 95);
            this.label5.Margin = new System.Windows.Forms.Padding(10, 15, 0, 10);
            this.label5.MinimumSize = new System.Drawing.Size(30, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "+5 ";
            // 
            // btnAdd5Init
            // 
            this.btnAdd5Init.BackgroundImage = global::Chummer.Properties.Resources.add;
            this.btnAdd5Init.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAdd5Init.Location = new System.Drawing.Point(107, 90);
            this.btnAdd5Init.Margin = new System.Windows.Forms.Padding(0, 10, 3, 3);
            this.btnAdd5Init.Name = "btnAdd5Init";
            this.btnAdd5Init.Size = new System.Drawing.Size(24, 24);
            this.btnAdd5Init.TabIndex = 10;
            this.btnAdd5Init.UseVisualStyleBackColor = true;
            this.btnAdd5Init.Click += new System.EventHandler(this.btnAdd5Init_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 133);
            this.label6.Margin = new System.Windows.Forms.Padding(10, 15, 0, 10);
            this.label6.MinimumSize = new System.Drawing.Size(30, 13);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "-10 ";
            // 
            // btnMinus10Init
            // 
            this.btnMinus10Init.BackgroundImage = global::Chummer.Properties.Resources.delete;
            this.btnMinus10Init.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMinus10Init.Location = new System.Drawing.Point(40, 128);
            this.btnMinus10Init.Margin = new System.Windows.Forms.Padding(0, 10, 3, 3);
            this.btnMinus10Init.Name = "btnMinus10Init";
            this.btnMinus10Init.Size = new System.Drawing.Size(24, 24);
            this.btnMinus10Init.TabIndex = 12;
            this.btnMinus10Init.UseVisualStyleBackColor = true;
            this.btnMinus10Init.Click += new System.EventHandler(this.btnMinus10Init_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(77, 133);
            this.label7.Margin = new System.Windows.Forms.Padding(10, 15, 0, 10);
            this.label7.MinimumSize = new System.Drawing.Size(30, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(30, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "+10 ";
            // 
            // btnAdd10Init
            // 
            this.btnAdd10Init.BackgroundImage = global::Chummer.Properties.Resources.add;
            this.btnAdd10Init.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAdd10Init.Location = new System.Drawing.Point(107, 128);
            this.btnAdd10Init.Margin = new System.Windows.Forms.Padding(0, 10, 3, 3);
            this.btnAdd10Init.Name = "btnAdd10Init";
            this.btnAdd10Init.Size = new System.Drawing.Size(24, 24);
            this.btnAdd10Init.TabIndex = 14;
            this.btnAdd10Init.UseVisualStyleBackColor = true;
            this.btnAdd10Init.Click += new System.EventHandler(this.btnAdd10Init_Click);
            // 
            // cboManeuvers
            // 
            this.cboManeuvers.Enabled = false;
            this.cboManeuvers.FormattingEnabled = true;
            this.cboManeuvers.Location = new System.Drawing.Point(21, 159);
            this.cboManeuvers.Margin = new System.Windows.Forms.Padding(21, 3, 21, 3);
            this.cboManeuvers.Name = "cboManeuvers";
            this.cboManeuvers.Size = new System.Drawing.Size(122, 21);
            this.cboManeuvers.TabIndex = 15;
            // 
            // btnApplyInterrupt
            // 
            this.btnApplyInterrupt.Enabled = false;
            this.btnApplyInterrupt.Location = new System.Drawing.Point(25, 186);
            this.btnApplyInterrupt.Margin = new System.Windows.Forms.Padding(25, 3, 25, 3);
            this.btnApplyInterrupt.Name = "btnApplyInterrupt";
            this.btnApplyInterrupt.Size = new System.Drawing.Size(114, 23);
            this.btnApplyInterrupt.TabIndex = 16;
            this.btnApplyInterrupt.Tag = "Button_ApplyManeuver";
            this.btnApplyInterrupt.Text = "{Apply Maneuver}";
            this.btnApplyInterrupt.UseVisualStyleBackColor = true;
            this.btnApplyInterrupt.Click += new System.EventHandler(this.btnApplyInterrupt_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.btnNext);
            this.flowLayoutPanel2.Controls.Add(this.btnSort);
            this.flowLayoutPanel2.Controls.Add(this.btnDelay);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 235);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(250, 30);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(3, 3);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 0;
            this.btnNext.Tag = "Button_Next";
            this.btnNext.Text = "{Next}";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnSort
            // 
            this.btnSort.Location = new System.Drawing.Point(84, 3);
            this.btnSort.Name = "btnSort";
            this.btnSort.Size = new System.Drawing.Size(75, 23);
            this.btnSort.TabIndex = 1;
            this.btnSort.Tag = "Button_Sort";
            this.btnSort.Text = "{Sort}";
            this.btnSort.UseVisualStyleBackColor = true;
            this.btnSort.Click += new System.EventHandler(this.btnSort_Click);
            // 
            // btnDelay
            // 
            this.btnDelay.Location = new System.Drawing.Point(165, 3);
            this.btnDelay.Name = "btnDelay";
            this.btnDelay.Size = new System.Drawing.Size(75, 23);
            this.btnDelay.TabIndex = 2;
            this.btnDelay.Tag = "Button_Delay";
            this.btnDelay.Text = "{Delay}";
            this.btnDelay.UseVisualStyleBackColor = true;
            this.btnDelay.Click += new System.EventHandler(this.btnDelay_Click);
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.lblRound);
            this.flowLayoutPanel3.Controls.Add(this.btnReset);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(250, 235);
            this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(170, 30);
            this.flowLayoutPanel3.TabIndex = 5;
            // 
            // lblRound
            // 
            this.lblRound.AutoSize = true;
            this.lblRound.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRound.Location = new System.Drawing.Point(3, 0);
            this.lblRound.MinimumSize = new System.Drawing.Size(75, 29);
            this.lblRound.Name = "lblRound";
            this.lblRound.Size = new System.Drawing.Size(75, 29);
            this.lblRound.TabIndex = 0;
            this.lblRound.Tag = "Label_Round";
            this.lblRound.Text = "{Round#}";
            this.lblRound.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(87, 3);
            this.btnReset.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 1;
            this.btnReset.Tag = "Button_Reset";
            this.btnReset.Text = "{Reset}";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // chkBoxChummer
            // 
            this.chkBoxChummer.BackColor = System.Drawing.SystemColors.Info;
            this.chkBoxChummer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkBoxChummer.FormattingEnabled = true;
            this.chkBoxChummer.Location = new System.Drawing.Point(3, 3);
            this.chkBoxChummer.Name = "chkBoxChummer";
            this.chkBoxChummer.Size = new System.Drawing.Size(244, 229);
            this.chkBoxChummer.TabIndex = 6;
            this.chkBoxChummer.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chkBoxChummer_MouseClick);
            this.chkBoxChummer.SelectedIndexChanged += new System.EventHandler(this.listBoxChummers_SelectedIndexChanged);
            // 
            // InitiativeUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "InitiativeUserControl";
            this.Size = new System.Drawing.Size(420, 265);
            this.Tag = string.Empty;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label lblInitiativeManeuvers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnMinusInit1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnAdd1Init;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnMinus5Init;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnAdd5Init;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnMinus10Init;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnAdd10Init;
        private System.Windows.Forms.ComboBox cboManeuvers;
        private System.Windows.Forms.Button btnApplyInterrupt;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnSort;
        private System.Windows.Forms.Button btnDelay;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Label lblRound;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.CheckedListBox chkBoxChummer;
    }
}
