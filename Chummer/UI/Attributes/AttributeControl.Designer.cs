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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.cmdImproveATT = new Chummer.helpers.Button();
            this.lblValue = new Chummer.helpers.Label();
            this.lblName = new Chummer.helpers.Label();
            this.lblLimits = new Chummer.helpers.Label();
            this.cmdBurnEdge = new Chummer.helpers.Button();
            this.nudKarma = new Chummer.helpers.NumericUpDownEx();
            this.nudBase = new Chummer.helpers.NumericUpDownEx();
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBase)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdImproveATT
            // 
            this.cmdImproveATT.FlatAppearance.BorderSize = 0;
            this.cmdImproveATT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdImproveATT.Image = global::Chummer.Properties.Resources.add;
            this.cmdImproveATT.Location = new System.Drawing.Point(153, -1);
            this.cmdImproveATT.Name = "cmdImproveATT";
            this.cmdImproveATT.Size = new System.Drawing.Size(24, 24);
            this.cmdImproveATT.TabIndex = 75;
            this.cmdImproveATT.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdImproveATT.UseVisualStyleBackColor = true;
            this.cmdImproveATT.Click += new System.EventHandler(this.cmdImproveATT_Click);
            // 
            // lblValue
            // 
            this.lblValue.AutoSize = true;
            this.lblValue.Location = new System.Drawing.Point(226, 5);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(19, 13);
            this.lblValue.TabIndex = 74;
            this.lblValue.Text = "[0]";
            this.lblValue.TooltipText = null;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(2, 5);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(61, 13);
            this.lblName.TabIndex = 71;
            this.lblName.Text = "Attrib (ATT)";
            this.lblName.TooltipText = null;
            // 
            // lblLimits
            // 
            this.lblLimits.AutoSize = true;
            this.lblLimits.Location = new System.Drawing.Point(277, 5);
            this.lblLimits.Name = "lblLimits";
            this.lblLimits.Size = new System.Drawing.Size(45, 13);
            this.lblLimits.TabIndex = 72;
            this.lblLimits.Text = "1 / 6 (9)";
            this.lblLimits.TooltipText = null;
            // 
            // cmdBurnEdge
            // 
            this.cmdBurnEdge.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdBurnEdge.Image = global::Chummer.Properties.Resources.delete;
            this.cmdBurnEdge.Location = new System.Drawing.Point(178, -1);
            this.cmdBurnEdge.Name = "cmdBurnEdge";
            this.cmdBurnEdge.Size = new System.Drawing.Size(24, 24);
            this.cmdBurnEdge.TabIndex = 78;
            this.cmdBurnEdge.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdBurnEdge.UseVisualStyleBackColor = true;
            this.cmdBurnEdge.Click += new System.EventHandler(this.cmdBurnEdge_Click);
            // 
            // nudKarma
            // 
            this.nudKarma.InterceptMouseWheel = Chummer.helpers.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudKarma.Location = new System.Drawing.Point(165, 1);
            this.nudKarma.Name = "nudKarma";
            this.nudKarma.Size = new System.Drawing.Size(40, 20);
            this.nudKarma.TabIndex = 77;
            this.nudKarma.BeforeValueIncrement += new System.ComponentModel.CancelEventHandler(this.nudKarma_BeforeValueIncrement);
            this.nudKarma.ValueChanged += new System.EventHandler(this.nudKarma_ValueChanged);
            // 
            // nudBase
            // 
            this.nudBase.InterceptMouseWheel = Chummer.helpers.NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver;
            this.nudBase.Location = new System.Drawing.Point(119, 1);
            this.nudBase.Name = "nudBase";
            this.nudBase.Size = new System.Drawing.Size(40, 20);
            this.nudBase.TabIndex = 76;
            this.nudBase.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBase.BeforeValueIncrement += new System.ComponentModel.CancelEventHandler(this.nudBase_BeforeValueIncrement);
            this.nudBase.ValueChanged += new System.EventHandler(this.nudBase_ValueChanged);
            // 
            // AttributeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmdBurnEdge);
            this.Controls.Add(this.nudKarma);
            this.Controls.Add(this.nudBase);
            this.Controls.Add(this.cmdImproveATT);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblLimits);
            this.Name = "AttributeControl";
            this.Size = new System.Drawing.Size(362, 22);
            ((System.ComponentModel.ISupportInitialize)(this.nudKarma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBase)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Chummer.helpers.Button cmdImproveATT;
        internal Chummer.helpers.Label lblValue;
        internal Chummer.helpers.Label lblName;
        internal Chummer.helpers.Label lblLimits;
        internal Chummer.helpers.NumericUpDownEx nudKarma;
        internal Chummer.helpers.NumericUpDownEx nudBase;
        private Chummer.helpers.Button cmdBurnEdge;
    }
}
