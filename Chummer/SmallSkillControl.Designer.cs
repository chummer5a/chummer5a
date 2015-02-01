namespace Chummer
{
    partial class SmallSkillControl
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cmdRoll = new System.Windows.Forms.Button();
            this.lblSkillName = new System.Windows.Forms.Label();
            this.chkUseSpecial = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.cmdRoll, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSkillName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkUseSpecial, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(301, 24);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // cmdRoll
            // 
            this.cmdRoll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRoll.FlatAppearance.BorderSize = 0;
            this.cmdRoll.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRoll.Image = global::Chummer.Properties.Resources.die;
            this.cmdRoll.Location = new System.Drawing.Point(184, 0);
            this.cmdRoll.Margin = new System.Windows.Forms.Padding(0);
            this.cmdRoll.Name = "cmdRoll";
            this.cmdRoll.Size = new System.Drawing.Size(24, 24);
            this.cmdRoll.TabIndex = 12;
            this.cmdRoll.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdRoll.UseVisualStyleBackColor = true;
            this.cmdRoll.Click += new System.EventHandler(this.cmdRoll_Click);
            // 
            // lblSkillName
            // 
            this.lblSkillName.AutoSize = true;
            this.lblSkillName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSkillName.Location = new System.Drawing.Point(3, 0);
            this.lblSkillName.Name = "lblSkillName";
            this.lblSkillName.Size = new System.Drawing.Size(178, 24);
            this.lblSkillName.TabIndex = 0;
            this.lblSkillName.Text = "SkillName";
            this.lblSkillName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSkillName.Click += new System.EventHandler(this.lblSkillName_Click);
            // 
            // chkUseSpecial
            // 
            this.chkUseSpecial.AutoSize = true;
            this.chkUseSpecial.Location = new System.Drawing.Point(211, 3);
            this.chkUseSpecial.Name = "chkUseSpecial";
            this.chkUseSpecial.Size = new System.Drawing.Size(83, 17);
            this.chkUseSpecial.TabIndex = 13;
            this.chkUseSpecial.Text = "Use Special";
            this.chkUseSpecial.UseVisualStyleBackColor = true;
            // 
            // SmallSkillControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SmallSkillControl";
            this.Size = new System.Drawing.Size(301, 24);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblSkillName;
        private System.Windows.Forms.Button cmdRoll;
        private System.Windows.Forms.CheckBox chkUseSpecial;
    }
}
