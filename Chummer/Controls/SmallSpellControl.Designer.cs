namespace Chummer.Controls
{
    partial class SmallSpellControl
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdRollSpell = new System.Windows.Forms.Button();
            this.txtSpellName = new System.Windows.Forms.TextBox();
            this.nudDram = new System.Windows.Forms.NumericUpDown();
            this.lblDram = new System.Windows.Forms.Label();
            this.btnDrain = new System.Windows.Forms.Button();
            this.nudForce = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.lblDrain = new System.Windows.Forms.Label();
            this.lblDrainValue = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudDram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdRollSpell
            // 
            this.cmdRollSpell.FlatAppearance.BorderSize = 0;
            this.cmdRollSpell.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRollSpell.Image = global::Chummer.Properties.Resources.die;
            this.cmdRollSpell.Location = new System.Drawing.Point(156, -3);
            this.cmdRollSpell.Name = "cmdRollSpell";
            this.cmdRollSpell.Size = new System.Drawing.Size(24, 24);
            this.cmdRollSpell.TabIndex = 127;
            this.cmdRollSpell.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmdRollSpell.UseVisualStyleBackColor = true;
            this.cmdRollSpell.Click += new System.EventHandler(this.cmdRollSpell_Click);
            // 
            // txtSpellName
            // 
            this.txtSpellName.Location = new System.Drawing.Point(0, 0);
            this.txtSpellName.Name = "txtSpellName";
            this.txtSpellName.Size = new System.Drawing.Size(150, 20);
            this.txtSpellName.TabIndex = 130;
            // 
            // nudDram
            // 
            this.nudDram.Location = new System.Drawing.Point(297, 1);
            this.nudDram.Name = "nudDram";
            this.nudDram.Size = new System.Drawing.Size(34, 20);
            this.nudDram.TabIndex = 131;
            // 
            // lblDram
            // 
            this.lblDram.AutoSize = true;
            this.lblDram.Location = new System.Drawing.Point(259, 3);
            this.lblDram.Name = "lblDram";
            this.lblDram.Size = new System.Drawing.Size(32, 13);
            this.lblDram.TabIndex = 132;
            this.lblDram.Text = "Dram";
            this.lblDram.Click += new System.EventHandler(this.lblDram_Click);
            // 
            // btnDrain
            // 
            this.btnDrain.FlatAppearance.BorderSize = 0;
            this.btnDrain.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDrain.Image = global::Chummer.Properties.Resources.die;
            this.btnDrain.Location = new System.Drawing.Point(337, -3);
            this.btnDrain.Name = "btnDrain";
            this.btnDrain.Size = new System.Drawing.Size(24, 24);
            this.btnDrain.TabIndex = 133;
            this.btnDrain.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnDrain.UseVisualStyleBackColor = true;
            this.btnDrain.Click += new System.EventHandler(this.btnDrain_Click);
            // 
            // nudForce
            // 
            this.nudForce.Location = new System.Drawing.Point(219, 1);
            this.nudForce.Name = "nudForce";
            this.nudForce.Size = new System.Drawing.Size(34, 20);
            this.nudForce.TabIndex = 134;
            this.nudForce.ValueChanged += new System.EventHandler(this.nudForce_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(181, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 135;
            this.label1.Text = "Force";
            // 
            // lblDrain
            // 
            this.lblDrain.AutoSize = true;
            this.lblDrain.Location = new System.Drawing.Point(407, 3);
            this.lblDrain.Name = "lblDrain";
            this.lblDrain.Size = new System.Drawing.Size(38, 13);
            this.lblDrain.TabIndex = 136;
            this.lblDrain.Text = "[Drain]";
            // 
            // lblDrainValue
            // 
            this.lblDrainValue.AutoSize = true;
            this.lblDrainValue.Location = new System.Drawing.Point(363, 3);
            this.lblDrainValue.Name = "lblDrainValue";
            this.lblDrainValue.Size = new System.Drawing.Size(38, 13);
            this.lblDrainValue.TabIndex = 137;
            this.lblDrainValue.Text = "[Drain]";
            // 
            // SmallSpellControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblDrainValue);
            this.Controls.Add(this.lblDrain);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudForce);
            this.Controls.Add(this.btnDrain);
            this.Controls.Add(this.lblDram);
            this.Controls.Add(this.nudDram);
            this.Controls.Add(this.txtSpellName);
            this.Controls.Add(this.cmdRollSpell);
            this.Name = "SmallSpellControl";
            this.Size = new System.Drawing.Size(445, 21);
            ((System.ComponentModel.ISupportInitialize)(this.nudDram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudForce)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cmdRollSpell;
        private System.Windows.Forms.TextBox txtSpellName;
        private System.Windows.Forms.NumericUpDown nudDram;
        private System.Windows.Forms.Label lblDram;
        private System.Windows.Forms.Button btnDrain;
        private System.Windows.Forms.NumericUpDown nudForce;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDrain;
        private System.Windows.Forms.Label lblDrainValue;
    }
}
