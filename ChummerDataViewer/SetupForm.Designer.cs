namespace ChummerDataViewer
{
	partial class SetupForm
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
			this.txtId = new System.Windows.Forms.TextBox();
			this.txtKey = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.lblID = new System.Windows.Forms.Label();
			this.lblKey = new System.Windows.Forms.Label();
			this.lblDb = new System.Windows.Forms.Label();
			this.btnCanel = new System.Windows.Forms.Button();
			this.lblDesc = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.lblDbDesc = new System.Windows.Forms.Label();
			this.lblBulk = new System.Windows.Forms.Label();
			this.txtBulk = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// txtId
			// 
			this.txtId.Location = new System.Drawing.Point(113, 111);
			this.txtId.Name = "txtId";
			this.txtId.Size = new System.Drawing.Size(268, 20);
			this.txtId.TabIndex = 0;
			this.txtId.TextChanged += new System.EventHandler(this.AnyTextChanged);
			// 
			// txtKey
			// 
			this.txtKey.Location = new System.Drawing.Point(113, 151);
			this.txtKey.Name = "txtKey";
			this.txtKey.PasswordChar = '*';
			this.txtKey.Size = new System.Drawing.Size(268, 20);
			this.txtKey.TabIndex = 1;
			this.txtKey.TextChanged += new System.EventHandler(this.AnyTextChanged);
			// 
			// btnOK
			// 
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(115, 204);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "Ok";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// lblID
			// 
			this.lblID.AutoSize = true;
			this.lblID.Location = new System.Drawing.Point(13, 111);
			this.lblID.Name = "lblID";
			this.lblID.Size = new System.Drawing.Size(70, 13);
			this.lblID.TabIndex = 3;
			this.lblID.Text = "AWS Key ID:";
			// 
			// lblKey
			// 
			this.lblKey.AutoSize = true;
			this.lblKey.Location = new System.Drawing.Point(13, 151);
			this.lblKey.Name = "lblKey";
			this.lblKey.Size = new System.Drawing.Size(94, 13);
			this.lblKey.TabIndex = 4;
			this.lblKey.Text = "AWS Access Key:";
			// 
			// lblDb
			// 
			this.lblDb.AutoSize = true;
			this.lblDb.Location = new System.Drawing.Point(13, 48);
			this.lblDb.Name = "lblDb";
			this.lblDb.Size = new System.Drawing.Size(62, 13);
			this.lblDb.TabIndex = 5;
			this.lblDb.Text = "[dblocation]";
			// 
			// btnCanel
			// 
			this.btnCanel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCanel.Location = new System.Drawing.Point(196, 204);
			this.btnCanel.Name = "btnCanel";
			this.btnCanel.Size = new System.Drawing.Size(75, 23);
			this.btnCanel.TabIndex = 6;
			this.btnCanel.Text = "Cancel";
			this.btnCanel.UseVisualStyleBackColor = true;
			this.btnCanel.Click += new System.EventHandler(this.btnCanel_Click);
			// 
			// lblDesc
			// 
			this.lblDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDesc.Location = new System.Drawing.Point(12, 9);
			this.lblDesc.Name = "lblDesc";
			this.lblDesc.Size = new System.Drawing.Size(372, 23);
			this.lblDesc.TabIndex = 7;
			this.lblDesc.Text = "Enter AWS Key Id and Access Key to continue.";
			this.lblDesc.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.Location = new System.Drawing.Point(14, 177);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(41, 13);
			this.lblStatus.TabIndex = 8;
			this.lblStatus.Text = "[status]";
			this.lblStatus.Visible = false;
			// 
			// lblDbDesc
			// 
			this.lblDbDesc.AutoSize = true;
			this.lblDbDesc.Location = new System.Drawing.Point(13, 32);
			this.lblDbDesc.Name = "lblDbDesc";
			this.lblDbDesc.Size = new System.Drawing.Size(96, 13);
			this.lblDbDesc.TabIndex = 9;
			this.lblDbDesc.Text = "Database location:";
			// 
			// lblBulk
			// 
			this.lblBulk.AutoSize = true;
			this.lblBulk.Location = new System.Drawing.Point(14, 74);
			this.lblBulk.Name = "lblBulk";
			this.lblBulk.Size = new System.Drawing.Size(71, 13);
			this.lblBulk.TabIndex = 10;
			this.lblBulk.Text = "Bulk Storage:";
			// 
			// txtBulk
			// 
			this.txtBulk.Location = new System.Drawing.Point(113, 74);
			this.txtBulk.Name = "txtBulk";
			this.txtBulk.Size = new System.Drawing.Size(268, 20);
			this.txtBulk.TabIndex = 11;
			// 
			// SetupForm
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCanel;
			this.ClientSize = new System.Drawing.Size(396, 234);
			this.Controls.Add(this.txtBulk);
			this.Controls.Add(this.lblBulk);
			this.Controls.Add(this.lblDbDesc);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.lblDesc);
			this.Controls.Add(this.btnCanel);
			this.Controls.Add(this.lblDb);
			this.Controls.Add(this.lblKey);
			this.Controls.Add(this.lblID);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.txtKey);
			this.Controls.Add(this.txtId);
			this.MaximumSize = new System.Drawing.Size(412, 273);
			this.MinimumSize = new System.Drawing.Size(412, 273);
			this.Name = "SetupForm";
			this.Text = "SetupForm";
			this.Load += new System.EventHandler(this.SetupForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtId;
		private System.Windows.Forms.TextBox txtKey;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Label lblID;
		private System.Windows.Forms.Label lblKey;
		private System.Windows.Forms.Label lblDb;
		private System.Windows.Forms.Button btnCanel;
		private System.Windows.Forms.Label lblDesc;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Label lblDbDesc;
		private System.Windows.Forms.Label lblBulk;
		private System.Windows.Forms.TextBox txtBulk;
	}
}