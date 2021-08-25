namespace Chummer.UI.Powers
{
    partial class PowersTabUserControl
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
                _table?.Dispose();
                UnbindPowersTabUserControl();
                if (!(ParentForm is CharacterShared frmParent) || frmParent.CharacterObject != _objCharacter)
                    _objCharacter?.Dispose();
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
            this.cmdAddPower = new System.Windows.Forms.Button();
            this.cboDisplayFilter = new Chummer.ElasticComboBox();
            this.lblPowerPoints = new System.Windows.Forms.Label();
            this.lblPowerPointsLabel = new System.Windows.Forms.Label();
            this._tipTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.pnlPowers = new System.Windows.Forms.Panel();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdAddPower
            // 
            this.cmdAddPower.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdAddPower.AutoSize = true;
            this.cmdAddPower.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddPower.Location = new System.Drawing.Point(163, 3);
            this.cmdAddPower.Name = "cmdAddPower";
            this.cmdAddPower.Size = new System.Drawing.Size(69, 23);
            this.cmdAddPower.TabIndex = 6;
            this.cmdAddPower.Tag = "Button_AddPower";
            this.cmdAddPower.Text = "Add Power";
            this.cmdAddPower.UseVisualStyleBackColor = true;
            this.cmdAddPower.Click += new System.EventHandler(this.cmdAddPower_Click);
            // 
            // cboDisplayFilter
            // 
            this.cboDisplayFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDisplayFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayFilter.FormattingEnabled = true;
            this.cboDisplayFilter.IntegralHeight = false;
            this.cboDisplayFilter.Location = new System.Drawing.Point(238, 4);
            this.cboDisplayFilter.Name = "cboDisplayFilter";
            this.cboDisplayFilter.Size = new System.Drawing.Size(240, 21);
            this.cboDisplayFilter.TabIndex = 5;
            this.cboDisplayFilter.TooltipText = "";
            this.cboDisplayFilter.SelectedIndexChanged += new System.EventHandler(this.cboDisplayFilter_SelectedIndexChanged);
            this.cboDisplayFilter.TextUpdate += new System.EventHandler(this.cboDisplayFilter_TextUpdate);
            // 
            // lblPowerPoints
            // 
            this.lblPowerPoints.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPowerPoints.AutoSize = true;
            this.lblPowerPoints.Location = new System.Drawing.Point(81, 35);
            this.lblPowerPoints.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPowerPoints.Name = "lblPowerPoints";
            this.lblPowerPoints.Size = new System.Drawing.Size(76, 13);
            this.lblPowerPoints.TabIndex = 9;
            this.lblPowerPoints.Text = "0 (0 remaining)";
            // 
            // lblPowerPointsLabel
            // 
            this.lblPowerPointsLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPowerPointsLabel.AutoSize = true;
            this.lblPowerPointsLabel.Location = new System.Drawing.Point(3, 35);
            this.lblPowerPointsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblPowerPointsLabel.Name = "lblPowerPointsLabel";
            this.lblPowerPointsLabel.Size = new System.Drawing.Size(72, 13);
            this.lblPowerPointsLabel.TabIndex = 8;
            this.lblPowerPointsLabel.Tag = "Label_PowerPoints";
            this.lblPowerPointsLabel.Text = "Power Points:";
            // 
            // _tipTooltip
            // 
            this._tipTooltip.AutoPopDelay = 10000;
            this._tipTooltip.InitialDelay = 250;
            this._tipTooltip.IsBalloon = true;
            this._tipTooltip.ReshowDelay = 100;
            this._tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this._tipTooltip.ToolTipTitle = "Chummer Help";
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 4;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.cmdAddPower, 2, 0);
            this.tlpMain.Controls.Add(this.lblPowerPointsLabel, 0, 1);
            this.tlpMain.Controls.Add(this.lblPowerPoints, 1, 1);
            this.tlpMain.Controls.Add(this.cboDisplayFilter, 3, 0);
            this.tlpMain.Controls.Add(this.pnlPowers, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(481, 154);
            this.tlpMain.TabIndex = 11;
            // 
            // pnlPowers
            // 
            this.pnlPowers.AutoScroll = true;
            this.tlpMain.SetColumnSpan(this.pnlPowers, 4);
            this.pnlPowers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPowers.Location = new System.Drawing.Point(0, 54);
            this.pnlPowers.Margin = new System.Windows.Forms.Padding(0);
            this.pnlPowers.Name = "pnlPowers";
            this.pnlPowers.Padding = new System.Windows.Forms.Padding(3, 3, 13, 3);
            this.pnlPowers.Size = new System.Drawing.Size(481, 100);
            this.pnlPowers.TabIndex = 10;
            // 
            // PowersTabUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(480, 80);
            this.Name = "PowersTabUserControl";
            this.Size = new System.Drawing.Size(481, 154);
            this.Load += new System.EventHandler(this.PowersTabUserControl_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        private System.Windows.Forms.Button cmdAddPower;
        private ElasticComboBox cboDisplayFilter;
        private System.Windows.Forms.Label lblPowerPoints;
        private System.Windows.Forms.Label lblPowerPointsLabel;
        private System.Windows.Forms.ToolTip _tipTooltip;
        private BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel pnlPowers;
    }
}
