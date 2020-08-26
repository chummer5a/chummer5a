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
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.flpLabels = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpMain.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.flpLabels.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdAddPower
            // 
            this.cmdAddPower.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdAddPower.AutoSize = true;
            this.cmdAddPower.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddPower.Location = new System.Drawing.Point(3, 3);
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
            this.cboDisplayFilter.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cboDisplayFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayFilter.FormattingEnabled = true;
            this.cboDisplayFilter.IntegralHeight = false;
            this.cboDisplayFilter.Location = new System.Drawing.Point(78, 4);
            this.cboDisplayFilter.Name = "cboDisplayFilter";
            this.cboDisplayFilter.Size = new System.Drawing.Size(201, 21);
            this.cboDisplayFilter.TabIndex = 5;
            this.cboDisplayFilter.TooltipText = "";
            this.cboDisplayFilter.SelectedIndexChanged += new System.EventHandler(this.cboDisplayFilter_SelectedIndexChanged);
            this.cboDisplayFilter.TextUpdate += new System.EventHandler(this.cboDisplayFilter_TextUpdate);
            // 
            // lblPowerPoints
            // 
            this.lblPowerPoints.AutoSize = true;
            this.lblPowerPoints.Location = new System.Drawing.Point(81, 0);
            this.lblPowerPoints.Name = "lblPowerPoints";
            this.lblPowerPoints.Size = new System.Drawing.Size(76, 13);
            this.lblPowerPoints.TabIndex = 9;
            this.lblPowerPoints.Text = "0 (0 remaining)";
            // 
            // lblPowerPointsLabel
            // 
            this.lblPowerPointsLabel.AutoSize = true;
            this.lblPowerPointsLabel.Location = new System.Drawing.Point(3, 0);
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
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.flpButtons, 0, 0);
            this.tlpMain.Controls.Add(this.flpLabels, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(480, 80);
            this.tlpMain.TabIndex = 11;
            // 
            // flpButtons
            // 
            this.flpButtons.AutoSize = true;
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtons.Controls.Add(this.cboDisplayFilter);
            this.flpButtons.Controls.Add(this.cmdAddPower);
            this.flpButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtons.Location = new System.Drawing.Point(198, 0);
            this.flpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(282, 29);
            this.flpButtons.TabIndex = 0;
            this.flpButtons.WrapContents = false;
            // 
            // flpLabels
            // 
            this.flpLabels.AutoSize = true;
            this.flpLabels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpLabels.Controls.Add(this.lblPowerPointsLabel);
            this.flpLabels.Controls.Add(this.lblPowerPoints);
            this.flpLabels.Dock = System.Windows.Forms.DockStyle.Left;
            this.flpLabels.Location = new System.Drawing.Point(0, 29);
            this.flpLabels.Margin = new System.Windows.Forms.Padding(0);
            this.flpLabels.Name = "flpLabels";
            this.flpLabels.Size = new System.Drawing.Size(160, 13);
            this.flpLabels.TabIndex = 1;
            this.flpLabels.WrapContents = false;
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
            this.Size = new System.Drawing.Size(480, 80);
            this.Load += new System.EventHandler(this.PowersTabUserControl_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.flpButtons.PerformLayout();
            this.flpLabels.ResumeLayout(false);
            this.flpLabels.PerformLayout();
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
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.FlowLayoutPanel flpLabels;
    }
}
