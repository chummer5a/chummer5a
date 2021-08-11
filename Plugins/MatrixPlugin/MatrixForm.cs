using Chummer;
using Chummer.Backend.Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MatrixPlugin
{
    public partial class MatrixForm : Form
    {
        Character character;
        List<Gear> CyberDecks;
        public Gear CurrentDeck;
        List<Gear> Software;
        int maximumSoft = 0;
        public MatrixForm(Character _character)
        {
            character = _character;
            InitializeComponent();
            dpcActionDicePool.DicePool = 10;
            dpcDefendDicePool.DicePool = 8;
            CyberDecks = new List<Gear>();
            Software = new List<Gear>();
            addGear(character);
            if (!character.Overclocker)
            {
                lOverClocker.Enabled = false;
                rbOverAttack.Enabled = false;
                rbOverSleaze.Enabled = false;
                rbOverDataProc.Enabled = false;
                rbOverFirewall.Enabled = false;
            }
            listCyberDecks.Items.AddRange((from cyberdeck in CyberDecks select new ListViewItem(cyberdeck.Name)).ToArray());
            CurrentDeck = CyberDecks[0];
            listSoftware.Items.AddRange((from soft in Software select (soft.Name)).ToArray());

        }

        private void addDataBinding()
        {
            lAttackRes.DataBindings.Add(new Binding("Text", CurrentDeck, "Attack"));
            lSleazeRes.DataBindings.Add(new Binding("Text", CurrentDeck, "Sleaze"));
            lDataProcRes.DataBindings.Add(new Binding("Text", CurrentDeck, "DataProcessing"));
            lFirewallRes.DataBindings.Add(new Binding("Text", CurrentDeck, "Firewall"));
        }



        private void addGear(Character input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            foreach (Gear g in input.Gear)
            {
                if (g.Category == "Cyberdecks")
                {
                    CyberDecks.Add(g);
                    foreach (Gear child in g.Children)
                        if (child.Category == "Common Programs" || child.Category == "Hacking Programs")
                            Software.Add(child);
                }
                else if (g.Category == "Common Programs" || g.Category == "Hacking Programs")
                    Software.Add(g);
            }
        }

        private void listCyberDecks_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listCyberDecks.SelectedItems.Count == 0)
                return;
            ListViewItem Item = listCyberDecks.SelectedItems[0];

            foreach (Gear deck in CyberDecks)
            {
                if (deck.Name == Item.Text)
                {
                    CurrentDeck = deck;
                    RefreshMatrixAttributeCBOs(CurrentDeck, cbAttack, cbSleaze, cbDataProc, cbFirewall);
                    maximumSoft = int.Parse(deck.ProgramLimit);
                }
            }
        }

        public void RefreshMatrixAttributeCBOs(IHasMatrixAttributes objThis, ComboBox cboAttack, ComboBox cboSleaze, ComboBox cboDP, ComboBox cboFirewall)
        {
            if (objThis == null)
                return;
            if (cboAttack == null)
                throw new ArgumentNullException(nameof(cboAttack));
            if (cboSleaze == null)
                throw new ArgumentNullException(nameof(cboSleaze));
            if (cboDP == null)
                throw new ArgumentNullException(nameof(cboDP));
            if (cboFirewall == null)
                throw new ArgumentNullException(nameof(cboFirewall));
            
            int intBaseAttack = objThis.GetBaseMatrixAttribute("Attack");
            int intBaseSleaze = objThis.GetBaseMatrixAttribute("Sleaze");
            int intBaseDP = objThis.GetBaseMatrixAttribute("Data Processing");
            int intBaseFirewall = objThis.GetBaseMatrixAttribute("Firewall");
            List<string> DataSource = new List<string>(4) { (intBaseAttack).ToString(GlobalOptions.InvariantCultureInfo), (intBaseSleaze).ToString(GlobalOptions.InvariantCultureInfo), (intBaseDP).ToString(GlobalOptions.InvariantCultureInfo), (intBaseFirewall).ToString(GlobalOptions.InvariantCultureInfo) };

            cboAttack.SuspendLayout();
            cboSleaze.SuspendLayout();
            cboDP.SuspendLayout();
            cboFirewall.SuspendLayout();
            cboAttack.BeginUpdate();
            cboSleaze.BeginUpdate();
            cboDP.BeginUpdate();
            cboFirewall.BeginUpdate();
        
            cboAttack.Enabled = false;
            cboAttack.BindingContext = new BindingContext();
            cboAttack.ValueMember = nameof(ListItem.Value);
            cboAttack.DisplayMember = nameof(ListItem.Name);
            cboAttack.DataSource = DataSource;
            cboAttack.SelectedIndex = 0;
            cboAttack.Visible = true;
            cboAttack.Enabled = objThis.CanSwapAttributes;

            cboSleaze.Enabled = false;
            cboSleaze.BindingContext = new BindingContext();
            cboSleaze.ValueMember = nameof(ListItem.Value);
            cboSleaze.DisplayMember = nameof(ListItem.Name);
            cboSleaze.DataSource = DataSource;
            cboSleaze.SelectedIndex = 1;
            cboSleaze.Visible = true;
            cboSleaze.Enabled = objThis.CanSwapAttributes;

            cboDP.Enabled = false;
            cboDP.BindingContext = new BindingContext();
            cboDP.ValueMember = nameof(ListItem.Value);
            cboDP.DisplayMember = nameof(ListItem.Name);
            cboDP.DataSource = DataSource;
            cboDP.SelectedIndex = 2;
            cboDP.Visible = true;
            cboDP.Enabled = objThis.CanSwapAttributes;

            cboFirewall.Enabled = false;
            cboFirewall.BindingContext = new BindingContext();
            cboFirewall.ValueMember = nameof(ListItem.Value);
            cboFirewall.DisplayMember = nameof(ListItem.Name);
            cboFirewall.DataSource = DataSource;
            cboFirewall.SelectedIndex = 3;
            cboFirewall.Visible = true;
            cboFirewall.Enabled = objThis.CanSwapAttributes;

            cboAttack.EndUpdate();
            cboSleaze.EndUpdate();
            cboDP.EndUpdate();
            cboFirewall.EndUpdate();
            cboAttack.ResumeLayout();
            cboSleaze.ResumeLayout();
            cboDP.ResumeLayout();
            cboFirewall.ResumeLayout();
        }


        private void listSoftware_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            /*if (e.NewValue == CheckState.Checked) if (maximumSoft+1 > listSoftware.CheckedItems.Count)
            {
                int indexChecked = listSoftware.CheckedIndices[0];
                listSoftware.SetItemChecked(indexChecked,false);
            }*/
        }

        private void cbAttribute_SelectedIndexChanged(object sender, EventArgs e)
        {
            //CurrentDeck.ProcessMatrixAttributeCBOChange(character, (ComboBox)sender, cbAttack, cbSleaze, cbDataProc, cbFirewall);
            
        }

        private void rbOverFirewall_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbOverDataProc_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbOverSleaze_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbOverAttack_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
