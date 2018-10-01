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
            this.cmdBurnEdge = new Chummer.ButtonWithToolTip();
            this.nudKarma = new Chummer.NumericUpDownEx();
            this.nudBase = new Chummer.NumericUpDownEx();
            this.cmdImproveATT = new Chummer.ButtonWithToolTip();
            this.lblValue = new Chummer.LabelWithToolTip();
            this.lblName = new Chummer.LabelWithToolTip();
            this.lblLimits = new Chummer.LabelWithToolTip();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBase)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdBurnEdge
            // 
            this.cmdBurnEdge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdBurnEdge.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdBurnEdge.Image = global::Chummer.Properties.Resources.delete;
            this.cmdBurnEdge.Location = new System.Drawing.Point(178, -1);
            this.cmdBurnEdge.Name = "cmdBurnEdge";
            this.cmdBurnEdge.Size = new System.Drawing.Size(24, 24);
            this.cmdBurnEdge.TabIndex = 78;
            this.cmdBurnEdge.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdBurnEdge.ToolTipText = "";
            this.cmdBurnEdge.UseVisualStyleBackColor = true;
            this.cmdBurnEdge.Click += new System.EventHandler(this.cmdBurnEdge_Click);
            // 
            // nudKarma
            // 
            this.nudKarma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudKarma.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(165, 1);
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(40, 20);
            this.nudKarma.TabIndex = 77;
            this.nudKarma.BeforeValueIncrement += new System.ComponentModel.CancelEventHandler(this.nudKarma_BeforeValueIncrement);
            this.nudKarma.ValueChanged += new System.EventHandler(this.nudKarma_ValueChanged);
            // 
            // nudBase
            // 
            this.nudBase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudBase.InterceptMouseWheel = Chummer.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudBase.Location = new System.Drawing.Point(119, 1);
            this.nudBase.Name = "nudBase";
            this.nudBase.Size = new System.Drawing.Size(40, 20);
            this.nudBase.TabIndex = 76;
            this.nudBase.BeforeValueIncrement += new System.ComponentModel.CancelEventHandler(this.nudBase_BeforeValueIncrement);
            this.nudBase.ValueChanged += new System.EventHandler(this.nudBase_ValueChanged);
            // 
            // cmdImproveATT
            // 
            this.cmdImproveATT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdImproveATT.FlatAppearance.BorderSize = 0;
            this.cmdImproveATT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdImproveATT.Image = global::Chummer.Properties.Resources.add;
            this.cmdImproveATT.Location = new System.Drawing.Point(153, -1);
            this.cmdImproveATT.Name = "cmdImproveATT";
            this.cmdImproveATT.Size = new System.Drawing.Size(24, 24);
            this.cmdImproveATT.TabIndex = 75;
            this.cmdImproveATT.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdImproveATT.ToolTipText = "";
            this.cmdImproveATT.UseVisualStyleBackColor = true;
            this.cmdImproveATT.Click += new System.EventHandler(this.cmdImproveATT_Click);
            // 
            // lblValue
            // 
            this.lblValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblValue.AutoSize = true;
            this.lblValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValue.Location = new System.Drawing.Point(226, 5);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(22, 13);
            this.lblValue.TabIndex = 74;
            this.lblValue.Text = "[0]";
            this.lblValue.ToolTipText = null;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(2, 5);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(61, 13);
            this.lblName.TabIndex = 71;
            this.lblName.Text = "Attrib (ATT)";
            this.lblName.ToolTipText = null;
            // 
            // lblLimits
            // 
            this.lblLimits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLimits.AutoSize = true;
            this.lblLimits.Location = new System.Drawing.Point(277, 5);
            this.lblLimits.Name = "lblLimits";
            this.lblLimits.Size = new System.Drawing.Size(45, 13);
            this.lblLimits.TabIndex = 72;
            this.lblLimits.Text = "1 / 6 (9)";
            this.lblLimits.ToolTipText = null;
            // 
            // AttributeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.cmdBurnEdge);
            this.Controls.Add(this.nudKarma);
            this.Controls.Add(this.nudBase);
            this.Controls.Add(this.cmdImproveATT);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblLimits);
            this.Name = "AttributeControl";
            this.Size = new System.Drawing.Size(362, 26);
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBase)).EndInit();
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
    }
}
