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
            gbxCreate = new GroupBox();
            cmdCreate = new Button();
            txtLanguageCode = new TextBox();
            lblLanguageCode = new Label();
            groupBox1 = new GroupBox();
            cmdEdit = new Button();
            cboLanguages = new ComboBox();
            txtLanguageName = new TextBox();
            label1 = new Label();
            gbxCreate.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            gbxCreate.Controls.Add(txtLanguageName);
            gbxCreate.Controls.Add(label1);
            gbxCreate.Controls.Add(cmdCreate);
            gbxCreate.Controls.Add(txtLanguageCode);
            gbxCreate.Controls.Add(lblLanguageCode);
            gbxCreate.Location = new Point(12, 12);
            gbxCreate.Name = "gbxCreate";
            gbxCreate.Size = new Size(201, 107);
            gbxCreate.TabIndex = 0;
            gbxCreate.TabStop = false;
            gbxCreate.Text = "Create a Language File";
            cmdCreate.Location = new Point(103, 71);
            cmdCreate.Name = "cmdCreate";
            cmdCreate.Size = new Size(75, 23);
            cmdCreate.TabIndex = 3;
            cmdCreate.Text = "Create";
            cmdCreate.UseVisualStyleBackColor = true;
            cmdCreate.Click += cmdCreate_Click;
            txtLanguageCode.Location = new Point(133, 19);
            txtLanguageCode.MaxLength = 2;
            txtLanguageCode.Name = "txtLanguageCode";
            txtLanguageCode.Size = new Size(45, 20);
            txtLanguageCode.TabIndex = 1;
            lblLanguageCode.AutoSize = true;
            lblLanguageCode.Location = new Point(10, 22);
            lblLanguageCode.Name = "lblLanguageCode";
            lblLanguageCode.Size = new Size(83, 13);
            lblLanguageCode.TabIndex = 143;
            lblLanguageCode.Tag = string.Empty;
            lblLanguageCode.Text = "Language Code";
            groupBox1.Controls.Add(cmdEdit);
            groupBox1.Controls.Add(cboLanguages);
            groupBox1.Location = new Point(233, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(201, 107);
            groupBox1.TabIndex = 146;
            groupBox1.TabStop = false;
            groupBox1.Text = "Edit Language File";
            cmdEdit.Location = new Point(63, 58);
            cmdEdit.Name = "cmdEdit";
            cmdEdit.Size = new Size(75, 23);
            cmdEdit.TabIndex = 5;
            cmdEdit.Text = "Edit";
            cmdEdit.UseVisualStyleBackColor = true;
            cmdEdit.Click += cmdEdit_Click;
            cboLanguages.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLanguages.FormattingEnabled = true;
            cboLanguages.Location = new Point(40, 19);
            cboLanguages.Name = "cboLanguages";
            cboLanguages.Size = new Size(122, 21);
            cboLanguages.TabIndex = 4;
            txtLanguageName.Location = new Point(102, 45);
            txtLanguageName.MaxLength = 100;
            txtLanguageName.Name = "txtLanguageName";
            txtLanguageName.Size = new Size(76, 20);
            txtLanguageName.TabIndex = 2;
            label1.AutoSize = true;
            label1.Location = new Point(10, 48);
            label1.Name = "label1";
            label1.Size = new Size(86, 13);
            label1.TabIndex = 146;
            label1.Tag = string.Empty;
            label1.Text = "Language Name";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(445, 127);
            Controls.Add(groupBox1);
            Controls.Add(gbxCreate);
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chummer Translator";
            Load += frmMain_Load;
            gbxCreate.ResumeLayout(false);
            gbxCreate.PerformLayout();
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

