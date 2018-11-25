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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDiceRoller));
            this.lblRoll = new System.Windows.Forms.Label();
            this.nudDice = new System.Windows.Forms.NumericUpDown();
            this.lblD6 = new System.Windows.Forms.Label();
            this.cmdRollDice = new System.Windows.Forms.Button();
            this.cboMethod = new Chummer.ElasticComboBox();
            this.cmdReroll = new System.Windows.Forms.Button();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpRollParameters = new System.Windows.Forms.FlowLayoutPanel();
            this.chkRushJob = new System.Windows.Forms.CheckBox();
            this.lblGremlins = new System.Windows.Forms.Label();
            this.lblThreshold = new System.Windows.Forms.Label();
            this.chkBubbleDie = new System.Windows.Forms.CheckBox();
            this.chkCinematicGameplay = new System.Windows.Forms.CheckBox();
            this.nudThreshold = new System.Windows.Forms.NumericUpDown();
            this.nudGremlins = new System.Windows.Forms.NumericUpDown();
            this.lblResultsLabel = new System.Windows.Forms.Label();
            this.lblResults = new System.Windows.Forms.Label();
            this.chkRuleOf6 = new System.Windows.Forms.CheckBox();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpTopBar = new System.Windows.Forms.TableLayoutPanel();
            this.flpRollControls = new System.Windows.Forms.FlowLayoutPanel();
            this.chkVariableGlitch = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudDice)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flpRollParameters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGremlins)).BeginInit();
            this.tlpTopBar.SuspendLayout();
            this.flpRollControls.SuspendLayout();
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
            this.nudDice.Location = new System.Drawing.Point(34, 3);
            this.nudDice.Name = "nudDice";
            this.nudDice.Size = new System.Drawing.Size(100, 20);
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
            this.lblD6.Location = new System.Drawing.Point(140, 6);
            this.lblD6.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblD6.Name = "lblD6";
            this.lblD6.Size = new System.Drawing.Size(21, 13);
            this.lblD6.TabIndex = 2;
            this.lblD6.Tag = "String_D6";
            this.lblD6.Text = "D6";
            // 
            // cmdRollDice
            // 
            this.cmdRollDice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRollDice.AutoSize = true;
            this.cmdRollDice.Location = new System.Drawing.Point(209, 3);
            this.cmdRollDice.Name = "cmdRollDice";
            this.cmdRollDice.Size = new System.Drawing.Size(50, 23);
            this.cmdRollDice.TabIndex = 10;
            this.cmdRollDice.Tag = "Button_DiceRoller_Roll";
            this.cmdRollDice.Text = "&Roll";
            this.cmdRollDice.UseVisualStyleBackColor = true;
            this.cmdRollDice.Click += new System.EventHandler(this.cmdRollDice_Click);
            // 
            // cboMethod
            // 
            this.cboMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMethod.FormattingEnabled = true;
            this.cboMethod.Location = new System.Drawing.Point(3, 3);
            this.cboMethod.Name = "cboMethod";
            this.cboMethod.Size = new System.Drawing.Size(200, 21);
            this.cboMethod.TabIndex = 3;
            this.cboMethod.TooltipText = "";
            this.cboMethod.SelectedIndexChanged += new System.EventHandler(this.cboMethod_SelectedIndexChanged);
            // 
            // cmdReroll
            // 
            this.cmdReroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdReroll.AutoSize = true;
            this.cmdReroll.Location = new System.Drawing.Point(265, 3);
            this.cmdReroll.Name = "cmdReroll";
            this.cmdReroll.Size = new System.Drawing.Size(100, 23);
            this.cmdReroll.TabIndex = 11;
            this.cmdReroll.Tag = "Button_DiceRoller_RollMisses";
            this.cmdReroll.Text = "Re-Roll Misses";
            this.cmdReroll.UseVisualStyleBackColor = true;
            this.cmdReroll.Click += new System.EventHandler(this.cmdReroll_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 151F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.lstResults, 0, 1);
            this.tlpMain.Controls.Add(this.lblResultsLabel, 1, 8);
            this.tlpMain.Controls.Add(this.nudGremlins, 2, 7);
            this.tlpMain.Controls.Add(this.lblGremlins, 1, 7);
            this.tlpMain.Controls.Add(this.nudThreshold, 2, 6);
            this.tlpMain.Controls.Add(this.chkRuleOf6, 1, 1);
            this.tlpMain.Controls.Add(this.lblThreshold, 1, 6);
            this.tlpMain.Controls.Add(this.chkCinematicGameplay, 1, 2);
            this.tlpMain.Controls.Add(this.chkRushJob, 1, 3);
            this.tlpMain.Controls.Add(this.tlpTopBar, 0, 0);
            this.tlpMain.Controls.Add(this.lblResults, 2, 8);
            this.tlpMain.Controls.Add(this.chkVariableGlitch, 1, 5);
            this.tlpMain.Controls.Add(this.chkBubbleDie, 1, 4);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 9;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(606, 423);
            this.tlpMain.TabIndex = 16;
            // 
            // flpRollParameters
            // 
            this.flpRollParameters.AutoSize = true;
            this.flpRollParameters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRollParameters.Controls.Add(this.lblRoll);
            this.flpRollParameters.Controls.Add(this.nudDice);
            this.flpRollParameters.Controls.Add(this.lblD6);
            this.flpRollParameters.Controls.Add(this.flowLayoutPanel2);
            this.flpRollParameters.Location = new System.Drawing.Point(0, 0);
            this.flpRollParameters.Margin = new System.Windows.Forms.Padding(0);
            this.flpRollParameters.Name = "flpRollParameters";
            this.flpRollParameters.Size = new System.Drawing.Size(170, 26);
            this.flpRollParameters.TabIndex = 13;
            this.flpRollParameters.WrapContents = false;
            // 
            // chkRushJob
            // 
            this.chkRushJob.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.chkRushJob, 2);
            this.chkRushJob.Location = new System.Drawing.Point(154, 83);
            this.chkRushJob.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkRushJob.Name = "chkRushJob";
            this.chkRushJob.Size = new System.Drawing.Size(164, 17);
            this.chkRushJob.TabIndex = 12;
            this.chkRushJob.Tag = "Checkbox_DiceRoller_RushedJob";
            this.chkRushJob.Text = "Rushed Job (Glitch on 1 or 2)";
            this.chkRushJob.UseVisualStyleBackColor = true;
            // 
            // lblGremlins
            // 
            this.lblGremlins.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGremlins.AutoSize = true;
            this.lblGremlins.Location = new System.Drawing.Point(161, 186);
            this.lblGremlins.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGremlins.Name = "lblGremlins";
            this.lblGremlins.Size = new System.Drawing.Size(50, 13);
            this.lblGremlins.TabIndex = 6;
            this.lblGremlins.Tag = "Label_DiceRoller_Gremlins";
            this.lblGremlins.Text = "Gremlins:";
            // 
            // lblThreshold
            // 
            this.lblThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblThreshold.AutoSize = true;
            this.lblThreshold.Location = new System.Drawing.Point(154, 160);
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
            this.tlpMain.SetColumnSpan(this.chkBubbleDie, 2);
            this.chkBubbleDie.Location = new System.Drawing.Point(154, 108);
            this.chkBubbleDie.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkBubbleDie.Name = "chkBubbleDie";
            this.chkBubbleDie.Size = new System.Drawing.Size(248, 17);
            this.chkBubbleDie.TabIndex = 15;
            this.chkBubbleDie.Tag = "Checkbox_DiceRoller_BubbleDie";
            this.chkBubbleDie.Text = "Bubble Die (Fix Even Dicepool Glitch Chances)";
            this.chkBubbleDie.UseVisualStyleBackColor = true;
            // 
            // chkCinematicGameplay
            // 
            this.chkCinematicGameplay.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.chkCinematicGameplay, 2);
            this.chkCinematicGameplay.Location = new System.Drawing.Point(154, 58);
            this.chkCinematicGameplay.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkCinematicGameplay.Name = "chkCinematicGameplay";
            this.chkCinematicGameplay.Size = new System.Drawing.Size(99, 17);
            this.chkCinematicGameplay.TabIndex = 5;
            this.chkCinematicGameplay.Tag = "Checkbox_DiceRoller_CinematicGameplay";
            this.chkCinematicGameplay.Text = "Hit on 4, 5, or 6";
            this.chkCinematicGameplay.UseVisualStyleBackColor = true;
            // 
            // nudThreshold
            // 
            this.nudThreshold.Location = new System.Drawing.Point(217, 157);
            this.nudThreshold.Name = "nudThreshold";
            this.nudThreshold.Size = new System.Drawing.Size(100, 20);
            this.nudThreshold.TabIndex = 14;
            // 
            // nudGremlins
            // 
            this.nudGremlins.Location = new System.Drawing.Point(217, 183);
            this.nudGremlins.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nudGremlins.Name = "nudGremlins";
            this.nudGremlins.Size = new System.Drawing.Size(100, 20);
            this.nudGremlins.TabIndex = 7;
            // 
            // lblResultsLabel
            // 
            this.lblResultsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblResultsLabel.AutoSize = true;
            this.lblResultsLabel.Location = new System.Drawing.Point(166, 212);
            this.lblResultsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblResultsLabel.Name = "lblResultsLabel";
            this.lblResultsLabel.Size = new System.Drawing.Size(45, 13);
            this.lblResultsLabel.TabIndex = 8;
            this.lblResultsLabel.Tag = "Label_DiceRoller_Result";
            this.lblResultsLabel.Text = "Results:";
            // 
            // lblResults
            // 
            this.lblResults.AutoSize = true;
            this.lblResults.Location = new System.Drawing.Point(217, 212);
            this.lblResults.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(48, 13);
            this.lblResults.TabIndex = 16;
            this.lblResults.Tag = "";
            this.lblResults.Text = "[Results]";
            // 
            // chkRuleOf6
            // 
            this.chkRuleOf6.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.chkRuleOf6, 2);
            this.chkRuleOf6.Location = new System.Drawing.Point(154, 33);
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
            this.lstResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstResults.FormattingEnabled = true;
            this.lstResults.Location = new System.Drawing.Point(3, 32);
            this.lstResults.Name = "lstResults";
            this.tlpMain.SetRowSpan(this.lstResults, 8);
            this.lstResults.Size = new System.Drawing.Size(145, 388);
            this.lstResults.TabIndex = 9;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(167, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(0, 0);
            this.flowLayoutPanel2.TabIndex = 12;
            // 
            // tlpTopBar
            // 
            this.tlpTopBar.AutoSize = true;
            this.tlpTopBar.ColumnCount = 2;
            this.tlpMain.SetColumnSpan(this.tlpTopBar, 3);
            this.tlpTopBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTopBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTopBar.Controls.Add(this.flpRollParameters, 0, 0);
            this.tlpTopBar.Controls.Add(this.flpRollControls, 1, 0);
            this.tlpTopBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTopBar.Location = new System.Drawing.Point(0, 0);
            this.tlpTopBar.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopBar.Name = "tlpTopBar";
            this.tlpTopBar.RowCount = 1;
            this.tlpTopBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopBar.Size = new System.Drawing.Size(606, 29);
            this.tlpTopBar.TabIndex = 17;
            // 
            // flpRollControls
            // 
            this.flpRollControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flpRollControls.AutoSize = true;
            this.flpRollControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRollControls.Controls.Add(this.cmdReroll);
            this.flpRollControls.Controls.Add(this.cmdRollDice);
            this.flpRollControls.Controls.Add(this.cboMethod);
            this.flpRollControls.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpRollControls.Location = new System.Drawing.Point(238, 0);
            this.flpRollControls.Margin = new System.Windows.Forms.Padding(0);
            this.flpRollControls.Name = "flpRollControls";
            this.flpRollControls.Size = new System.Drawing.Size(368, 29);
            this.flpRollControls.TabIndex = 14;
            this.flpRollControls.WrapContents = false;
            // 
            // chkVariableGlitch
            // 
            this.chkVariableGlitch.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.chkVariableGlitch, 2);
            this.chkVariableGlitch.Location = new System.Drawing.Point(154, 133);
            this.chkVariableGlitch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkVariableGlitch.Name = "chkVariableGlitch";
            this.chkVariableGlitch.Size = new System.Drawing.Size(246, 17);
            this.chkVariableGlitch.TabIndex = 18;
            this.chkVariableGlitch.Tag = "Checkbox_DiceRoller_VariableGlitch";
            this.chkVariableGlitch.Text = "Glitch on More 1\'s than Hits, Not Half Dicepool";
            this.chkVariableGlitch.UseVisualStyleBackColor = true;
            // 
            // frmDiceRoller
            // 
            this.AcceptButton = this.cmdRollDice;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDiceRoller";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_DiceRoller";
            this.Text = "Dice Roller";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDiceRoller_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nudDice)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flpRollParameters.ResumeLayout(false);
            this.flpRollParameters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGremlins)).EndInit();
            this.tlpTopBar.ResumeLayout(false);
            this.tlpTopBar.PerformLayout();
            this.flpRollControls.ResumeLayout(false);
            this.flpRollControls.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblRoll;
        private System.Windows.Forms.NumericUpDown nudDice;
        private System.Windows.Forms.Label lblD6;
        private System.Windows.Forms.Button cmdRollDice;
        private ElasticComboBox cboMethod;
        private System.Windows.Forms.Button cmdReroll;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblResults;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.Label lblResultsLabel;
        private System.Windows.Forms.NumericUpDown nudGremlins;
        private System.Windows.Forms.Label lblGremlins;
        private System.Windows.Forms.NumericUpDown nudThreshold;
        private System.Windows.Forms.CheckBox chkRuleOf6;
        private System.Windows.Forms.Label lblThreshold;
        private System.Windows.Forms.CheckBox chkBubbleDie;
        private System.Windows.Forms.CheckBox chkCinematicGameplay;
        private System.Windows.Forms.CheckBox chkRushJob;
        private System.Windows.Forms.TableLayoutPanel tlpTopBar;
        private System.Windows.Forms.FlowLayoutPanel flpRollParameters;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flpRollControls;
        private System.Windows.Forms.CheckBox chkVariableGlitch;
    }
}
