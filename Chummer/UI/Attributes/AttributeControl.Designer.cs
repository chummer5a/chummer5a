namespace Chummer.UI.Attributes
{
    partial class AttributeControl
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
                UnbindAttributeControl();
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
            this.lblName = new Chummer.LabelWithToolTip();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.flpButtonsCareer = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdImproveATT = new Chummer.ButtonWithToolTip();
            this.cmdBurnEdge = new Chummer.ButtonWithToolTip();
            this.flpButtonsCreate = new System.Windows.Forms.FlowLayoutPanel();
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.nudBase = new Chummer.NumericUpDownEx();
            this.flpRight = new System.Windows.Forms.FlowLayoutPanel();
            this.lblValue = new Chummer.LabelWithToolTip();
            this.lblLimits = new Chummer.LabelWithToolTip();
            this.tlpMain.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.flpButtonsCareer.SuspendLayout();
            this.flpButtonsCreate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBase)).BeginInit();
            this.flpRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.pnlButtons, 1, 0);
            this.tlpMain.Controls.Add(this.flpRight, 2, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(336, 46);
            this.tlpMain.TabIndex = 79;
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 16);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(61, 13);
            this.lblName.TabIndex = 71;
            this.lblName.Text = "Attrib (ATT)";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblName.ToolTipText = null;
            // 
            // pnlButtons
            // 
            this.pnlButtons.AutoSize = true;
            this.pnlButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlButtons.Controls.Add(this.flpButtonsCareer);
            this.pnlButtons.Controls.Add(this.flpButtonsCreate);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButtons.Location = new System.Drawing.Point(67, 0);
            this.pnlButtons.Margin = new System.Windows.Forms.Padding(0);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(142, 46);
            this.pnlButtons.TabIndex = 75;
            // 
            // flpButtonsCareer
            // 
            this.flpButtonsCareer.AutoSize = true;
            this.flpButtonsCareer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtonsCareer.Controls.Add(this.cmdImproveATT);
            this.flpButtonsCareer.Controls.Add(this.cmdBurnEdge);
            this.flpButtonsCareer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButtonsCareer.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtonsCareer.Location = new System.Drawing.Point(0, 0);
            this.flpButtonsCareer.Margin = new System.Windows.Forms.Padding(0);
            this.flpButtonsCareer.Name = "flpButtonsCareer";
            this.flpButtonsCareer.Size = new System.Drawing.Size(142, 46);
            this.flpButtonsCareer.TabIndex = 79;
            this.flpButtonsCareer.WrapContents = false;
            // 
            // cmdImproveATT
            // 
            this.cmdImproveATT.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdImproveATT.AutoSize = true;
            this.cmdImproveATT.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdImproveATT.Image = global::Chummer.Properties.Resources.add;
            this.cmdImproveATT.Location = new System.Drawing.Point(115, 0);
            this.cmdImproveATT.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdImproveATT.MinimumSize = new System.Drawing.Size(24, 24);
            this.cmdImproveATT.Name = "cmdImproveATT";
            this.cmdImproveATT.Size = new System.Drawing.Size(24, 24);
            this.cmdImproveATT.TabIndex = 75;
            this.cmdImproveATT.ToolTipText = "";
            this.cmdImproveATT.UseVisualStyleBackColor = true;
            this.cmdImproveATT.Click += new System.EventHandler(this.cmdImproveATT_Click);
            // 
            // cmdBurnEdge
            // 
            this.cmdBurnEdge.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cmdBurnEdge.AutoSize = true;
            this.cmdBurnEdge.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdBurnEdge.Image = global::Chummer.Properties.Resources.delete;
            this.cmdBurnEdge.Location = new System.Drawing.Point(85, 0);
            this.cmdBurnEdge.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdBurnEdge.MinimumSize = new System.Drawing.Size(24, 24);
            this.cmdBurnEdge.Name = "cmdBurnEdge";
            this.cmdBurnEdge.Size = new System.Drawing.Size(24, 24);
            this.cmdBurnEdge.TabIndex = 78;
            this.cmdBurnEdge.ToolTipText = "";
            this.cmdBurnEdge.UseVisualStyleBackColor = true;
            this.cmdBurnEdge.Click += new System.EventHandler(this.cmdBurnEdge_Click);
            // 
            // flpButtonsCreate
            // 
            this.flpButtonsCreate.AutoSize = true;
            this.flpButtonsCreate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtonsCreate.Controls.Add(this.nudKarma);
            this.flpButtonsCreate.Controls.Add(this.nudBase);
            this.flpButtonsCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButtonsCreate.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtonsCreate.Location = new System.Drawing.Point(0, 0);
            this.flpButtonsCreate.Margin = new System.Windows.Forms.Padding(0);
            this.flpButtonsCreate.Name = "flpButtonsCreate";
            this.flpButtonsCreate.Size = new System.Drawing.Size(142, 46);
            this.flpButtonsCreate.TabIndex = 0;
            this.flpButtonsCreate.WrapContents = false;
            // 
            // nudKarma
            // 
            this.nudKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudKarma.AutoSize = true;
            this.nudKarma.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(104, 2);
            this.nudKarma.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.nudKarma.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudKarma.MinimumSize = new System.Drawing.Size(35, 0);
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(35, 20);
            this.nudKarma.TabIndex = 77;
            this.nudKarma.BeforeValueIncrement += new System.ComponentModel.CancelEventHandler(this.nudKarma_BeforeValueIncrement);
            this.nudKarma.ValueChanged += new System.EventHandler(this.nudKarma_ValueChanged);
            // 
            // nudBase
            // 
            this.nudBase.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudBase.AutoSize = true;
            this.nudBase.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudBase.Location = new System.Drawing.Point(63, 2);
            this.nudBase.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.nudBase.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudBase.MinimumSize = new System.Drawing.Size(35, 0);
            this.nudBase.Name = "nudBase";
            this.nudBase.Size = new System.Drawing.Size(35, 20);
            this.nudBase.TabIndex = 76;
            this.nudBase.BeforeValueIncrement += new System.ComponentModel.CancelEventHandler(this.nudBase_BeforeValueIncrement);
            this.nudBase.ValueChanged += new System.EventHandler(this.nudBase_ValueChanged);
            // 
            // flpRight
            // 
            this.flpRight.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.flpRight.AutoSize = true;
            this.flpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRight.Controls.Add(this.lblValue);
            this.flpRight.Controls.Add(this.lblLimits);
            this.flpRight.Location = new System.Drawing.Point(209, 16);
            this.flpRight.Margin = new System.Windows.Forms.Padding(0);
            this.flpRight.Name = "flpRight";
            this.flpRight.Size = new System.Drawing.Size(127, 13);
            this.flpRight.TabIndex = 76;
            this.flpRight.WrapContents = false;
            // 
            // lblValue
            // 
            this.lblValue.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblValue.AutoSize = true;
            this.lblValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValue.Location = new System.Drawing.Point(3, 0);
            this.lblValue.MinimumSize = new System.Drawing.Size(50, 0);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(50, 13);
            this.lblValue.TabIndex = 74;
            this.lblValue.Text = "00 (00)";
            this.lblValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblValue.ToolTipText = null;
            // 
            // lblLimits
            // 
            this.lblLimits.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblLimits.AutoSize = true;
            this.lblLimits.Location = new System.Drawing.Point(59, 0);
            this.lblLimits.MinimumSize = new System.Drawing.Size(65, 0);
            this.lblLimits.Name = "lblLimits";
            this.lblLimits.Size = new System.Drawing.Size(65, 13);
            this.lblLimits.TabIndex = 72;
            this.lblLimits.Text = "00 / 00 (00)";
            this.lblLimits.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLimits.ToolTipText = null;
            // 
            // AttributeControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.MinimumSize = new System.Drawing.Size(0, 24);
            this.Name = "AttributeControl";
            this.Size = new System.Drawing.Size(336, 46);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.flpButtonsCareer.ResumeLayout(false);
            this.flpButtonsCareer.PerformLayout();
            this.flpButtonsCreate.ResumeLayout(false);
            this.flpButtonsCreate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBase)).EndInit();
            this.flpRight.ResumeLayout(false);
            this.flpRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ButtonWithToolTip cmdImproveATT;
        private LabelWithToolTip lblValue;
        private LabelWithToolTip lblName;
        private LabelWithToolTip lblLimits;
        private NumericUpDownEx nudKarma;
        private NumericUpDownEx nudBase;
        private ButtonWithToolTip cmdBurnEdge;
        private BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.FlowLayoutPanel flpButtonsCreate;
        private System.Windows.Forms.FlowLayoutPanel flpButtonsCareer;
        private System.Windows.Forms.FlowLayoutPanel flpRight;
    }
}
