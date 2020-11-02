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
            this.flpRight = new System.Windows.Forms.FlowLayoutPanel();
            this.lblValue = new Chummer.LabelWithToolTip();
            this.lblLimits = new Chummer.LabelWithToolTip();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
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
            // flpRight
            // 
            this.flpRight.AutoSize = true;
            this.flpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpRight.Controls.Add(this.lblLimits);
            this.flpRight.Controls.Add(this.lblValue);
            this.flpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpRight.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpRight.Location = new System.Drawing.Point(67, 0);
            this.flpRight.Margin = new System.Windows.Forms.Padding(0);
            this.flpRight.Name = "flpRight";
            this.flpRight.Size = new System.Drawing.Size(127, 24);
            this.flpRight.TabIndex = 76;
            this.flpRight.WrapContents = false;
            // 
            // lblValue
            // 
            this.lblValue.AutoSize = true;
            this.lblValue.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.lblLimits.AutoSize = true;
            this.lblLimits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLimits.Location = new System.Drawing.Point(59, 0);
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
            this.tlpMain.Size = new System.Drawing.Size(194, 24);
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
            this.Size = new System.Drawing.Size(194, 24);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.flpRight.ResumeLayout(false);
            this.flpRight.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LabelWithToolTip lblValue;
        private LabelWithToolTip lblName;
        private LabelWithToolTip lblLimits;
        private System.Windows.Forms.FlowLayoutPanel flpRight;
        private BufferedTableLayoutPanel tlpMain;
    }
}
