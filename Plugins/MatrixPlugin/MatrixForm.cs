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
        MatrixLogic logic;

        public MatrixForm(List<MatrixAction> actions, MatrixLogic logic)
        {
            this.logic = logic;
            
            InitializeComponent();
            InitializeBinding();
            InitializeContent();
        }

        private void InitializeContent()
        {
            foreach(var person in logic.Persons)
                if (person is Gear)
                    listCyberDecks.Items.Add(((Gear)person).Name);
            foreach (var software in logic.Software)
                listSoftware.Items.Add(software.Name);
            foreach (var action in logic.Actions)
                cbActions.Items.Add(action.Name);

            cbActions.SelectedIndex = 0;
            listCyberDecks.SelectedIndex = listCyberDecks.Items.IndexOf(((Gear)logic.currentPerson).Name);
        }

        private void InitializeBinding()
        {
            //Attribute section
            lOverClocker.DataBindings.Add(new Binding("Enabled", logic, "OverClocker"));

            rbOverAttack.DataBindings.Add(new Binding("Enabled", logic, "OverClocker"));
            rbOverSleaze.DataBindings.Add(new Binding("Enabled", logic, "OverClocker"));
            rbOverDataProc.DataBindings.Add(new Binding("Enabled", logic, "OverClocker"));
            rbOverFirewall.DataBindings.Add(new Binding("Enabled", logic, "OverClocker"));
            AddRadioCheckedBinding(rbOverAttack, logic, "Overclocked", "Attack");
            AddRadioCheckedBinding(rbOverSleaze, logic, "Overclocked", "Sleaze");
            AddRadioCheckedBinding(rbOverDataProc, logic, "Overclocked", "DataProcessing");
            AddRadioCheckedBinding(rbOverFirewall, logic, "Overclocked", "Firewall");

            lAttackMod.DataBindings.Add(new Binding("Text", logic, "AttackMod"));
            lSleazeMod.DataBindings.Add(new Binding("Text", logic, "SleazeMod"));
            lDataProcMod.DataBindings.Add(new Binding("Text", logic, "DataProcessingMod"));
            lFirewallMod.DataBindings.Add(new Binding("Text", logic, "FirewallMod"));

            lAttackRes.DataBindings.Add(new Binding("Text", logic, "TotalAttack"));
            lSleazeRes.DataBindings.Add(new Binding("Text", logic, "TotalSleaze"));
            lDataProcRes.DataBindings.Add(new Binding("Text", logic, "TotalDataProcessing"));
            lFirewallRes.DataBindings.Add(new Binding("Text", logic, "TotalFirewall"));
            //Action section
            lSkillDescription.DataBindings.Add(new Binding("Text", logic, "currentAction.Description"));
            lActionAttributeName.DataBindings.Add(new Binding("Text", logic, "currentAction.ActionAttribute"));
            lActionSkillName.DataBindings.Add(new Binding("Text", logic, "currentAction.ActionSkill"));
            lSkillLimitName.DataBindings.Add(new Binding("Text", logic, "currentAction.Limit"));
            lDefendAttributeName.DataBindings.Add(new Binding("Text", logic, "currentAction.DefenceAttribute"));
            lDefendSkillName.DataBindings.Add(new Binding("Text", logic, "currentAction.DefenceSkill"));

            bindValue(lActionAttributeValue, "currentAction.ActionAttribute", new ConvertEventHandler(AttributeToValue));
            bindValue(lActionSkillValue, "currentAction.ActionSkill", new ConvertEventHandler(SkillToValue));
            bindValue(lSkillLimitValue, "currentAction.Limit", new ConvertEventHandler(MatrixAttributeToValue));
            bindValue(lDefendAttributeValue, "currentAction.DefenceAttribute", new ConvertEventHandler(AttributeToValue));
            bindValue(lDefendSkillValue, "currentAction.DefenceSkill", new ConvertEventHandler(MatrixAttributeToValue));
            lActionModifier.DataBindings.Add(new Binding("Text", logic, "currentAction.ActionModifier"));
            lDefendModifier.DataBindings.Add(new Binding("Text", logic, "currentAction.DefenceModifier"));
            dpcActionDicePool.DataBindings.Add(new Binding("DicePool", logic, "ActionDicePool"));
            dpcDefendDicePool.DataBindings.Add(new Binding("DicePool", logic, "DefenceDicePool"));
        }

        private void bindValue(Label label, string bind, ConvertEventHandler convertEventHandler)
        {
            Binding b = new Binding("Text", logic, bind);
            b.Format += convertEventHandler;
            label.DataBindings.Add(b);
        }

        private void AttributeToValue(object sender, ConvertEventArgs cevent)
        {
            if (cevent.DesiredType != typeof(string)) return;
            cevent.Value = logic.getTotalAttribute((string)cevent.Value).ToString();
        }

        private void MatrixAttributeToValue(object sender, ConvertEventArgs cevent)
        {
            if (cevent.DesiredType != typeof(string)) return;
            cevent.Value = logic.getTotalMatrixAttribute((string)cevent.Value).ToString();
        }
        private void SkillToValue(object sender, ConvertEventArgs cevent)
        {
            if (cevent.DesiredType != typeof(string)) return;
            cevent.Value = logic.getTotalSkill((string)cevent.Value).ToString();
        }

        private void AddRadioCheckedBinding(RadioButton radio, object dataSource, string dataMember, string trueValue)
        {
            var binding = new Binding(nameof(RadioButton.Checked), dataSource, dataMember, true, DataSourceUpdateMode.OnPropertyChanged);
            binding.Parse += (s, a) => { if ((bool)a.Value) a.Value = trueValue; logic.OverClocked = trueValue; };
            binding.Format += (s, a) => a.Value = (a.Value).Equals(trueValue);
            radio.DataBindings.Add(binding);
        }

        public void RefreshMatrixAttributeCBOs(MatrixLogic objThis, ComboBox cboAttack, ComboBox cboSleaze, ComboBox cboDP, ComboBox cboFirewall)
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

            int intBaseAttack = (objThis.Attack);
            int intBaseSleaze = (objThis.Sleaze);
            int intBaseDP = (objThis.DataProcessing);
            int intBaseFirewall = (objThis.Firewall);
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
            cboAttack.Enabled = objThis.currentPerson.CanSwapAttributes;
            cboAttack.SelectedIndexChanged += cbAttribute_SelectedIndexChanged;

            cboSleaze.SelectedIndexChanged -= cbAttribute_SelectedIndexChanged;
            cboSleaze.Enabled = false;
            cboSleaze.BindingContext = new BindingContext();
            cboSleaze.ValueMember = nameof(ListItem.Value);
            cboSleaze.DisplayMember = nameof(ListItem.Name);
            cboSleaze.DataSource = DataSource;
            cboSleaze.SelectedIndex = 1;
            cboSleaze.Visible = true;
            cboSleaze.Enabled = objThis.currentPerson.CanSwapAttributes;
            cboSleaze.SelectedIndexChanged += cbAttribute_SelectedIndexChanged;

            cboDP.SelectedIndexChanged -= cbAttribute_SelectedIndexChanged;
            cboDP.Enabled = false;
            cboDP.BindingContext = new BindingContext();
            cboDP.ValueMember = nameof(ListItem.Value);
            cboDP.DisplayMember = nameof(ListItem.Name);
            cboDP.DataSource = DataSource;
            cboDP.SelectedIndex = 2;
            cboDP.Visible = true;
            cboDP.Enabled = objThis.currentPerson.CanSwapAttributes;
            cboDP.SelectedIndexChanged += cbAttribute_SelectedIndexChanged;

            cboFirewall.SelectedIndexChanged -= cbAttribute_SelectedIndexChanged;
            cboFirewall.Enabled = false;
            cboFirewall.BindingContext = new BindingContext();
            cboFirewall.ValueMember = nameof(ListItem.Value);
            cboFirewall.DisplayMember = nameof(ListItem.Name);
            cboFirewall.DataSource = DataSource;
            cboFirewall.SelectedIndex = 3;
            cboFirewall.Visible = true;
            cboFirewall.Enabled = objThis.currentPerson.CanSwapAttributes;
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
            logic.ActivateSoftware(listSoftware.Items[e.Index].ToString(), e.NewValue == CheckState.Checked);
        }

        private void cbAttribute_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox from = (ComboBox)sender;
            Action<int> funcAttributePropertySetter;
            int oldValue = 0;
            if (from == cbAttack) {
                oldValue = logic.Attack;
                funcAttributePropertySetter = (x => logic.Attack = x);
            } else if (from == cbSleaze) {
                oldValue = logic.Sleaze;
                funcAttributePropertySetter = (x => logic.Sleaze = x);
            } else if (from == cbDataProc) {
                oldValue = logic.DataProcessing;
                funcAttributePropertySetter = (x => logic.DataProcessing = x);
            } else if (from == cbFirewall) {
                oldValue = logic.Firewall;
                funcAttributePropertySetter = (x => logic.Firewall = x);
            } else return;

            if (from.SelectedItem.ToString() == cbAttack.Items[cbAttack.SelectedIndex].ToString() && cbAttack != from)
            {
                funcAttributePropertySetter.Invoke(logic.Attack);
                logic.Attack = oldValue;
            }
            else if (from.SelectedItem.ToString() == cbSleaze.Items[cbSleaze.SelectedIndex].ToString() && cbSleaze != from)
            {
                funcAttributePropertySetter.Invoke(logic.Sleaze);
                logic.Sleaze = oldValue;
            }
            else if (from.SelectedItem.ToString() == cbDataProc.Items[cbDataProc.SelectedIndex].ToString() && cbDataProc != from)
            {
                funcAttributePropertySetter.Invoke(logic.DataProcessing);
                logic.DataProcessing = oldValue;
            }
            else if (from.SelectedItem.ToString() == cbFirewall.Items[cbFirewall.SelectedIndex].ToString() && cbFirewall != from)
            {
                funcAttributePropertySetter.Invoke(logic.Firewall);
                logic.Firewall = oldValue;
            }

            RefreshMatrixAttributeCBOs(logic, cbAttack, cbSleaze, cbDataProc, cbFirewall);
        }

        private void cbActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbActions.SelectedIndex >= 0 && cbActions.SelectedIndex < logic.Actions.Count)
                logic.currentActionIndex = cbActions.SelectedIndex;
        }

        private void listCyberDecks_SelectedIndexChanged(object sender, EventArgs e)
        {
            logic.currentPerson = logic.Persons[listCyberDecks.SelectedIndex];
            RefreshMatrixAttributeCBOs(logic, cbAttack, cbSleaze, cbDataProc, cbFirewall);
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            
        }

    }
}
