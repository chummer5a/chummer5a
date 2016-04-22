namespace Chummer.UI.Shared
{
	partial class BindingListDisplay<TType>
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tblContents = new System.Windows.Forms.TableLayoutPanel();
			this.SuspendLayout();
			// 
			// tblContents
			// 
			this.tblContents.ColumnCount = 1;
			this.tblContents.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tblContents.Location = new System.Drawing.Point(0, 0);
			this.tblContents.Margin = new System.Windows.Forms.Padding(0);
			this.tblContents.Name = "tblContents";
			this.tblContents.RowCount = 1;
			this.tblContents.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContents.Size = new System.Drawing.Size(606, 433);
			this.tblContents.TabIndex = 0;
			// 
			// BindingListDisplay
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.Controls.Add(this.tblContents);
			this.Name = "BindingListDisplay";
			this.Size = new System.Drawing.Size(606, 433);
			this.Load += new System.EventHandler(this.SkillsDisplay_Load);
			this.Resize += new System.EventHandler(this.SkillsDisplay_Resize);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tblContents;
	}
}
