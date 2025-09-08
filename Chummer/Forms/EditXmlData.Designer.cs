namespace Chummer
{
    partial class EditXmlData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditXmlData));
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.gpbFileSelection = new System.Windows.Forms.GroupBox();
            this.tlpFileSelection = new System.Windows.Forms.TableLayoutPanel();
            this.lblXmlFile = new System.Windows.Forms.Label();
            this.cboXmlFiles = new Chummer.ElasticComboBox();
            this.cmdLoadXml = new System.Windows.Forms.Button();
            this.gpbBaseXml = new System.Windows.Forms.GroupBox();
            this.txtBaseXml = new System.Windows.Forms.TextBox();
            this.gpbAmendmentXml = new System.Windows.Forms.GroupBox();
            this.tlpAmendment = new System.Windows.Forms.TableLayoutPanel();
            this.txtAmendmentXml = new System.Windows.Forms.TextBox();
            this.flpAmendmentButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdApplyAmendment = new System.Windows.Forms.Button();
            this.cmdSaveAmendment = new System.Windows.Forms.Button();
            this.gpbResult = new System.Windows.Forms.GroupBox();
            this.tlpResult = new System.Windows.Forms.TableLayoutPanel();
            this.txtResultXml = new System.Windows.Forms.TextBox();
            this.txtDiffPreview = new System.Windows.Forms.TextBox();
            this.tlpMain.SuspendLayout();
            this.gpbFileSelection.SuspendLayout();
            this.tlpFileSelection.SuspendLayout();
            this.gpbBaseXml.SuspendLayout();
            this.gpbAmendmentXml.SuspendLayout();
            this.tlpAmendment.SuspendLayout();
            this.flpAmendmentButtons.SuspendLayout();
            this.gpbResult.SuspendLayout();
            this.tlpResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.gpbFileSelection, 0, 0);
            this.tlpMain.Controls.Add(this.gpbBaseXml, 0, 1);
            this.tlpMain.Controls.Add(this.gpbAmendmentXml, 0, 2);
            this.tlpMain.Controls.Add(this.gpbResult, 0, 3);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpMain.Size = new System.Drawing.Size(1400, 900);
            this.tlpMain.TabIndex = 0;
            // 
            // gpbFileSelection
            // 
            this.gpbFileSelection.Controls.Add(this.tlpFileSelection);
            this.gpbFileSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbFileSelection.Location = new System.Drawing.Point(3, 3);
            this.gpbFileSelection.Name = "gpbFileSelection";
            this.gpbFileSelection.Size = new System.Drawing.Size(1394, 74);
            this.gpbFileSelection.TabIndex = 0;
            this.gpbFileSelection.TabStop = false;
            this.gpbFileSelection.Text = "Base XML File Selection";
            // 
            // tlpFileSelection
            // 
            this.tlpFileSelection.ColumnCount = 3;
            this.tlpFileSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpFileSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFileSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpFileSelection.Controls.Add(this.lblXmlFile, 0, 0);
            this.tlpFileSelection.Controls.Add(this.cboXmlFiles, 1, 0);
            this.tlpFileSelection.Controls.Add(this.cmdLoadXml, 2, 0);
            this.tlpFileSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFileSelection.Location = new System.Drawing.Point(3, 16);
            this.tlpFileSelection.Name = "tlpFileSelection";
            this.tlpFileSelection.RowCount = 1;
            this.tlpFileSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFileSelection.Size = new System.Drawing.Size(1388, 55);
            this.tlpFileSelection.TabIndex = 0;
            // 
            // lblXmlFile
            // 
            this.lblXmlFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblXmlFile.AutoSize = true;
            this.lblXmlFile.Location = new System.Drawing.Point(3, 20);
            this.lblXmlFile.Name = "lblXmlFile";
            this.lblXmlFile.Size = new System.Drawing.Size(54, 13);
            this.lblXmlFile.TabIndex = 0;
            this.lblXmlFile.Text = "Base File:";
            // 
            // cboXmlFiles
            // 
            this.cboXmlFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboXmlFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboXmlFiles.FormattingEnabled = true;
            this.cboXmlFiles.Location = new System.Drawing.Point(103, 16);
            this.cboXmlFiles.Name = "cboXmlFiles";
            this.cboXmlFiles.Size = new System.Drawing.Size(1182, 21);
            this.cboXmlFiles.TabIndex = 1;
            // 
            // cmdLoadXml
            // 
            this.cmdLoadXml.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdLoadXml.Location = new System.Drawing.Point(1291, 15);
            this.cmdLoadXml.Name = "cmdLoadXml";
            this.cmdLoadXml.Size = new System.Drawing.Size(94, 23);
            this.cmdLoadXml.TabIndex = 2;
            this.cmdLoadXml.Text = "Load Base";
            this.cmdLoadXml.UseVisualStyleBackColor = true;
            this.cmdLoadXml.Click += new System.EventHandler(this.cmdLoadXml_Click);
            // 
            // gpbBaseXml
            // 
            this.gpbBaseXml.Controls.Add(this.txtBaseXml);
            this.gpbBaseXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbBaseXml.Location = new System.Drawing.Point(3, 83);
            this.gpbBaseXml.Name = "gpbBaseXml";
            this.gpbBaseXml.Size = new System.Drawing.Size(1394, 240);
            this.gpbBaseXml.TabIndex = 1;
            this.gpbBaseXml.TabStop = false;
            this.gpbBaseXml.Text = "Base XML (Read-Only)";
            // 
            // txtBaseXml
            // 
            this.txtBaseXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBaseXml.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBaseXml.Location = new System.Drawing.Point(3, 16);
            this.txtBaseXml.Multiline = true;
            this.txtBaseXml.Name = "txtBaseXml";
            this.txtBaseXml.ReadOnly = true;
            this.txtBaseXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBaseXml.Size = new System.Drawing.Size(1388, 221);
            this.txtBaseXml.TabIndex = 0;
            this.txtBaseXml.WordWrap = false;
            // 
            // gpbAmendmentXml
            // 
            this.gpbAmendmentXml.Controls.Add(this.tlpAmendment);
            this.gpbAmendmentXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbAmendmentXml.Location = new System.Drawing.Point(3, 329);
            this.gpbAmendmentXml.Name = "gpbAmendmentXml";
            this.gpbAmendmentXml.Size = new System.Drawing.Size(1394, 240);
            this.gpbAmendmentXml.TabIndex = 2;
            this.gpbAmendmentXml.TabStop = false;
            this.gpbAmendmentXml.Text = "Amendment XML (Edit Here)";
            // 
            // tlpAmendment
            // 
            this.tlpAmendment.ColumnCount = 1;
            this.tlpAmendment.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAmendment.Controls.Add(this.txtAmendmentXml, 0, 0);
            this.tlpAmendment.Controls.Add(this.flpAmendmentButtons, 0, 1);
            this.tlpAmendment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAmendment.Location = new System.Drawing.Point(3, 16);
            this.tlpAmendment.Name = "tlpAmendment";
            this.tlpAmendment.RowCount = 2;
            this.tlpAmendment.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAmendment.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpAmendment.Size = new System.Drawing.Size(1388, 221);
            this.tlpAmendment.TabIndex = 0;
            // 
            // txtAmendmentXml
            // 
            this.txtAmendmentXml.AcceptsReturn = true;
            this.txtAmendmentXml.AcceptsTab = true;
            this.txtAmendmentXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAmendmentXml.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAmendmentXml.Location = new System.Drawing.Point(3, 3);
            this.txtAmendmentXml.Multiline = true;
            this.txtAmendmentXml.Name = "txtAmendmentXml";
            this.txtAmendmentXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtAmendmentXml.Size = new System.Drawing.Size(1382, 175);
            this.txtAmendmentXml.TabIndex = 0;
            this.txtAmendmentXml.WordWrap = false;
            this.txtAmendmentXml.TextChanged += new System.EventHandler(this.txtAmendmentXml_TextChanged);
            // 
            // flpAmendmentButtons
            // 
            this.flpAmendmentButtons.Controls.Add(this.cmdApplyAmendment);
            this.flpAmendmentButtons.Controls.Add(this.cmdSaveAmendment);
            this.flpAmendmentButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpAmendmentButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flpAmendmentButtons.Location = new System.Drawing.Point(3, 184);
            this.flpAmendmentButtons.Name = "flpAmendmentButtons";
            this.flpAmendmentButtons.Size = new System.Drawing.Size(1382, 34);
            this.flpAmendmentButtons.TabIndex = 1;
            // 
            // cmdApplyAmendment
            // 
            this.cmdApplyAmendment.Enabled = false;
            this.cmdApplyAmendment.Location = new System.Drawing.Point(3, 3);
            this.cmdApplyAmendment.Name = "cmdApplyAmendment";
            this.cmdApplyAmendment.Size = new System.Drawing.Size(120, 23);
            this.cmdApplyAmendment.TabIndex = 0;
            this.cmdApplyAmendment.Text = "Apply Amendment";
            this.cmdApplyAmendment.UseVisualStyleBackColor = true;
            this.cmdApplyAmendment.Click += new System.EventHandler(this.cmdApplyAmendment_Click);
            // 
            // cmdSaveAmendment
            // 
            this.cmdSaveAmendment.Enabled = false;
            this.cmdSaveAmendment.Location = new System.Drawing.Point(129, 3);
            this.cmdSaveAmendment.Name = "cmdSaveAmendment";
            this.cmdSaveAmendment.Size = new System.Drawing.Size(120, 23);
            this.cmdSaveAmendment.TabIndex = 1;
            this.cmdSaveAmendment.Text = "Save Amendment";
            this.cmdSaveAmendment.UseVisualStyleBackColor = true;
            this.cmdSaveAmendment.Click += new System.EventHandler(this.cmdSaveAmendment_Click);
            // 
            // gpbResult
            // 
            this.gpbResult.Controls.Add(this.tlpResult);
            this.gpbResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbResult.Location = new System.Drawing.Point(3, 575);
            this.gpbResult.Name = "gpbResult";
            this.gpbResult.Size = new System.Drawing.Size(1394, 322);
            this.gpbResult.TabIndex = 3;
            this.gpbResult.TabStop = false;
            this.gpbResult.Text = "Result & Diff Preview";
            // 
            // tlpResult
            // 
            this.tlpResult.ColumnCount = 2;
            this.tlpResult.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpResult.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpResult.Controls.Add(this.txtResultXml, 0, 0);
            this.tlpResult.Controls.Add(this.txtDiffPreview, 1, 0);
            this.tlpResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpResult.Location = new System.Drawing.Point(3, 16);
            this.tlpResult.Name = "tlpResult";
            this.tlpResult.RowCount = 1;
            this.tlpResult.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpResult.Size = new System.Drawing.Size(1388, 303);
            this.tlpResult.TabIndex = 0;
            // 
            // txtResultXml
            // 
            this.txtResultXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtResultXml.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtResultXml.Location = new System.Drawing.Point(3, 3);
            this.txtResultXml.Multiline = true;
            this.txtResultXml.Name = "txtResultXml";
            this.txtResultXml.ReadOnly = true;
            this.txtResultXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResultXml.Size = new System.Drawing.Size(688, 297);
            this.txtResultXml.TabIndex = 0;
            this.txtResultXml.WordWrap = false;
            // 
            // txtDiffPreview
            // 
            this.txtDiffPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDiffPreview.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDiffPreview.Location = new System.Drawing.Point(697, 3);
            this.txtDiffPreview.Multiline = true;
            this.txtDiffPreview.Name = "txtDiffPreview";
            this.txtDiffPreview.ReadOnly = true;
            this.txtDiffPreview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDiffPreview.Size = new System.Drawing.Size(688, 297);
            this.txtDiffPreview.TabIndex = 1;
            this.txtDiffPreview.WordWrap = false;
            // 
            // EditXmlData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 900);
            this.Controls.Add(this.tlpMain);
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.Name = "EditXmlData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "XML Amendment Developer";
            this.Load += new System.EventHandler(this.EditXmlData_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditXmlData_FormClosing);
            this.tlpMain.ResumeLayout(false);
            this.gpbFileSelection.ResumeLayout(false);
            this.tlpFileSelection.ResumeLayout(false);
            this.tlpFileSelection.PerformLayout();
            this.gpbBaseXml.ResumeLayout(false);
            this.gpbBaseXml.PerformLayout();
            this.gpbAmendmentXml.ResumeLayout(false);
            this.tlpAmendment.ResumeLayout(false);
            this.tlpAmendment.PerformLayout();
            this.flpAmendmentButtons.ResumeLayout(false);
            this.gpbResult.ResumeLayout(false);
            this.gpbResult.PerformLayout();
            this.tlpResult.ResumeLayout(false);
            this.tlpResult.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.GroupBox gpbFileSelection;
        private System.Windows.Forms.TableLayoutPanel tlpFileSelection;
        private System.Windows.Forms.Label lblXmlFile;
        private Chummer.ElasticComboBox cboXmlFiles;
        private System.Windows.Forms.Button cmdLoadXml;
        private System.Windows.Forms.GroupBox gpbBaseXml;
        private System.Windows.Forms.TextBox txtBaseXml;
        private System.Windows.Forms.GroupBox gpbAmendmentXml;
        private System.Windows.Forms.TableLayoutPanel tlpAmendment;
        private System.Windows.Forms.TextBox txtAmendmentXml;
        private System.Windows.Forms.FlowLayoutPanel flpAmendmentButtons;
        private System.Windows.Forms.Button cmdApplyAmendment;
        private System.Windows.Forms.Button cmdSaveAmendment;
        private System.Windows.Forms.GroupBox gpbResult;
        private System.Windows.Forms.TableLayoutPanel tlpResult;
        private System.Windows.Forms.TextBox txtResultXml;
        private System.Windows.Forms.TextBox txtDiffPreview;
    }
}
