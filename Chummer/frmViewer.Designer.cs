namespace Chummer
{
	partial class frmViewer
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
			this.cmdSaveHTML = new System.Windows.Forms.Button();
			this.webBrowser1 = new System.Windows.Forms.WebBrowser();
			this.cmdPrint = new System.Windows.Forms.Button();
			this.SaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.cboXSLT = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cmdSaveXML = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// cmdSaveHTML
			// 
			this.cmdSaveHTML.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.cmdSaveHTML.Location = new System.Drawing.Point(144, 12);
			this.cmdSaveHTML.Name = "cmdSaveHTML";
			this.cmdSaveHTML.Size = new System.Drawing.Size(117, 23);
			this.cmdSaveHTML.TabIndex = 1;
			this.cmdSaveHTML.Tag = "Button_Viewer_SaveAsHtml";
			this.cmdSaveHTML.Text = "Save as HTML";
			this.cmdSaveHTML.UseVisualStyleBackColor = true;
			this.cmdSaveHTML.Click += new System.EventHandler(this.cmdSaveHTML_Click);
			// 
			// webBrowser1
			// 
			this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.webBrowser1.Location = new System.Drawing.Point(12, 41);
			this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.ScriptErrorsSuppressed = true;
			this.webBrowser1.Size = new System.Drawing.Size(756, 583);
			this.webBrowser1.TabIndex = 5;
			this.webBrowser1.WebBrowserShortcutsEnabled = false;
			// 
			// cmdPrint
			// 
			this.cmdPrint.AutoSize = true;
			this.cmdPrint.Image = global::Chummer.Properties.Resources.printer;
			this.cmdPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.cmdPrint.Location = new System.Drawing.Point(12, 12);
			this.cmdPrint.Name = "cmdPrint";
			this.cmdPrint.Size = new System.Drawing.Size(102, 23);
			this.cmdPrint.TabIndex = 0;
			this.cmdPrint.Tag = "Button_Viewer_PrintCharacter";
			this.cmdPrint.Text = "Print Character";
			this.cmdPrint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cmdPrint.UseVisualStyleBackColor = true;
			this.cmdPrint.Click += new System.EventHandler(this.cmdPrint_Click);
			// 
			// cboXSLT
			// 
			this.cboXSLT.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboXSLT.FormattingEnabled = true;
			this.cboXSLT.Location = new System.Drawing.Point(501, 14);
			this.cboXSLT.Name = "cboXSLT";
			this.cboXSLT.Size = new System.Drawing.Size(266, 21);
			this.cboXSLT.TabIndex = 4;
			this.cboXSLT.SelectedIndexChanged += new System.EventHandler(this.cboXSLT_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(408, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(87, 13);
			this.label1.TabIndex = 3;
			this.label1.Tag = "Label_Viewer_CharacterSheet";
			this.label1.Text = "Character Sheet:";
			// 
			// cmdSaveXML
			// 
			this.cmdSaveXML.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.cmdSaveXML.Location = new System.Drawing.Point(267, 12);
			this.cmdSaveXML.Name = "cmdSaveXML";
			this.cmdSaveXML.Size = new System.Drawing.Size(91, 23);
			this.cmdSaveXML.TabIndex = 2;
			this.cmdSaveXML.Tag = "Button_Viewer_SaveAsXml";
			this.cmdSaveXML.Text = "Save XML";
			this.cmdSaveXML.UseVisualStyleBackColor = true;
			this.cmdSaveXML.Click += new System.EventHandler(this.cmdSaveXML_Click);
			// 
			// frmViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(779, 637);
			this.Controls.Add(this.cmdSaveXML);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cboXSLT);
			this.Controls.Add(this.cmdSaveHTML);
			this.Controls.Add(this.webBrowser1);
			this.Controls.Add(this.cmdPrint);
			this.Name = "frmViewer";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Tag = "Title_CharacterViewer";
			this.Text = "Character Viewer";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmViewer_FormClosing);
			this.Load += new System.EventHandler(this.frmViewer_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.Button cmdSaveHTML;
		private System.Windows.Forms.WebBrowser webBrowser1;
		internal System.Windows.Forms.Button cmdPrint;
		internal System.Windows.Forms.SaveFileDialog SaveFileDialog1;
		private System.Windows.Forms.ComboBox cboXSLT;
		private System.Windows.Forms.Label label1;
		internal System.Windows.Forms.Button cmdSaveXML;

	}
}