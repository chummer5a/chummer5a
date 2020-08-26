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
            if (disposing)
            {
                components?.Dispose();
                if (!(ParentForm is CharacterShared frmParent) || frmParent.CharacterObject != _objCharacter)
                    _objCharacter?.Dispose();
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
            this.cmdRoll = new System.Windows.Forms.Button();
            this.tlpContainer = new System.Windows.Forms.TableLayoutPanel();
            this.lblDicePool = new Chummer.LabelWithToolTip();
            this.tlpContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdRoll
            // 
            this.cmdRoll.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdRoll.AutoSize = true;
            this.cmdRoll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdRoll.FlatAppearance.BorderSize = 0;
            this.cmdRoll.Image = global::Chummer.Properties.Resources.die;
            this.cmdRoll.Location = new System.Drawing.Point(40, 0);
            this.cmdRoll.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cmdRoll.Name = "cmdRoll";
            this.cmdRoll.Padding = new System.Windows.Forms.Padding(1);
            this.cmdRoll.Size = new System.Drawing.Size(24, 24);
            this.cmdRoll.TabIndex = 119;
            this.cmdRoll.UseVisualStyleBackColor = true;
            this.cmdRoll.Click += new System.EventHandler(this.cmdRoll_Click);
            // 
            // tlpContainer
            // 
            this.tlpContainer.AutoSize = true;
            this.tlpContainer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpContainer.ColumnCount = 2;
            this.tlpContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpContainer.Controls.Add(this.lblDicePool, 0, 0);
            this.tlpContainer.Controls.Add(this.cmdRoll, 1, 0);
            this.tlpContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpContainer.Location = new System.Drawing.Point(0, 0);
            this.tlpContainer.Name = "tlpContainer";
            this.tlpContainer.RowCount = 1;
            this.tlpContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpContainer.Size = new System.Drawing.Size(64, 24);
            this.tlpContainer.TabIndex = 1;
            // 
            // lblDicePool
            // 
            this.lblDicePool.AutoSize = true;
            this.lblDicePool.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDicePool.Location = new System.Drawing.Point(0, 0);
            this.lblDicePool.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.lblDicePool.Name = "lblDicePool";
            this.lblDicePool.Size = new System.Drawing.Size(34, 24);
            this.lblDicePool.TabIndex = 118;
            this.lblDicePool.Text = "[Pool]";
            this.lblDicePool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblDicePool.ToolTipText = "";
            // 
            // DicePoolControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpContainer);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.Name = "DicePoolControl";
            this.Size = new System.Drawing.Size(64, 24);
            this.Load += new System.EventHandler(this.DicePoolControl_Load);
            this.tlpContainer.ResumeLayout(false);
            this.tlpContainer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cmdRoll;
        private LabelWithToolTip lblDicePool;
        private System.Windows.Forms.TableLayoutPanel tlpContainer;
    }
}
