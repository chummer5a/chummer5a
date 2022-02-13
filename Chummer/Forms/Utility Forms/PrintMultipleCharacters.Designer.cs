namespace Chummer
{
    partial class PrintMultipleCharacters
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
                _frmPrintView?.Dispose();
                _objPrinterCancellationTokenSource?.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintMultipleCharacters));
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.cmdSelectCharacter = new System.Windows.Forms.Button();
            this.cmdPrint = new System.Windows.Forms.Button();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.treCharacters = new System.Windows.Forms.TreeView();
            this.prgProgress = new System.Windows.Forms.ProgressBar();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpMain.SuspendLayout();
            this.tlpButtons.SuspendLayout();
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
            this.cmdSelectCharacter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdSelectCharacter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdSelectCharacter.Location = new System.Drawing.Point(3, 3);
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
            this.cmdPrint.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPrint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdPrint.Location = new System.Drawing.Point(3, 61);
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
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDelete.Location = new System.Drawing.Point(3, 32);
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
            this.treCharacters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treCharacters.HideSelection = false;
            this.treCharacters.Location = new System.Drawing.Point(3, 3);
            this.treCharacters.MinimumSize = new System.Drawing.Size(328, 228);
            this.treCharacters.Name = "treCharacters";
            this.treCharacters.ShowLines = false;
            this.treCharacters.ShowPlusMinus = false;
            this.treCharacters.ShowRootLines = false;
            this.treCharacters.Size = new System.Drawing.Size(328, 228);
            this.treCharacters.TabIndex = 0;
            // 
            // prgProgress
            // 
            this.tlpMain.SetColumnSpan(this.prgProgress, 2);
            this.prgProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.prgProgress.Location = new System.Drawing.Point(3, 237);
            this.prgProgress.Name = "prgProgress";
            this.prgProgress.Size = new System.Drawing.Size(440, 23);
            this.prgProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgProgress.TabIndex = 4;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Controls.Add(this.prgProgress, 0, 1);
            this.tlpMain.Controls.Add(this.treCharacters, 0, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.MinimumSize = new System.Drawing.Size(446, 263);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(446, 263);
            this.tlpMain.TabIndex = 5;
            // 
            // tlpButtons
            // 
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 1;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Controls.Add(this.cmdSelectCharacter, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdPrint, 0, 2);
            this.tlpButtons.Controls.Add(this.cmdDelete, 0, 1);
            this.tlpButtons.Location = new System.Drawing.Point(334, 0);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 3;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpButtons.Size = new System.Drawing.Size(112, 87);
            this.tlpButtons.TabIndex = 5;
            // 
            // frmPrintMultiple
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPrintMultiple";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_PrintMultiple";
            this.Text = "Select Characters to Print";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmPrintMultiple_FormClosing);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
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
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private BufferedTableLayoutPanel tlpButtons;
    }
}
