namespace Chummer
{
    partial class frmPrintMultiple
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
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.cmdSelectCharacter = new System.Windows.Forms.Button();
            this.cmdPrint = new System.Windows.Forms.Button();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.treCharacters = new System.Windows.Forms.TreeView();
            this.prgProgress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // dlgOpenFile
            // 
            this.dlgOpenFile.Filter = "Chummer5 Files (*.chum5)|*.chum5";
            this.dlgOpenFile.Multiselect = true;
            this.dlgOpenFile.Title = "Select Character(s)";
            // 
            // cmdSelectCharacter
            // 
            this.cmdSelectCharacter.AutoSize = true;
            this.cmdSelectCharacter.Location = new System.Drawing.Point(271, 12);
            this.cmdSelectCharacter.Name = "cmdSelectCharacter";
            this.cmdSelectCharacter.Size = new System.Drawing.Size(106, 23);
            this.cmdSelectCharacter.TabIndex = 1;
            this.cmdSelectCharacter.Tag = "Button_PrintMultiple_AddCharacter";
            this.cmdSelectCharacter.Text = "&Add Character";
            this.cmdSelectCharacter.UseVisualStyleBackColor = true;
            this.cmdSelectCharacter.Click += new System.EventHandler(this.cmdSelectCharacter_Click);
            // 
            // cmdPrint
            // 
            this.cmdPrint.AutoSize = true;
            this.cmdPrint.Location = new System.Drawing.Point(271, 107);
            this.cmdPrint.Name = "cmdPrint";
            this.cmdPrint.Size = new System.Drawing.Size(106, 23);
            this.cmdPrint.TabIndex = 3;
            this.cmdPrint.Tag = "Button_PrintMultiple_PrintCharacter";
            this.cmdPrint.Text = "&Print Characters";
            this.cmdPrint.UseVisualStyleBackColor = true;
            this.cmdPrint.Click += new System.EventHandler(this.cmdPrint_Click);
            // 
            // cmdDelete
            // 
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.Location = new System.Drawing.Point(271, 41);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(106, 23);
            this.cmdDelete.TabIndex = 2;
            this.cmdDelete.Tag = "Button_PrintMultiple_RemoveCharacter";
            this.cmdDelete.Text = "Remove Character";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // treCharacters
            // 
            this.treCharacters.HideSelection = false;
            this.treCharacters.Location = new System.Drawing.Point(12, 12);
            this.treCharacters.Name = "treCharacters";
            this.treCharacters.ShowLines = false;
            this.treCharacters.ShowPlusMinus = false;
            this.treCharacters.ShowRootLines = false;
            this.treCharacters.Size = new System.Drawing.Size(253, 214);
            this.treCharacters.TabIndex = 0;
            // 
            // prgProgress
            // 
            this.prgProgress.Location = new System.Drawing.Point(12, 232);
            this.prgProgress.Name = "prgProgress";
            this.prgProgress.Size = new System.Drawing.Size(359, 23);
            this.prgProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgProgress.TabIndex = 4;
            // 
            // frmPrintMultiple
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 264);
            this.Controls.Add(this.prgProgress);
            this.Controls.Add(this.treCharacters);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.cmdPrint);
            this.Controls.Add(this.cmdSelectCharacter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPrintMultiple";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_PrintMultiple";
            this.Text = "Select Characters to Print";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog dlgOpenFile;
        private System.Windows.Forms.Button cmdSelectCharacter;
        private System.Windows.Forms.Button cmdPrint;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.TreeView treCharacters;
        private System.Windows.Forms.ProgressBar prgProgress;
    }
}