namespace Chummer
{
    partial class frmSelectMentorSpirit
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
            this.lblDisadvantage = new System.Windows.Forms.Label();
            this.lblDisadvantageLabel = new System.Windows.Forms.Label();
            this.lblAdvantage = new System.Windows.Forms.Label();
            this.lblAdvantageLabel = new System.Windows.Forms.Label();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lstMentor = new System.Windows.Forms.ListBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.lblChoice1 = new System.Windows.Forms.Label();
            this.lblChoice2 = new System.Windows.Forms.Label();
            this.cboChoice1 = new Chummer.ElasticComboBox();
            this.cboChoice2 = new Chummer.ElasticComboBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblBonus1 = new System.Windows.Forms.Label();
            this.lblBonusText1 = new System.Windows.Forms.Label();
            this.lblBonusText2 = new System.Windows.Forms.Label();
            this.lblBonus2 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDisadvantage
            // 
            this.lblDisadvantage.AutoSize = true;
            this.lblDisadvantage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDisadvantage.Location = new System.Drawing.Point(85, 161);
            this.lblDisadvantage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDisadvantage.Name = "lblDisadvantage";
            this.lblDisadvantage.Size = new System.Drawing.Size(372, 13);
            this.lblDisadvantage.TabIndex = 10;
            this.lblDisadvantage.Text = "[Disadvantage]";
            // 
            // lblDisadvantageLabel
            // 
            this.lblDisadvantageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDisadvantageLabel.AutoSize = true;
            this.lblDisadvantageLabel.Location = new System.Drawing.Point(3, 161);
            this.lblDisadvantageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDisadvantageLabel.Name = "lblDisadvantageLabel";
            this.lblDisadvantageLabel.Size = new System.Drawing.Size(76, 13);
            this.lblDisadvantageLabel.TabIndex = 9;
            this.lblDisadvantageLabel.Tag = "Label_SelectMetamagic_Disadvantage";
            this.lblDisadvantageLabel.Text = "Disadvantage:";
            // 
            // lblAdvantage
            // 
            this.lblAdvantage.AutoSize = true;
            this.lblAdvantage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAdvantage.Location = new System.Drawing.Point(85, 136);
            this.lblAdvantage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAdvantage.Name = "lblAdvantage";
            this.lblAdvantage.Size = new System.Drawing.Size(372, 13);
            this.lblAdvantage.TabIndex = 4;
            this.lblAdvantage.Text = "[Advantage]";
            // 
            // lblAdvantageLabel
            // 
            this.lblAdvantageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAdvantageLabel.AutoSize = true;
            this.lblAdvantageLabel.Location = new System.Drawing.Point(17, 136);
            this.lblAdvantageLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblAdvantageLabel.Name = "lblAdvantageLabel";
            this.lblAdvantageLabel.Size = new System.Drawing.Size(62, 13);
            this.lblAdvantageLabel.TabIndex = 3;
            this.lblAdvantageLabel.Tag = "Label_SelectMentorSpirit_Advantage";
            this.lblAdvantageLabel.Text = "Advantage:";
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(59, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(50, 23);
            this.cmdOK.TabIndex = 13;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.AcceptForm);
            // 
            // lstMentor
            // 
            this.lstMentor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMentor.FormattingEnabled = true;
            this.lstMentor.Location = new System.Drawing.Point(3, 3);
            this.lstMentor.Name = "lstMentor";
            this.tlpMain.SetRowSpan(this.lstMentor, 2);
            this.lstMentor.Size = new System.Drawing.Size(300, 537);
            this.lstMentor.TabIndex = 0;
            this.lstMentor.SelectedIndexChanged += new System.EventHandler(this.lstMentor_SelectedIndexChanged);
            this.lstMentor.DoubleClick += new System.EventHandler(this.AcceptForm);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(85, 495);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 12;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(35, 495);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 11;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // lblChoice1
            // 
            this.lblChoice1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblChoice1.AutoSize = true;
            this.lblChoice1.Location = new System.Drawing.Point(10, 32);
            this.lblChoice1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblChoice1.Name = "lblChoice1";
            this.lblChoice1.Size = new System.Drawing.Size(69, 13);
            this.lblChoice1.TabIndex = 5;
            this.lblChoice1.Tag = "Label_SelectMentor_ChooseOne";
            this.lblChoice1.Text = "Choose One:";
            // 
            // lblChoice2
            // 
            this.lblChoice2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblChoice2.AutoSize = true;
            this.lblChoice2.Location = new System.Drawing.Point(10, 84);
            this.lblChoice2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblChoice2.Name = "lblChoice2";
            this.lblChoice2.Size = new System.Drawing.Size(69, 13);
            this.lblChoice2.TabIndex = 7;
            this.lblChoice2.Tag = "Label_SelectMentor_ChooseOne";
            this.lblChoice2.Text = "Choose One:";
            // 
            // cboChoice1
            // 
            this.cboChoice1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboChoice1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboChoice1.FormattingEnabled = true;
            this.cboChoice1.Location = new System.Drawing.Point(85, 29);
            this.cboChoice1.Name = "cboChoice1";
            this.cboChoice1.Size = new System.Drawing.Size(372, 21);
            this.cboChoice1.TabIndex = 6;
            this.cboChoice1.TooltipText = "";
            // 
            // cboChoice2
            // 
            this.cboChoice2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboChoice2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboChoice2.FormattingEnabled = true;
            this.cboChoice2.Location = new System.Drawing.Point(85, 81);
            this.cboChoice2.Name = "cboChoice2";
            this.cboChoice2.Size = new System.Drawing.Size(372, 21);
            this.cboChoice2.TabIndex = 8;
            this.cboChoice2.TooltipText = "";
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
            this.cmdCancel.TabIndex = 15;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpMain.Controls.Add(this.lstMentor, 0, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 16;
            // 
            // lblBonus1
            // 
            this.lblBonus1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBonus1.AutoSize = true;
            this.lblBonus1.Location = new System.Drawing.Point(39, 59);
            this.lblBonus1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBonus1.Name = "lblBonus1";
            this.lblBonus1.Size = new System.Drawing.Size(40, 13);
            this.lblBonus1.TabIndex = 76;
            this.lblBonus1.Tag = "Label_SelectMentor_Bonus";
            this.lblBonus1.Text = "Bonus:";
            // 
            // lblBonusText1
            // 
            this.lblBonusText1.AutoSize = true;
            this.lblBonusText1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBonusText1.Location = new System.Drawing.Point(85, 59);
            this.lblBonusText1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBonusText1.Name = "lblBonusText1";
            this.lblBonusText1.Size = new System.Drawing.Size(372, 13);
            this.lblBonusText1.TabIndex = 77;
            this.lblBonusText1.Text = "[Bonus Text]";
            // 
            // lblBonusText2
            // 
            this.lblBonusText2.AutoSize = true;
            this.lblBonusText2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBonusText2.Location = new System.Drawing.Point(85, 111);
            this.lblBonusText2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBonusText2.Name = "lblBonusText2";
            this.lblBonusText2.Size = new System.Drawing.Size(372, 13);
            this.lblBonusText2.TabIndex = 79;
            this.lblBonusText2.Text = "[Bonus Text]";
            // 
            // lblBonus2
            // 
            this.lblBonus2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBonus2.AutoSize = true;
            this.lblBonus2.Location = new System.Drawing.Point(39, 111);
            this.lblBonus2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblBonus2.Name = "lblBonus2";
            this.lblBonus2.Size = new System.Drawing.Size(40, 13);
            this.lblBonus2.TabIndex = 78;
            this.lblBonus2.Tag = "Label_SelectMentor_Bonus";
            this.lblBonus2.Text = "Bonus:";
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(85, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(372, 20);
            this.txtSearch.TabIndex = 72;
            this.txtSearch.TextChanged += new System.EventHandler(this.RefreshMentorsList);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(35, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 69;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
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
            this.tlpButtons.Location = new System.Drawing.Point(654, 514);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.Size = new System.Drawing.Size(112, 29);
            this.tlpButtons.TabIndex = 75;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoScroll = true;
            this.tlpRight.ColumnCount = 2;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Controls.Add(this.cboChoice1, 1, 1);
            this.tlpRight.Controls.Add(this.lblBonusText1, 1, 2);
            this.tlpRight.Controls.Add(this.lblBonusText2, 1, 4);
            this.tlpRight.Controls.Add(this.cboChoice2, 1, 3);
            this.tlpRight.Controls.Add(this.lblDisadvantage, 1, 6);
            this.tlpRight.Controls.Add(this.lblBonus2, 0, 4);
            this.tlpRight.Controls.Add(this.lblChoice2, 0, 3);
            this.tlpRight.Controls.Add(this.lblBonus1, 0, 2);
            this.tlpRight.Controls.Add(this.lblDisadvantageLabel, 0, 6);
            this.tlpRight.Controls.Add(this.lblChoice1, 0, 1);
            this.tlpRight.Controls.Add(this.lblAdvantage, 1, 5);
            this.tlpRight.Controls.Add(this.lblAdvantageLabel, 0, 5);
            this.tlpRight.Controls.Add(this.lblSearchLabel, 0, 0);
            this.tlpRight.Controls.Add(this.txtSearch, 1, 0);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 8);
            this.tlpRight.Controls.Add(this.lblSource, 1, 8);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(306, 0);
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
            this.tlpRight.Size = new System.Drawing.Size(460, 514);
            this.tlpRight.TabIndex = 80;
            // 
            // frmSelectMentorSpirit
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectMentorSpirit";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectMentorSpirit";
            this.Text = "Select a Mentor Spirit";
            this.Load += new System.EventHandler(this.RefreshMentorsList);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDisadvantage;
        private System.Windows.Forms.Label lblDisadvantageLabel;
        private System.Windows.Forms.Label lblAdvantage;
        private System.Windows.Forms.Label lblAdvantageLabel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.ListBox lstMentor;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private System.Windows.Forms.Label lblChoice1;
        private System.Windows.Forms.Label lblChoice2;
        private ElasticComboBox cboChoice1;
        private ElasticComboBox cboChoice2;
        private System.Windows.Forms.Button cmdCancel;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.TextBox txtSearch;
        private BufferedTableLayoutPanel tlpButtons;
        private System.Windows.Forms.Label lblBonus1;
        private System.Windows.Forms.Label lblBonusText1;
        private System.Windows.Forms.Label lblBonusText2;
        private System.Windows.Forms.Label lblBonus2;
        private BufferedTableLayoutPanel tlpRight;
    }
}
