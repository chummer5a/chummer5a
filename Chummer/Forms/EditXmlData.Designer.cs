using System.Threading;

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
            if (disposing)
            {
                CancellationTokenSource objOldSource = Interlocked.Exchange(ref _objApplyAmendmentCancellationTokenSource, null);
                if (objOldSource != null)
                {
                    objOldSource.Cancel(false);
                    objOldSource.Dispose();
                }
                objOldSource = Interlocked.Exchange(ref _objSaveAmendmentCancellationTokenSource, null);
                if (objOldSource != null)
                {
                    objOldSource.Cancel(false);
                    objOldSource.Dispose();
                }
                Interlocked.Exchange(ref _objFormClosingSemaphore, null)?.Dispose();
                if (components != null)
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
            this.cmdLoadBaseXml = new System.Windows.Forms.Button();
            this.cboXmlFiles = new Chummer.ElasticComboBox();
            this.splitContainerTop = new System.Windows.Forms.SplitContainer();
            this.gpbBaseXml = new System.Windows.Forms.GroupBox();
            this.txtBaseXml = new System.Windows.Forms.TextBox();
            this.splitContainerBottom = new System.Windows.Forms.SplitContainer();
            this.gpbAmendmentXml = new System.Windows.Forms.GroupBox();
            this.tlpAmendment = new System.Windows.Forms.TableLayoutPanel();
            this.txtAmendmentXml = new System.Windows.Forms.TextBox();
            this.cmdPreviewAmendment = new System.Windows.Forms.Button();
            this.cmdSaveAmendment = new System.Windows.Forms.Button();
            this.lblWikiLink = new System.Windows.Forms.LinkLabel();
            this.gpbResult = new System.Windows.Forms.GroupBox();
            this.splitResult = new System.Windows.Forms.SplitContainer();
            this.txtResultXml = new System.Windows.Forms.TextBox();
            this.txtDiffPreview = new System.Windows.Forms.TextBox();
            this.tlpMain.SuspendLayout();
            this.gpbFileSelection.SuspendLayout();
            this.tlpFileSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).BeginInit();
            this.splitContainerTop.Panel1.SuspendLayout();
            this.splitContainerTop.Panel2.SuspendLayout();
            this.splitContainerTop.SuspendLayout();
            this.gpbBaseXml.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBottom)).BeginInit();
            this.splitContainerBottom.Panel1.SuspendLayout();
            this.splitContainerBottom.Panel2.SuspendLayout();
            this.splitContainerBottom.SuspendLayout();
            this.gpbAmendmentXml.SuspendLayout();
            this.tlpAmendment.SuspendLayout();
            this.gpbResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitResult)).BeginInit();
            this.splitResult.Panel1.SuspendLayout();
            this.splitResult.Panel2.SuspendLayout();
            this.splitResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.gpbFileSelection, 0, 0);
            this.tlpMain.Controls.Add(this.splitContainerTop, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(784, 561);
            this.tlpMain.TabIndex = 0;
            // 
            // gpbFileSelection
            // 
            this.gpbFileSelection.AutoSize = true;
            this.gpbFileSelection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbFileSelection.Controls.Add(this.tlpFileSelection);
            this.gpbFileSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbFileSelection.Location = new System.Drawing.Point(3, 3);
            this.gpbFileSelection.Name = "gpbFileSelection";
            this.gpbFileSelection.Size = new System.Drawing.Size(778, 48);
            this.gpbFileSelection.TabIndex = 0;
            this.gpbFileSelection.TabStop = false;
            this.gpbFileSelection.Tag = "Label_SelectBaseXmlFile";
            this.gpbFileSelection.Text = "Select Base XML File";
            // 
            // tlpFileSelection
            // 
            this.tlpFileSelection.AutoSize = true;
            this.tlpFileSelection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpFileSelection.ColumnCount = 2;
            this.tlpFileSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFileSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFileSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpFileSelection.Controls.Add(this.cmdLoadBaseXml, 1, 0);
            this.tlpFileSelection.Controls.Add(this.cboXmlFiles, 0, 0);
            this.tlpFileSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFileSelection.Location = new System.Drawing.Point(3, 16);
            this.tlpFileSelection.Name = "tlpFileSelection";
            this.tlpFileSelection.RowCount = 1;
            this.tlpFileSelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFileSelection.Size = new System.Drawing.Size(772, 29);
            this.tlpFileSelection.TabIndex = 0;
            // 
            // cmdLoadBaseXml
            // 
            this.cmdLoadBaseXml.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdLoadBaseXml.AutoSize = true;
            this.cmdLoadBaseXml.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdLoadBaseXml.Location = new System.Drawing.Point(676, 3);
            this.cmdLoadBaseXml.Name = "cmdLoadBaseXml";
            this.cmdLoadBaseXml.Size = new System.Drawing.Size(93, 23);
            this.cmdLoadBaseXml.TabIndex = 2;
            this.cmdLoadBaseXml.Tag = "Button_LoadBaseXml";
            this.cmdLoadBaseXml.Text = "Load Base XML";
            this.cmdLoadBaseXml.UseVisualStyleBackColor = true;
            this.cmdLoadBaseXml.Click += new System.EventHandler(this.cmdLoadXml_Click);
            // 
            // cboXmlFiles
            // 
            this.cboXmlFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboXmlFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboXmlFiles.FormattingEnabled = true;
            this.cboXmlFiles.Location = new System.Drawing.Point(3, 4);
            this.cboXmlFiles.Name = "cboXmlFiles";
            this.cboXmlFiles.Size = new System.Drawing.Size(667, 21);
            this.cboXmlFiles.TabIndex = 1;
            // 
            // splitContainerTop
            // 
            this.splitContainerTop.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.splitContainerTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerTop.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.splitContainerTop.Location = new System.Drawing.Point(3, 57);
            this.splitContainerTop.Name = "splitContainerTop";
            this.splitContainerTop.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerTop.Panel1
            // 
            this.splitContainerTop.Panel1.Controls.Add(this.gpbBaseXml);
            // 
            // splitContainerTop.Panel2
            // 
            this.splitContainerTop.Panel2.Controls.Add(this.splitContainerBottom);
            this.splitContainerTop.Size = new System.Drawing.Size(778, 501);
            this.splitContainerTop.SplitterDistance = 150;
            this.splitContainerTop.TabIndex = 4;
            // 
            // gpbBaseXml
            // 
            this.gpbBaseXml.AutoSize = true;
            this.gpbBaseXml.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbBaseXml.BackColor = System.Drawing.SystemColors.Control;
            this.gpbBaseXml.Controls.Add(this.txtBaseXml);
            this.gpbBaseXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbBaseXml.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gpbBaseXml.Location = new System.Drawing.Point(0, 0);
            this.gpbBaseXml.Name = "gpbBaseXml";
            this.gpbBaseXml.Size = new System.Drawing.Size(778, 150);
            this.gpbBaseXml.TabIndex = 1;
            this.gpbBaseXml.TabStop = false;
            this.gpbBaseXml.Tag = "Label_BaseXml";
            this.gpbBaseXml.Text = "Base XML (Read-Only)";
            // 
            // txtBaseXml
            // 
            this.txtBaseXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBaseXml.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBaseXml.Location = new System.Drawing.Point(3, 16);
            this.txtBaseXml.MaxLength = 2147483647;
            this.txtBaseXml.Multiline = true;
            this.txtBaseXml.Name = "txtBaseXml";
            this.txtBaseXml.ReadOnly = true;
            this.txtBaseXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBaseXml.Size = new System.Drawing.Size(772, 131);
            this.txtBaseXml.TabIndex = 0;
            this.txtBaseXml.WordWrap = false;
            // 
            // splitContainerBottom
            // 
            this.splitContainerBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerBottom.Location = new System.Drawing.Point(0, 0);
            this.splitContainerBottom.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerBottom.Name = "splitContainerBottom";
            this.splitContainerBottom.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerBottom.Panel1
            // 
            this.splitContainerBottom.Panel1.Controls.Add(this.gpbAmendmentXml);
            // 
            // splitContainerBottom.Panel2
            // 
            this.splitContainerBottom.Panel2.Controls.Add(this.gpbResult);
            this.splitContainerBottom.Size = new System.Drawing.Size(778, 347);
            this.splitContainerBottom.SplitterDistance = 197;
            this.splitContainerBottom.TabIndex = 0;
            // 
            // gpbAmendmentXml
            // 
            this.gpbAmendmentXml.AutoSize = true;
            this.gpbAmendmentXml.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbAmendmentXml.BackColor = System.Drawing.SystemColors.Control;
            this.gpbAmendmentXml.Controls.Add(this.tlpAmendment);
            this.gpbAmendmentXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbAmendmentXml.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gpbAmendmentXml.Location = new System.Drawing.Point(0, 0);
            this.gpbAmendmentXml.Name = "gpbAmendmentXml";
            this.gpbAmendmentXml.Size = new System.Drawing.Size(778, 197);
            this.gpbAmendmentXml.TabIndex = 2;
            this.gpbAmendmentXml.TabStop = false;
            this.gpbAmendmentXml.Tag = "Label_AmendmentXml";
            this.gpbAmendmentXml.Text = "Amendment XML (Edit Here)";
            // 
            // tlpAmendment
            // 
            this.tlpAmendment.AutoSize = true;
            this.tlpAmendment.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAmendment.ColumnCount = 3;
            this.tlpAmendment.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAmendment.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAmendment.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAmendment.Controls.Add(this.txtAmendmentXml, 0, 0);
            this.tlpAmendment.Controls.Add(this.cmdPreviewAmendment, 0, 1);
            this.tlpAmendment.Controls.Add(this.cmdSaveAmendment, 2, 1);
            this.tlpAmendment.Controls.Add(this.lblWikiLink, 1, 1);
            this.tlpAmendment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAmendment.Location = new System.Drawing.Point(3, 16);
            this.tlpAmendment.Name = "tlpAmendment";
            this.tlpAmendment.RowCount = 2;
            this.tlpAmendment.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAmendment.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAmendment.Size = new System.Drawing.Size(772, 178);
            this.tlpAmendment.TabIndex = 0;
            // 
            // txtAmendmentXml
            // 
            this.txtAmendmentXml.AcceptsReturn = true;
            this.txtAmendmentXml.AcceptsTab = true;
            this.tlpAmendment.SetColumnSpan(this.txtAmendmentXml, 3);
            this.txtAmendmentXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAmendmentXml.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAmendmentXml.Location = new System.Drawing.Point(3, 3);
            this.txtAmendmentXml.MaxLength = 2147483647;
            this.txtAmendmentXml.Multiline = true;
            this.txtAmendmentXml.Name = "txtAmendmentXml";
            this.txtAmendmentXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtAmendmentXml.Size = new System.Drawing.Size(766, 143);
            this.txtAmendmentXml.TabIndex = 0;
            this.txtAmendmentXml.WordWrap = false;
            this.txtAmendmentXml.TextChanged += new System.EventHandler(this.txtAmendmentXml_TextChanged);
            // 
            // cmdPreviewAmendment
            // 
            this.cmdPreviewAmendment.AutoSize = true;
            this.cmdPreviewAmendment.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPreviewAmendment.Enabled = false;
            this.cmdPreviewAmendment.Location = new System.Drawing.Point(3, 152);
            this.cmdPreviewAmendment.Name = "cmdPreviewAmendment";
            this.cmdPreviewAmendment.Size = new System.Drawing.Size(150, 23);
            this.cmdPreviewAmendment.TabIndex = 0;
            this.cmdPreviewAmendment.Tag = "Button_PreviewAmendment";
            this.cmdPreviewAmendment.Text = "Preview Amendment Effects";
            this.cmdPreviewAmendment.UseVisualStyleBackColor = true;
            this.cmdPreviewAmendment.Click += new System.EventHandler(this.cmdPreviewAmendment_Click);
            // 
            // cmdSaveAmendment
            // 
            this.cmdSaveAmendment.AutoSize = true;
            this.cmdSaveAmendment.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdSaveAmendment.Enabled = false;
            this.cmdSaveAmendment.Location = new System.Drawing.Point(637, 152);
            this.cmdSaveAmendment.Name = "cmdSaveAmendment";
            this.cmdSaveAmendment.Size = new System.Drawing.Size(132, 23);
            this.cmdSaveAmendment.TabIndex = 1;
            this.cmdSaveAmendment.Tag = "Button_SaveAmendment";
            this.cmdSaveAmendment.Text = "Save Amendment to File";
            this.cmdSaveAmendment.UseVisualStyleBackColor = true;
            this.cmdSaveAmendment.Click += new System.EventHandler(this.cmdSaveAmendment_Click);
            // 
            // lblWikiLink
            // 
            this.lblWikiLink.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblWikiLink.AutoSize = true;
            this.lblWikiLink.Location = new System.Drawing.Point(159, 157);
            this.lblWikiLink.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblWikiLink.Name = "lblWikiLink";
            this.lblWikiLink.Size = new System.Drawing.Size(175, 13);
            this.lblWikiLink.TabIndex = 2;
            this.lblWikiLink.TabStop = true;
            this.lblWikiLink.Tag = "Label_AmendSystemDocumentation";
            this.lblWikiLink.Text = "Amendment System Documentation";
            this.lblWikiLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblWikiLink_LinkClicked);
            // 
            // gpbResult
            // 
            this.gpbResult.AutoSize = true;
            this.gpbResult.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gpbResult.BackColor = System.Drawing.SystemColors.Control;
            this.gpbResult.Controls.Add(this.splitResult);
            this.gpbResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpbResult.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gpbResult.Location = new System.Drawing.Point(0, 0);
            this.gpbResult.Name = "gpbResult";
            this.gpbResult.Size = new System.Drawing.Size(778, 146);
            this.gpbResult.TabIndex = 3;
            this.gpbResult.TabStop = false;
            this.gpbResult.Tag = "Label_ResultAndDiffPreview";
            this.gpbResult.Text = "Result and Diff Preview";
            // 
            // splitResult
            // 
            this.splitResult.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.splitResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitResult.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.splitResult.Location = new System.Drawing.Point(3, 16);
            this.splitResult.Margin = new System.Windows.Forms.Padding(0);
            this.splitResult.Name = "splitResult";
            // 
            // splitResult.Panel1
            // 
            this.splitResult.Panel1.Controls.Add(this.txtResultXml);
            // 
            // splitResult.Panel2
            // 
            this.splitResult.Panel2.Controls.Add(this.txtDiffPreview);
            this.splitResult.Size = new System.Drawing.Size(772, 127);
            this.splitResult.SplitterDistance = 385;
            this.splitResult.TabIndex = 1;
            // 
            // txtResultXml
            // 
            this.txtResultXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtResultXml.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtResultXml.Location = new System.Drawing.Point(0, 0);
            this.txtResultXml.MaxLength = 2147483647;
            this.txtResultXml.Multiline = true;
            this.txtResultXml.Name = "txtResultXml";
            this.txtResultXml.ReadOnly = true;
            this.txtResultXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResultXml.Size = new System.Drawing.Size(385, 127);
            this.txtResultXml.TabIndex = 0;
            this.txtResultXml.WordWrap = false;
            // 
            // txtDiffPreview
            // 
            this.txtDiffPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDiffPreview.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDiffPreview.Location = new System.Drawing.Point(0, 0);
            this.txtDiffPreview.MaxLength = 2147483647;
            this.txtDiffPreview.Multiline = true;
            this.txtDiffPreview.Name = "txtDiffPreview";
            this.txtDiffPreview.ReadOnly = true;
            this.txtDiffPreview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDiffPreview.Size = new System.Drawing.Size(383, 127);
            this.txtDiffPreview.TabIndex = 1;
            this.txtDiffPreview.WordWrap = false;
            // 
            // EditXmlData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditXmlData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_XmlAmendmentEditor";
            this.Text = "XML Amendment Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditXmlData_FormClosing);
            this.Load += new System.EventHandler(this.EditXmlData_Load);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.gpbFileSelection.ResumeLayout(false);
            this.gpbFileSelection.PerformLayout();
            this.tlpFileSelection.ResumeLayout(false);
            this.tlpFileSelection.PerformLayout();
            this.splitContainerTop.Panel1.ResumeLayout(false);
            this.splitContainerTop.Panel1.PerformLayout();
            this.splitContainerTop.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).EndInit();
            this.splitContainerTop.ResumeLayout(false);
            this.gpbBaseXml.ResumeLayout(false);
            this.gpbBaseXml.PerformLayout();
            this.splitContainerBottom.Panel1.ResumeLayout(false);
            this.splitContainerBottom.Panel1.PerformLayout();
            this.splitContainerBottom.Panel2.ResumeLayout(false);
            this.splitContainerBottom.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBottom)).EndInit();
            this.splitContainerBottom.ResumeLayout(false);
            this.gpbAmendmentXml.ResumeLayout(false);
            this.gpbAmendmentXml.PerformLayout();
            this.tlpAmendment.ResumeLayout(false);
            this.tlpAmendment.PerformLayout();
            this.gpbResult.ResumeLayout(false);
            this.splitResult.Panel1.ResumeLayout(false);
            this.splitResult.Panel1.PerformLayout();
            this.splitResult.Panel2.ResumeLayout(false);
            this.splitResult.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitResult)).EndInit();
            this.splitResult.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.GroupBox gpbFileSelection;
        private System.Windows.Forms.TableLayoutPanel tlpFileSelection;
        private Chummer.ElasticComboBox cboXmlFiles;
        private System.Windows.Forms.Button cmdLoadBaseXml;
        private System.Windows.Forms.GroupBox gpbBaseXml;
        private System.Windows.Forms.TextBox txtBaseXml;
        private System.Windows.Forms.GroupBox gpbAmendmentXml;
        private System.Windows.Forms.TableLayoutPanel tlpAmendment;
        private System.Windows.Forms.TextBox txtAmendmentXml;
        private System.Windows.Forms.Button cmdPreviewAmendment;
        private System.Windows.Forms.Button cmdSaveAmendment;
        private System.Windows.Forms.GroupBox gpbResult;
        private System.Windows.Forms.TextBox txtResultXml;
        private System.Windows.Forms.TextBox txtDiffPreview;
        private System.Windows.Forms.SplitContainer splitContainerTop;
        private System.Windows.Forms.SplitContainer splitContainerBottom;
        private System.Windows.Forms.SplitContainer splitResult;
        private System.Windows.Forms.LinkLabel lblWikiLink;
    }
}
