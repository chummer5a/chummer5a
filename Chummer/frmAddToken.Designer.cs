namespace Chummer
{
    partial class frmAddToken
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddToken));
            this.lblName = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblInit = new System.Windows.Forms.Label();
            this.nudInit = new System.Windows.Forms.NumericUpDown();
            this.lbld6 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.chkAutoRollInit = new System.Windows.Forms.CheckBox();
            this.nudInitStart = new System.Windows.Forms.NumericUpDown();
            this.lblStartingInit = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudInit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitStart)).BeginInit();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(12, 19);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(43, 13);
            this.lblName.TabIndex = 99;
            this.lblName.Tag = "Label_Name";
            this.lblName.Text = "{Name}";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackgroundImage = global::Chummer.Properties.Resources.folder_page;
            this.btnBrowse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnBrowse.Location = new System.Drawing.Point(248, 12);
            this.btnBrowse.MaximumSize = new System.Drawing.Size(24, 24);
            this.btnBrowse.MinimumSize = new System.Drawing.Size(24, 24);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(24, 24);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.OpenFile);
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(72, 16);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(170, 20);
            this.txtName.TabIndex = 0;
            // 
            // lblInit
            // 
            this.lblInit.AutoSize = true;
            this.lblInit.Location = new System.Drawing.Point(12, 44);
            this.lblInit.Name = "lblInit";
            this.lblInit.Size = new System.Drawing.Size(88, 13);
            this.lblInit.TabIndex = 4;
            this.lblInit.Tag = "Label_InitiativePasses";
            this.lblInit.Text = "{InitiativePasses}";
            this.lblInit.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // nudInit
            // 
            this.nudInit.Location = new System.Drawing.Point(110, 42);
            this.nudInit.Name = "nudInit";
            this.nudInit.Size = new System.Drawing.Size(43, 20);
            this.nudInit.TabIndex = 2;
            // 
            // lbld6
            // 
            this.lbld6.AutoSize = true;
            this.lbld6.Location = new System.Drawing.Point(159, 44);
            this.lbld6.Name = "lbld6";
            this.lbld6.Size = new System.Drawing.Size(19, 13);
            this.lbld6.TabIndex = 6;
            this.lbld6.Text = "d6";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(197, 68);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Tag = "String_Cancel";
            this.btnCancel.Text = "{Cancel}";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(116, 68);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Tag = "String_OK";
            this.btnOK.Text = "{OK}";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // chkAutoRollInit
            // 
            this.chkAutoRollInit.AutoSize = true;
            this.chkAutoRollInit.Location = new System.Drawing.Point(184, 42);
            this.chkAutoRollInit.Name = "chkAutoRollInit";
            this.chkAutoRollInit.Size = new System.Drawing.Size(88, 17);
            this.chkAutoRollInit.TabIndex = 3;
            this.chkAutoRollInit.Tag = "Label_AutoRollInit";
            this.chkAutoRollInit.Text = "{AutoRollInit}";
            this.chkAutoRollInit.UseVisualStyleBackColor = true;
            // 
            // nudInitStart
            // 
            this.nudInitStart.Location = new System.Drawing.Point(72, 71);
            this.nudInitStart.Name = "nudInitStart";
            this.nudInitStart.Size = new System.Drawing.Size(43, 20);
            this.nudInitStart.TabIndex = 4;
            // 
            // lblStartingInit
            // 
            this.lblStartingInit.AutoSize = true;
            this.lblStartingInit.Location = new System.Drawing.Point(12, 73);
            this.lblStartingInit.Name = "lblStartingInit";
            this.lblStartingInit.Size = new System.Drawing.Size(54, 13);
            this.lblStartingInit.TabIndex = 14;
            this.lblStartingInit.Tag = "String_AttributeINILong";
            this.lblStartingInit.Text = "{Initiative}";
            this.lblStartingInit.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // frmAddToken
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(284, 99);
            this.Controls.Add(this.nudInitStart);
            this.Controls.Add(this.lblStartingInit);
            this.Controls.Add(this.chkAutoRollInit);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lbld6);
            this.Controls.Add(this.nudInit);
            this.Controls.Add(this.lblInit);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblName);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAddToken";
            this.Tag = "AddToken_Header";
            this.Text = "{AddToken}";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.nudInit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitStart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblInit;
        private System.Windows.Forms.NumericUpDown nudInit;
        private System.Windows.Forms.Label lbld6;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox chkAutoRollInit;
        private System.Windows.Forms.NumericUpDown nudInitStart;
        private System.Windows.Forms.Label lblStartingInit;

    }
}