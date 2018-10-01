namespace Chummer
{
    partial class DiceRollerControl
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
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel();
            this.txtResults = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel();
            this.nudLimit = new System.Windows.Forms.NumericUpDown();
            this.cboEdgeUse = new System.Windows.Forms.ComboBox();
            this.lblLimit = new System.Windows.Forms.Label();
            this.lblResults = new System.Windows.Forms.Label();
            this.nudGremlins = new System.Windows.Forms.NumericUpDown();
            this.lblGremlins = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new Chummer.BufferedTableLayoutPanel();
            this.chkBubbleDie = new System.Windows.Forms.CheckBox();
            this.nudThreshold = new System.Windows.Forms.NumericUpDown();
            this.lblThreshold = new System.Windows.Forms.Label();
            this.tableLayoutPanel4 = new Chummer.BufferedTableLayoutPanel();
            this.chkRuleOf6 = new System.Windows.Forms.CheckBox();
            this.cmdRollDice = new System.Windows.Forms.Button();
            this.chkRushJob = new System.Windows.Forms.CheckBox();
            this.lblD6 = new System.Windows.Forms.Label();
            this.nudDice = new System.Windows.Forms.NumericUpDown();
            this.lblRoll = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGremlins)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDice)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.txtResults, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(428, 188);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // txtResults
            // 
            this.txtResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtResults.Location = new System.Drawing.Point(3, 83);
            this.txtResults.Name = "txtResults";
            this.txtResults.Size = new System.Drawing.Size(422, 102);
            this.txtResults.TabIndex = 3;
            this.txtResults.Text = "";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.Controls.Add(this.nudLimit, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.cboEdgeUse, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblLimit, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblResults, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.nudGremlins, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblGremlins, 4, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 55);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(428, 25);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // nudLimit
            // 
            this.nudLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudLimit.Location = new System.Drawing.Point(201, 3);
            this.nudLimit.Name = "nudLimit";
            this.nudLimit.Size = new System.Drawing.Size(39, 20);
            this.nudLimit.TabIndex = 55;
            // 
            // cboEdgeUse
            // 
            this.cboEdgeUse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboEdgeUse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEdgeUse.FormattingEnabled = true;
            this.cboEdgeUse.Location = new System.Drawing.Point(246, 3);
            this.cboEdgeUse.Name = "cboEdgeUse";
            this.cboEdgeUse.Size = new System.Drawing.Size(78, 21);
            this.cboEdgeUse.TabIndex = 54;
            // 
            // lblLimit
            // 
            this.lblLimit.AutoSize = true;
            this.lblLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLimit.Location = new System.Drawing.Point(163, 0);
            this.lblLimit.Name = "lblLimit";
            this.lblLimit.Size = new System.Drawing.Size(32, 25);
            this.lblLimit.TabIndex = 51;
            this.lblLimit.Tag = "String_Limit";
            this.lblLimit.Text = "Limit:";
            this.lblLimit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblResults
            // 
            this.lblResults.AutoSize = true;
            this.lblResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResults.Location = new System.Drawing.Point(3, 0);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(154, 25);
            this.lblResults.TabIndex = 50;
            this.lblResults.Tag = "Label_DiceRoller_Result";
            this.lblResults.Text = "Results:";
            this.lblResults.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudGremlins
            // 
            this.nudGremlins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudGremlins.Location = new System.Drawing.Point(386, 3);
            this.nudGremlins.Name = "nudGremlins";
            this.nudGremlins.Size = new System.Drawing.Size(39, 20);
            this.nudGremlins.TabIndex = 53;
            this.nudGremlins.ValueChanged += new System.EventHandler(this.nudGremlins_ValueChanged);
            // 
            // lblGremlins
            // 
            this.lblGremlins.AutoSize = true;
            this.lblGremlins.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblGremlins.Location = new System.Drawing.Point(330, 0);
            this.lblGremlins.Name = "lblGremlins";
            this.lblGremlins.Size = new System.Drawing.Size(50, 25);
            this.lblGremlins.TabIndex = 48;
            this.lblGremlins.Tag = "Label_DiceRoller_Gremlins";
            this.lblGremlins.Text = "Gremlins:";
            this.lblGremlins.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 257F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel3.Controls.Add(this.chkBubbleDie, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.nudThreshold, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.lblThreshold, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(428, 30);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // chkBubbleDie
            // 
            this.chkBubbleDie.AutoSize = true;
            this.chkBubbleDie.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkBubbleDie.Location = new System.Drawing.Point(3, 3);
            this.chkBubbleDie.Name = "chkBubbleDie";
            this.chkBubbleDie.Size = new System.Drawing.Size(248, 24);
            this.chkBubbleDie.TabIndex = 51;
            this.chkBubbleDie.Tag = "Checkbox_DiceRoller_BubbleDie";
            this.chkBubbleDie.Text = "Bubble Die (Fix Even Dicepool Glitch Chances)";
            this.chkBubbleDie.UseVisualStyleBackColor = true;
            // 
            // nudThreshold
            // 
            this.nudThreshold.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudThreshold.Location = new System.Drawing.Point(386, 3);
            this.nudThreshold.Name = "nudThreshold";
            this.nudThreshold.Size = new System.Drawing.Size(39, 20);
            this.nudThreshold.TabIndex = 49;
            this.nudThreshold.ValueChanged += new System.EventHandler(this.nudThreshold_ValueChanged);
            // 
            // lblThreshold
            // 
            this.lblThreshold.AutoSize = true;
            this.lblThreshold.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblThreshold.Location = new System.Drawing.Point(323, 0);
            this.lblThreshold.Name = "lblThreshold";
            this.lblThreshold.Size = new System.Drawing.Size(57, 30);
            this.lblThreshold.TabIndex = 47;
            this.lblThreshold.Tag = "Label_DiceRoller_Threshold";
            this.lblThreshold.Text = "Threshold:";
            this.lblThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 6;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 53F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.529411F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.05105F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.63063F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel4.Controls.Add(this.chkRuleOf6, 4, 0);
            this.tableLayoutPanel4.Controls.Add(this.cmdRollDice, 5, 0);
            this.tableLayoutPanel4.Controls.Add(this.chkRushJob, 3, 0);
            this.tableLayoutPanel4.Controls.Add(this.lblD6, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.nudDice, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.lblRoll, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(428, 25);
            this.tableLayoutPanel4.TabIndex = 7;
            // 
            // chkRuleOf6
            // 
            this.chkRuleOf6.AutoSize = true;
            this.chkRuleOf6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRuleOf6.Location = new System.Drawing.Point(287, 3);
            this.chkRuleOf6.Name = "chkRuleOf6";
            this.chkRuleOf6.Size = new System.Drawing.Size(96, 19);
            this.chkRuleOf6.TabIndex = 59;
            this.chkRuleOf6.Tag = "Checkbox_DiceRoller_RuleOfSix";
            this.chkRuleOf6.Text = "using Rule of 6";
            this.chkRuleOf6.UseVisualStyleBackColor = true;
            // 
            // cmdRollDice
            // 
            this.cmdRollDice.AutoSize = true;
            this.cmdRollDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRollDice.Location = new System.Drawing.Point(386, 0);
            this.cmdRollDice.Margin = new System.Windows.Forms.Padding(0);
            this.cmdRollDice.Name = "cmdRollDice";
            this.cmdRollDice.Size = new System.Drawing.Size(42, 25);
            this.cmdRollDice.TabIndex = 58;
            this.cmdRollDice.Tag = "Button_DiceRoller_Roll";
            this.cmdRollDice.Text = "&Roll";
            this.cmdRollDice.UseVisualStyleBackColor = true;
            // 
            // chkRushJob
            // 
            this.chkRushJob.AutoSize = true;
            this.chkRushJob.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRushJob.Location = new System.Drawing.Point(117, 3);
            this.chkRushJob.Name = "chkRushJob";
            this.chkRushJob.Size = new System.Drawing.Size(164, 19);
            this.chkRushJob.TabIndex = 55;
            this.chkRushJob.Tag = "Checkbox_DiceRoller_RushedJob";
            this.chkRushJob.Text = "Rushed Job (Glitch on 1 or 2)";
            this.chkRushJob.UseVisualStyleBackColor = true;
            // 
            // lblD6
            // 
            this.lblD6.AutoSize = true;
            this.lblD6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblD6.Location = new System.Drawing.Point(89, 0);
            this.lblD6.Name = "lblD6";
            this.lblD6.Size = new System.Drawing.Size(22, 25);
            this.lblD6.TabIndex = 54;
            this.lblD6.Tag = "String_D6";
            this.lblD6.Text = "D6";
            this.lblD6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudDice
            // 
            this.nudDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudDice.Location = new System.Drawing.Point(36, 3);
            this.nudDice.Name = "nudDice";
            this.nudDice.Size = new System.Drawing.Size(47, 20);
            this.nudDice.TabIndex = 53;
            this.nudDice.ValueChanged += new System.EventHandler(this.nudDice_ValueChanged);
            // 
            // lblRoll
            // 
            this.lblRoll.AutoSize = true;
            this.lblRoll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRoll.Location = new System.Drawing.Point(3, 0);
            this.lblRoll.Name = "lblRoll";
            this.lblRoll.Size = new System.Drawing.Size(27, 25);
            this.lblRoll.TabIndex = 48;
            this.lblRoll.Tag = "String_DiceRoller_Roll";
            this.lblRoll.Text = "Roll";
            this.lblRoll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DiceRollerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DiceRollerControl";
            this.Size = new System.Drawing.Size(428, 188);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGremlins)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDice)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RichTextBox txtResults;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.NumericUpDown nudGremlins;
        private System.Windows.Forms.Label lblGremlins;
        private System.Windows.Forms.Label lblLimit;
        private System.Windows.Forms.Label lblResults;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label lblThreshold;
        private System.Windows.Forms.NumericUpDown nudThreshold;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.CheckBox chkRuleOf6;
        private System.Windows.Forms.Button cmdRollDice;
        private System.Windows.Forms.CheckBox chkRushJob;
        private System.Windows.Forms.Label lblD6;
        private System.Windows.Forms.NumericUpDown nudDice;
        private System.Windows.Forms.Label lblRoll;
        private System.Windows.Forms.NumericUpDown nudLimit;
        private System.Windows.Forms.ComboBox cboEdgeUse;
        private System.Windows.Forms.CheckBox chkBubbleDie;
    }
}
