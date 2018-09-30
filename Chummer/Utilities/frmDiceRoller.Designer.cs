namespace Chummer
{
    partial class frmDiceRoller
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDiceRoller));
            this.lblRoll = new System.Windows.Forms.Label();
            this.nudDice = new System.Windows.Forms.NumericUpDown();
            this.lblD6 = new System.Windows.Forms.Label();
            this.cmdRollDice = new System.Windows.Forms.Button();
            this.chkRuleOf6 = new System.Windows.Forms.CheckBox();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.lblResultsLabel = new System.Windows.Forms.Label();
            this.lblGremlins = new System.Windows.Forms.Label();
            this.nudGremlins = new System.Windows.Forms.NumericUpDown();
            this.cboMethod = new ElasticComboBox();
            this.chkCinematicGameplay = new System.Windows.Forms.CheckBox();
            this.cmdReroll = new System.Windows.Forms.Button();
            this.chkRushJob = new System.Windows.Forms.CheckBox();
            this.nudThreshold = new System.Windows.Forms.NumericUpDown();
            this.lblThreshold = new System.Windows.Forms.Label();
            this.chkBubbleDie = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel();
            this.tableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel();
            this.lblResults = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudDice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGremlins)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblRoll
            // 
            this.lblRoll.AutoSize = true;
            this.lblRoll.Location = new System.Drawing.Point(3, 6);
            this.lblRoll.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblRoll.Name = "lblRoll";
            this.lblRoll.Size = new System.Drawing.Size(25, 13);
            this.lblRoll.TabIndex = 0;
            this.lblRoll.Tag = "String_DiceRoller_Roll";
            this.lblRoll.Text = "Roll";
            // 
            // nudDice
            // 
            this.nudDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudDice.Location = new System.Drawing.Point(47, 3);
            this.nudDice.Name = "nudDice";
            this.nudDice.Size = new System.Drawing.Size(82, 20);
            this.nudDice.TabIndex = 1;
            this.nudDice.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblD6
            // 
            this.lblD6.AutoSize = true;
            this.lblD6.Location = new System.Drawing.Point(135, 6);
            this.lblD6.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblD6.Name = "lblD6";
            this.lblD6.Size = new System.Drawing.Size(21, 13);
            this.lblD6.TabIndex = 2;
            this.lblD6.Tag = "String_D6";
            this.lblD6.Text = "D6";
            // 
            // cmdRollDice
            // 
            this.cmdRollDice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRollDice.Location = new System.Drawing.Point(333, 3);
            this.cmdRollDice.Name = "cmdRollDice";
            this.cmdRollDice.Size = new System.Drawing.Size(104, 23);
            this.cmdRollDice.TabIndex = 10;
            this.cmdRollDice.Tag = "Button_DiceRoller_Roll";
            this.cmdRollDice.Text = "&Roll";
            this.cmdRollDice.UseVisualStyleBackColor = true;
            this.cmdRollDice.Click += new System.EventHandler(this.cmdRollDice_Click);
            // 
            // chkRuleOf6
            // 
            this.chkRuleOf6.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkRuleOf6, 2);
            this.chkRuleOf6.Location = new System.Drawing.Point(3, 33);
            this.chkRuleOf6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkRuleOf6.Name = "chkRuleOf6";
            this.chkRuleOf6.Size = new System.Drawing.Size(97, 17);
            this.chkRuleOf6.TabIndex = 4;
            this.chkRuleOf6.Tag = "Checkbox_DiceRoller_RuleOfSix";
            this.chkRuleOf6.Text = "using Rule of 6";
            this.chkRuleOf6.UseVisualStyleBackColor = true;
            // 
            // lstResults
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.lstResults, 2);
            this.lstResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstResults.FormattingEnabled = true;
            this.lstResults.Location = new System.Drawing.Point(3, 61);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(126, 193);
            this.lstResults.TabIndex = 9;
            // 
            // lblResultsLabel
            // 
            this.lblResultsLabel.AutoSize = true;
            this.lblResultsLabel.Location = new System.Drawing.Point(3, 108);
            this.lblResultsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblResultsLabel.Name = "lblResultsLabel";
            this.lblResultsLabel.Size = new System.Drawing.Size(45, 13);
            this.lblResultsLabel.TabIndex = 8;
            this.lblResultsLabel.Tag = "Label_DiceRoller_Result";
            this.lblResultsLabel.Text = "Results:";
            // 
            // lblGremlins
            // 
            this.lblGremlins.AutoSize = true;
            this.lblGremlins.Location = new System.Drawing.Point(3, 82);
            this.lblGremlins.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGremlins.Name = "lblGremlins";
            this.lblGremlins.Size = new System.Drawing.Size(50, 13);
            this.lblGremlins.TabIndex = 6;
            this.lblGremlins.Tag = "Label_DiceRoller_Gremlins";
            this.lblGremlins.Text = "Gremlins:";
            // 
            // nudGremlins
            // 
            this.nudGremlins.Location = new System.Drawing.Point(80, 79);
            this.nudGremlins.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nudGremlins.Name = "nudGremlins";
            this.nudGremlins.Size = new System.Drawing.Size(45, 20);
            this.nudGremlins.TabIndex = 7;
            // 
            // cboMethod
            // 
            this.cboMethod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMethod.FormattingEnabled = true;
            this.cboMethod.Location = new System.Drawing.Point(179, 3);
            this.cboMethod.Name = "cboMethod";
            this.cboMethod.Size = new System.Drawing.Size(148, 21);
            this.cboMethod.TabIndex = 3;
            this.cboMethod.SelectedIndexChanged += new System.EventHandler(this.cboMethod_SelectedIndexChanged);
            // 
            // chkCinematicGameplay
            // 
            this.chkCinematicGameplay.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkCinematicGameplay, 2);
            this.chkCinematicGameplay.Location = new System.Drawing.Point(135, 33);
            this.chkCinematicGameplay.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkCinematicGameplay.Name = "chkCinematicGameplay";
            this.chkCinematicGameplay.Size = new System.Drawing.Size(99, 17);
            this.chkCinematicGameplay.TabIndex = 5;
            this.chkCinematicGameplay.Tag = "Checkbox_DiceRoller_CinematicGameplay";
            this.chkCinematicGameplay.Text = "Hit on 4, 5, or 6";
            this.chkCinematicGameplay.UseVisualStyleBackColor = true;
            // 
            // cmdReroll
            // 
            this.cmdReroll.AutoSize = true;
            this.cmdReroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdReroll.Location = new System.Drawing.Point(333, 32);
            this.cmdReroll.Name = "cmdReroll";
            this.cmdReroll.Size = new System.Drawing.Size(104, 23);
            this.cmdReroll.TabIndex = 11;
            this.cmdReroll.Tag = "Button_DiceRoller_RollMisses";
            this.cmdReroll.Text = "Re-Roll Misses";
            this.cmdReroll.UseVisualStyleBackColor = true;
            this.cmdReroll.Click += new System.EventHandler(this.cmdReroll_Click);
            // 
            // chkRushJob
            // 
            this.chkRushJob.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.chkRushJob, 2);
            this.chkRushJob.Location = new System.Drawing.Point(3, 4);
            this.chkRushJob.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkRushJob.Name = "chkRushJob";
            this.chkRushJob.Size = new System.Drawing.Size(164, 17);
            this.chkRushJob.TabIndex = 12;
            this.chkRushJob.Tag = "Checkbox_DiceRoller_RushedJob";
            this.chkRushJob.Text = "Rushed Job (Glitch on 1 or 2)";
            this.chkRushJob.UseVisualStyleBackColor = true;
            // 
            // nudThreshold
            // 
            this.nudThreshold.Location = new System.Drawing.Point(80, 53);
            this.nudThreshold.Name = "nudThreshold";
            this.nudThreshold.Size = new System.Drawing.Size(45, 20);
            this.nudThreshold.TabIndex = 14;
            // 
            // lblThreshold
            // 
            this.lblThreshold.AutoSize = true;
            this.lblThreshold.Location = new System.Drawing.Point(3, 56);
            this.lblThreshold.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblThreshold.Name = "lblThreshold";
            this.lblThreshold.Size = new System.Drawing.Size(57, 13);
            this.lblThreshold.TabIndex = 13;
            this.lblThreshold.Tag = "Label_DiceRoller_Threshold";
            this.lblThreshold.Text = "Threshold:";
            // 
            // chkBubbleDie
            // 
            this.chkBubbleDie.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.chkBubbleDie, 2);
            this.chkBubbleDie.Location = new System.Drawing.Point(3, 29);
            this.chkBubbleDie.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkBubbleDie.Name = "chkBubbleDie";
            this.chkBubbleDie.Size = new System.Drawing.Size(248, 17);
            this.chkBubbleDie.TabIndex = 15;
            this.chkBubbleDie.Tag = "Checkbox_DiceRoller_BubbleDie";
            this.chkBubbleDie.Text = "Bubble Die (Fix Even Dicepool Glitch Chances)";
            this.chkBubbleDie.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.lblRoll, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.nudDice, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblD6, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkRuleOf6, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.cmdReroll, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.lstResults, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkCinematicGameplay, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.cboMethod, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmdRollDice, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(440, 257);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 3);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel2.Controls.Add(this.lblResults, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.lblResultsLabel, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.nudGremlins, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.nudThreshold, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.chkBubbleDie, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblThreshold, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.lblGremlins, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.chkRushJob, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(132, 58);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(308, 199);
            this.tableLayoutPanel2.TabIndex = 12;
            // 
            // lblResults
            // 
            this.lblResults.AutoSize = true;
            this.lblResults.Location = new System.Drawing.Point(80, 108);
            this.lblResults.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(45, 13);
            this.lblResults.TabIndex = 16;
            this.lblResults.Tag = "[Results]";
            this.lblResults.Text = "Results:";
            // 
            // frmDiceRoller
            // 
            this.AcceptButton = this.cmdRollDice;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDiceRoller";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_DiceRoller";
            this.Text = "Dice Roller";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDiceRoller_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nudDice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGremlins)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblRoll;
        private System.Windows.Forms.NumericUpDown nudDice;
        private System.Windows.Forms.Label lblD6;
        private System.Windows.Forms.Button cmdRollDice;
        private System.Windows.Forms.CheckBox chkRuleOf6;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.Label lblResultsLabel;
        private System.Windows.Forms.Label lblGremlins;
        private System.Windows.Forms.NumericUpDown nudGremlins;
        private ElasticComboBox cboMethod;
        private System.Windows.Forms.CheckBox chkCinematicGameplay;
        private System.Windows.Forms.Button cmdReroll;
        private System.Windows.Forms.CheckBox chkRushJob;
        private System.Windows.Forms.NumericUpDown nudThreshold;
        private System.Windows.Forms.Label lblThreshold;
        private System.Windows.Forms.CheckBox chkBubbleDie;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lblResults;
    }
}
