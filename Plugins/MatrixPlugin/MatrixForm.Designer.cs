
using System.Windows.Forms;

namespace MatrixPlugin
{
    partial class MatrixForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        public System.ComponentModel.IContainer components = null;

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
        public void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.MatrixTabPage = new System.Windows.Forms.TabPage();
            this.listCyberDecks = new System.Windows.Forms.ListBox();
            this.gbAttributes = new System.Windows.Forms.GroupBox();
            this.lFirewallMod = new System.Windows.Forms.Label();
            this.lDataProcMod = new System.Windows.Forms.Label();
            this.lSleazeMod = new System.Windows.Forms.Label();
            this.lAttackMod = new System.Windows.Forms.Label();
            this.lFirewallRes = new System.Windows.Forms.Label();
            this.lDataProcRes = new System.Windows.Forms.Label();
            this.lSleazeRes = new System.Windows.Forms.Label();
            this.lAttackRes = new System.Windows.Forms.Label();
            this.rbOverFirewall = new System.Windows.Forms.RadioButton();
            this.rbOverDataProc = new System.Windows.Forms.RadioButton();
            this.rbOverSleaze = new System.Windows.Forms.RadioButton();
            this.rbOverAttack = new System.Windows.Forms.RadioButton();
            this.lOverClocker = new System.Windows.Forms.Label();
            this.lFirewall = new System.Windows.Forms.Label();
            this.cbFirewall = new System.Windows.Forms.ComboBox();
            this.lDataProc = new System.Windows.Forms.Label();
            this.cbDataProc = new System.Windows.Forms.ComboBox();
            this.lSleaze = new System.Windows.Forms.Label();
            this.cbSleaze = new System.Windows.Forms.ComboBox();
            this.lAttack = new System.Windows.Forms.Label();
            this.cbAttack = new System.Windows.Forms.ComboBox();
            this.gbSoftware = new System.Windows.Forms.GroupBox();
            this.listSoftware = new System.Windows.Forms.CheckedListBox();
            this.gbActions = new System.Windows.Forms.GroupBox();
            this.lActionType = new System.Windows.Forms.Label();
            this.lDefendModifier = new System.Windows.Forms.Label();
            this.lSkillLimitValue = new System.Windows.Forms.Label();
            this.lSkillLimitName = new System.Windows.Forms.Label();
            this.lSkillDescription = new System.Windows.Forms.Label();
            this.lActionModifier = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lDefendSkillValue = new System.Windows.Forms.Label();
            this.lDefendSkillName = new System.Windows.Forms.Label();
            this.lDefendAttributeValue = new System.Windows.Forms.Label();
            this.lDefendAttributeName = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lActionSkillValue = new System.Windows.Forms.Label();
            this.lActionSkillName = new System.Windows.Forms.Label();
            this.lActionAttributeValue = new System.Windows.Forms.Label();
            this.lActionAttributeName = new System.Windows.Forms.Label();
            this.dpcDefendDicePool = new Chummer.UI.Shared.Components.DicePoolControl();
            this.dpcActionDicePool = new Chummer.UI.Shared.Components.DicePoolControl();
            this.cbActions = new System.Windows.Forms.ComboBox();
            this.gbModifier = new System.Windows.Forms.GroupBox();
            this.cSilent = new System.Windows.Forms.CheckBox();
            this.cHotVR = new System.Windows.Forms.CheckBox();
            this.nNoize = new System.Windows.Forms.NumericUpDown();
            this.lNoize = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.MatrixTabPage.SuspendLayout();
            this.gbAttributes.SuspendLayout();
            this.gbSoftware.SuspendLayout();
            this.gbActions.SuspendLayout();
            this.gbModifier.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nNoize)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.MatrixTabPage);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(985, 578);
            this.tabControl1.TabIndex = 0;
            // 
            // MatrixTabPage
            // 
            this.MatrixTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.MatrixTabPage.Controls.Add(this.listCyberDecks);
            this.MatrixTabPage.Controls.Add(this.gbAttributes);
            this.MatrixTabPage.Controls.Add(this.gbSoftware);
            this.MatrixTabPage.Controls.Add(this.gbActions);
            this.MatrixTabPage.Controls.Add(this.gbModifier);
            this.MatrixTabPage.Location = new System.Drawing.Point(4, 22);
            this.MatrixTabPage.Name = "MatrixTabPage";
            this.MatrixTabPage.Size = new System.Drawing.Size(977, 552);
            this.MatrixTabPage.TabIndex = 0;
            this.MatrixTabPage.Text = "Matrix";
            // 
            // listCyberDecks
            // 
            this.listCyberDecks.FormattingEnabled = true;
            this.listCyberDecks.Location = new System.Drawing.Point(7, 8);
            this.listCyberDecks.Name = "listCyberDecks";
            this.listCyberDecks.Size = new System.Drawing.Size(208, 186);
            this.listCyberDecks.TabIndex = 1;
            this.listCyberDecks.SelectedIndexChanged += new System.EventHandler(this.ListCyberDecks_SelectedIndexChanged);
            // 
            // gbAttributes
            // 
            this.gbAttributes.Controls.Add(this.lFirewallMod);
            this.gbAttributes.Controls.Add(this.lDataProcMod);
            this.gbAttributes.Controls.Add(this.lSleazeMod);
            this.gbAttributes.Controls.Add(this.lAttackMod);
            this.gbAttributes.Controls.Add(this.lFirewallRes);
            this.gbAttributes.Controls.Add(this.lDataProcRes);
            this.gbAttributes.Controls.Add(this.lSleazeRes);
            this.gbAttributes.Controls.Add(this.lAttackRes);
            this.gbAttributes.Controls.Add(this.rbOverFirewall);
            this.gbAttributes.Controls.Add(this.rbOverDataProc);
            this.gbAttributes.Controls.Add(this.rbOverSleaze);
            this.gbAttributes.Controls.Add(this.rbOverAttack);
            this.gbAttributes.Controls.Add(this.lOverClocker);
            this.gbAttributes.Controls.Add(this.lFirewall);
            this.gbAttributes.Controls.Add(this.cbFirewall);
            this.gbAttributes.Controls.Add(this.lDataProc);
            this.gbAttributes.Controls.Add(this.cbDataProc);
            this.gbAttributes.Controls.Add(this.lSleaze);
            this.gbAttributes.Controls.Add(this.cbSleaze);
            this.gbAttributes.Controls.Add(this.lAttack);
            this.gbAttributes.Controls.Add(this.cbAttack);
            this.gbAttributes.Location = new System.Drawing.Point(7, 200);
            this.gbAttributes.Name = "gbAttributes";
            this.gbAttributes.Size = new System.Drawing.Size(208, 148);
            this.gbAttributes.TabIndex = 0;
            this.gbAttributes.TabStop = false;
            this.gbAttributes.Text = "Attributes";
            // 
            // lFirewallMod
            // 
            this.lFirewallMod.AutoSize = true;
            this.lFirewallMod.Location = new System.Drawing.Point(117, 122);
            this.lFirewallMod.Name = "lFirewallMod";
            this.lFirewallMod.Size = new System.Drawing.Size(19, 13);
            this.lFirewallMod.TabIndex = 20;
            this.lFirewallMod.Text = "+0";
            // 
            // lDataProcMod
            // 
            this.lDataProcMod.AutoSize = true;
            this.lDataProcMod.Location = new System.Drawing.Point(117, 95);
            this.lDataProcMod.Name = "lDataProcMod";
            this.lDataProcMod.Size = new System.Drawing.Size(19, 13);
            this.lDataProcMod.TabIndex = 19;
            this.lDataProcMod.Text = "+0";
            // 
            // lSleazeMod
            // 
            this.lSleazeMod.AutoSize = true;
            this.lSleazeMod.Location = new System.Drawing.Point(117, 68);
            this.lSleazeMod.Name = "lSleazeMod";
            this.lSleazeMod.Size = new System.Drawing.Size(19, 13);
            this.lSleazeMod.TabIndex = 18;
            this.lSleazeMod.Text = "+0";
            // 
            // lAttackMod
            // 
            this.lAttackMod.AutoSize = true;
            this.lAttackMod.Location = new System.Drawing.Point(117, 41);
            this.lAttackMod.Name = "lAttackMod";
            this.lAttackMod.Size = new System.Drawing.Size(19, 13);
            this.lAttackMod.TabIndex = 17;
            this.lAttackMod.Text = "+0";
            // 
            // lFirewallRes
            // 
            this.lFirewallRes.AutoSize = true;
            this.lFirewallRes.Location = new System.Drawing.Point(189, 122);
            this.lFirewallRes.Name = "lFirewallRes";
            this.lFirewallRes.Size = new System.Drawing.Size(13, 13);
            this.lFirewallRes.TabIndex = 16;
            this.lFirewallRes.Text = "0";
            // 
            // lDataProcRes
            // 
            this.lDataProcRes.AutoSize = true;
            this.lDataProcRes.Location = new System.Drawing.Point(189, 95);
            this.lDataProcRes.Name = "lDataProcRes";
            this.lDataProcRes.Size = new System.Drawing.Size(13, 13);
            this.lDataProcRes.TabIndex = 15;
            this.lDataProcRes.Text = "0";
            // 
            // lSleazeRes
            // 
            this.lSleazeRes.AutoSize = true;
            this.lSleazeRes.Location = new System.Drawing.Point(189, 68);
            this.lSleazeRes.Name = "lSleazeRes";
            this.lSleazeRes.Size = new System.Drawing.Size(13, 13);
            this.lSleazeRes.TabIndex = 14;
            this.lSleazeRes.Text = "0";
            // 
            // lAttackRes
            // 
            this.lAttackRes.AutoSize = true;
            this.lAttackRes.Location = new System.Drawing.Point(189, 41);
            this.lAttackRes.Name = "lAttackRes";
            this.lAttackRes.Size = new System.Drawing.Size(13, 13);
            this.lAttackRes.TabIndex = 13;
            this.lAttackRes.Text = "0";
            // 
            // rbOverFirewall
            // 
            this.rbOverFirewall.AutoSize = true;
            this.rbOverFirewall.Location = new System.Drawing.Point(142, 122);
            this.rbOverFirewall.Name = "rbOverFirewall";
            this.rbOverFirewall.Size = new System.Drawing.Size(14, 13);
            this.rbOverFirewall.TabIndex = 12;
            this.rbOverFirewall.TabStop = true;
            this.rbOverFirewall.UseVisualStyleBackColor = true;
            // 
            // rbOverDataProc
            // 
            this.rbOverDataProc.AutoSize = true;
            this.rbOverDataProc.Location = new System.Drawing.Point(142, 95);
            this.rbOverDataProc.Name = "rbOverDataProc";
            this.rbOverDataProc.Size = new System.Drawing.Size(14, 13);
            this.rbOverDataProc.TabIndex = 11;
            this.rbOverDataProc.TabStop = true;
            this.rbOverDataProc.UseVisualStyleBackColor = true;
            // 
            // rbOverSleaze
            // 
            this.rbOverSleaze.AutoSize = true;
            this.rbOverSleaze.Location = new System.Drawing.Point(142, 68);
            this.rbOverSleaze.Name = "rbOverSleaze";
            this.rbOverSleaze.Size = new System.Drawing.Size(14, 13);
            this.rbOverSleaze.TabIndex = 10;
            this.rbOverSleaze.TabStop = true;
            this.rbOverSleaze.UseVisualStyleBackColor = true;
            // 
            // rbOverAttack
            // 
            this.rbOverAttack.AutoSize = true;
            this.rbOverAttack.Location = new System.Drawing.Point(142, 41);
            this.rbOverAttack.Name = "rbOverAttack";
            this.rbOverAttack.Size = new System.Drawing.Size(14, 13);
            this.rbOverAttack.TabIndex = 9;
            this.rbOverAttack.TabStop = true;
            this.rbOverAttack.UseVisualStyleBackColor = true;
            // 
            // lOverClocker
            // 
            this.lOverClocker.AutoSize = true;
            this.lOverClocker.Location = new System.Drawing.Point(117, 16);
            this.lOverClocker.Name = "lOverClocker";
            this.lOverClocker.Size = new System.Drawing.Size(66, 13);
            this.lOverClocker.TabIndex = 8;
            this.lOverClocker.Text = "OverClocker";
            // 
            // lFirewall
            // 
            this.lFirewall.AutoSize = true;
            this.lFirewall.Location = new System.Drawing.Point(6, 122);
            this.lFirewall.Name = "lFirewall";
            this.lFirewall.Size = new System.Drawing.Size(42, 13);
            this.lFirewall.TabIndex = 7;
            this.lFirewall.Text = "Firewall";
            // 
            // cbFirewall
            // 
            this.cbFirewall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFirewall.FormattingEnabled = true;
            this.cbFirewall.Location = new System.Drawing.Point(67, 119);
            this.cbFirewall.Name = "cbFirewall";
            this.cbFirewall.Size = new System.Drawing.Size(44, 21);
            this.cbFirewall.TabIndex = 6;
            this.cbFirewall.SelectedIndexChanged += new System.EventHandler(this.CbAttribute_SelectedIndexChanged);
            // 
            // lDataProc
            // 
            this.lDataProc.AutoSize = true;
            this.lDataProc.Location = new System.Drawing.Point(6, 95);
            this.lDataProc.Name = "lDataProc";
            this.lDataProc.Size = new System.Drawing.Size(55, 13);
            this.lDataProc.TabIndex = 5;
            this.lDataProc.Text = "Data Proc";
            // 
            // cbDataProc
            // 
            this.cbDataProc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDataProc.FormattingEnabled = true;
            this.cbDataProc.Location = new System.Drawing.Point(67, 92);
            this.cbDataProc.Name = "cbDataProc";
            this.cbDataProc.Size = new System.Drawing.Size(44, 21);
            this.cbDataProc.TabIndex = 4;
            this.cbDataProc.SelectedIndexChanged += new System.EventHandler(this.CbAttribute_SelectedIndexChanged);
            // 
            // lSleaze
            // 
            this.lSleaze.AutoSize = true;
            this.lSleaze.Location = new System.Drawing.Point(6, 68);
            this.lSleaze.Name = "lSleaze";
            this.lSleaze.Size = new System.Drawing.Size(39, 13);
            this.lSleaze.TabIndex = 3;
            this.lSleaze.Text = "Sleaze";
            // 
            // cbSleaze
            // 
            this.cbSleaze.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSleaze.FormattingEnabled = true;
            this.cbSleaze.Location = new System.Drawing.Point(67, 65);
            this.cbSleaze.Name = "cbSleaze";
            this.cbSleaze.Size = new System.Drawing.Size(44, 21);
            this.cbSleaze.TabIndex = 2;
            this.cbSleaze.SelectedIndexChanged += new System.EventHandler(this.CbAttribute_SelectedIndexChanged);
            // 
            // lAttack
            // 
            this.lAttack.AutoSize = true;
            this.lAttack.Location = new System.Drawing.Point(6, 41);
            this.lAttack.Name = "lAttack";
            this.lAttack.Size = new System.Drawing.Size(38, 13);
            this.lAttack.TabIndex = 1;
            this.lAttack.Text = "Attack";
            // 
            // cbAttack
            // 
            this.cbAttack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAttack.FormattingEnabled = true;
            this.cbAttack.Location = new System.Drawing.Point(67, 38);
            this.cbAttack.Name = "cbAttack";
            this.cbAttack.Size = new System.Drawing.Size(44, 21);
            this.cbAttack.TabIndex = 0;
            this.cbAttack.SelectedIndexChanged += new System.EventHandler(this.CbAttribute_SelectedIndexChanged);
            // 
            // gbSoftware
            // 
            this.gbSoftware.Controls.Add(this.listSoftware);
            this.gbSoftware.Location = new System.Drawing.Point(7, 354);
            this.gbSoftware.Name = "gbSoftware";
            this.gbSoftware.Size = new System.Drawing.Size(208, 188);
            this.gbSoftware.TabIndex = 5;
            this.gbSoftware.TabStop = false;
            this.gbSoftware.Text = "Software";
            // 
            // listSoftware
            // 
            this.listSoftware.FormattingEnabled = true;
            this.listSoftware.Location = new System.Drawing.Point(6, 19);
            this.listSoftware.Name = "listSoftware";
            this.listSoftware.Size = new System.Drawing.Size(196, 154);
            this.listSoftware.TabIndex = 2;
            this.listSoftware.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ListSoftware_ItemCheck);
            // 
            // gbActions
            // 
            this.gbActions.BackColor = System.Drawing.SystemColors.Control;
            this.gbActions.Controls.Add(this.lActionType);
            this.gbActions.Controls.Add(this.lDefendModifier);
            this.gbActions.Controls.Add(this.lSkillLimitValue);
            this.gbActions.Controls.Add(this.lSkillLimitName);
            this.gbActions.Controls.Add(this.lSkillDescription);
            this.gbActions.Controls.Add(this.lActionModifier);
            this.gbActions.Controls.Add(this.label7);
            this.gbActions.Controls.Add(this.label8);
            this.gbActions.Controls.Add(this.lDefendSkillValue);
            this.gbActions.Controls.Add(this.lDefendSkillName);
            this.gbActions.Controls.Add(this.lDefendAttributeValue);
            this.gbActions.Controls.Add(this.lDefendAttributeName);
            this.gbActions.Controls.Add(this.label5);
            this.gbActions.Controls.Add(this.label6);
            this.gbActions.Controls.Add(this.lActionSkillValue);
            this.gbActions.Controls.Add(this.lActionSkillName);
            this.gbActions.Controls.Add(this.lActionAttributeValue);
            this.gbActions.Controls.Add(this.lActionAttributeName);
            this.gbActions.Controls.Add(this.dpcDefendDicePool);
            this.gbActions.Controls.Add(this.dpcActionDicePool);
            this.gbActions.Controls.Add(this.cbActions);
            this.gbActions.Location = new System.Drawing.Point(221, 200);
            this.gbActions.Name = "gbActions";
            this.gbActions.Size = new System.Drawing.Size(354, 342);
            this.gbActions.TabIndex = 3;
            this.gbActions.TabStop = false;
            this.gbActions.Text = "Actions";
            // 
            // lActionType
            // 
            this.lActionType.AutoSize = true;
            this.lActionType.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lActionType.Location = new System.Drawing.Point(6, 185);
            this.lActionType.Name = "lActionType";
            this.lActionType.Size = new System.Drawing.Size(26, 13);
            this.lActionType.TabIndex = 141;
            this.lActionType.Text = " [0]";
            // 
            // lDefendModifier
            // 
            this.lDefendModifier.AutoSize = true;
            this.lDefendModifier.Location = new System.Drawing.Point(152, 143);
            this.lDefendModifier.Name = "lDefendModifier";
            this.lDefendModifier.Size = new System.Drawing.Size(22, 13);
            this.lDefendModifier.TabIndex = 140;
            this.lDefendModifier.Text = " [0]";
            // 
            // lSkillLimitValue
            // 
            this.lSkillLimitValue.AutoSize = true;
            this.lSkillLimitValue.Location = new System.Drawing.Point(212, 81);
            this.lSkillLimitValue.Name = "lSkillLimitValue";
            this.lSkillLimitValue.Size = new System.Drawing.Size(22, 13);
            this.lSkillLimitValue.TabIndex = 139;
            this.lSkillLimitValue.Text = " [0]";
            // 
            // lSkillLimitName
            // 
            this.lSkillLimitName.AutoSize = true;
            this.lSkillLimitName.Location = new System.Drawing.Point(212, 57);
            this.lSkillLimitName.Name = "lSkillLimitName";
            this.lSkillLimitName.Size = new System.Drawing.Size(10, 13);
            this.lSkillLimitName.TabIndex = 138;
            this.lSkillLimitName.Text = " ";
            // 
            // lSkillDescription
            // 
            this.lSkillDescription.Location = new System.Drawing.Point(6, 219);
            this.lSkillDescription.Name = "lSkillDescription";
            this.lSkillDescription.Size = new System.Drawing.Size(342, 108);
            this.lSkillDescription.TabIndex = 137;
            // 
            // lActionModifier
            // 
            this.lActionModifier.AutoSize = true;
            this.lActionModifier.Location = new System.Drawing.Point(152, 81);
            this.lActionModifier.Name = "lActionModifier";
            this.lActionModifier.Size = new System.Drawing.Size(22, 13);
            this.lActionModifier.TabIndex = 136;
            this.lActionModifier.Text = " [0]";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(47, 143);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(13, 13);
            this.label7.TabIndex = 135;
            this.label7.Text = "+";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(47, 119);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(13, 13);
            this.label8.TabIndex = 134;
            this.label8.Text = "+";
            // 
            // lDefendSkillValue
            // 
            this.lDefendSkillValue.AutoSize = true;
            this.lDefendSkillValue.Location = new System.Drawing.Point(66, 143);
            this.lDefendSkillValue.Name = "lDefendSkillValue";
            this.lDefendSkillValue.Size = new System.Drawing.Size(22, 13);
            this.lDefendSkillValue.TabIndex = 133;
            this.lDefendSkillValue.Text = " [0]";
            // 
            // lDefendSkillName
            // 
            this.lDefendSkillName.AutoSize = true;
            this.lDefendSkillName.Location = new System.Drawing.Point(66, 119);
            this.lDefendSkillName.Name = "lDefendSkillName";
            this.lDefendSkillName.Size = new System.Drawing.Size(10, 13);
            this.lDefendSkillName.TabIndex = 132;
            this.lDefendSkillName.Text = " ";
            // 
            // lDefendAttributeValue
            // 
            this.lDefendAttributeValue.AutoSize = true;
            this.lDefendAttributeValue.Location = new System.Drawing.Point(6, 143);
            this.lDefendAttributeValue.Name = "lDefendAttributeValue";
            this.lDefendAttributeValue.Size = new System.Drawing.Size(22, 13);
            this.lDefendAttributeValue.TabIndex = 131;
            this.lDefendAttributeValue.Text = " [0]";
            // 
            // lDefendAttributeName
            // 
            this.lDefendAttributeName.AutoSize = true;
            this.lDefendAttributeName.Location = new System.Drawing.Point(6, 119);
            this.lDefendAttributeName.Name = "lDefendAttributeName";
            this.lDefendAttributeName.Size = new System.Drawing.Size(10, 13);
            this.lDefendAttributeName.TabIndex = 130;
            this.lDefendAttributeName.Text = " ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 81);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(13, 13);
            this.label5.TabIndex = 129;
            this.label5.Text = "+";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(47, 57);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(13, 13);
            this.label6.TabIndex = 128;
            this.label6.Text = "+";
            // 
            // lActionSkillValue
            // 
            this.lActionSkillValue.AutoSize = true;
            this.lActionSkillValue.Location = new System.Drawing.Point(66, 81);
            this.lActionSkillValue.Name = "lActionSkillValue";
            this.lActionSkillValue.Size = new System.Drawing.Size(22, 13);
            this.lActionSkillValue.TabIndex = 127;
            this.lActionSkillValue.Text = " [0]";
            // 
            // lActionSkillName
            // 
            this.lActionSkillName.AutoSize = true;
            this.lActionSkillName.Location = new System.Drawing.Point(66, 57);
            this.lActionSkillName.Name = "lActionSkillName";
            this.lActionSkillName.Size = new System.Drawing.Size(10, 13);
            this.lActionSkillName.TabIndex = 126;
            this.lActionSkillName.Text = " ";
            // 
            // lActionAttributeValue
            // 
            this.lActionAttributeValue.AutoSize = true;
            this.lActionAttributeValue.Location = new System.Drawing.Point(6, 81);
            this.lActionAttributeValue.Name = "lActionAttributeValue";
            this.lActionAttributeValue.Size = new System.Drawing.Size(22, 13);
            this.lActionAttributeValue.TabIndex = 125;
            this.lActionAttributeValue.Text = " [0]";
            // 
            // lActionAttributeName
            // 
            this.lActionAttributeName.AutoSize = true;
            this.lActionAttributeName.Location = new System.Drawing.Point(6, 57);
            this.lActionAttributeName.Name = "lActionAttributeName";
            this.lActionAttributeName.Size = new System.Drawing.Size(10, 13);
            this.lActionAttributeName.TabIndex = 124;
            this.lActionAttributeName.Text = " ";
            // 
            // dpcDefendDicePool
            // 
            this.dpcDefendDicePool.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.dpcDefendDicePool.AutoSize = true;
            this.dpcDefendDicePool.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.dpcDefendDicePool.BackColor = System.Drawing.SystemColors.Control;
            this.dpcDefendDicePool.CanBeRolled = true;
            this.dpcDefendDicePool.CanEverBeRolled = true;
            this.dpcDefendDicePool.DicePool = 0;
            this.dpcDefendDicePool.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dpcDefendDicePool.Location = new System.Drawing.Point(284, 143);
            this.dpcDefendDicePool.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.dpcDefendDicePool.Name = "dpcDefendDicePool";
            this.dpcDefendDicePool.Size = new System.Drawing.Size(64, 24);
            this.dpcDefendDicePool.TabIndex = 123;
            this.dpcDefendDicePool.ToolTipText = "";
            // 
            // dpcActionDicePool
            // 
            this.dpcActionDicePool.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.dpcActionDicePool.AutoSize = true;
            this.dpcActionDicePool.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.dpcActionDicePool.BackColor = System.Drawing.SystemColors.Control;
            this.dpcActionDicePool.CanBeRolled = true;
            this.dpcActionDicePool.CanEverBeRolled = true;
            this.dpcActionDicePool.DicePool = 0;
            this.dpcActionDicePool.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dpcActionDicePool.Location = new System.Drawing.Point(284, 70);
            this.dpcActionDicePool.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.dpcActionDicePool.Name = "dpcActionDicePool";
            this.dpcActionDicePool.Size = new System.Drawing.Size(64, 24);
            this.dpcActionDicePool.TabIndex = 122;
            this.dpcActionDicePool.ToolTipText = "";
            // 
            // cbActions
            // 
            this.cbActions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbActions.FormattingEnabled = true;
            this.cbActions.Location = new System.Drawing.Point(6, 33);
            this.cbActions.Name = "cbActions";
            this.cbActions.Size = new System.Drawing.Size(342, 21);
            this.cbActions.TabIndex = 21;
            this.cbActions.SelectedIndexChanged += new System.EventHandler(this.CbActions_SelectedIndexChanged);
            // 
            // gbModifier
            // 
            this.gbModifier.BackColor = System.Drawing.SystemColors.Control;
            this.gbModifier.Controls.Add(this.cSilent);
            this.gbModifier.Controls.Add(this.cHotVR);
            this.gbModifier.Controls.Add(this.nNoize);
            this.gbModifier.Controls.Add(this.lNoize);
            this.gbModifier.Location = new System.Drawing.Point(227, 6);
            this.gbModifier.Name = "gbModifier";
            this.gbModifier.Size = new System.Drawing.Size(348, 188);
            this.gbModifier.TabIndex = 4;
            this.gbModifier.TabStop = false;
            this.gbModifier.Text = "Modifiers";
            // 
            // cSilent
            // 
            this.cSilent.AutoSize = true;
            this.cSilent.Location = new System.Drawing.Point(9, 68);
            this.cSilent.Name = "cSilent";
            this.cSilent.Size = new System.Drawing.Size(52, 17);
            this.cSilent.TabIndex = 3;
            this.cSilent.Text = "Silent";
            this.cSilent.UseVisualStyleBackColor = true;
            this.cSilent.CheckedChanged += new System.EventHandler(this.ValueChanged);
            // 
            // cHotVR
            // 
            this.cHotVR.AutoSize = true;
            this.cHotVR.Location = new System.Drawing.Point(9, 45);
            this.cHotVR.Name = "cHotVR";
            this.cHotVR.Size = new System.Drawing.Size(61, 17);
            this.cHotVR.TabIndex = 2;
            this.cHotVR.Text = "Hot VR";
            this.cHotVR.UseVisualStyleBackColor = true;
            this.cHotVR.CheckedChanged += new System.EventHandler(this.ValueChanged);
            // 
            // nNoize
            // 
            this.nNoize.Location = new System.Drawing.Point(46, 19);
            this.nNoize.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nNoize.Name = "nNoize";
            this.nNoize.Size = new System.Drawing.Size(45, 20);
            this.nNoize.TabIndex = 1;
            this.nNoize.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // lNoize
            // 
            this.lNoize.AutoSize = true;
            this.lNoize.Location = new System.Drawing.Point(6, 21);
            this.lNoize.Name = "lNoize";
            this.lNoize.Size = new System.Drawing.Size(34, 13);
            this.lNoize.TabIndex = 0;
            this.lNoize.Text = "Noize";
            // 
            // MatrixForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 661);
            this.Controls.Add(this.tabControl1);
            this.Name = "MatrixForm";
            this.Text = "MatrixForm";
            this.tabControl1.ResumeLayout(false);
            this.MatrixTabPage.ResumeLayout(false);
            this.gbAttributes.ResumeLayout(false);
            this.gbAttributes.PerformLayout();
            this.gbSoftware.ResumeLayout(false);
            this.gbActions.ResumeLayout(false);
            this.gbActions.PerformLayout();
            this.gbModifier.ResumeLayout(false);
            this.gbModifier.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nNoize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TabControl tabControl1;
        public System.Windows.Forms.TabPage MatrixTabPage;
        public System.Windows.Forms.GroupBox gbAttributes;
        public System.Windows.Forms.Label lFirewall;
        public System.Windows.Forms.ComboBox cbFirewall;
        public System.Windows.Forms.Label lDataProc;
        public System.Windows.Forms.ComboBox cbDataProc;
        public System.Windows.Forms.Label lSleaze;
        public System.Windows.Forms.ComboBox cbSleaze;
        public System.Windows.Forms.Label lAttack;
        public System.Windows.Forms.ComboBox cbAttack;
        public System.Windows.Forms.Label lFirewallRes;
        public System.Windows.Forms.Label lDataProcRes;
        public System.Windows.Forms.Label lSleazeRes;
        public System.Windows.Forms.Label lAttackRes;
        public System.Windows.Forms.RadioButton rbOverFirewall;
        public System.Windows.Forms.RadioButton rbOverDataProc;
        public System.Windows.Forms.RadioButton rbOverSleaze;
        public System.Windows.Forms.RadioButton rbOverAttack;
        public System.Windows.Forms.Label lOverClocker;
        public System.Windows.Forms.CheckedListBox listSoftware;
        public System.Windows.Forms.Label lFirewallMod;
        public System.Windows.Forms.Label lDataProcMod;
        public System.Windows.Forms.Label lSleazeMod;
        public System.Windows.Forms.Label lAttackMod;
        private System.Windows.Forms.GroupBox gbSoftware;
        private System.Windows.Forms.GroupBox gbModifier;
        private System.Windows.Forms.GroupBox gbActions;
        public System.Windows.Forms.ComboBox cbActions;
        private Chummer.UI.Shared.Components.DicePoolControl dpcActionDicePool;
        private Chummer.UI.Shared.Components.DicePoolControl dpcDefendDicePool;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lActionSkillValue;
        private System.Windows.Forms.Label lActionSkillName;
        private System.Windows.Forms.Label lActionAttributeValue;
        private System.Windows.Forms.Label lActionAttributeName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lDefendSkillValue;
        private System.Windows.Forms.Label lDefendSkillName;
        private System.Windows.Forms.Label lDefendAttributeValue;
        private System.Windows.Forms.Label lDefendAttributeName;
        private System.Windows.Forms.Label lActionModifier;
        private System.Windows.Forms.Label lSkillDescription;
        private System.Windows.Forms.Label lSkillLimitValue;
        private System.Windows.Forms.Label lSkillLimitName;
        private System.Windows.Forms.ListBox listCyberDecks;
        private System.Windows.Forms.NumericUpDown nNoize;
        private System.Windows.Forms.Label lNoize;
        private System.Windows.Forms.CheckBox cSilent;
        private System.Windows.Forms.CheckBox cHotVR;
        private Label lDefendModifier;
        private Label lActionType;
    }
}
