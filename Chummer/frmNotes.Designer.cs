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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmNotes));
            this.htmNotes = new Chummer.UI.Editor.HtmlEditor();
            this.SuspendLayout();
            // 
            // htmNotes
            // 
            this.htmNotes.BodyText = "";
            this.htmNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.htmNotes.DocumentText = resources.GetString("htmNotes.DocumentText");
            this.htmNotes.Html = "";
            this.htmNotes.Location = new System.Drawing.Point(0, 0);
            this.htmNotes.Name = "htmNotes";
            this.htmNotes.Size = new System.Drawing.Size(624, 321);
            this.htmNotes.TabIndex = 0;
            // 
            // frmNotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 321);
            this.Controls.Add(this.htmNotes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(640, 360);
            this.Name = "frmNotes";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_Notes";
            this.Text = "Notes";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmNotes_FormClosing);
            this.Resize += new System.EventHandler(this.frmNotes_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private UI.Editor.HtmlEditor htmNotes;
    }
}
