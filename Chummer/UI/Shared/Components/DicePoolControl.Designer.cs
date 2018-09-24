namespace Chummer.UI.Shared.Components
{
    partial class DicePoolControl
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
			this.lblDicePool = new LabelWithToolTip();
			this.cmdRoll = new System.Windows.Forms.Button();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblDicePool
			// 
			this.lblDicePool.AutoSize = true;
			this.lblDicePool.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDicePool.Location = new System.Drawing.Point(3, 0);
			this.lblDicePool.Name = "lblDicePool";
			this.lblDicePool.Size = new System.Drawing.Size(34, 30);
			this.lblDicePool.TabIndex = 118;
			this.lblDicePool.Text = "[Pool]";
			this.lblDicePool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cmdRoll
			// 
			this.cmdRoll.FlatAppearance.BorderSize = 0;
			this.cmdRoll.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdRoll.Image = global::Chummer.Properties.Resources.die;
			this.cmdRoll.Location = new System.Drawing.Point(43, 3);
			this.cmdRoll.Name = "cmdRoll";
			this.cmdRoll.Size = new System.Drawing.Size(24, 24);
			this.cmdRoll.TabIndex = 119;
			this.cmdRoll.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.cmdRoll.UseVisualStyleBackColor = true;
			this.cmdRoll.Visible = false;
			this.cmdRoll.Click += new System.EventHandler(this.cmdRoll_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.lblDicePool, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.cmdRoll, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(70, 30);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// DicePoolControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "DicePoolControl";
			this.Size = new System.Drawing.Size(70, 30);
			this.Load += new System.EventHandler(this.DicePoolControl_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblDicePool;
        private System.Windows.Forms.Button cmdRoll;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
