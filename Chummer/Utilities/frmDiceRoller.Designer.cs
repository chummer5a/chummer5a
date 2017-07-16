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
            this.lblResults = new System.Windows.Forms.Label();
            this.lblGremlins = new System.Windows.Forms.Label();
            this.nudGremlins = new System.Windows.Forms.NumericUpDown();
            this.cboMethod = new System.Windows.Forms.ComboBox();
            this.chkCinematicGameplay = new System.Windows.Forms.CheckBox();
            this.cmdReroll = new System.Windows.Forms.Button();
            this.chkRushJob = new System.Windows.Forms.CheckBox();
            this.nudThreshold = new System.Windows.Forms.NumericUpDown();
            this.lblThreshold = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudDice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGremlins)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRoll
            // 
            this.lblRoll.AutoSize = true;
            this.lblRoll.Location = new System.Drawing.Point(12, 9);
            this.lblRoll.Name = "lblRoll";
            this.lblRoll.Size = new System.Drawing.Size(25, 13);
            this.lblRoll.TabIndex = 0;
            this.lblRoll.Tag = "String_DiceRoller_Roll";
            this.lblRoll.Text = "Roll";
            // 
            // nudDice
            // 
            this.nudDice.Location = new System.Drawing.Point(43, 7);
            this.nudDice.Name = "nudDice";
            this.nudDice.Size = new System.Drawing.Size(42, 20);
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
            this.lblD6.Location = new System.Drawing.Point(91, 9);
            this.lblD6.Name = "lblD6";
            this.lblD6.Size = new System.Drawing.Size(21, 13);
            this.lblD6.TabIndex = 2;
            this.lblD6.Text = "D6";
            // 
            // cmdRollDice
            // 
            this.cmdRollDice.AutoSize = true;
            this.cmdRollDice.Location = new System.Drawing.Point(250, 4);
            this.cmdRollDice.Name = "cmdRollDice";
            this.cmdRollDice.Size = new System.Drawing.Size(86, 23);
            this.cmdRollDice.TabIndex = 10;
            this.cmdRollDice.Tag = "Button_DiceRoller_Roll";
            this.cmdRollDice.Text = "&Roll";
            this.cmdRollDice.UseVisualStyleBackColor = true;
            this.cmdRollDice.Click += new System.EventHandler(this.cmdRollDice_Click);
            // 
            // chkRuleOf6
            // 
            this.chkRuleOf6.AutoSize = true;
            this.chkRuleOf6.Location = new System.Drawing.Point(15, 33);
            this.chkRuleOf6.Name = "chkRuleOf6";
            this.chkRuleOf6.Size = new System.Drawing.Size(97, 17);
            this.chkRuleOf6.TabIndex = 4;
            this.chkRuleOf6.Tag = "Checkbox_DiceRoller_RuleOfSix";
            this.chkRuleOf6.Text = "using Rule of 6";
            this.chkRuleOf6.UseVisualStyleBackColor = true;
            // 
            // lstResults
            // 
            this.lstResults.FormattingEnabled = true;
            this.lstResults.Location = new System.Drawing.Point(12, 59);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(120, 199);
            this.lstResults.TabIndex = 9;
            // 
            // lblResults
            // 
            this.lblResults.AutoSize = true;
            this.lblResults.Location = new System.Drawing.Point(138, 138);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(45, 13);
            this.lblResults.TabIndex = 8;
            this.lblResults.Tag = "Label_DiceRoller_Result";
            this.lblResults.Text = "Results:";
            // 
            // lblGremlins
            // 
            this.lblGremlins.AutoSize = true;
            this.lblGremlins.Location = new System.Drawing.Point(138, 108);
            this.lblGremlins.Name = "lblGremlins";
            this.lblGremlins.Size = new System.Drawing.Size(50, 13);
            this.lblGremlins.TabIndex = 6;
            this.lblGremlins.Tag = "Label_DiceRoller_Gremlins";
            this.lblGremlins.Text = "Gremlins:";
            // 
            // nudGremlins
            // 
            this.nudGremlins.Location = new System.Drawing.Point(201, 106);
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
            this.cboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMethod.FormattingEnabled = true;
            this.cboMethod.Location = new System.Drawing.Point(118, 6);
            this.cboMethod.Name = "cboMethod";
            this.cboMethod.Size = new System.Drawing.Size(128, 21);
            this.cboMethod.TabIndex = 3;
            this.cboMethod.SelectedIndexChanged += new System.EventHandler(this.cboMethod_SelectedIndexChanged);
            // 
            // chkCinematicGameplay
            // 
            this.chkCinematicGameplay.AutoSize = true;
            this.chkCinematicGameplay.Location = new System.Drawing.Point(141, 33);
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
            this.cmdReroll.Location = new System.Drawing.Point(249, 33);
            this.cmdReroll.Name = "cmdReroll";
            this.cmdReroll.Size = new System.Drawing.Size(87, 23);
            this.cmdReroll.TabIndex = 11;
            this.cmdReroll.Tag = "Button_DiceRoller_RollMisses";
            this.cmdReroll.Text = "Re-Roll Misses";
            this.cmdReroll.UseVisualStyleBackColor = true;
            this.cmdReroll.Click += new System.EventHandler(this.cmdReroll_Click);
            // 
            // chkRushJob
            // 
            this.chkRushJob.AutoSize = true;
            this.chkRushJob.Location = new System.Drawing.Point(141, 56);
            this.chkRushJob.Name = "chkRushJob";
            this.chkRushJob.Size = new System.Drawing.Size(164, 17);
            this.chkRushJob.TabIndex = 12;
            this.chkRushJob.Tag = "Checkbox_DiceRoller_RushedJob";
            this.chkRushJob.Text = "Rushed Job (Glitch on 1 or 2)";
            this.chkRushJob.UseVisualStyleBackColor = true;
            // 
            // nudThreshold
            // 
            this.nudThreshold.Location = new System.Drawing.Point(201, 80);
            this.nudThreshold.Name = "nudThreshold";
            this.nudThreshold.Size = new System.Drawing.Size(45, 20);
            this.nudThreshold.TabIndex = 14;
            // 
            // lblThreshold
            // 
            this.lblThreshold.AutoSize = true;
            this.lblThreshold.Location = new System.Drawing.Point(138, 82);
            this.lblThreshold.Name = "lblThreshold";
            this.lblThreshold.Size = new System.Drawing.Size(57, 13);
            this.lblThreshold.TabIndex = 13;
            this.lblThreshold.Tag = "Label_DiceRoller_Threshold";
            this.lblThreshold.Text = "Threshold:";
            // 
            // frmDiceRoller
            // 
            this.AcceptButton = this.cmdRollDice;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 268);
            this.Controls.Add(this.nudThreshold);
            this.Controls.Add(this.lblThreshold);
            this.Controls.Add(this.chkRushJob);
            this.Controls.Add(this.cmdReroll);
            this.Controls.Add(this.chkCinematicGameplay);
            this.Controls.Add(this.cboMethod);
            this.Controls.Add(this.nudGremlins);
            this.Controls.Add(this.lblGremlins);
            this.Controls.Add(this.lblResults);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.chkRuleOf6);
            this.Controls.Add(this.cmdRollDice);
            this.Controls.Add(this.lblD6);
            this.Controls.Add(this.nudDice);
            this.Controls.Add(this.lblRoll);
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRoll;
        private System.Windows.Forms.NumericUpDown nudDice;
        private System.Windows.Forms.Label lblD6;
        private System.Windows.Forms.Button cmdRollDice;
        private System.Windows.Forms.CheckBox chkRuleOf6;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.Label lblResults;
        private System.Windows.Forms.Label lblGremlins;
        private System.Windows.Forms.NumericUpDown nudGremlins;
        private System.Windows.Forms.ComboBox cboMethod;
        private System.Windows.Forms.CheckBox chkCinematicGameplay;
        private System.Windows.Forms.Button cmdReroll;
        private System.Windows.Forms.CheckBox chkRushJob;
        private System.Windows.Forms.NumericUpDown nudThreshold;
        private System.Windows.Forms.Label lblThreshold;
    }
}