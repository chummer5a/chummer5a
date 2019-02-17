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
            this.cboSort = new System.Windows.Forms.ComboBox();
            this.cmdAddPower = new System.Windows.Forms.Button();
            this.cboDisplayFilter = new System.Windows.Forms.ComboBox();
            this.lblPowerPoints = new System.Windows.Forms.Label();
            this.lblPowerPointsLabel = new System.Windows.Forms.Label();
            this.pnlPowers = new System.Windows.Forms.Panel();
            this.lblLevels = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblDiscountLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cboSort
            // 
            this.cboSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSort.FormattingEnabled = true;
            this.cboSort.IntegralHeight = false;
            this.cboSort.Location = new System.Drawing.Point(487, 3);
            this.cboSort.Name = "cboSort";
            this.cboSort.Size = new System.Drawing.Size(133, 21);
            this.cboSort.TabIndex = 7;
            this.cboSort.SelectedIndexChanged += new System.EventHandler(this.cboSort_SelectedIndexChanged);
            // 
            // cmdAddPower
            // 
            this.cmdAddPower.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdAddPower.Location = new System.Drawing.Point(392, 3);
            this.cmdAddPower.Name = "cmdAddPower";
            this.cmdAddPower.Size = new System.Drawing.Size(89, 21);
            this.cmdAddPower.TabIndex = 6;
            this.cmdAddPower.Tag = "Button_AddPower";
            this.cmdAddPower.Text = "Add Power";
            this.cmdAddPower.UseVisualStyleBackColor = true;
            this.cmdAddPower.Click += new System.EventHandler(this.cmdAddPower_Click);
            // 
            // cboDisplayFilter
            // 
            this.cboDisplayFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDisplayFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayFilter.FormattingEnabled = true;
            this.cboDisplayFilter.IntegralHeight = false;
            this.cboDisplayFilter.Location = new System.Drawing.Point(626, 3);
            this.cboDisplayFilter.Name = "cboDisplayFilter";
            this.cboDisplayFilter.Size = new System.Drawing.Size(201, 21);
            this.cboDisplayFilter.TabIndex = 5;
            this.cboDisplayFilter.SelectedIndexChanged += new System.EventHandler(this.cboDisplayFilter_SelectedIndexChanged);
            this.cboDisplayFilter.TextUpdate += new System.EventHandler(this.cboDisplayFilter_TextUpdate);
            // 
            // lblPowerPoints
            // 
            this.lblPowerPoints.AutoSize = true;
            this.lblPowerPoints.Location = new System.Drawing.Point(81, 30);
            this.lblPowerPoints.Name = "lblPowerPoints";
            this.lblPowerPoints.Size = new System.Drawing.Size(76, 13);
            this.lblPowerPoints.TabIndex = 9;
            this.lblPowerPoints.Text = "0 (0 remaining)";
            // 
            // lblPowerPointsLabel
            // 
            this.lblPowerPointsLabel.AutoSize = true;
            this.lblPowerPointsLabel.Location = new System.Drawing.Point(3, 30);
            this.lblPowerPointsLabel.Name = "lblPowerPointsLabel";
            this.lblPowerPointsLabel.Size = new System.Drawing.Size(72, 13);
            this.lblPowerPointsLabel.TabIndex = 8;
            this.lblPowerPointsLabel.Tag = "Label_PowerPoints";
            this.lblPowerPointsLabel.Text = "Power Points:";
            // 
            // pnlPowers
            // 
            this.pnlPowers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPowers.Location = new System.Drawing.Point(6, 47);
            this.pnlPowers.Name = "pnlPowers";
            this.pnlPowers.Size = new System.Drawing.Size(821, 561);
            this.pnlPowers.TabIndex = 10;
            // 
            // lblLevels
            // 
            this.lblLevels.AutoSize = true;
            this.lblLevels.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLevels.Location = new System.Drawing.Point(330, 30);
            this.lblLevels.Name = "lblLevels";
            this.lblLevels.Size = new System.Drawing.Size(41, 13);
            this.lblLevels.TabIndex = 2;
            this.lblLevels.Tag = "Label_PowerLevels";
            this.lblLevels.Text = "Levels:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(376, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Power Points:";
            // 
            // lblDiscountLabel
            // 
            this.lblDiscountLabel.AutoSize = true;
            this.lblDiscountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiscountLabel.Location = new System.Drawing.Point(454, 30);
            this.lblDiscountLabel.Name = "lblDiscountLabel";
            this.lblDiscountLabel.Size = new System.Drawing.Size(74, 13);
            this.lblDiscountLabel.TabIndex = 12;
            this.lblDiscountLabel.Tag = "Label_Power_DiscountedBy";
            this.lblDiscountLabel.Text = "Discounted by";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(285, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 13;
            this.label2.Tag = "Label_Action";
            this.label2.Text = "Action:";
            // 
            // PowersTabUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblDiscountLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblLevels);
            this.Controls.Add(this.pnlPowers);
            this.Controls.Add(this.lblPowerPoints);
            this.Controls.Add(this.lblPowerPointsLabel);
            this.Controls.Add(this.cboSort);
            this.Controls.Add(this.cmdAddPower);
            this.Controls.Add(this.cboDisplayFilter);
            this.Name = "PowersTabUserControl";
            this.Size = new System.Drawing.Size(830, 611);
            this.Load += new System.EventHandler(this.PowersTabUserControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboSort;
        private System.Windows.Forms.Button cmdAddPower;
        private System.Windows.Forms.ComboBox cboDisplayFilter;
        private System.Windows.Forms.Label lblPowerPoints;
        private System.Windows.Forms.Label lblPowerPointsLabel;
        private System.Windows.Forms.Panel pnlPowers;
        private System.Windows.Forms.Label lblLevels;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDiscountLabel;
        private System.Windows.Forms.Label label2;
    }
}
