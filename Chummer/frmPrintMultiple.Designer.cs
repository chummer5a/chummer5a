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
            if (disposing)
            {
                components?.Dispose();
                _workerPrinter?.Dispose();
                _frmPrintView?.Dispose();
                if (_lstCharacters?.Count > 0)
                    foreach (Character objCharacter in _lstCharacters)
                        objCharacter.Dispose();
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
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
            this.cmdSelectCharacter.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmdSelectCharacter.Location = new System.Drawing.Point(0, 0);
            this.cmdSelectCharacter.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
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
            this.cmdPrint.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmdPrint.Location = new System.Drawing.Point(0, 58);
            this.cmdPrint.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
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
            this.cmdDelete.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmdDelete.Location = new System.Drawing.Point(0, 29);
            this.cmdDelete.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
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
            this.tableLayoutPanel1.SetColumnSpan(this.prgProgress, 2);
            this.prgProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.prgProgress.Location = new System.Drawing.Point(3, 237);
            this.prgProgress.Name = "prgProgress";
            this.prgProgress.Size = new System.Drawing.Size(440, 23);
            this.prgProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgProgress.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.prgProgress, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.treCharacters, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(446, 263);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(446, 263);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.cmdSelectCharacter);
            this.flowLayoutPanel1.Controls.Add(this.cmdDelete);
            this.flowLayoutPanel1.Controls.Add(this.cmdPrint);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(337, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(106, 81);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // frmPrintMultiple
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPrintMultiple";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_PrintMultiple";
            this.Text = "Select Characters to Print";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
