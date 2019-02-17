namespace Chummer
{
    partial class PowerControl
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
            this.components = new System.ComponentModel.Container();
            this.lblPowerName = new System.Windows.Forms.Label();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.nudRating = new Chummer.helpers.NumericUpDownEx();
            this.lblPowerPoints = new System.Windows.Forms.Label();
            this.chkDiscountedAdeptWay = new System.Windows.Forms.CheckBox();
            this.chkDiscountedGeas = new System.Windows.Forms.CheckBox();
            this.imgNotes = new System.Windows.Forms.PictureBox();
            this.tipTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.lblActivation = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPowerName
            // 
            this.lblPowerName.AutoSize = true;
            this.lblPowerName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPowerName.Location = new System.Drawing.Point(0, 5);
            this.lblPowerName.Name = "lblPowerName";
            this.lblPowerName.Size = new System.Drawing.Size(67, 13);
            this.lblPowerName.TabIndex = 0;
            this.lblPowerName.Text = "Power Name";
            this.lblPowerName.Click += new System.EventHandler(this.lblPowerName_Click);
            // 
            // cmdDelete
            // 
            this.cmdDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdDelete.Location = new System.Drawing.Point(767, 0);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(60, 23);
            this.cmdDelete.TabIndex = 8;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // nudRating
            // 
            this.nudRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudRating.Location = new System.Drawing.Point(332, 2);
            this.nudRating.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.nudRating.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRating.Name = "nudRating";
            this.nudRating.Size = new System.Drawing.Size(40, 18);
            this.nudRating.TabIndex = 2;
            this.nudRating.Increment = 1;
            this.nudRating.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblPowerPoints
            // 
            this.lblPowerPoints.AutoSize = true;
            this.lblPowerPoints.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPowerPoints.Location = new System.Drawing.Point(378, 5);
            this.lblPowerPoints.Name = "lblPowerPoints";
            this.lblPowerPoints.Size = new System.Drawing.Size(40, 13);
            this.lblPowerPoints.TabIndex = 4;
            this.lblPowerPoints.Text = "[00.00]";
            // 
            // chkDiscountedAdeptWay
            // 
            this.chkDiscountedAdeptWay.AutoSize = true;
            this.chkDiscountedAdeptWay.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDiscountedAdeptWay.Location = new System.Drawing.Point(608, 4);
            this.chkDiscountedAdeptWay.Name = "chkDiscountedAdeptWay";
            this.chkDiscountedAdeptWay.Size = new System.Drawing.Size(77, 17);
            this.chkDiscountedAdeptWay.TabIndex = 6;
            this.chkDiscountedAdeptWay.Tag = "Checkbox_Power_AdeptWay";
            this.chkDiscountedAdeptWay.Text = "Adept Way";
            this.chkDiscountedAdeptWay.UseVisualStyleBackColor = true;
            // 
            // chkDiscountedGeas
            // 
            this.chkDiscountedGeas.AutoSize = true;
            this.chkDiscountedGeas.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDiscountedGeas.Location = new System.Drawing.Point(693, 4);
            this.chkDiscountedGeas.Name = "chkDiscountedGeas";
            this.chkDiscountedGeas.Size = new System.Drawing.Size(51, 17);
            this.chkDiscountedGeas.TabIndex = 7;
            this.chkDiscountedGeas.Tag = "Checkbox_Power_Geas";
            this.chkDiscountedGeas.Text = "Geas";
            this.chkDiscountedGeas.UseVisualStyleBackColor = true;
            this.chkDiscountedGeas.Visible = false;
            // 
            // imgNotes
            // 
            this.imgNotes.Image = global::Chummer.Properties.Resources.note_edit;
            this.imgNotes.Location = new System.Drawing.Point(745, 3);
            this.imgNotes.Name = "imgNotes";
            this.imgNotes.Size = new System.Drawing.Size(16, 16);
            this.imgNotes.TabIndex = 11;
            this.imgNotes.TabStop = false;
            this.tipTooltip.SetToolTip(this.imgNotes, "Edit Adept Power Notes.");
            this.imgNotes.Click += new System.EventHandler(this.imgNotes_Click);
            // 
            // tipTooltip
            // 
            this.tipTooltip.AutoPopDelay = 10000;
            this.tipTooltip.InitialDelay = 250;
            this.tipTooltip.IsBalloon = true;
            this.tipTooltip.ReshowDelay = 100;
            this.tipTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tipTooltip.ToolTipTitle = "Chummer Help";
            // 
            // lblActivation
            // 
            this.lblActivation.AutoSize = true;
            this.lblActivation.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActivation.Location = new System.Drawing.Point(280, 6);
            this.lblActivation.Name = "lblActivation";
            this.lblActivation.Size = new System.Drawing.Size(46, 13);
            this.lblActivation.TabIndex = 12;
            this.lblActivation.Text = "Interrupt";
            // 
            // PowerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblActivation);
            this.Controls.Add(this.imgNotes);
            this.Controls.Add(this.chkDiscountedGeas);
            this.Controls.Add(this.chkDiscountedAdeptWay);
            this.Controls.Add(this.lblPowerPoints);
            this.Controls.Add(this.lblPowerName);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.nudRating);
            this.Name = "PowerControl";
            this.Size = new System.Drawing.Size(829, 23);
            this.Load += new System.EventHandler(this.PowerControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgNotes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Label lblPowerName;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Label lblPowerPoints;
        private System.Windows.Forms.CheckBox chkDiscountedAdeptWay;
        private System.Windows.Forms.CheckBox chkDiscountedGeas;
        private System.Windows.Forms.PictureBox imgNotes;
        private System.Windows.Forms.ToolTip tipTooltip;
        public Chummer.helpers.NumericUpDownEx nudRating;
        private System.Windows.Forms.Label lblActivation;
    }
}
