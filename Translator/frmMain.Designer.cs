using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace Translator
{
    public partial class frmMain : Form
    {

        private IContainer components;

        private GroupBox gbxCreate;

        private Button cmdCreate;

        private TextBox txtLanguageCode;

        private Label lblLanguageCode;

        private GroupBox groupBox1;

        private ComboBox cboLanguages;

        private Button cmdEdit;

        private TextBox txtLanguageName;

        private Label label1;

        private void InitializeComponent()
        {
            this.gbxCreate = new System.Windows.Forms.GroupBox();
            this.txtLanguageName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdCreate = new System.Windows.Forms.Button();
            this.txtLanguageCode = new System.Windows.Forms.TextBox();
            this.lblLanguageCode = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmdEdit = new System.Windows.Forms.Button();
            this.cboLanguages = new System.Windows.Forms.ComboBox();
            this.txtRegionCode = new System.Windows.Forms.TextBox();
            this.lblDash = new System.Windows.Forms.Label();
            this.cmdRebuild = new System.Windows.Forms.Button();
            this.gbxCreate.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbxCreate
            // 
            this.gbxCreate.Controls.Add(this.lblDash);
            this.gbxCreate.Controls.Add(this.txtRegionCode);
            this.gbxCreate.Controls.Add(this.txtLanguageName);
            this.gbxCreate.Controls.Add(this.label1);
            this.gbxCreate.Controls.Add(this.cmdCreate);
            this.gbxCreate.Controls.Add(this.txtLanguageCode);
            this.gbxCreate.Controls.Add(this.lblLanguageCode);
            this.gbxCreate.Location = new System.Drawing.Point(12, 12);
            this.gbxCreate.Name = "gbxCreate";
            this.gbxCreate.Size = new System.Drawing.Size(201, 107);
            this.gbxCreate.TabIndex = 0;
            this.gbxCreate.TabStop = false;
            this.gbxCreate.Text = "Create a Language File";
            // 
            // txtLanguageName
            // 
            this.txtLanguageName.Location = new System.Drawing.Point(102, 45);
            this.txtLanguageName.MaxLength = 100;
            this.txtLanguageName.Name = "txtLanguageName";
            this.txtLanguageName.Size = new System.Drawing.Size(76, 20);
            this.txtLanguageName.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 146;
            this.label1.Tag = "";
            this.label1.Text = "Language Name";
            // 
            // cmdCreate
            // 
            this.cmdCreate.Enabled = false;
            this.cmdCreate.Location = new System.Drawing.Point(103, 71);
            this.cmdCreate.Name = "cmdCreate";
            this.cmdCreate.Size = new System.Drawing.Size(75, 23);
            this.cmdCreate.TabIndex = 4;
            this.cmdCreate.Text = "Create";
            this.cmdCreate.UseVisualStyleBackColor = true;
            this.cmdCreate.Click += new System.EventHandler(this.cmdCreate_Click);
            // 
            // txtLanguageCode
            // 
            this.txtLanguageCode.Location = new System.Drawing.Point(103, 19);
            this.txtLanguageCode.MaxLength = 2;
            this.txtLanguageCode.Name = "txtLanguageCode";
            this.txtLanguageCode.Size = new System.Drawing.Size(30, 20);
            this.txtLanguageCode.TabIndex = 1;
            this.txtLanguageCode.TextChanged += new System.EventHandler(this.txtLanguageCode_TextChanged);
            // 
            // lblLanguageCode
            // 
            this.lblLanguageCode.AutoSize = true;
            this.lblLanguageCode.Location = new System.Drawing.Point(10, 22);
            this.lblLanguageCode.Name = "lblLanguageCode";
            this.lblLanguageCode.Size = new System.Drawing.Size(83, 13);
            this.lblLanguageCode.TabIndex = 143;
            this.lblLanguageCode.Tag = "";
            this.lblLanguageCode.Text = "Language Code";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmdRebuild);
            this.groupBox1.Controls.Add(this.cmdEdit);
            this.groupBox1.Controls.Add(this.cboLanguages);
            this.groupBox1.Location = new System.Drawing.Point(233, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(201, 107);
            this.groupBox1.TabIndex = 146;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Edit Language File";
            // 
            // cmdEdit
            // 
            this.cmdEdit.Enabled = false;
            this.cmdEdit.Location = new System.Drawing.Point(15, 71);
            this.cmdEdit.Name = "cmdEdit";
            this.cmdEdit.Size = new System.Drawing.Size(75, 23);
            this.cmdEdit.TabIndex = 6;
            this.cmdEdit.Text = "Edit";
            this.cmdEdit.UseVisualStyleBackColor = true;
            this.cmdEdit.Click += new System.EventHandler(this.cmdEdit_Click);
            // 
            // cboLanguages
            // 
            this.cboLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguages.FormattingEnabled = true;
            this.cboLanguages.Location = new System.Drawing.Point(15, 19);
            this.cboLanguages.Name = "cboLanguages";
            this.cboLanguages.Size = new System.Drawing.Size(171, 21);
            this.cboLanguages.TabIndex = 5;
            this.cboLanguages.SelectedIndexChanged += new System.EventHandler(this.cboLanguages_SelectedIndexChanged);
            // 
            // txtRegionCode
            // 
            this.txtRegionCode.Location = new System.Drawing.Point(148, 19);
            this.txtRegionCode.MaxLength = 2;
            this.txtRegionCode.Name = "txtRegionCode";
            this.txtRegionCode.Size = new System.Drawing.Size(30, 20);
            this.txtRegionCode.TabIndex = 2;
            this.txtRegionCode.TextChanged += new System.EventHandler(this.txtRegionCode_TextChanged);
            // 
            // lblDash
            // 
            this.lblDash.AutoSize = true;
            this.lblDash.Location = new System.Drawing.Point(136, 22);
            this.lblDash.Name = "lblDash";
            this.lblDash.Size = new System.Drawing.Size(10, 13);
            this.lblDash.TabIndex = 148;
            this.lblDash.Tag = "";
            this.lblDash.Text = "-";
            // 
            // cmdRebuild
            // 
            this.cmdRebuild.Enabled = false;
            this.cmdRebuild.Location = new System.Drawing.Point(111, 71);
            this.cmdRebuild.Name = "cmdRebuild";
            this.cmdRebuild.Size = new System.Drawing.Size(75, 23);
            this.cmdRebuild.TabIndex = 7;
            this.cmdRebuild.Text = "Rebuild";
            this.cmdRebuild.UseVisualStyleBackColor = true;
            this.cmdRebuild.Click += new System.EventHandler(this.cmdRebuild_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(445, 127);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbxCreate);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chummer Translator";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.gbxCreate.ResumeLayout(false);
            this.gbxCreate.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

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
    }
}

