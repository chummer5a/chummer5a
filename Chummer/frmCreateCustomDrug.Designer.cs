namespace Chummer
{
    partial class frmCreateCustomDrug
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Foundations");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Blocks");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Enhancers");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Foundations");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Blocks");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Enhancers");
            this.treAvailableComponents = new System.Windows.Forms.TreeView();
            this.btnAddComponent = new System.Windows.Forms.Button();
            this.btnRemoveComponent = new System.Windows.Forms.Button();
            this.treChosenComponents = new System.Windows.Forms.TreeView();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.lblDrugDescription = new System.Windows.Forms.Label();
            this.lblBlockDescription = new System.Windows.Forms.Label();
            this.lblDrugNameLabel = new System.Windows.Forms.Label();
            this.txtDrugName = new System.Windows.Forms.TextBox();
            this.lblGrade = new System.Windows.Forms.Label();
            this.cboGrade = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // treAvailableComponents
            // 
            this.treAvailableComponents.HideSelection = false;
            this.treAvailableComponents.Location = new System.Drawing.Point(12, 35);
            this.treAvailableComponents.Name = "treAvailableComponents";
            treeNode1.Name = "Foundations";
            treeNode1.Text = "Foundations";
            treeNode2.Name = "Blocks";
            treeNode2.Text = "Blocks";
            treeNode3.Name = "Enhancers";
            treeNode3.Text = "Enhancers";
            this.treAvailableComponents.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3});
            this.treAvailableComponents.Size = new System.Drawing.Size(265, 243);
            this.treAvailableComponents.TabIndex = 0;
            this.treAvailableComponents.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treAvailableComponents_AfterSelect);
            this.treAvailableComponents.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treAvailableComponents_NodeMouseDoubleClick);
            // 
            // btnAddComponent
            // 
            this.btnAddComponent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddComponent.Location = new System.Drawing.Point(283, 122);
            this.btnAddComponent.Name = "btnAddComponent";
            this.btnAddComponent.Size = new System.Drawing.Size(44, 23);
            this.btnAddComponent.TabIndex = 2;
            this.btnAddComponent.Text = ">>";
            this.btnAddComponent.UseVisualStyleBackColor = true;
            this.btnAddComponent.Click += new System.EventHandler(this.btnAddComponent_Click);
            // 
            // btnRemoveComponent
            // 
            this.btnRemoveComponent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveComponent.Location = new System.Drawing.Point(283, 151);
            this.btnRemoveComponent.Name = "btnRemoveComponent";
            this.btnRemoveComponent.Size = new System.Drawing.Size(44, 23);
            this.btnRemoveComponent.TabIndex = 2;
            this.btnRemoveComponent.Text = "<<";
            this.btnRemoveComponent.UseVisualStyleBackColor = true;
            this.btnRemoveComponent.Click += new System.EventHandler(this.btnRemoveComponent_Click);
            // 
            // treChosenComponents
            // 
            this.treChosenComponents.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.treChosenComponents.HideSelection = false;
            this.treChosenComponents.Location = new System.Drawing.Point(333, 35);
            this.treChosenComponents.Name = "treChosenComponents";
            treeNode4.Name = "Foundations";
            treeNode4.Text = "Foundations";
            treeNode5.Name = "Blocks";
            treeNode5.Text = "Blocks";
            treeNode6.Name = "Enhancers";
            treeNode6.Text = "Enhancers";
            this.treChosenComponents.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode4,
            treeNode5,
            treeNode6});
            this.treChosenComponents.Size = new System.Drawing.Size(279, 243);
            this.treChosenComponents.TabIndex = 0;
            this.treChosenComponents.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treChoosenComponents_AfterSelect);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(456, 406);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(537, 406);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Add";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // lblDrugDescription
            // 
            this.lblDrugDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDrugDescription.Location = new System.Drawing.Point(333, 281);
            this.lblDrugDescription.Name = "lblDrugDescription";
            this.lblDrugDescription.Size = new System.Drawing.Size(279, 122);
            this.lblDrugDescription.TabIndex = 4;
            // 
            // lblBlockDescription
            // 
            this.lblBlockDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblBlockDescription.Location = new System.Drawing.Point(12, 281);
            this.lblBlockDescription.Name = "lblBlockDescription";
            this.lblBlockDescription.Size = new System.Drawing.Size(265, 122);
            this.lblBlockDescription.TabIndex = 5;
            // 
            // lblDrugNameLabel
            // 
            this.lblDrugNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDrugNameLabel.AutoSize = true;
            this.lblDrugNameLabel.Location = new System.Drawing.Point(289, 12);
            this.lblDrugNameLabel.Name = "lblDrugNameLabel";
            this.lblDrugNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblDrugNameLabel.TabIndex = 6;
            this.lblDrugNameLabel.Text = "Name:";
            // 
            // txtDrugName
            // 
            this.txtDrugName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDrugName.Location = new System.Drawing.Point(333, 9);
            this.txtDrugName.Name = "txtDrugName";
            this.txtDrugName.Size = new System.Drawing.Size(279, 20);
            this.txtDrugName.TabIndex = 7;
            this.txtDrugName.Text = "New Drug";
            this.txtDrugName.TextChanged += new System.EventHandler(this.txtDrugName_TextChanged);
            // 
            // lblGrade
            // 
            this.lblGrade.AutoSize = true;
            this.lblGrade.Location = new System.Drawing.Point(12, 12);
            this.lblGrade.Name = "lblGrade";
            this.lblGrade.Size = new System.Drawing.Size(39, 13);
            this.lblGrade.TabIndex = 26;
            this.lblGrade.Tag = "Label_Grade";
            this.lblGrade.Text = "Grade:";
            // 
            // cboGrade
            // 
            this.cboGrade.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGrade.FormattingEnabled = true;
            this.cboGrade.Location = new System.Drawing.Point(57, 9);
            this.cboGrade.Name = "cboGrade";
            this.cboGrade.Size = new System.Drawing.Size(220, 21);
            this.cboGrade.TabIndex = 27;
            this.cboGrade.SelectedIndexChanged += new System.EventHandler(this.cboGrade_SelectedIndexChanged);
            // 
            // frmCreateCustomDrug
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.lblGrade);
            this.Controls.Add(this.cboGrade);
            this.Controls.Add(this.txtDrugName);
            this.Controls.Add(this.lblDrugNameLabel);
            this.Controls.Add(this.lblBlockDescription);
            this.Controls.Add(this.lblDrugDescription);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRemoveComponent);
            this.Controls.Add(this.btnAddComponent);
            this.Controls.Add(this.treChosenComponents);
            this.Controls.Add(this.treAvailableComponents);
            this.Name = "frmCreateCustomDrug";
            this.Text = "Create Custom Drug";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treAvailableComponents;
        private System.Windows.Forms.Button btnAddComponent;
        private System.Windows.Forms.Button btnRemoveComponent;
        private System.Windows.Forms.TreeView treChosenComponents;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label lblDrugDescription;
        private System.Windows.Forms.Label lblBlockDescription;
        private System.Windows.Forms.Label lblDrugNameLabel;
        private System.Windows.Forms.TextBox txtDrugName;
		private System.Windows.Forms.Label lblGrade;
		private System.Windows.Forms.ComboBox cboGrade;
	}
}