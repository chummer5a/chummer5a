namespace Chummer
{
    partial class frmSelectAttribute
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
            this.cmdOK = new System.Windows.Forms.Button();
            this.cboAttribute = new System.Windows.Forms.ComboBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.chkDoNotAffectMetatypeMaximum = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(256, 43);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 3;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cboAttribute
            // 
            this.cboAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAttribute.FormattingEnabled = true;
            this.cboAttribute.Location = new System.Drawing.Point(12, 45);
            this.cboAttribute.Name = "cboAttribute";
            this.cboAttribute.Size = new System.Drawing.Size(234, 21);
            this.cboAttribute.TabIndex = 1;
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(12, 9);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(319, 33);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Text = "Description goes here.";
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(256, 72);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 4;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // chkDoNotAffectMetatypeMaximum
            // 
            this.chkDoNotAffectMetatypeMaximum.AutoSize = true;
            this.chkDoNotAffectMetatypeMaximum.Location = new System.Drawing.Point(15, 76);
            this.chkDoNotAffectMetatypeMaximum.Name = "chkDoNotAffectMetatypeMaximum";
            this.chkDoNotAffectMetatypeMaximum.Size = new System.Drawing.Size(182, 17);
            this.chkDoNotAffectMetatypeMaximum.TabIndex = 2;
            this.chkDoNotAffectMetatypeMaximum.Tag = "Checkbox_DoNotAffectMaximum";
            this.chkDoNotAffectMetatypeMaximum.Text = "Do not affect Metatype Maximum";
            this.chkDoNotAffectMetatypeMaximum.UseVisualStyleBackColor = true;
            this.chkDoNotAffectMetatypeMaximum.Visible = false;
            // 
            // frmSelectAttribute
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(343, 101);
            this.ControlBox = false;
            this.Controls.Add(this.chkDoNotAffectMetatypeMaximum);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.cboAttribute);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectAttribute";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectAttribute";
            this.Text = "Choose an Attribute";
            this.Load += new System.EventHandler(this.frmSelectAttribute_Load);
            this.Shown += new System.EventHandler(this.frmSelectAttribute_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.ComboBox cboAttribute;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.CheckBox chkDoNotAffectMetatypeMaximum;
    }
}