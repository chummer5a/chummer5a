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
            this.lblName = new Chummer.LabelWithToolTip();
            this.cmdImproveATT = new Chummer.ButtonWithToolTip();
            this.cmdBurnEdge = new Chummer.ButtonWithToolTip();
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.nudBase = new Chummer.NumericUpDownEx();
            this.flpRight = new System.Windows.Forms.FlowLayoutPanel();
            this.lblValue = new Chummer.LabelWithToolTip();
            this.lblLimits = new Chummer.LabelWithToolTip();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBase)).BeginInit();
            this.flpRight.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 5);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(61, 13);
            this.lblName.TabIndex = 71;
            this.lblName.Text = "Attrib (ATT)";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblName.ToolTipText = null;
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
            this.cmdBurnEdge.Image = global::Chummer.Properties.Resources.fire;
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
            // nudKarma
            // 
            this.nudKarma.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.nudKarma.AutoSize = true;
            this.nudKarma.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(44, 2);
            this.nudKarma.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
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
            this.nudBase.Location = new System.Drawing.Point(3, 2);
            this.nudBase.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
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
            this.flpRight.AutoSize = true;
            this.flpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRight.Controls.Add(this.nudBase);
            this.flpRight.Controls.Add(this.nudKarma);
            this.flpRight.Controls.Add(this.cmdBurnEdge);
            this.flpRight.Controls.Add(this.cmdImproveATT);
            this.flpRight.Controls.Add(this.lblValue);
            this.flpRight.Controls.Add(this.lblLimits);
            this.flpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpRight.Location = new System.Drawing.Point(67, 0);
            this.flpRight.Margin = new System.Windows.Forms.Padding(0);
            this.flpRight.Name = "flpRight";
            this.flpRight.Size = new System.Drawing.Size(269, 24);
            this.flpRight.TabIndex = 76;
            this.flpRight.WrapContents = false;
            // 
            // lblValue
            // 
            this.lblValue.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblValue.AutoSize = true;
            this.lblValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValue.Location = new System.Drawing.Point(145, 5);
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
            this.lblLimits.Location = new System.Drawing.Point(201, 5);
            this.lblLimits.MinimumSize = new System.Drawing.Size(65, 0);
            this.lblLimits.Name = "lblLimits";
            this.lblLimits.Size = new System.Drawing.Size(65, 13);
            this.lblLimits.TabIndex = 72;
            this.lblLimits.Text = "00 / 00 (00)";
            this.lblLimits.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLimits.ToolTipText = null;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.flpRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(336, 24);
            this.tlpMain.TabIndex = 79;
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
            this.Size = new System.Drawing.Size(336, 24);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBase)).EndInit();
            this.flpRight.ResumeLayout(false);
            this.flpRight.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
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
        private System.Windows.Forms.FlowLayoutPanel flpRight;
        private BufferedTableLayoutPanel tlpMain;
    }
}
