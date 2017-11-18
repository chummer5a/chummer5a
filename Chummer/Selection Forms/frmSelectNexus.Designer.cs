namespace Chummer
{
    partial class frmSelectNexus
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
            this.lblProcessorLabel = new System.Windows.Forms.Label();
            this.nudProcessor = new System.Windows.Forms.NumericUpDown();
            this.nudSystem = new System.Windows.Forms.NumericUpDown();
            this.lblSystemLabel = new System.Windows.Forms.Label();
            this.lblPersonaLimitLabel = new System.Windows.Forms.Label();
            this.nudResponse = new System.Windows.Forms.NumericUpDown();
            this.lblResponseLabel = new System.Windows.Forms.Label();
            this.nudFirewall = new System.Windows.Forms.NumericUpDown();
            this.lblFirewallLabel = new System.Windows.Forms.Label();
            this.lblSystemAvail = new System.Windows.Forms.Label();
            this.lblSystemAvailLabel = new System.Windows.Forms.Label();
            this.lblResponseAvail = new System.Windows.Forms.Label();
            this.lblResponseAvailLabel = new System.Windows.Forms.Label();
            this.lblFirewallAvail = new System.Windows.Forms.Label();
            this.lblFirewallAvailLabel = new System.Windows.Forms.Label();
            this.lblCost = new System.Windows.Forms.Label();
            this.lblCostLabel = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.chkFreeItem = new System.Windows.Forms.CheckBox();
            this.nudPersona = new System.Windows.Forms.NumericUpDown();
            this.nudSignal = new System.Windows.Forms.NumericUpDown();
            this.lblSignalLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudProcessor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSystem)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudResponse)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFirewall)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPersona)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSignal)).BeginInit();
            this.SuspendLayout();
            // 
            // lblProcessorLabel
            // 
            this.lblProcessorLabel.AutoSize = true;
            this.lblProcessorLabel.Location = new System.Drawing.Point(12, 9);
            this.lblProcessorLabel.Name = "lblProcessorLabel";
            this.lblProcessorLabel.Size = new System.Drawing.Size(57, 13);
            this.lblProcessorLabel.TabIndex = 0;
            this.lblProcessorLabel.Tag = "Label_SelectNexus_Processor";
            this.lblProcessorLabel.Text = "Processor:";
            // 
            // nudProcessor
            // 
            this.nudProcessor.Location = new System.Drawing.Point(75, 7);
            this.nudProcessor.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudProcessor.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudProcessor.Name = "nudProcessor";
            this.nudProcessor.Size = new System.Drawing.Size(40, 20);
            this.nudProcessor.TabIndex = 1;
            this.nudProcessor.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudProcessor.ValueChanged += new System.EventHandler(this.nudProcessor_ValueChanged);
            // 
            // nudSystem
            // 
            this.nudSystem.Location = new System.Drawing.Point(75, 33);
            this.nudSystem.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudSystem.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSystem.Name = "nudSystem";
            this.nudSystem.Size = new System.Drawing.Size(40, 20);
            this.nudSystem.TabIndex = 3;
            this.nudSystem.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSystem.ValueChanged += new System.EventHandler(this.nudSystem_ValueChanged);
            // 
            // lblSystemLabel
            // 
            this.lblSystemLabel.AutoSize = true;
            this.lblSystemLabel.Location = new System.Drawing.Point(12, 35);
            this.lblSystemLabel.Name = "lblSystemLabel";
            this.lblSystemLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSystemLabel.TabIndex = 2;
            this.lblSystemLabel.Tag = "Label_System";
            this.lblSystemLabel.Text = "System:";
            // 
            // lblPersonaLimitLabel
            // 
            this.lblPersonaLimitLabel.AutoSize = true;
            this.lblPersonaLimitLabel.Location = new System.Drawing.Point(219, 35);
            this.lblPersonaLimitLabel.Name = "lblPersonaLimitLabel";
            this.lblPersonaLimitLabel.Size = new System.Drawing.Size(73, 13);
            this.lblPersonaLimitLabel.TabIndex = 16;
            this.lblPersonaLimitLabel.Tag = "Label_SelectNexus_PersonaLimit";
            this.lblPersonaLimitLabel.Text = "Persona Limit:";
            // 
            // nudResponse
            // 
            this.nudResponse.Location = new System.Drawing.Point(75, 59);
            this.nudResponse.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudResponse.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudResponse.Name = "nudResponse";
            this.nudResponse.Size = new System.Drawing.Size(40, 20);
            this.nudResponse.TabIndex = 5;
            this.nudResponse.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudResponse.ValueChanged += new System.EventHandler(this.nudResponse_ValueChanged);
            // 
            // lblResponseLabel
            // 
            this.lblResponseLabel.AutoSize = true;
            this.lblResponseLabel.Location = new System.Drawing.Point(12, 61);
            this.lblResponseLabel.Name = "lblResponseLabel";
            this.lblResponseLabel.Size = new System.Drawing.Size(58, 13);
            this.lblResponseLabel.TabIndex = 4;
            this.lblResponseLabel.Tag = "Label_Response";
            this.lblResponseLabel.Text = "Response:";
            // 
            // nudFirewall
            // 
            this.nudFirewall.Location = new System.Drawing.Point(75, 85);
            this.nudFirewall.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudFirewall.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFirewall.Name = "nudFirewall";
            this.nudFirewall.Size = new System.Drawing.Size(40, 20);
            this.nudFirewall.TabIndex = 7;
            this.nudFirewall.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFirewall.ValueChanged += new System.EventHandler(this.nudFirewall_ValueChanged);
            // 
            // lblFirewallLabel
            // 
            this.lblFirewallLabel.AutoSize = true;
            this.lblFirewallLabel.Location = new System.Drawing.Point(12, 87);
            this.lblFirewallLabel.Name = "lblFirewallLabel";
            this.lblFirewallLabel.Size = new System.Drawing.Size(45, 13);
            this.lblFirewallLabel.TabIndex = 6;
            this.lblFirewallLabel.Tag = "Label_Firewall";
            this.lblFirewallLabel.Text = "Firewall:";
            // 
            // lblSystemAvail
            // 
            this.lblSystemAvail.AutoSize = true;
            this.lblSystemAvail.Location = new System.Drawing.Point(171, 35);
            this.lblSystemAvail.Name = "lblSystemAvail";
            this.lblSystemAvail.Size = new System.Drawing.Size(36, 13);
            this.lblSystemAvail.TabIndex = 9;
            this.lblSystemAvail.Text = "[Avail]";
            // 
            // lblSystemAvailLabel
            // 
            this.lblSystemAvailLabel.AutoSize = true;
            this.lblSystemAvailLabel.Location = new System.Drawing.Point(132, 35);
            this.lblSystemAvailLabel.Name = "lblSystemAvailLabel";
            this.lblSystemAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblSystemAvailLabel.TabIndex = 8;
            this.lblSystemAvailLabel.Tag = "Label_Avail";
            this.lblSystemAvailLabel.Text = "Avail:";
            // 
            // lblResponseAvail
            // 
            this.lblResponseAvail.AutoSize = true;
            this.lblResponseAvail.Location = new System.Drawing.Point(171, 61);
            this.lblResponseAvail.Name = "lblResponseAvail";
            this.lblResponseAvail.Size = new System.Drawing.Size(36, 13);
            this.lblResponseAvail.TabIndex = 11;
            this.lblResponseAvail.Text = "[Avail]";
            // 
            // lblResponseAvailLabel
            // 
            this.lblResponseAvailLabel.AutoSize = true;
            this.lblResponseAvailLabel.Location = new System.Drawing.Point(132, 61);
            this.lblResponseAvailLabel.Name = "lblResponseAvailLabel";
            this.lblResponseAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblResponseAvailLabel.TabIndex = 10;
            this.lblResponseAvailLabel.Tag = "Label_Avail";
            this.lblResponseAvailLabel.Text = "Avail:";
            // 
            // lblFirewallAvail
            // 
            this.lblFirewallAvail.AutoSize = true;
            this.lblFirewallAvail.Location = new System.Drawing.Point(171, 87);
            this.lblFirewallAvail.Name = "lblFirewallAvail";
            this.lblFirewallAvail.Size = new System.Drawing.Size(36, 13);
            this.lblFirewallAvail.TabIndex = 13;
            this.lblFirewallAvail.Text = "[Avail]";
            // 
            // lblFirewallAvailLabel
            // 
            this.lblFirewallAvailLabel.AutoSize = true;
            this.lblFirewallAvailLabel.Location = new System.Drawing.Point(132, 87);
            this.lblFirewallAvailLabel.Name = "lblFirewallAvailLabel";
            this.lblFirewallAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblFirewallAvailLabel.TabIndex = 12;
            this.lblFirewallAvailLabel.Tag = "Label_Avail";
            this.lblFirewallAvailLabel.Text = "Avail:";
            // 
            // lblCost
            // 
            this.lblCost.AutoSize = true;
            this.lblCost.Location = new System.Drawing.Point(298, 61);
            this.lblCost.Name = "lblCost";
            this.lblCost.Size = new System.Drawing.Size(44, 13);
            this.lblCost.TabIndex = 19;
            this.lblCost.Text = "[Nuyen]";
            // 
            // lblCostLabel
            // 
            this.lblCostLabel.AutoSize = true;
            this.lblCostLabel.Location = new System.Drawing.Point(219, 61);
            this.lblCostLabel.Name = "lblCostLabel";
            this.lblCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblCostLabel.TabIndex = 18;
            this.lblCostLabel.Tag = "Label_Cost";
            this.lblCostLabel.Text = "Cost:";
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(246, 119);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 22;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(327, 119);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 21;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.Location = new System.Drawing.Point(222, 86);
            this.chkFreeItem.Name = "chkFreeItem";
            this.chkFreeItem.Size = new System.Drawing.Size(50, 17);
            this.chkFreeItem.TabIndex = 20;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.Visible = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // nudPersona
            // 
            this.nudPersona.Location = new System.Drawing.Point(302, 35);
            this.nudPersona.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudPersona.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudPersona.Name = "nudPersona";
            this.nudPersona.Size = new System.Drawing.Size(40, 20);
            this.nudPersona.TabIndex = 17;
            this.nudPersona.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudPersona.ValueChanged += new System.EventHandler(this.nudPersona_ValueChanged);
            // 
            // nudSignal
            // 
            this.nudSignal.Location = new System.Drawing.Point(301, 7);
            this.nudSignal.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudSignal.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSignal.Name = "nudSignal";
            this.nudSignal.Size = new System.Drawing.Size(40, 20);
            this.nudSignal.TabIndex = 15;
            this.nudSignal.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudSignal.ValueChanged += new System.EventHandler(this.nudSignal_ValueChanged);
            // 
            // lblSignalLabel
            // 
            this.lblSignalLabel.AutoSize = true;
            this.lblSignalLabel.Location = new System.Drawing.Point(218, 7);
            this.lblSignalLabel.Name = "lblSignalLabel";
            this.lblSignalLabel.Size = new System.Drawing.Size(39, 13);
            this.lblSignalLabel.TabIndex = 14;
            this.lblSignalLabel.Tag = "Label_Signal";
            this.lblSignalLabel.Text = "Signal:";
            // 
            // frmSelectNexus
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(414, 154);
            this.Controls.Add(this.nudSignal);
            this.Controls.Add(this.lblSignalLabel);
            this.Controls.Add(this.nudPersona);
            this.Controls.Add(this.chkFreeItem);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lblCost);
            this.Controls.Add(this.lblCostLabel);
            this.Controls.Add(this.lblFirewallAvail);
            this.Controls.Add(this.lblFirewallAvailLabel);
            this.Controls.Add(this.lblResponseAvail);
            this.Controls.Add(this.lblResponseAvailLabel);
            this.Controls.Add(this.lblSystemAvail);
            this.Controls.Add(this.lblSystemAvailLabel);
            this.Controls.Add(this.nudFirewall);
            this.Controls.Add(this.lblFirewallLabel);
            this.Controls.Add(this.nudResponse);
            this.Controls.Add(this.lblResponseLabel);
            this.Controls.Add(this.lblPersonaLimitLabel);
            this.Controls.Add(this.nudSystem);
            this.Controls.Add(this.lblSystemLabel);
            this.Controls.Add(this.nudProcessor);
            this.Controls.Add(this.lblProcessorLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectNexus";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "Title_SelectNexus";
            this.Text = "Build Nexus";
            this.Load += new System.EventHandler(this.frmSelectNexus_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudProcessor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSystem)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudResponse)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFirewall)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPersona)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSignal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblProcessorLabel;
        private System.Windows.Forms.NumericUpDown nudProcessor;
        private System.Windows.Forms.NumericUpDown nudSystem;
        private System.Windows.Forms.Label lblSystemLabel;
        private System.Windows.Forms.Label lblPersonaLimitLabel;
        private System.Windows.Forms.NumericUpDown nudResponse;
        private System.Windows.Forms.Label lblResponseLabel;
        private System.Windows.Forms.NumericUpDown nudFirewall;
        private System.Windows.Forms.Label lblFirewallLabel;
        private System.Windows.Forms.Label lblSystemAvail;
        private System.Windows.Forms.Label lblSystemAvailLabel;
        private System.Windows.Forms.Label lblResponseAvail;
        private System.Windows.Forms.Label lblResponseAvailLabel;
        private System.Windows.Forms.Label lblFirewallAvail;
        private System.Windows.Forms.Label lblFirewallAvailLabel;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.Label lblCostLabel;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.CheckBox chkFreeItem;
        private System.Windows.Forms.NumericUpDown nudPersona;
        private System.Windows.Forms.NumericUpDown nudSignal;
        private System.Windows.Forms.Label lblSignalLabel;
    }
}