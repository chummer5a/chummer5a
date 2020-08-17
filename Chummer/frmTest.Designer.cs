namespace Chummer
{
    partial class frmTest
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
                _objCharacter?.Dispose();
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
            this.cboTest = new Chummer.ElasticComboBox();
            this.cmdTest = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.pgbProgress = new System.Windows.Forms.ProgressBar();
            this.chkAddExceptionInfoToErrors = new System.Windows.Forms.CheckBox();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboTest
            // 
            this.cboTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboTest.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTest.FormattingEnabled = true;
            this.cboTest.Items.AddRange(new object[] {
            "armor.xml",
            "bioware.xml",
            "critters.xml",
            "cyberware.xml",
            "gear.xml",
            "metatypes.xml",
            "qualities.xml",
            "vehicles.xml",
            "weapons.xml"});
            this.cboTest.Location = new System.Drawing.Point(6, 7);
            this.cboTest.Name = "cboTest";
            this.cboTest.Size = new System.Drawing.Size(187, 21);
            this.cboTest.Sorted = true;
            this.cboTest.TabIndex = 0;
            this.cboTest.TooltipText = "";
            // 
            // cmdTest
            // 
            this.cmdTest.AutoSize = true;
            this.cmdTest.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdTest.Location = new System.Drawing.Point(199, 6);
            this.cmdTest.Name = "cmdTest";
            this.cmdTest.Size = new System.Drawing.Size(38, 23);
            this.cmdTest.TabIndex = 1;
            this.cmdTest.Text = "&Test";
            this.cmdTest.UseVisualStyleBackColor = true;
            this.cmdTest.Click += new System.EventHandler(this.cmdTest_Click);
            // 
            // txtOutput
            // 
            this.tlpMain.SetColumnSpan(this.txtOutput, 3);
            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOutput.Location = new System.Drawing.Point(6, 64);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(772, 491);
            this.txtOutput.TabIndex = 2;
            // 
            // pgbProgress
            // 
            this.tlpMain.SetColumnSpan(this.pgbProgress, 3);
            this.pgbProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgbProgress.Location = new System.Drawing.Point(6, 35);
            this.pgbProgress.Name = "pgbProgress";
            this.pgbProgress.Size = new System.Drawing.Size(772, 23);
            this.pgbProgress.Step = 1;
            this.pgbProgress.TabIndex = 3;
            // 
            // chkAddExceptionInfoToErrors
            // 
            this.chkAddExceptionInfoToErrors.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkAddExceptionInfoToErrors.AutoSize = true;
            this.chkAddExceptionInfoToErrors.Location = new System.Drawing.Point(243, 9);
            this.chkAddExceptionInfoToErrors.Name = "chkAddExceptionInfoToErrors";
            this.chkAddExceptionInfoToErrors.Size = new System.Drawing.Size(253, 17);
            this.chkAddExceptionInfoToErrors.TabIndex = 4;
            this.chkAddExceptionInfoToErrors.Text = "Include Exception Information in Error Messages";
            this.chkAddExceptionInfoToErrors.UseVisualStyleBackColor = true;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.txtOutput, 0, 2);
            this.tlpMain.Controls.Add(this.pgbProgress, 0, 1);
            this.tlpMain.Controls.Add(this.chkAddExceptionInfoToErrors, 2, 0);
            this.tlpMain.Controls.Add(this.cboTest, 0, 0);
            this.tlpMain.Controls.Add(this.cmdTest, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(3);
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(784, 561);
            this.tlpMain.TabIndex = 5;
            // 
            // frmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.Name = "frmTest";
            this.Text = "XML Test";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ElasticComboBox cboTest;
        private System.Windows.Forms.Button cmdTest;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.ProgressBar pgbProgress;
        private System.Windows.Forms.CheckBox chkAddExceptionInfoToErrors;
        private BufferedTableLayoutPanel tlpMain;
    }
}
