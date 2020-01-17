namespace Chummer
{
	partial class frmNewOptions
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
            this.textBox1 = new Chummer.helpers.TextBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.chkStartupFullscreen = new System.Windows.Forms.CheckBox();
            this.chkAutomaticUpdate = new System.Windows.Forms.CheckBox();
            this.chkSingleDiceRoller = new System.Windows.Forms.CheckBox();
            this.chkDatesIncludeTime = new System.Windows.Forms.CheckBox();
            this.chkLocalisedUpdatesOnly = new System.Windows.Forms.CheckBox();
            this.chkUseLogging = new System.Windows.Forms.CheckBox();
            this.chkLifeModule = new System.Windows.Forms.CheckBox();
            this.chkOmaeEnabled = new System.Windows.Forms.CheckBox();
            this.chkPreferNightlyBuilds = new System.Windows.Forms.CheckBox();
            this.cboXSLT = new System.Windows.Forms.ComboBox();
            this.lblXSLT = new System.Windows.Forms.Label();
            this.cmdVerifyData = new System.Windows.Forms.Button();
            this.cmdVerify = new System.Windows.Forms.Button();
            this.cboLanguage = new System.Windows.Forms.ComboBox();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnDefault = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.ForeColor = System.Drawing.Color.Gray;
            this.textBox1.Location = new System.Drawing.Point(8, 8);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(160, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.Tag = "String_Search";
            this.textBox1.WatermarkActive = true;
            this.textBox1.WatermarkText = "";
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treeView1.Location = new System.Drawing.Point(8, 32);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(160, 507);
            this.treeView1.TabIndex = 7;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // chkStartupFullscreen
            // 
            this.chkStartupFullscreen.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chkStartupFullscreen, true);
            this.chkStartupFullscreen.Location = new System.Drawing.Point(8, 248);
            this.chkStartupFullscreen.Name = "chkStartupFullscreen";
            this.chkStartupFullscreen.Size = new System.Drawing.Size(154, 17);
            this.chkStartupFullscreen.TabIndex = 29;
            this.chkStartupFullscreen.Tag = "Checkbox_Options_StartupFullscreen";
            this.chkStartupFullscreen.Text = "Start Chummer in fullscreen";
            this.chkStartupFullscreen.UseVisualStyleBackColor = true;
            // 
            // chkAutomaticUpdate
            // 
            this.chkAutomaticUpdate.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chkAutomaticUpdate, true);
            this.chkAutomaticUpdate.Location = new System.Drawing.Point(8, 225);
            this.chkAutomaticUpdate.Name = "chkAutomaticUpdate";
            this.chkAutomaticUpdate.Size = new System.Drawing.Size(116, 17);
            this.chkAutomaticUpdate.TabIndex = 27;
            this.chkAutomaticUpdate.Tag = "Checkbox_Options_AutomaticUpdates";
            this.chkAutomaticUpdate.Text = "Automatic Updates";
            this.chkAutomaticUpdate.UseVisualStyleBackColor = true;
            // 
            // chkSingleDiceRoller
            // 
            this.chkSingleDiceRoller.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chkSingleDiceRoller, true);
            this.chkSingleDiceRoller.Location = new System.Drawing.Point(8, 202);
            this.chkSingleDiceRoller.Name = "chkSingleDiceRoller";
            this.chkSingleDiceRoller.Size = new System.Drawing.Size(251, 17);
            this.chkSingleDiceRoller.TabIndex = 30;
            this.chkSingleDiceRoller.Tag = "Checkbox_Options_SingleDiceRoller";
            this.chkSingleDiceRoller.Text = "Use a single instance of the Dice Roller window";
            this.chkSingleDiceRoller.UseVisualStyleBackColor = true;
            // 
            // chkDatesIncludeTime
            // 
            this.chkDatesIncludeTime.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chkDatesIncludeTime, true);
            this.chkDatesIncludeTime.Location = new System.Drawing.Point(8, 179);
            this.chkDatesIncludeTime.Name = "chkDatesIncludeTime";
            this.chkDatesIncludeTime.Size = new System.Drawing.Size(189, 17);
            this.chkDatesIncludeTime.TabIndex = 31;
            this.chkDatesIncludeTime.Tag = "Checkbox_Options_DatesIncludeTime";
            this.chkDatesIncludeTime.Text = "Expense dates should include time";
            this.chkDatesIncludeTime.UseVisualStyleBackColor = true;
            // 
            // chkLocalisedUpdatesOnly
            // 
            this.chkLocalisedUpdatesOnly.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chkLocalisedUpdatesOnly, true);
            this.chkLocalisedUpdatesOnly.Location = new System.Drawing.Point(8, 156);
            this.chkLocalisedUpdatesOnly.Name = "chkLocalisedUpdatesOnly";
            this.chkLocalisedUpdatesOnly.Size = new System.Drawing.Size(254, 17);
            this.chkLocalisedUpdatesOnly.TabIndex = 28;
            this.chkLocalisedUpdatesOnly.Tag = "Checkbox_Options_LocalisedUpdatesOnly";
            this.chkLocalisedUpdatesOnly.Text = "Only download updates in my selected language";
            this.chkLocalisedUpdatesOnly.UseVisualStyleBackColor = true;
            // 
            // chkUseLogging
            // 
            this.chkUseLogging.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chkUseLogging, true);
            this.chkUseLogging.Location = new System.Drawing.Point(8, 133);
            this.chkUseLogging.Name = "chkUseLogging";
            this.chkUseLogging.Size = new System.Drawing.Size(121, 17);
            this.chkUseLogging.TabIndex = 26;
            this.chkUseLogging.Tag = "Checkbox_Options_UseLogging";
            this.chkUseLogging.Text = "Use Debug Logging";
            this.chkUseLogging.UseVisualStyleBackColor = true;
            // 
            // chkLifeModule
            // 
            this.chkLifeModule.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chkLifeModule, true);
            this.chkLifeModule.Location = new System.Drawing.Point(8, 110);
            this.chkLifeModule.Name = "chkLifeModule";
            this.chkLifeModule.Size = new System.Drawing.Size(117, 17);
            this.chkLifeModule.TabIndex = 32;
            this.chkLifeModule.Tag = "Checkbox_Options_UseLifeModule";
            this.chkLifeModule.Text = "Life modules visible";
            this.chkLifeModule.UseVisualStyleBackColor = true;
            // 
            // chkOmaeEnabled
            // 
            this.chkOmaeEnabled.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chkOmaeEnabled, true);
            this.chkOmaeEnabled.Location = new System.Drawing.Point(8, 87);
            this.chkOmaeEnabled.Name = "chkOmaeEnabled";
            this.chkOmaeEnabled.Size = new System.Drawing.Size(101, 17);
            this.chkOmaeEnabled.TabIndex = 33;
            this.chkOmaeEnabled.Tag = "Checkbox_Options_OmaeEnabled";
            this.chkOmaeEnabled.Text = "[Omae enabled]";
            this.chkOmaeEnabled.UseVisualStyleBackColor = true;
            // 
            // chkPreferNightlyBuilds
            // 
            this.chkPreferNightlyBuilds.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chkPreferNightlyBuilds, true);
            this.chkPreferNightlyBuilds.Location = new System.Drawing.Point(8, 64);
            this.chkPreferNightlyBuilds.Name = "chkPreferNightlyBuilds";
            this.chkPreferNightlyBuilds.Size = new System.Drawing.Size(120, 17);
            this.chkPreferNightlyBuilds.TabIndex = 34;
            this.chkPreferNightlyBuilds.Tag = "Checkbox_Options_PreferNightlyBuilds";
            this.chkPreferNightlyBuilds.Text = "Prefer Nightly Builds";
            this.chkPreferNightlyBuilds.UseVisualStyleBackColor = true;
            // 
            // cboXSLT
            // 
            this.cboXSLT.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.flowLayoutPanel1.SetFlowBreak(this.cboXSLT, true);
            this.cboXSLT.FormattingEnabled = true;
            this.cboXSLT.Location = new System.Drawing.Point(138, 37);
            this.cboXSLT.Name = "cboXSLT";
            this.cboXSLT.Size = new System.Drawing.Size(266, 21);
            this.cboXSLT.TabIndex = 10;
            // 
            // lblXSLT
            // 
            this.lblXSLT.AutoSize = true;
            this.lblXSLT.Location = new System.Drawing.Point(8, 34);
            this.lblXSLT.Name = "lblXSLT";
            this.lblXSLT.Size = new System.Drawing.Size(124, 13);
            this.lblXSLT.TabIndex = 9;
            this.lblXSLT.Tag = "Label_Options_DefaultCharacterSheet";
            this.lblXSLT.Text = "Default Character Sheet:";
            // 
            // cmdVerifyData
            // 
            this.cmdVerifyData.Enabled = false;
            this.flowLayoutPanel1.SetFlowBreak(this.cmdVerifyData, true);
            this.cmdVerifyData.Location = new System.Drawing.Point(321, 8);
            this.cmdVerifyData.Name = "cmdVerifyData";
            this.cmdVerifyData.Size = new System.Drawing.Size(90, 23);
            this.cmdVerifyData.TabIndex = 7;
            this.cmdVerifyData.Text = "Verify Data File";
            this.cmdVerifyData.UseVisualStyleBackColor = true;
            // 
            // cmdVerify
            // 
            this.cmdVerify.Enabled = false;
            this.cmdVerify.Location = new System.Drawing.Point(240, 8);
            this.cmdVerify.Name = "cmdVerify";
            this.cmdVerify.Size = new System.Drawing.Size(75, 23);
            this.cmdVerify.TabIndex = 6;
            this.cmdVerify.Text = "Verify";
            this.cmdVerify.UseVisualStyleBackColor = true;
            // 
            // cboLanguage
            // 
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Location = new System.Drawing.Point(72, 8);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(162, 21);
            this.cboLanguage.TabIndex = 5;
            // 
            // lblLanguage
            // 
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new System.Drawing.Point(8, 5);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.lblLanguage.Size = new System.Drawing.Size(58, 18);
            this.lblLanguage.TabIndex = 4;
            this.lblLanguage.Tag = "Label_Options_Language";
            this.lblLanguage.Text = "Language:";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Controls.Add(this.lblLanguage);
            this.flowLayoutPanel1.Controls.Add(this.cboLanguage);
            this.flowLayoutPanel1.Controls.Add(this.cmdVerify);
            this.flowLayoutPanel1.Controls.Add(this.cmdVerifyData);
            this.flowLayoutPanel1.Controls.Add(this.lblXSLT);
            this.flowLayoutPanel1.Controls.Add(this.cboXSLT);
            this.flowLayoutPanel1.Controls.Add(this.chkPreferNightlyBuilds);
            this.flowLayoutPanel1.Controls.Add(this.chkOmaeEnabled);
            this.flowLayoutPanel1.Controls.Add(this.chkLifeModule);
            this.flowLayoutPanel1.Controls.Add(this.chkUseLogging);
            this.flowLayoutPanel1.Controls.Add(this.chkLocalisedUpdatesOnly);
            this.flowLayoutPanel1.Controls.Add(this.chkDatesIncludeTime);
            this.flowLayoutPanel1.Controls.Add(this.chkSingleDiceRoller);
            this.flowLayoutPanel1.Controls.Add(this.chkAutomaticUpdate);
            this.flowLayoutPanel1.Controls.Add(this.chkStartupFullscreen);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(176, 8);
            this.flowLayoutPanel1.MinimumSize = new System.Drawing.Size(300, 300);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(5);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(967, 531);
            this.flowLayoutPanel1.TabIndex = 5;
            this.flowLayoutPanel1.Tag = "Default";
            this.flowLayoutPanel1.Visible = false;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(1068, 545);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 8;
            this.btnOK.Tag = "String_OK";
            this.btnOK.Text = "[OK]";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(987, 545);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Tag = "String_Cancel";
            this.btnCancel.Text = "[Cancel]";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.Location = new System.Drawing.Point(906, 545);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 10;
            this.btnReset.Tag = "Button_Reset";
            this.btnReset.Text = "[Reset]";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnDefault
            // 
            this.btnDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDefault.Location = new System.Drawing.Point(8, 545);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(160, 23);
            this.btnDefault.TabIndex = 11;
            this.btnDefault.Tag = "Button_RestoreDefaultSettings";
            this.btnDefault.Text = "[Restore Default Settings]";
            this.btnDefault.UseVisualStyleBackColor = true;
            this.btnDefault.Click += new System.EventHandler(this.btnDefault_Click);
            // 
            // frmNewOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1151, 580);
            this.Controls.Add(this.btnDefault);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.textBox1);
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "frmNewOptions";
            this.Tag = "Title_Options";
            this.Text = "Options";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private Chummer.helpers.TextBox textBox1;
		private UI.Options.OptionsNumberControl optionsNumberControl1;
		private UI.Options.OptionsNumberControl optionsNumberControl2;
		private UI.Options.OptionsNumberControl optionsNumberControl3;
        private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.CheckBox chkStartupFullscreen;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label lblLanguage;
		private System.Windows.Forms.ComboBox cboLanguage;
		private System.Windows.Forms.Button cmdVerify;
		private System.Windows.Forms.Button cmdVerifyData;
		private System.Windows.Forms.Label lblXSLT;
		private System.Windows.Forms.ComboBox cboXSLT;
		private System.Windows.Forms.CheckBox chkPreferNightlyBuilds;
		private System.Windows.Forms.CheckBox chkOmaeEnabled;
		private System.Windows.Forms.CheckBox chkLifeModule;
		private System.Windows.Forms.CheckBox chkUseLogging;
		private System.Windows.Forms.CheckBox chkLocalisedUpdatesOnly;
		private System.Windows.Forms.CheckBox chkDatesIncludeTime;
		private System.Windows.Forms.CheckBox chkSingleDiceRoller;
		private System.Windows.Forms.CheckBox chkAutomaticUpdate;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnDefault;
    }
}