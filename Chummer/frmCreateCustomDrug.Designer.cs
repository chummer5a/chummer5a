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
            System.Windows.Forms.TreeNode treeNode19 = new System.Windows.Forms.TreeNode("Foundations");
            System.Windows.Forms.TreeNode treeNode20 = new System.Windows.Forms.TreeNode("Blocks");
            System.Windows.Forms.TreeNode treeNode21 = new System.Windows.Forms.TreeNode("Enhancers");
            System.Windows.Forms.TreeNode treeNode22 = new System.Windows.Forms.TreeNode("Foundations");
            System.Windows.Forms.TreeNode treeNode23 = new System.Windows.Forms.TreeNode("Blocks");
            System.Windows.Forms.TreeNode treeNode24 = new System.Windows.Forms.TreeNode("Enhancers");
            this.treAvailableComponents = new System.Windows.Forms.TreeView();
            this.btnAddComponent = new System.Windows.Forms.Button();
            this.btnRemoveComponent = new System.Windows.Forms.Button();
            this.treChosenComponents = new System.Windows.Forms.TreeView();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lblDrugDescription = new System.Windows.Forms.Label();
            this.lblBlockDescription = new System.Windows.Forms.Label();
            this.lblDrugNameLabel = new System.Windows.Forms.Label();
            this.txtDrugName = new System.Windows.Forms.TextBox();
            this.lblGrade = new System.Windows.Forms.Label();
            this.cboGrade = new ElasticComboBox();
            this.tableLayoutPanel1 = new Chummer.BufferedTableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treAvailableComponents
            // 
            this.treAvailableComponents.HideSelection = false;
            this.treAvailableComponents.Location = new System.Drawing.Point(12, 35);
            this.treAvailableComponents.Name = "treAvailableComponents";
            treeNode19.Name = "Node_Foundation";
            treeNode19.Tag = "Node_Foundation";
            treeNode19.Text = "Foundations";
            treeNode20.Name = "Node_Block";
            treeNode20.Tag = "Node_Block";
            treeNode20.Text = "Blocks";
            treeNode21.Name = "Node_Enhancer";
            treeNode21.Tag = "Node_Enhancer";
            treeNode21.Text = "Enhancers";
            this.treAvailableComponents.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode19,
            treeNode20,
            treeNode21});
            this.treAvailableComponents.Size = new System.Drawing.Size(265, 243);
            this.treAvailableComponents.TabIndex = 0;
            this.treAvailableComponents.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treAvailableComponents_AfterSelect);
            this.treAvailableComponents.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treAvailableComponents_NodeMouseDoubleClick);
            // 
            // btnAddComponent
            // 
            this.btnAddComponent.Anchor = System.Windows.Forms.AnchorStyles.Top;
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
            this.btnRemoveComponent.Anchor = System.Windows.Forms.AnchorStyles.Top;
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
            treeNode22.Name = "Node_Foundation";
            treeNode22.Tag = "Node_Foundation";
            treeNode22.Text = "Foundations";
            treeNode23.Name = "Node_Block";
            treeNode23.Tag = "Node_Block";
            treeNode23.Text = "Blocks";
            treeNode24.Name = "Node_Enhancer";
            treeNode24.Tag = "Node_Enhancer";
            treeNode24.Text = "Enhancers";
            this.treChosenComponents.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode22,
            treeNode23,
            treeNode24});
            this.treChosenComponents.Size = new System.Drawing.Size(279, 243);
            this.treChosenComponents.TabIndex = 0;
            this.treChosenComponents.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treChoosenComponents_AfterSelect);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(456, 406);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Tag = "String_Cancel";
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(537, 406);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Tag = "Button_Add";
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // lblDrugDescription
            // 
            this.lblDrugDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDrugDescription.Location = new System.Drawing.Point(333, 281);
            this.lblDrugDescription.Name = "lblDrugDescription";
            this.lblDrugDescription.Size = new System.Drawing.Size(279, 122);
            this.lblDrugDescription.TabIndex = 4;
            // 
            // lblBlockDescription
            // 
            this.lblBlockDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblBlockDescription.Location = new System.Drawing.Point(12, 281);
            this.lblBlockDescription.Name = "lblBlockDescription";
            this.lblBlockDescription.Size = new System.Drawing.Size(265, 122);
            this.lblBlockDescription.TabIndex = 5;
            // 
            // lblDrugNameLabel
            // 
            this.lblDrugNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDrugNameLabel.AutoSize = true;
            this.lblDrugNameLabel.Location = new System.Drawing.Point(289, 12);
            this.lblDrugNameLabel.Name = "lblDrugNameLabel";
            this.lblDrugNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblDrugNameLabel.TabIndex = 6;
            this.lblDrugNameLabel.Tag = "Label_Name";
            this.lblDrugNameLabel.Text = "Name:";
            // 
            // txtDrugName
            // 
            this.txtDrugName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDrugName.Location = new System.Drawing.Point(333, 9);
            this.txtDrugName.Name = "txtDrugName";
            this.txtDrugName.Size = new System.Drawing.Size(279, 20);
            this.txtDrugName.TabIndex = 7;
            this.txtDrugName.TextChanged += new System.EventHandler(this.txtDrugName_TextChanged);
            // 
            // lblGrade
            // 
            this.lblGrade.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGrade.AutoSize = true;
            this.lblGrade.Location = new System.Drawing.Point(3, 6);
            this.lblGrade.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGrade.Name = "lblGrade";
            this.lblGrade.Size = new System.Drawing.Size(39, 15);
            this.lblGrade.TabIndex = 26;
            this.lblGrade.Tag = "Label_Grade";
            this.lblGrade.Text = "Grade:";
            // 
            // cboGrade
            // 
            this.cboGrade.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboGrade.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGrade.FormattingEnabled = true;
            this.cboGrade.Location = new System.Drawing.Point(48, 3);
            this.cboGrade.Name = "cboGrade";
            this.cboGrade.Size = new System.Drawing.Size(214, 21);
            this.cboGrade.TabIndex = 27;
            this.cboGrade.SelectedIndexChanged += new System.EventHandler(this.cboGrade_SelectedIndexChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.cboGrade, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblGrade, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 9);
            this.tableLayoutPanel1.MaximumSize = new System.Drawing.Size(265, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(265, 27);
            this.tableLayoutPanel1.TabIndex = 28;
            // 
            // frmCreateCustomDrug
            // 
            this.AcceptButton = this.btnAdd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.txtDrugName);
            this.Controls.Add(this.lblDrugNameLabel);
            this.Controls.Add(this.lblBlockDescription);
            this.Controls.Add(this.lblDrugDescription);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRemoveComponent);
            this.Controls.Add(this.btnAddComponent);
            this.Controls.Add(this.treChosenComponents);
            this.Controls.Add(this.treAvailableComponents);
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "frmCreateCustomDrug";
            this.Tag = "Button_CreateCustomDrug";
            this.Text = "Create Custom Drug";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treAvailableComponents;
        private System.Windows.Forms.Button btnAddComponent;
        private System.Windows.Forms.Button btnRemoveComponent;
        private System.Windows.Forms.TreeView treChosenComponents;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Label lblDrugDescription;
        private System.Windows.Forms.Label lblBlockDescription;
        private System.Windows.Forms.Label lblDrugNameLabel;
        private System.Windows.Forms.TextBox txtDrugName;
		private System.Windows.Forms.Label lblGrade;
		private ElasticComboBox cboGrade;
        private Chummer.BufferedTableLayoutPanel tableLayoutPanel1;
    }
}
