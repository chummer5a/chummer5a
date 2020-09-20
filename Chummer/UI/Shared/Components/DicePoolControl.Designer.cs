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
            this.lblDicePool = new Chummer.LabelWithToolTip();
            this.flpContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.flpContainer.SuspendLayout();
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
            // lblDicePool
            // 
            this.lblDicePool.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDicePool.AutoSize = true;
            this.lblDicePool.Location = new System.Drawing.Point(0, 5);
            this.lblDicePool.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.lblDicePool.Name = "lblDicePool";
            this.lblDicePool.Size = new System.Drawing.Size(34, 13);
            this.lblDicePool.TabIndex = 118;
            this.lblDicePool.Text = "[Pool]";
            this.lblDicePool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblDicePool.ToolTipText = "";
            // 
            // flpContainer
            // 
            this.flpContainer.AutoSize = true;
            this.flpContainer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpContainer.Controls.Add(this.lblDicePool);
            this.flpContainer.Controls.Add(this.cmdRoll);
            this.flpContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpContainer.Location = new System.Drawing.Point(0, 0);
            this.flpContainer.Name = "flpContainer";
            this.flpContainer.Size = new System.Drawing.Size(64, 24);
            this.flpContainer.TabIndex = 2;
            this.flpContainer.WrapContents = false;
            // 
            // DicePoolControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.flpContainer);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.Name = "DicePoolControl";
            this.Size = new System.Drawing.Size(64, 24);
            this.Load += new System.EventHandler(this.DicePoolControl_Load);
            this.flpContainer.ResumeLayout(false);
            this.flpContainer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cmdRoll;
        private LabelWithToolTip lblDicePool;
        private System.Windows.Forms.FlowLayoutPanel flpContainer;
    }
}
