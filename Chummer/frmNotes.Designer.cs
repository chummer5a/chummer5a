namespace Chummer
{
    partial class frmNotes
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

        #region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.rtfNotes = new Chummer.UI.Editors.RtfEditor();
            this.SuspendLayout();
            // 
            // rtfNotes
            // 
            this.rtfNotes.AllowFormatting = false;
            this.rtfNotes.AutoSize = true;
            this.rtfNotes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rtfNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfNotes.Location = new System.Drawing.Point(0, 0);
            this.rtfNotes.Name = "rtfNotes";
            this.rtfNotes.Rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 " +
    "Microsoft Sans Serif;}}\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \r\n\\par" +
    "d\\f0\\fs17\\par\r\n}\r\n";
            this.rtfNotes.Size = new System.Drawing.Size(624, 321);
            this.rtfNotes.TabIndex = 0;
            // 
            // frmNotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 321);
            this.Controls.Add(this.rtfNotes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(640, 360);
            this.Name = "frmNotes";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_Notes";
            this.Text = "Notes";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmNotes_FormClosing);
            this.Shown += new System.EventHandler(this.frmNotes_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtfNotes_KeyDown);
            this.Resize += new System.EventHandler(this.frmNotes_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UI.Editors.RtfEditor rtfNotes;
    }
}
