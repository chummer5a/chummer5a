using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace Translator
{
    public partial class frmTranslatorMain : Form
    {

        private IContainer components;

        private GroupBox gbxCreate;

        private Button cmdCreate;

        private TextBox txtLanguageCode;

        private Label lblLanguageCode;

        private GroupBox gbxEdit;

        private ComboBox cboLanguages;

        private Button cmdEdit;

        private TextBox txtLanguageName;

        private Label lblName;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTranslatorMain));
            this.gbxCreate = new System.Windows.Forms.GroupBox();
            this.tlpCreate = new System.Windows.Forms.TableLayoutPanel();
            this.chkRightToLeft = new System.Windows.Forms.CheckBox();
            this.lblLanguageCode = new System.Windows.Forms.Label();
            this.cmdCreate = new System.Windows.Forms.Button();
            this.lblName = new System.Windows.Forms.Label();
            this.txtLanguageName = new System.Windows.Forms.TextBox();
            this.flpLanguageCode = new System.Windows.Forms.FlowLayoutPanel();
            this.txtLanguageCode = new System.Windows.Forms.TextBox();
            this.lblDash = new System.Windows.Forms.Label();
            this.txtRegionCode = new System.Windows.Forms.TextBox();
            this.gbxEdit = new System.Windows.Forms.GroupBox();
            this.tlpEdit = new System.Windows.Forms.TableLayoutPanel();
            this.cmdRebuild = new System.Windows.Forms.Button();
            this.cmdUpdate = new System.Windows.Forms.Button();
            this.cboLanguages = new System.Windows.Forms.ComboBox();
            this.cmdEdit = new System.Windows.Forms.Button();
            this.pbProcessProgress = new System.Windows.Forms.ProgressBar();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.tlpGroupBoxes = new System.Windows.Forms.TableLayoutPanel();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.gbxCreate.SuspendLayout();
            this.tlpCreate.SuspendLayout();
            this.flpLanguageCode.SuspendLayout();
            this.gbxEdit.SuspendLayout();
            this.tlpEdit.SuspendLayout();
            this.tlpGroupBoxes.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbxCreate
            // 
            this.gbxCreate.AutoSize = true;
            this.gbxCreate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbxCreate.Controls.Add(this.tlpCreate);
            this.gbxCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxCreate.Location = new System.Drawing.Point(3, 3);
            this.gbxCreate.Name = "gbxCreate";
            this.gbxCreate.Size = new System.Drawing.Size(217, 102);
            this.gbxCreate.TabIndex = 0;
            this.gbxCreate.TabStop = false;
            this.gbxCreate.Text = "Create a Language File";
            // 
            // tlpCreate
            // 
            this.tlpCreate.AutoSize = true;
            this.tlpCreate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpCreate.ColumnCount = 3;
            this.tlpCreate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCreate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCreate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCreate.Controls.Add(this.chkRightToLeft, 0, 2);
            this.tlpCreate.Controls.Add(this.lblLanguageCode, 0, 0);
            this.tlpCreate.Controls.Add(this.cmdCreate, 2, 2);
            this.tlpCreate.Controls.Add(this.lblName, 0, 1);
            this.tlpCreate.Controls.Add(this.txtLanguageName, 1, 1);
            this.tlpCreate.Controls.Add(this.flpLanguageCode, 2, 0);
            this.tlpCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCreate.Location = new System.Drawing.Point(3, 16);
            this.tlpCreate.Name = "tlpCreate";
            this.tlpCreate.RowCount = 3;
            this.tlpCreate.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCreate.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCreate.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpCreate.Size = new System.Drawing.Size(211, 83);
            this.tlpCreate.TabIndex = 149;
            // 
            // chkRightToLeft
            // 
            this.chkRightToLeft.AutoSize = true;
            this.tlpCreate.SetColumnSpan(this.chkRightToLeft, 2);
            this.chkRightToLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkRightToLeft.Location = new System.Drawing.Point(3, 57);
            this.chkRightToLeft.Name = "chkRightToLeft";
            this.chkRightToLeft.Size = new System.Drawing.Size(84, 23);
            this.chkRightToLeft.TabIndex = 9;
            this.chkRightToLeft.Text = "Right-to-Left";
            this.chkRightToLeft.UseVisualStyleBackColor = true;
            this.chkRightToLeft.CheckedChanged += new System.EventHandler(this.chkRightToLeft_CheckedChanged);
            // 
            // lblLanguageCode
            // 
            this.lblLanguageCode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLanguageCode.AutoSize = true;
            this.tlpCreate.SetColumnSpan(this.lblLanguageCode, 2);
            this.lblLanguageCode.Location = new System.Drawing.Point(3, 6);
            this.lblLanguageCode.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblLanguageCode.Name = "lblLanguageCode";
            this.lblLanguageCode.Size = new System.Drawing.Size(83, 13);
            this.lblLanguageCode.TabIndex = 143;
            this.lblLanguageCode.Tag = "";
            this.lblLanguageCode.Text = "Language Code";
            // 
            // cmdCreate
            // 
            this.cmdCreate.AutoSize = true;
            this.cmdCreate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCreate.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmdCreate.Enabled = false;
            this.cmdCreate.Location = new System.Drawing.Point(160, 57);
            this.cmdCreate.Name = "cmdCreate";
            this.cmdCreate.Size = new System.Drawing.Size(48, 23);
            this.cmdCreate.TabIndex = 4;
            this.cmdCreate.Text = "Create";
            this.cmdCreate.UseVisualStyleBackColor = true;
            this.cmdCreate.Click += new System.EventHandler(this.cmdCreate_Click);
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 33);
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(35, 13);
            this.lblName.TabIndex = 146;
            this.lblName.Tag = "";
            this.lblName.Text = "Name";
            // 
            // txtLanguageName
            // 
            this.txtLanguageName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpCreate.SetColumnSpan(this.txtLanguageName, 2);
            this.txtLanguageName.Location = new System.Drawing.Point(44, 29);
            this.txtLanguageName.MaxLength = 100;
            this.txtLanguageName.Name = "txtLanguageName";
            this.txtLanguageName.Size = new System.Drawing.Size(164, 20);
            this.txtLanguageName.TabIndex = 3;
            // 
            // flpLanguageCode
            // 
            this.flpLanguageCode.AutoSize = true;
            this.flpLanguageCode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpLanguageCode.Controls.Add(this.txtRegionCode);
            this.flpLanguageCode.Controls.Add(this.lblDash);
            this.flpLanguageCode.Controls.Add(this.txtLanguageCode);
            this.flpLanguageCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpLanguageCode.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpLanguageCode.Location = new System.Drawing.Point(90, 0);
            this.flpLanguageCode.Margin = new System.Windows.Forms.Padding(0);
            this.flpLanguageCode.Name = "flpLanguageCode";
            this.flpLanguageCode.Size = new System.Drawing.Size(121, 25);
            this.flpLanguageCode.TabIndex = 147;
            // 
            // txtLanguageCode
            // 
            this.txtLanguageCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLanguageCode.Location = new System.Drawing.Point(32, 3);
            this.txtLanguageCode.Name = "txtLanguageCode";
            this.txtLanguageCode.Size = new System.Drawing.Size(32, 20);
            this.txtLanguageCode.TabIndex = 1;
            this.txtLanguageCode.TextChanged += new System.EventHandler(this.txtLanguageCode_TextChanged);
            this.txtLanguageCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtLanguageCode_KeyDown);
            // 
            // lblDash
            // 
            this.lblDash.AutoSize = true;
            this.lblDash.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDash.Location = new System.Drawing.Point(70, 6);
            this.lblDash.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDash.Name = "lblDash";
            this.lblDash.Size = new System.Drawing.Size(10, 13);
            this.lblDash.TabIndex = 148;
            this.lblDash.Tag = "";
            this.lblDash.Text = "-";
            // 
            // txtRegionCode
            // 
            this.txtRegionCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRegionCode.Location = new System.Drawing.Point(86, 3);
            this.txtRegionCode.MaxLength = 2;
            this.txtRegionCode.Name = "txtRegionCode";
            this.txtRegionCode.Size = new System.Drawing.Size(32, 20);
            this.txtRegionCode.TabIndex = 2;
            this.txtRegionCode.TextChanged += new System.EventHandler(this.txtRegionCode_TextChanged);
            this.txtRegionCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtRegionCode_KeyDown);
            // 
            // gbxEdit
            // 
            this.gbxEdit.AutoSize = true;
            this.gbxEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbxEdit.Controls.Add(this.tlpEdit);
            this.gbxEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxEdit.Location = new System.Drawing.Point(226, 3);
            this.gbxEdit.Name = "gbxEdit";
            this.gbxEdit.Size = new System.Drawing.Size(217, 102);
            this.gbxEdit.TabIndex = 146;
            this.gbxEdit.TabStop = false;
            this.gbxEdit.Text = "Edit Language File";
            // 
            // tlpEdit
            // 
            this.tlpEdit.AutoSize = true;
            this.tlpEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpEdit.ColumnCount = 3;
            this.tlpEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpEdit.Controls.Add(this.cmdRebuild, 2, 1);
            this.tlpEdit.Controls.Add(this.cmdUpdate, 1, 1);
            this.tlpEdit.Controls.Add(this.cboLanguages, 0, 0);
            this.tlpEdit.Controls.Add(this.cmdEdit, 0, 1);
            this.tlpEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpEdit.Location = new System.Drawing.Point(3, 16);
            this.tlpEdit.Name = "tlpEdit";
            this.tlpEdit.RowCount = 2;
            this.tlpEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpEdit.Size = new System.Drawing.Size(211, 83);
            this.tlpEdit.TabIndex = 9;
            // 
            // cmdRebuild
            // 
            this.cmdRebuild.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdRebuild.Enabled = false;
            this.cmdRebuild.Location = new System.Drawing.Point(143, 57);
            this.cmdRebuild.Name = "cmdRebuild";
            this.cmdRebuild.Size = new System.Drawing.Size(65, 23);
            this.cmdRebuild.TabIndex = 7;
            this.cmdRebuild.Text = "Rebuild";
            this.cmdRebuild.UseVisualStyleBackColor = true;
            this.cmdRebuild.Click += new System.EventHandler(this.cmdRebuild_Click);
            // 
            // cmdUpdate
            // 
            this.cmdUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdUpdate.Enabled = false;
            this.cmdUpdate.Location = new System.Drawing.Point(73, 57);
            this.cmdUpdate.Name = "cmdUpdate";
            this.cmdUpdate.Size = new System.Drawing.Size(64, 23);
            this.cmdUpdate.TabIndex = 8;
            this.cmdUpdate.Text = "Update";
            this.cmdUpdate.UseVisualStyleBackColor = true;
            this.cmdUpdate.Click += new System.EventHandler(this.cmdUpdate_Click);
            // 
            // cboLanguages
            // 
            this.tlpEdit.SetColumnSpan(this.cboLanguages, 3);
            this.cboLanguages.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguages.FormattingEnabled = true;
            this.cboLanguages.Location = new System.Drawing.Point(3, 3);
            this.cboLanguages.Name = "cboLanguages";
            this.cboLanguages.Size = new System.Drawing.Size(205, 21);
            this.cboLanguages.TabIndex = 5;
            this.cboLanguages.SelectedIndexChanged += new System.EventHandler(this.cboLanguages_SelectedIndexChanged);
            // 
            // cmdEdit
            // 
            this.cmdEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdEdit.Enabled = false;
            this.cmdEdit.Location = new System.Drawing.Point(3, 57);
            this.cmdEdit.Name = "cmdEdit";
            this.cmdEdit.Size = new System.Drawing.Size(64, 23);
            this.cmdEdit.TabIndex = 6;
            this.cmdEdit.Text = "Edit";
            this.cmdEdit.UseVisualStyleBackColor = true;
            this.cmdEdit.Click += new System.EventHandler(this.cmdEdit_Click);
            // 
            // pbProcessProgress
            // 
            this.pbProcessProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pbProcessProgress.Location = new System.Drawing.Point(3, 117);
            this.pbProcessProgress.Name = "pbProcessProgress";
            this.pbProcessProgress.Size = new System.Drawing.Size(384, 23);
            this.pbProcessProgress.Step = 1;
            this.pbProcessProgress.TabIndex = 147;
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cmdCancel.Enabled = false;
            this.cmdCancel.Location = new System.Drawing.Point(393, 117);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(50, 23);
            this.cmdCancel.TabIndex = 8;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // tlpGroupBoxes
            // 
            this.tlpGroupBoxes.ColumnCount = 2;
            this.tlpMain.SetColumnSpan(this.tlpGroupBoxes, 2);
            this.tlpGroupBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGroupBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGroupBoxes.Controls.Add(this.gbxCreate, 0, 0);
            this.tlpGroupBoxes.Controls.Add(this.gbxEdit, 1, 0);
            this.tlpGroupBoxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGroupBoxes.Location = new System.Drawing.Point(0, 0);
            this.tlpGroupBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.tlpGroupBoxes.Name = "tlpGroupBoxes";
            this.tlpGroupBoxes.RowCount = 1;
            this.tlpGroupBoxes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGroupBoxes.Size = new System.Drawing.Size(446, 108);
            this.tlpGroupBoxes.TabIndex = 148;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.tlpGroupBoxes, 0, 0);
            this.tlpMain.Controls.Add(this.cmdCancel, 1, 1);
            this.tlpMain.Controls.Add(this.pbProcessProgress, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(446, 143);
            this.tlpMain.TabIndex = 149;
            // 
            // frmTranslatorMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(464, 161);
            this.Controls.Add(this.tlpMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmTranslatorMain";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chummer Translator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmTranslatorMain_FormClosing);
            this.Load += new System.EventHandler(this.frmTranslatorMain_Load);
            this.gbxCreate.ResumeLayout(false);
            this.gbxCreate.PerformLayout();
            this.tlpCreate.ResumeLayout(false);
            this.tlpCreate.PerformLayout();
            this.flpLanguageCode.ResumeLayout(false);
            this.flpLanguageCode.PerformLayout();
            this.gbxEdit.ResumeLayout(false);
            this.gbxEdit.PerformLayout();
            this.tlpEdit.ResumeLayout(false);
            this.tlpGroupBoxes.ResumeLayout(false);
            this.tlpGroupBoxes.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private Label lblDash;
        private TextBox txtRegionCode;
        private Button cmdRebuild;
        private ProgressBar pbProcessProgress;
        private Button cmdCancel;
        private Button cmdUpdate;
        private CheckBox chkRightToLeft;
        private TableLayoutPanel tlpGroupBoxes;
        private TableLayoutPanel tlpMain;
        private TableLayoutPanel tlpEdit;
        private TableLayoutPanel tlpCreate;
        private FlowLayoutPanel flpLanguageCode;
    }
}

