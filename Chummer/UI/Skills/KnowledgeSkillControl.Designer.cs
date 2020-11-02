namespace Chummer.UI.Skills
{
    public sealed partial class KnowledgeSkillControl
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
                components?.Dispose();
                _objGraphics?.Dispose();
                UnbindKnowledgeSkillControl();
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
            this.components = new System.ComponentModel.Container();
            this.lblModifiedRating = new Chummer.LabelWithToolTip();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cboName = new Chummer.ElasticComboBox();
            this.cboType = new Chummer.ElasticComboBox();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpLeft = new Chummer.BufferedTableLayoutPanel(this.components);
            this.lblName = new System.Windows.Forms.Label();
            this.tlpMiddle = new System.Windows.Forms.TableLayoutPanel();
            this.tlpRight = new System.Windows.Forms.TableLayoutPanel();
            this.tlpMain.SuspendLayout();
            this.tlpLeft.SuspendLayout();
            this.tlpMiddle.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblModifiedRating
            // 
            this.lblModifiedRating.AutoSize = true;
            this.lblModifiedRating.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblModifiedRating.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModifiedRating.Location = new System.Drawing.Point(3, 0);
            this.lblModifiedRating.MinimumSize = new System.Drawing.Size(50, 0);
            this.lblModifiedRating.Name = "lblModifiedRating";
            this.lblModifiedRating.Size = new System.Drawing.Size(50, 24);
            this.lblModifiedRating.TabIndex = 16;
            this.lblModifiedRating.Text = "00 (00)";
            this.lblModifiedRating.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblModifiedRating.ToolTipText = "";
            // 
            // cmdDelete
            // 
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdDelete.Location = new System.Drawing.Point(165, 0);
            this.cmdDelete.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(48, 24);
            this.cmdDelete.TabIndex = 19;
            this.cmdDelete.Tag = "String_Delete";
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // cboName
            // 
            this.cboName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboName.DropDownWidth = 200;
            this.cboName.FormattingEnabled = true;
            this.cboName.Location = new System.Drawing.Point(50, 2);
            this.cboName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
            this.cboName.Name = "cboName";
            this.cboName.Size = new System.Drawing.Size(163, 21);
            this.cboName.Sorted = true;
            this.cboName.TabIndex = 20;
            this.cboName.TabStop = false;
            this.cboName.TooltipText = "";
            this.cboName.TextChanged += new System.EventHandler(this.cboName_TextChanged);
            // 
            // cboType
            // 
            this.cboType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(3, 1);
            this.cboType.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(156, 21);
            this.cboType.Sorted = true;
            this.cboType.TabIndex = 21;
            this.cboType.TooltipText = "";
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 5;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpMain.Controls.Add(this.tlpLeft, 0, 0);
            this.tlpMain.Controls.Add(this.tlpMiddle, 3, 0);
            this.tlpMain.Controls.Add(this.tlpRight, 4, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(863, 24);
            this.tlpMain.TabIndex = 27;
            // 
            // tlpLeft
            // 
            this.tlpLeft.AutoSize = true;
            this.tlpLeft.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLeft.ColumnCount = 2;
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLeft.Controls.Add(this.lblName, 0, 0);
            this.tlpLeft.Controls.Add(this.cboName, 1, 0);
            this.tlpLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLeft.Location = new System.Drawing.Point(0, 0);
            this.tlpLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLeft.Name = "tlpLeft";
            this.tlpLeft.RowCount = 1;
            this.tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpLeft.Size = new System.Drawing.Size(216, 24);
            this.tlpLeft.TabIndex = 28;
            // 
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 5);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(41, 13);
            this.lblName.TabIndex = 21;
            this.lblName.Text = "[Name]";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpMiddle
            // 
            this.tlpMiddle.AutoSize = true;
            this.tlpMiddle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMiddle.ColumnCount = 4;
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMiddle.Controls.Add(this.lblModifiedRating, 0, 0);
            this.tlpMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMiddle.Location = new System.Drawing.Point(359, 0);
            this.tlpMiddle.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMiddle.Name = "tlpMiddle";
            this.tlpMiddle.RowCount = 1;
            this.tlpMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMiddle.Size = new System.Drawing.Size(288, 24);
            this.tlpMiddle.TabIndex = 29;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 2;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.Controls.Add(this.cmdDelete, 1, 0);
            this.tlpRight.Controls.Add(this.cboType, 0, 0);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(647, 0);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 1;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Size = new System.Drawing.Size(216, 24);
            this.tlpRight.TabIndex = 30;
            // 
            // KnowledgeSkillControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "KnowledgeSkillControl";
            this.Size = new System.Drawing.Size(863, 24);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.DpiChangedAfterParent += new System.EventHandler(this.KnowledgeSkillControl_DpiChangedAfterParent);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpLeft.ResumeLayout(false);
            this.tlpLeft.PerformLayout();
            this.tlpMiddle.ResumeLayout(false);
            this.tlpMiddle.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private LabelWithToolTip lblModifiedRating;
        private System.Windows.Forms.Button cmdDelete;
        private ElasticComboBox cboName;
        private ElasticComboBox cboType;
        private BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblName;
        private BufferedTableLayoutPanel tlpLeft;
        private System.Windows.Forms.TableLayoutPanel tlpMiddle;
        private System.Windows.Forms.TableLayoutPanel tlpRight;
    }
}
