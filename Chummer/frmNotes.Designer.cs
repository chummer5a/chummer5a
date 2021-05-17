namespace Chummer
{
    partial class frmNotes
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
            this.components = new System.ComponentModel.Container();
            this.bufferedTableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.btnColorSelect = new System.Windows.Forms.Button();
            this.bufferedTableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.bufferedTableLayoutPanel1.SuspendLayout();
            this.bufferedTableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // bufferedTableLayoutPanel1
            // 
            this.bufferedTableLayoutPanel1.ColumnCount = 2;
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel1.Controls.Add(this.txtNotes, 0, 0);
            this.bufferedTableLayoutPanel1.Controls.Add(this.btnColorSelect, 0, 1);
            this.bufferedTableLayoutPanel1.Controls.Add(this.bufferedTableLayoutPanel2, 1, 1);
            this.bufferedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.bufferedTableLayoutPanel1.Name = "bufferedTableLayoutPanel1";
            this.bufferedTableLayoutPanel1.RowCount = 2;
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel1.Size = new System.Drawing.Size(624, 321);
            this.bufferedTableLayoutPanel1.TabIndex = 1;
            // 
            // txtNotes
            // 
            this.txtNotes.AcceptsReturn = true;
            this.txtNotes.AcceptsTab = true;
            this.txtNotes.AllowDrop = true;
            this.bufferedTableLayoutPanel1.SetColumnSpan(this.txtNotes, 2);
            this.txtNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNotes.Location = new System.Drawing.Point(3, 3);
            this.txtNotes.MaxLength = 2147483647;
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtNotes.Size = new System.Drawing.Size(618, 281);
            this.txtNotes.TabIndex = 1;
            this.txtNotes.TextChanged += new System.EventHandler(this.txtNotes_TextChanged);
            // 
            // btnColorSelect
            // 
            this.btnColorSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnColorSelect.AutoSize = true;
            this.btnColorSelect.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnColorSelect.Image = global::Chummer.Properties.Resources.color_wheel;
            this.btnColorSelect.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnColorSelect.Location = new System.Drawing.Point(3, 295);
            this.btnColorSelect.Name = "btnColorSelect";
            this.btnColorSelect.Size = new System.Drawing.Size(190, 23);
            this.btnColorSelect.TabIndex = 2;
            this.btnColorSelect.Tag = "Button_ChooseCustomColor";
            this.btnColorSelect.Text = "Choose custom color for this note";
            this.btnColorSelect.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnColorSelect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnColorSelect.UseVisualStyleBackColor = true;
            this.btnColorSelect.Click += new System.EventHandler(this.btnColorSelect_Click);
            // 
            // bufferedTableLayoutPanel2
            // 
            this.bufferedTableLayoutPanel2.ColumnCount = 2;
            this.bufferedTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanel2.Controls.Add(this.btnOK, 1, 0);
            this.bufferedTableLayoutPanel2.Controls.Add(this.btnCancel, 0, 0);
            this.bufferedTableLayoutPanel2.Location = new System.Drawing.Point(477, 290);
            this.bufferedTableLayoutPanel2.Name = "bufferedTableLayoutPanel2";
            this.bufferedTableLayoutPanel2.RowCount = 1;
            this.bufferedTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.bufferedTableLayoutPanel2.Size = new System.Drawing.Size(144, 28);
            this.bufferedTableLayoutPanel2.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.AutoSize = true;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(75, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(66, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Tag = "String_OK";
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(3, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(66, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Tag = "String_Cancel";
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // colorDialog1
            // 
            this.colorDialog1.AnyColor = true;
            // 
            // frmNotes
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(624, 321);
            this.Controls.Add(this.bufferedTableLayoutPanel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "frmNotes";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_Notes";
            this.Text = "Notes";
            this.Shown += new System.EventHandler(this.frmNotes_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNotes_KeyDown);
            this.Resize += new System.EventHandler(this.frmNotes_Resize);
            this.bufferedTableLayoutPanel1.ResumeLayout(false);
            this.bufferedTableLayoutPanel1.PerformLayout();
            this.bufferedTableLayoutPanel2.ResumeLayout(false);
            this.bufferedTableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private BufferedTableLayoutPanel bufferedTableLayoutPanel1;
        private System.Windows.Forms.TextBox txtNotes;
        private System.Windows.Forms.Button btnColorSelect;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private BufferedTableLayoutPanel bufferedTableLayoutPanel2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
