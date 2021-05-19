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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Foundations");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Blocks");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Enhancers");
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Foundations");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Blocks");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Enhancers");
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
            this.cboGrade = new Chummer.ElasticComboBox();
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpMiddle = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpMain.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.tlpMiddle.SuspendLayout();
            this.SuspendLayout();
            // 
            // treAvailableComponents
            // 
            this.tlpMain.SetColumnSpan(this.treAvailableComponents, 2);
            this.treAvailableComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treAvailableComponents.HideSelection = false;
            this.treAvailableComponents.Location = new System.Drawing.Point(3, 30);
            this.treAvailableComponents.Name = "treAvailableComponents";
            treeNode4.Name = "Node_Foundation";
            treeNode4.Tag = "Node_Foundation";
            treeNode4.Text = "Foundations";
            treeNode5.Name = "Node_Block";
            treeNode5.Tag = "Node_Block";
            treeNode5.Text = "Blocks";
            treeNode6.Name = "Node_Enhancer";
            treeNode6.Tag = "Node_Enhancer";
            treeNode6.Text = "Enhancers";
            this.treAvailableComponents.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode4,
            treeNode5,
            treeNode6});
            this.treAvailableComponents.Size = new System.Drawing.Size(344, 243);
            this.treAvailableComponents.TabIndex = 0;
            this.treAvailableComponents.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treAvailableComponents_AfterSelect);
            this.treAvailableComponents.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treAvailableComponents_NodeMouseDoubleClick);
            // 
            // btnAddComponent
            // 
            this.btnAddComponent.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnAddComponent.AutoSize = true;
            this.btnAddComponent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddComponent.Location = new System.Drawing.Point(3, 127);
            this.btnAddComponent.Name = "btnAddComponent";
            this.btnAddComponent.Size = new System.Drawing.Size(29, 23);
            this.btnAddComponent.TabIndex = 2;
            this.btnAddComponent.Text = ">>";
            this.btnAddComponent.UseVisualStyleBackColor = true;
            this.btnAddComponent.Click += new System.EventHandler(this.btnAddComponent_Click);
            // 
            // btnRemoveComponent
            // 
            this.btnRemoveComponent.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnRemoveComponent.AutoSize = true;
            this.btnRemoveComponent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRemoveComponent.Location = new System.Drawing.Point(3, 98);
            this.btnRemoveComponent.Name = "btnRemoveComponent";
            this.btnRemoveComponent.Size = new System.Drawing.Size(29, 23);
            this.btnRemoveComponent.TabIndex = 2;
            this.btnRemoveComponent.Text = "<<";
            this.btnRemoveComponent.UseVisualStyleBackColor = true;
            this.btnRemoveComponent.Click += new System.EventHandler(this.btnRemoveComponent_Click);
            // 
            // treChosenComponents
            // 
            this.tlpMain.SetColumnSpan(this.treChosenComponents, 2);
            this.treChosenComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treChosenComponents.HideSelection = false;
            this.treChosenComponents.Location = new System.Drawing.Point(388, 30);
            this.treChosenComponents.Name = "treChosenComponents";
            treeNode1.Name = "Node_Foundation";
            treeNode1.Tag = "Node_Foundation";
            treeNode1.Text = "Foundations";
            treeNode2.Name = "Node_Block";
            treeNode2.Tag = "Node_Block";
            treeNode2.Text = "Blocks";
            treeNode3.Name = "Node_Enhancer";
            treeNode3.Tag = "Node_Enhancer";
            treeNode3.Text = "Enhancers";
            this.treChosenComponents.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3});
            this.treChosenComponents.Size = new System.Drawing.Size(343, 243);
            this.treChosenComponents.TabIndex = 0;
            this.treChosenComponents.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treChoosenComponents_AfterSelect);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(3, 3);
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
            this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnAdd.AutoSize = true;
            this.btnAdd.Location = new System.Drawing.Point(84, 3);
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
            this.tlpMain.SetColumnSpan(this.lblDrugDescription, 2);
            this.lblDrugDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDrugDescription.Location = new System.Drawing.Point(388, 276);
            this.lblDrugDescription.Name = "lblDrugDescription";
            this.lblDrugDescription.Size = new System.Drawing.Size(343, 214);
            this.lblDrugDescription.TabIndex = 4;
            // 
            // lblBlockDescription
            // 
            this.tlpMain.SetColumnSpan(this.lblBlockDescription, 2);
            this.lblBlockDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBlockDescription.Location = new System.Drawing.Point(3, 276);
            this.lblBlockDescription.Name = "lblBlockDescription";
            this.lblBlockDescription.Size = new System.Drawing.Size(344, 214);
            this.lblBlockDescription.TabIndex = 5;
            // 
            // lblDrugNameLabel
            // 
            this.lblDrugNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblDrugNameLabel.AutoSize = true;
            this.lblDrugNameLabel.Location = new System.Drawing.Point(388, 7);
            this.lblDrugNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblDrugNameLabel.Name = "lblDrugNameLabel";
            this.lblDrugNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblDrugNameLabel.TabIndex = 6;
            this.lblDrugNameLabel.Tag = "Label_Name";
            this.lblDrugNameLabel.Text = "Name:";
            this.lblDrugNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDrugName
            // 
            this.txtDrugName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDrugName.Location = new System.Drawing.Point(432, 3);
            this.txtDrugName.Name = "txtDrugName";
            this.txtDrugName.Size = new System.Drawing.Size(299, 20);
            this.txtDrugName.TabIndex = 7;
            this.txtDrugName.TextChanged += new System.EventHandler(this.txtDrugName_TextChanged);
            // 
            // lblGrade
            // 
            this.lblGrade.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblGrade.AutoSize = true;
            this.lblGrade.Location = new System.Drawing.Point(3, 7);
            this.lblGrade.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblGrade.Name = "lblGrade";
            this.lblGrade.Size = new System.Drawing.Size(39, 13);
            this.lblGrade.TabIndex = 26;
            this.lblGrade.Tag = "Label_Grade";
            this.lblGrade.Text = "Grade:";
            this.lblGrade.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboGrade
            // 
            this.cboGrade.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboGrade.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGrade.FormattingEnabled = true;
            this.cboGrade.Location = new System.Drawing.Point(48, 3);
            this.cboGrade.Name = "cboGrade";
            this.cboGrade.Size = new System.Drawing.Size(299, 21);
            this.cboGrade.TabIndex = 27;
            this.cboGrade.TooltipText = "";
            this.cboGrade.SelectedIndexChanged += new System.EventHandler(this.cboGrade_SelectedIndexChanged);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 5;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.Controls.Add(this.lblDrugDescription, 3, 2);
            this.tlpMain.Controls.Add(this.cboGrade, 1, 0);
            this.tlpMain.Controls.Add(this.lblGrade, 0, 0);
            this.tlpMain.Controls.Add(this.lblBlockDescription, 0, 2);
            this.tlpMain.Controls.Add(this.treAvailableComponents, 0, 1);
            this.tlpMain.Controls.Add(this.txtDrugName, 4, 0);
            this.tlpMain.Controls.Add(this.lblDrugNameLabel, 3, 0);
            this.tlpMain.Controls.Add(this.treChosenComponents, 3, 1);
            this.tlpMain.Controls.Add(this.flpButtons, 0, 3);
            this.tlpMain.Controls.Add(this.tlpMiddle, 2, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(734, 519);
            this.tlpMain.TabIndex = 28;
            // 
            // flpButtons
            // 
            this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpButtons.AutoSize = true;
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.SetColumnSpan(this.flpButtons, 5);
            this.flpButtons.Controls.Add(this.btnAdd);
            this.flpButtons.Controls.Add(this.btnCancel);
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtons.Location = new System.Drawing.Point(572, 490);
            this.flpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(162, 29);
            this.flpButtons.TabIndex = 29;
            // 
            // tlpMiddle
            // 
            this.tlpMiddle.AutoSize = true;
            this.tlpMiddle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMiddle.ColumnCount = 1;
            this.tlpMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMiddle.Controls.Add(this.btnAddComponent, 0, 1);
            this.tlpMiddle.Controls.Add(this.btnRemoveComponent, 0, 0);
            this.tlpMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMiddle.Location = new System.Drawing.Point(350, 27);
            this.tlpMiddle.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMiddle.Name = "tlpMiddle";
            this.tlpMiddle.RowCount = 2;
            this.tlpMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMiddle.Size = new System.Drawing.Size(35, 249);
            this.tlpMiddle.TabIndex = 30;
            // 
            // frmCreateCustomDrug
            // 
            this.AcceptButton = this.btnAdd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(752, 537);
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(768, 576);
            this.Name = "frmCreateCustomDrug";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.Tag = "Button_CreateCustomDrug";
            this.Text = "Create Custom Drug";
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.flpButtons.PerformLayout();
            this.tlpMiddle.ResumeLayout(false);
            this.tlpMiddle.PerformLayout();
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
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private Chummer.BufferedTableLayoutPanel tlpMiddle;
    }
}
