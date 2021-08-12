
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
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.listSoftware = new System.Windows.Forms.CheckedListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dpcDefendDicePool = new Chummer.UI.Shared.Components.DicePoolControl();
            this.dpcActionDicePool = new Chummer.UI.Shared.Components.DicePoolControl();
            this.cbActions = new System.Windows.Forms.ComboBox();
            this.listCyberDecks = new System.Windows.Forms.ListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.MatrixTabPage.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.MatrixTabPage);
            this.tabControl1.Location = new System.Drawing.Point(1, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(797, 443);
            this.tabControl1.TabIndex = 0;
            // 
            // MatrixTabPage
            // 
            this.MatrixTabPage.BackColor = System.Drawing.Color.Transparent;
            this.MatrixTabPage.Controls.Add(this.groupBox4);
            this.MatrixTabPage.Controls.Add(this.groupBox3);
            this.MatrixTabPage.Controls.Add(this.groupBox2);
            this.MatrixTabPage.Controls.Add(this.listCyberDecks);
            this.MatrixTabPage.Controls.Add(this.groupBox1);
            this.MatrixTabPage.Location = new System.Drawing.Point(4, 22);
            this.MatrixTabPage.Name = "MatrixTabPage";
            this.MatrixTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.MatrixTabPage.Size = new System.Drawing.Size(789, 417);
            this.MatrixTabPage.TabIndex = 0;
            this.MatrixTabPage.Text = "Matrix";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.listSoftware);
            this.groupBox4.Location = new System.Drawing.Point(7, 263);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(208, 139);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Software";
            // 
            // listSoftware
            // 
            this.listSoftware.FormattingEnabled = true;
            this.listSoftware.Location = new System.Drawing.Point(6, 19);
            this.listSoftware.Name = "listSoftware";
            this.listSoftware.Size = new System.Drawing.Size(196, 109);
            this.listSoftware.TabIndex = 2;
            this.listSoftware.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listSoftware_ItemCheck);
            // 
            // groupBox3
            // 
            this.groupBox3.Location = new System.Drawing.Point(227, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(354, 97);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Modifiers";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.dpcDefendDicePool);
            this.groupBox2.Controls.Add(this.dpcActionDicePool);
            this.groupBox2.Controls.Add(this.cbActions);
            this.groupBox2.Location = new System.Drawing.Point(227, 109);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(354, 293);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Actions";
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
            this.dpcDefendDicePool.Location = new System.Drawing.Point(284, 132);
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
            this.cbActions.SelectedIndexChanged += new System.EventHandler(this.cbActions_SelectedIndexChanged);
            // 
            // listCyberDecks
            // 
            this.listCyberDecks.HideSelection = false;
            this.listCyberDecks.Location = new System.Drawing.Point(7, 6);
            this.listCyberDecks.Name = "listCyberDecks";
            this.listCyberDecks.Size = new System.Drawing.Size(208, 97);
            this.listCyberDecks.TabIndex = 1;
            this.listCyberDecks.UseCompatibleStateImageBehavior = false;
            this.listCyberDecks.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listCyberDecks_ItemSelectionChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lFirewallMod);
            this.groupBox1.Controls.Add(this.lDataProcMod);
            this.groupBox1.Controls.Add(this.lSleazeMod);
            this.groupBox1.Controls.Add(this.lAttackMod);
            this.groupBox1.Controls.Add(this.lFirewallRes);
            this.groupBox1.Controls.Add(this.lDataProcRes);
            this.groupBox1.Controls.Add(this.lSleazeRes);
            this.groupBox1.Controls.Add(this.lAttackRes);
            this.groupBox1.Controls.Add(this.rbOverFirewall);
            this.groupBox1.Controls.Add(this.rbOverDataProc);
            this.groupBox1.Controls.Add(this.rbOverSleaze);
            this.groupBox1.Controls.Add(this.rbOverAttack);
            this.groupBox1.Controls.Add(this.lOverClocker);
            this.groupBox1.Controls.Add(this.lFirewall);
            this.groupBox1.Controls.Add(this.cbFirewall);
            this.groupBox1.Controls.Add(this.lDataProc);
            this.groupBox1.Controls.Add(this.cbDataProc);
            this.groupBox1.Controls.Add(this.lSleaze);
            this.groupBox1.Controls.Add(this.cbSleaze);
            this.groupBox1.Controls.Add(this.lAttack);
            this.groupBox1.Controls.Add(this.cbAttack);
            this.groupBox1.Location = new System.Drawing.Point(7, 109);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(208, 148);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Attributes";
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
            this.rbOverFirewall.CheckedChanged += new System.EventHandler(this.rbOverFirewall_CheckedChanged);
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
            this.rbOverDataProc.CheckedChanged += new System.EventHandler(this.rbOverDataProc_CheckedChanged);
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
            this.rbOverSleaze.CheckedChanged += new System.EventHandler(this.rbOverSleaze_CheckedChanged);
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
            this.rbOverAttack.CheckedChanged += new System.EventHandler(this.rbOverAttack_CheckedChanged);
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
            this.cbFirewall.SelectedIndexChanged += new System.EventHandler(this.cbAttribute_SelectedIndexChanged);
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
            this.cbDataProc.SelectedIndexChanged += new System.EventHandler(this.cbAttribute_SelectedIndexChanged);
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
            this.cbSleaze.SelectedIndexChanged += new System.EventHandler(this.cbAttribute_SelectedIndexChanged);
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
            this.cbAttack.SelectedIndexChanged += new System.EventHandler(this.cbAttribute_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(10, 13);
            this.label1.TabIndex = 124;
            this.label1.Text = " ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 125;
            this.label2.Text = " ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(66, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 127;
            this.label3.Text = " ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(66, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 13);
            this.label4.TabIndex = 126;
            this.label4.Text = " ";
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
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(66, 143);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(10, 13);
            this.label9.TabIndex = 133;
            this.label9.Text = " ";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(66, 119);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(10, 13);
            this.label10.TabIndex = 132;
            this.label10.Text = " ";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 143);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(10, 13);
            this.label11.TabIndex = 131;
            this.label11.Text = " ";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 119);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(10, 13);
            this.label12.TabIndex = 130;
            this.label12.Text = " ";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(165, 81);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(10, 13);
            this.label13.TabIndex = 136;
            this.label13.Text = " ";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 173);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(10, 13);
            this.label14.TabIndex = 137;
            this.label14.Text = " ";
            // 
            // MatrixForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "MatrixForm";
            this.Text = "MatrixForm";
            this.tabControl1.ResumeLayout(false);
            this.MatrixTabPage.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TabControl tabControl1;
        public System.Windows.Forms.TabPage MatrixTabPage;
        public System.Windows.Forms.ListView listCyberDecks;
        public System.Windows.Forms.GroupBox groupBox1;
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
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.ComboBox cbActions;
        private Chummer.UI.Shared.Components.DicePoolControl dpcActionDicePool;
        private Chummer.UI.Shared.Components.DicePoolControl dpcDefendDicePool;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
    }
}
