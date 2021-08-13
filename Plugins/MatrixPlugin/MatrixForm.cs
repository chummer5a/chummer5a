using Chummer;
using Chummer.Backend.Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace MatrixPlugin
{
    public partial class MatrixForm : Form
    {
        Character character;
        List<Gear> CyberDecks;
        List<Gear> Software;
        List<MatrixAction> Actions;
        MatrixAction currentAction;
        int modAttack = 0;
        int modSleaze = 0;
        int modDataProc = 0;
        int modFirewall = 0;
        public MatrixForm(frmCareer input, List<MatrixAction> actions)
        {
            character = input.CharacterObject;
            InitializeComponent();
            CyberDecks = new List<Gear>();
            Software = new List<Gear>();
            Actions = actions;
            addGear(character);
            
            
            listCyberDecks.Items.AddRange((from cyberdeck in CyberDecks select cyberdeck.Name).ToArray());
            listSoftware.Items.AddRange((from soft in Software select (soft.Name)).ToArray());
            cbActions.Items.AddRange((from action in Actions select (action.Name)).ToArray());

            listCyberDecks.SelectedIndex = listCyberDecks.Items.IndexOf(character.ActiveCommlink);
            cbActions.SelectedIndex = 0;

            UpdateFormElements();
        }

        private void UpdateFormElements()
        {
            UpdateOverclock();
            addDataBinding();
            UpdateSkillLabels();
        }

        private void UpdateOverclock()
        {
            if (!character.Overclocker)
            {
                lOverClocker.Enabled = false;
                rbOverAttack.Enabled = false;
                rbOverSleaze.Enabled = false;
                rbOverDataProc.Enabled = false;
                rbOverFirewall.Enabled = false;
            }
            else switch (character.ActiveCommlink.Overclocked)
                {
                    case "Attack": rbOverAttack.Checked = true; break;
                    case "Sleaze": rbOverSleaze.Checked = true; break;
                    case "Data Processing": rbOverDataProc.Checked = true; break;
                    case "Firewall": rbOverFirewall.Checked = true; break;
                    default: rbOverAttack.Checked = true; break;
                }
        }

        private void addDataBinding()
        {
            Bind(lAttackRes, modAttack+character.ActiveCommlink.GetTotalMatrixAttribute("Attack"));
            Bind(lSleazeRes, modSleaze + character.ActiveCommlink.GetTotalMatrixAttribute("Sleaze"));
            Bind(lDataProcRes, modDataProc + character.ActiveCommlink.GetTotalMatrixAttribute("Data Processing"));
            Bind(lFirewallRes, modFirewall + character.ActiveCommlink.GetTotalMatrixAttribute("Firewall"));
            Bind(lAttackMod, modAttack + character.ActiveCommlink.GetBonusMatrixAttribute("Attack"));
            Bind(lSleazeMod, modSleaze + character.ActiveCommlink.GetBonusMatrixAttribute("Sleaze"));
            Bind(lDataProcMod, modDataProc + character.ActiveCommlink.GetBonusMatrixAttribute("Data Processing"));
            Bind(lFirewallMod, modFirewall + character.ActiveCommlink.GetBonusMatrixAttribute("Firewall"));
        }

        private void Bind(Label label, object val)
        {
            label.Text = val.ToString();
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

            int intBaseAttack = int.Parse(objThis.Attack);
            int intBaseSleaze = int.Parse(objThis.Sleaze);
            int intBaseDP = int.Parse(objThis.DataProcessing);
            int intBaseFirewall = int.Parse(objThis.Firewall);
            List<string> DataSource = new List<string>(4) { (intBaseAttack).ToString(GlobalOptions.InvariantCultureInfo), (intBaseSleaze).ToString(GlobalOptions.InvariantCultureInfo), (intBaseDP).ToString(GlobalOptions.InvariantCultureInfo), (intBaseFirewall).ToString(GlobalOptions.InvariantCultureInfo) };

            cboAttack.SuspendLayout();
            cboSleaze.SuspendLayout();
            cboDP.SuspendLayout();
            cboFirewall.SuspendLayout();
            cboAttack.BeginUpdate();
            cboSleaze.BeginUpdate();
            cboDP.BeginUpdate();
            cboFirewall.BeginUpdate();

            cboAttack.SelectedIndexChanged -= cbAttribute_SelectedIndexChanged;
            cboAttack.Enabled = false;
            cboAttack.BindingContext = new BindingContext();
            cboAttack.ValueMember = nameof(ListItem.Value);
            cboAttack.DisplayMember = nameof(ListItem.Name);
            cboAttack.DataSource = DataSource;
            cboAttack.SelectedIndex = 0;
            cboAttack.Visible = true;
            cboAttack.Enabled = objThis.CanSwapAttributes;
            cboAttack.SelectedIndexChanged += cbAttribute_SelectedIndexChanged;

            cboSleaze.SelectedIndexChanged -= cbAttribute_SelectedIndexChanged;
            cboSleaze.Enabled = false;
            cboSleaze.BindingContext = new BindingContext();
            cboSleaze.ValueMember = nameof(ListItem.Value);
            cboSleaze.DisplayMember = nameof(ListItem.Name);
            cboSleaze.DataSource = DataSource;
            cboSleaze.SelectedIndex = 1;
            cboSleaze.Visible = true;
            cboSleaze.Enabled = objThis.CanSwapAttributes;
            cboSleaze.SelectedIndexChanged += cbAttribute_SelectedIndexChanged;

            cboDP.SelectedIndexChanged -= cbAttribute_SelectedIndexChanged;
            cboDP.Enabled = false;
            cboDP.BindingContext = new BindingContext();
            cboDP.ValueMember = nameof(ListItem.Value);
            cboDP.DisplayMember = nameof(ListItem.Name);
            cboDP.DataSource = DataSource;
            cboDP.SelectedIndex = 2;
            cboDP.Visible = true;
            cboDP.Enabled = objThis.CanSwapAttributes;
            cboDP.SelectedIndexChanged += cbAttribute_SelectedIndexChanged;

            cboFirewall.SelectedIndexChanged -= cbAttribute_SelectedIndexChanged;
            cboFirewall.Enabled = false;
            cboFirewall.BindingContext = new BindingContext();
            cboFirewall.ValueMember = nameof(ListItem.Value);
            cboFirewall.DisplayMember = nameof(ListItem.Name);
            cboFirewall.DataSource = DataSource;
            cboFirewall.SelectedIndex = 3;
            cboFirewall.Visible = true;
            cboFirewall.Enabled = objThis.CanSwapAttributes;
            cboFirewall.SelectedIndexChanged += cbAttribute_SelectedIndexChanged;

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
            switch (listSoftware.Items[e.Index].ToString())
            {
                case "Shredder":
                case "Edit":
                    Actions.Find(i => i.Name == "Edit File").LimitModifier += e.NewValue == CheckState.Checked ? + 2 : - 2;
                    break;
                case "Encryption":
                    modFirewall += e.NewValue == CheckState.Checked ? modFirewall + 1 : modFirewall - 1;
                    break;
                case "Search":
                    Actions.Find(i => i.Name == "Matrix Search").DicePoolModifier += e.NewValue == CheckState.Checked ? +2 : -2;
                    break;
                case "Signal Scrub":
                    break;
                case "Toolbox":
                    modDataProc += e.NewValue == CheckState.Checked ? 1 : -1;
                    break;
                case "Virtual Machine":
                    break;
                case "Decryption":
                    modAttack += e.NewValue == CheckState.Checked ? + 1 : - 1;
                    break;
                case "Exploit":
                    Actions.Find(i => i.Name.Contains("Hack on the Fly")).LimitModifier += e.NewValue == CheckState.Checked ? +2 : -2;
                    break;
                case "Paintjob":
                    Actions.Find(i => i.Name.Contains("Erase Marks")).LimitModifier += e.NewValue == CheckState.Checked ? +2 : -2;
                    break;
                case "Stealth":
                    modSleaze = e.NewValue == CheckState.Checked ? modSleaze + 1 : modSleaze - 1;
                    break;
                case "Tarball":
                    Actions.Find(i => i.Name.Contains("Crash Program")).LimitModifier += e.NewValue == CheckState.Checked ? +2 : -2;
                    Actions.Find(i => i.Name.Contains("Crash Program")).DicePoolModifier += e.NewValue == CheckState.Checked ? +1 : -1;
                    break;
                default:
                    break;
            }
            addDataBinding();
        }

        private void cbAttribute_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox from = (ComboBox)sender;
            Action<string> funcAttributePropertySetter;
            string oldValue = "";
            if (from == cbAttack) {
                oldValue = character.ActiveCommlink.Attack;
                funcAttributePropertySetter = (x => character.ActiveCommlink.Attack = x);
            } else if (from == cbSleaze) {
                oldValue = character.ActiveCommlink.Sleaze;
                funcAttributePropertySetter = (x => character.ActiveCommlink.Sleaze = x);
            } else if (from == cbDataProc) {
                oldValue = character.ActiveCommlink.DataProcessing;
                funcAttributePropertySetter = (x => character.ActiveCommlink.DataProcessing = x);
            } else if (from == cbFirewall) {
                oldValue = character.ActiveCommlink.Firewall;
                funcAttributePropertySetter = (x => character.ActiveCommlink.Firewall = x);
            } else return;

            if (from.SelectedItem.ToString() == cbAttack.Items[cbAttack.SelectedIndex].ToString() && cbAttack != from)
            {
                funcAttributePropertySetter.Invoke(character.ActiveCommlink.Attack);
                character.ActiveCommlink.Attack = oldValue;
            }
            else if (from.SelectedItem.ToString() == cbSleaze.Items[cbSleaze.SelectedIndex].ToString() && cbSleaze != from)
            {
                funcAttributePropertySetter.Invoke(character.ActiveCommlink.Sleaze);
                character.ActiveCommlink.Sleaze = oldValue;
            }
            else if (from.SelectedItem.ToString() == cbDataProc.Items[cbDataProc.SelectedIndex].ToString() && cbDataProc != from)
            {
                funcAttributePropertySetter.Invoke(character.ActiveCommlink.DataProcessing);
                character.ActiveCommlink.DataProcessing = oldValue;
            }
            else if (from.SelectedItem.ToString() == cbFirewall.Items[cbFirewall.SelectedIndex].ToString() && cbFirewall != from)
            {
                funcAttributePropertySetter.Invoke(character.ActiveCommlink.Firewall);
                character.ActiveCommlink.Firewall = oldValue;
            }

            RefreshMatrixAttributeCBOs(character.ActiveCommlink, cbAttack, cbSleaze, cbDataProc, cbFirewall);
            UpdateFormElements();
        }

        private void rbOverFirewall_CheckedChanged(object sender, EventArgs e)
        {
            if (!rbOverFirewall.Checked)
                return;
            character.ActiveCommlink.Overclocked = "Firewall";
            UpdateFormElements();
        }

        private void rbOverDataProc_CheckedChanged(object sender, EventArgs e)
        {
            if (!rbOverDataProc.Checked)
                return;
            character.ActiveCommlink.Overclocked = "Data Processing";
            UpdateFormElements();
        }

        private void rbOverSleaze_CheckedChanged(object sender, EventArgs e)
        {
            if (!rbOverSleaze.Checked)
                return;
            character.ActiveCommlink.Overclocked = "Sleaze";
            UpdateFormElements();
        }

        private void rbOverAttack_CheckedChanged(object sender, EventArgs e)
        {
            if (!rbOverAttack.Checked)
                return;
            character.ActiveCommlink.Overclocked = "Attack";
            UpdateFormElements();
        }

        private void cbActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach(MatrixAction action in Actions)
            if (cbActions.SelectedItem.ToString() == action.Name)
                {
                    currentAction = action;
                    UpdateFormElements();
                }
        }

        private void UpdateSkillLabels()
        {
            if (currentAction == null)
                return;
            int attributeValue = character.GetAttribute(currentAction.Attribute).TotalValue;
            int skillValue = character.SkillsSection.GetActiveSkill(currentAction.Skill).TotalBaseRating;
            int defAttributeValue = character.GetAttribute(currentAction.Attribute).TotalValue;
            int defSkill = character.ActiveCommlink.GetTotalMatrixAttribute(currentAction.DSkill);
            int limitValue = character.ActiveCommlink.GetTotalMatrixAttribute(currentAction.Limit);

            switch (currentAction.DSkill)
            {
                case "Attack": defSkill += modAttack; break;
                case "Sleaze": defSkill += modSleaze; break;
                case "Data Processing": defSkill += modDataProc; break;
                case "Firewall": defSkill += modFirewall; break;
            }
            
            lSkillAttributeName.Text = currentAction.Attribute;
            lSkillAttributeValue.Text = attributeValue.ToString();

            lActionSkillName.Text = currentAction.Skill;
            lActionSkillValue.Text = skillValue.ToString();

            lActionModValue.Text = currentAction.DicePoolModifier.ToString();

            lSkillLimitName.Text = "[" + currentAction.Limit + "]";
            lSkillLimitValue.Text = "[" + limitValue.ToString() + "]";

            lDefendAttributeName.Text = currentAction.DAttribute;
            lDefendAttributeValue.Text = defAttributeValue.ToString();

            lDefendSkillName.Text = currentAction.DSkill;
            lDefendSkillValue.Text = defSkill.ToString();

            lSkillDescription.Text = currentAction.Description;

            int allModifier = 0;
            allModifier -= (int)nNoize.Value;
            if (listSoftware.CheckedItems.Contains("Signal Scrub")) allModifier += 2;
            if (allModifier > 0) allModifier = 0;

            allModifier -= character.WoundModifier;
            if (cSilent.Checked) allModifier -= 2;
            if (cHotVR.Checked) allModifier += 2;
            dpcActionDicePool.DicePool = attributeValue + skillValue + currentAction.DicePoolModifier + allModifier;
            dpcDefendDicePool.DicePool = defAttributeValue + defSkill + allModifier;
        }

        private void listCyberDecks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listCyberDecks.SelectedItems.Count == 0)
                return;
            var Item = listCyberDecks.SelectedItems[0];
            

            foreach (Gear deck in CyberDecks)
            {
                if (deck.Name == Item.ToString())
                {
                    character.ActiveCommlink = deck;
                    RefreshMatrixAttributeCBOs(character.ActiveCommlink, cbAttack, cbSleaze, cbDataProc, cbFirewall);
                }
            }
            UpdateFormElements();
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            UpdateFormElements();
        }

    }
}
