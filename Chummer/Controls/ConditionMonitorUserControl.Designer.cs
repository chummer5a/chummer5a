namespace Chummer
{
    partial class ConditionMonitorUserControl
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
            this._progressBarPhysical = new System.Windows.Forms.ProgressBar();
            this._progressBarStun = new System.Windows.Forms.ProgressBar();
            this._btnApplyPhysicalDamage = new System.Windows.Forms.Button();
            this._btnApplyStunDamage = new System.Windows.Forms.Button();
            this._lblPhysical = new System.Windows.Forms.Label();
            this._lblStun = new System.Windows.Forms.Label();
            this.nudStun = new System.Windows.Forms.NumericUpDown();
            this._lblModifier = new System.Windows.Forms.Label();
            this._pbOverflow = new System.Windows.Forms.ProgressBar();
            this.lblDead = new System.Windows.Forms.Label();
            this.lblOverflowValue = new System.Windows.Forms.Label();
            this._btnHealPhysical = new System.Windows.Forms.Button();
            this._btnHealStun = new System.Windows.Forms.Button();
            this.nudRecoverStun = new System.Windows.Forms.NumericUpDown();
            this.nudHeal = new System.Windows.Forms.NumericUpDown();
            this._nudPhysical = new System.Windows.Forms.NumericUpDown();
            this.lblKnockOut = new System.Windows.Forms.Label();
            this.lblMaxOverflow = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudStun)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecoverStun)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudPhysical)).BeginInit();
            this.SuspendLayout();
            // 
            // _progressBarPhysical
            // 
            this._progressBarPhysical.Location = new System.Drawing.Point(3, 28);
            this._progressBarPhysical.Name = "_progressBarPhysical";
            this._progressBarPhysical.Size = new System.Drawing.Size(204, 41);
            this._progressBarPhysical.TabIndex = 8;
            // 
            // _progressBarStun
            // 
            this._progressBarStun.Location = new System.Drawing.Point(217, 28);
            this._progressBarStun.Name = "_progressBarStun";
            this._progressBarStun.Size = new System.Drawing.Size(200, 41);
            this._progressBarStun.TabIndex = 10;
            // 
            // _btnApplyPhysicalDamage
            // 
            this._btnApplyPhysicalDamage.Location = new System.Drawing.Point(117, 75);
            this._btnApplyPhysicalDamage.Name = "_btnApplyPhysicalDamage";
            this._btnApplyPhysicalDamage.Size = new System.Drawing.Size(90, 23);
            this._btnApplyPhysicalDamage.TabIndex = 7;
            this._btnApplyPhysicalDamage.Text = "Apply Physical";
            this._btnApplyPhysicalDamage.UseVisualStyleBackColor = true;
            this._btnApplyPhysicalDamage.Click += new System.EventHandler(this._btnPhysical_Click);
            // 
            // _btnApplyStunDamage
            // 
            this._btnApplyStunDamage.Location = new System.Drawing.Point(327, 75);
            this._btnApplyStunDamage.Name = "_btnApplyStunDamage";
            this._btnApplyStunDamage.Size = new System.Drawing.Size(90, 23);
            this._btnApplyStunDamage.TabIndex = 11;
            this._btnApplyStunDamage.Text = "Apply Stun";
            this._btnApplyStunDamage.UseVisualStyleBackColor = true;
            this._btnApplyStunDamage.Click += new System.EventHandler(this._btnApplyStun_Click);
            // 
            // _lblPhysical
            // 
            this._lblPhysical.AutoSize = true;
            this._lblPhysical.Location = new System.Drawing.Point(75, 4);
            this._lblPhysical.Name = "_lblPhysical";
            this._lblPhysical.Size = new System.Drawing.Size(72, 13);
            this._lblPhysical.TabIndex = 12;
            this._lblPhysical.Tag = "Label_CMPhysical";
            this._lblPhysical.Text = "{Physical:HP}";
            // 
            // _lblStun
            // 
            this._lblStun.AutoSize = true;
            this._lblStun.Location = new System.Drawing.Point(285, 4);
            this._lblStun.Name = "_lblStun";
            this._lblStun.Size = new System.Drawing.Size(55, 13);
            this._lblStun.TabIndex = 13;
            this._lblStun.Tag = "Label_CMStun";
            this._lblStun.Text = "{Stun:HP}";
            // 
            // nudStun
            // 
            this.nudStun.Location = new System.Drawing.Point(217, 78);
            this.nudStun.Name = "nudStun";
            this.nudStun.Size = new System.Drawing.Size(104, 20);
            this.nudStun.TabIndex = 15;
            // 
            // _lblModifier
            // 
            this._lblModifier.AutoSize = true;
            this._lblModifier.Location = new System.Drawing.Point(174, 4);
            this._lblModifier.Name = "_lblModifier";
            this._lblModifier.Size = new System.Drawing.Size(81, 13);
            this._lblModifier.TabIndex = 16;
            this._lblModifier.Tag = "Label_CMModifier";
            this._lblModifier.Text = "{Modifier:value}";
            // 
            // _pbOverflow
            // 
            this._pbOverflow.Location = new System.Drawing.Point(117, 156);
            this._pbOverflow.Name = "_pbOverflow";
            this._pbOverflow.Size = new System.Drawing.Size(204, 41);
            this._pbOverflow.TabIndex = 17;
            this._pbOverflow.Visible = false;
            // 
            // lblDead
            // 
            this.lblDead.AutoSize = true;
            this.lblDead.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDead.Location = new System.Drawing.Point(75, 41);
            this.lblDead.Name = "lblDead";
            this.lblDead.Size = new System.Drawing.Size(68, 13);
            this.lblDead.TabIndex = 18;
            this.lblDead.Tag = "Label_CMStun";
            this.lblDead.Text = "Drop Dead";
            this.lblDead.Visible = false;
            // 
            // lblOverflowValue
            // 
            this.lblOverflowValue.AutoSize = true;
            this.lblOverflowValue.Location = new System.Drawing.Point(114, 140);
            this.lblOverflowValue.Name = "lblOverflowValue";
            this.lblOverflowValue.Size = new System.Drawing.Size(13, 13);
            this.lblOverflowValue.TabIndex = 19;
            this.lblOverflowValue.Tag = "";
            this.lblOverflowValue.Text = "0";
            this.lblOverflowValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblOverflowValue.Visible = false;
            // 
            // _btnHealPhysical
            // 
            this._btnHealPhysical.Location = new System.Drawing.Point(117, 104);
            this._btnHealPhysical.Name = "_btnHealPhysical";
            this._btnHealPhysical.Size = new System.Drawing.Size(90, 23);
            this._btnHealPhysical.TabIndex = 20;
            this._btnHealPhysical.Text = "Heal Physical";
            this._btnHealPhysical.UseVisualStyleBackColor = true;
            this._btnHealPhysical.Click += new System.EventHandler(this._btnHealPhysical_Click);
            // 
            // _btnHealStun
            // 
            this._btnHealStun.Location = new System.Drawing.Point(327, 104);
            this._btnHealStun.Name = "_btnHealStun";
            this._btnHealStun.Size = new System.Drawing.Size(90, 23);
            this._btnHealStun.TabIndex = 21;
            this._btnHealStun.Text = "Heal Stun";
            this._btnHealStun.UseVisualStyleBackColor = true;
            this._btnHealStun.Click += new System.EventHandler(this._btnHealStun_Click);
            // 
            // nudRecoverStun
            // 
            this.nudRecoverStun.Location = new System.Drawing.Point(217, 107);
            this.nudRecoverStun.Name = "nudRecoverStun";
            this.nudRecoverStun.Size = new System.Drawing.Size(104, 20);
            this.nudRecoverStun.TabIndex = 23;
            // 
            // nudHeal
            // 
            this.nudHeal.Location = new System.Drawing.Point(3, 107);
            this.nudHeal.Name = "nudHeal";
            this.nudHeal.Size = new System.Drawing.Size(108, 20);
            this.nudHeal.TabIndex = 22;
            // 
            // _nudPhysical
            // 
            this._nudPhysical.Location = new System.Drawing.Point(3, 78);
            this._nudPhysical.Name = "_nudPhysical";
            this._nudPhysical.Size = new System.Drawing.Size(108, 20);
            this._nudPhysical.TabIndex = 14;
            // 
            // lblKnockOut
            // 
            this.lblKnockOut.AutoSize = true;
            this.lblKnockOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKnockOut.Location = new System.Drawing.Point(285, 41);
            this.lblKnockOut.Name = "lblKnockOut";
            this.lblKnockOut.Size = new System.Drawing.Size(81, 13);
            this.lblKnockOut.TabIndex = 24;
            this.lblKnockOut.Tag = "";
            this.lblKnockOut.Text = "Knocked Out";
            this.lblKnockOut.Visible = false;
            // 
            // lblMaxOverflow
            // 
            this.lblMaxOverflow.AutoSize = true;
            this.lblMaxOverflow.Location = new System.Drawing.Point(308, 140);
            this.lblMaxOverflow.Name = "lblMaxOverflow";
            this.lblMaxOverflow.Size = new System.Drawing.Size(13, 13);
            this.lblMaxOverflow.TabIndex = 25;
            this.lblMaxOverflow.Tag = "";
            this.lblMaxOverflow.Text = "0";
            this.lblMaxOverflow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMaxOverflow.Visible = false;
            // 
            // ConditionMonitorUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblMaxOverflow);
            this.Controls.Add(this.lblKnockOut);
            this.Controls.Add(this.nudRecoverStun);
            this.Controls.Add(this.nudHeal);
            this.Controls.Add(this._btnHealPhysical);
            this.Controls.Add(this._btnHealStun);
            this.Controls.Add(this.lblOverflowValue);
            this.Controls.Add(this.lblDead);
            this.Controls.Add(this._pbOverflow);
            this.Controls.Add(this._lblModifier);
            this.Controls.Add(this.nudStun);
            this.Controls.Add(this._nudPhysical);
            this.Controls.Add(this._lblStun);
            this.Controls.Add(this._lblPhysical);
            this.Controls.Add(this._progressBarPhysical);
            this.Controls.Add(this._progressBarStun);
            this.Controls.Add(this._btnApplyPhysicalDamage);
            this.Controls.Add(this._btnApplyStunDamage);
            this.Name = "ConditionMonitorUserControl";
            this.Size = new System.Drawing.Size(420, 224);
            ((System.ComponentModel.ISupportInitialize)(this.nudStun)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecoverStun)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._nudPhysical)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar _progressBarPhysical;
        private System.Windows.Forms.ProgressBar _progressBarStun;
        private System.Windows.Forms.Button _btnApplyPhysicalDamage;
        private System.Windows.Forms.Button _btnApplyStunDamage;
        private System.Windows.Forms.Label _lblPhysical;
        private System.Windows.Forms.Label _lblStun;
        private System.Windows.Forms.NumericUpDown nudStun;
        private System.Windows.Forms.Label _lblModifier;
        private System.Windows.Forms.ProgressBar _pbOverflow;
        private System.Windows.Forms.Label lblDead;
        private System.Windows.Forms.Label lblOverflowValue;
        private System.Windows.Forms.Button _btnHealPhysical;
        private System.Windows.Forms.Button _btnHealStun;
        private System.Windows.Forms.NumericUpDown _nudPhysical;
        private System.Windows.Forms.NumericUpDown nudHeal;
        private System.Windows.Forms.NumericUpDown nudRecoverStun;
        private System.Windows.Forms.Label lblKnockOut;
        private System.Windows.Forms.Label lblMaxOverflow;
    }
}
