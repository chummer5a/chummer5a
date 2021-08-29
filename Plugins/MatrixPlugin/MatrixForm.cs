using Chummer;
using Chummer.WinFormsExtensions;
using Chummer.Backend.Equipment;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MatrixPlugin
{
    public partial class MatrixForm : Form
    {
        private readonly MatrixLogic logic;

        public MatrixForm(MatrixLogic logic)
        {
            this.logic = logic;
            
            InitializeComponent();
            InitializeBinding();
            InitializeContent();
        }

        private void InitializeContent()
        {
            foreach(var person in logic.Persons)
                if (person is Gear gear)
                    listCyberDecks.Items.Add(gear.Name);
            foreach (var software in logic.Software)
                listSoftware.Items.Add(software.Name);
            foreach (var action in logic.Actions)
                cbActions.Items.Add(action.Name);

            cbActions.SelectedIndex = 0;
            listCyberDecks.SelectedIndex = listCyberDecks.Items.IndexOf(((Gear)logic.CurrentPerson).Name);
        }

        private void InitializeBinding()
        {
            //Attribute section
            WinFormsExtensions.DoDataBinding(lOverClocker, "Enabled", logic, "OverClocker");
            WinFormsExtensions.DoDataBinding(lOverClocker, "Enabled", logic, "OverClocker");

            WinFormsExtensions.DoDataBinding(rbOverAttack, "Enabled", logic, "OverClocker");
            WinFormsExtensions.DoDataBinding(rbOverSleaze, "Enabled", logic, "OverClocker");
            WinFormsExtensions.DoDataBinding(rbOverDataProc, "Enabled", logic, "OverClocker");
            WinFormsExtensions.DoDataBinding(rbOverFirewall, "Enabled", logic, "OverClocker");
            AddRadioCheckedBinding(rbOverAttack, logic, "Overclocked", "Attack");
            AddRadioCheckedBinding(rbOverSleaze, logic, "Overclocked", "Sleaze");
            AddRadioCheckedBinding(rbOverDataProc, logic, "Overclocked", "DataProcessing");
            AddRadioCheckedBinding(rbOverFirewall, logic, "Overclocked", "Firewall");

            WinFormsExtensions.DoDataBinding(lAttackMod, "Text", logic, "AttackMod");
            WinFormsExtensions.DoDataBinding(lSleazeMod, "Text", logic, "SleazeMod");
            WinFormsExtensions.DoDataBinding(lDataProcMod, "Text", logic, "DataProcessingMod");
            WinFormsExtensions.DoDataBinding(lFirewallMod, "Text", logic, "FirewallMod");

            WinFormsExtensions.DoDataBinding(lAttackRes, "Text", logic, "TotalAttack");
            WinFormsExtensions.DoDataBinding(lSleazeRes, "Text", logic, "TotalSleaze");
            WinFormsExtensions.DoDataBinding(lDataProcRes, "Text", logic, "TotalDataProcessing");
            WinFormsExtensions.DoDataBinding(lFirewallRes, "Text", logic, "TotalFirewall");
            //Action section
            WinFormsExtensions.DoDataBinding(lSkillDescription, "Text", logic, "currentAction.Description");
            WinFormsExtensions.DoDataBinding(lActionType, "Text", logic, "currentAction.Type");
            WinFormsExtensions.DoDataBinding(lActionAttributeName, "Text", logic, "currentAction.ActionAttribute");
            WinFormsExtensions.DoDataBinding(lActionSkillName, "Text", logic, "currentAction.ActionSkill");
            WinFormsExtensions.DoDataBinding(lSkillLimitName, "Text", logic, "currentAction.Limit");
            WinFormsExtensions.DoDataBinding(lDefendAttributeName, "Text", logic, "currentAction.DefenceAttribute");
            WinFormsExtensions.DoDataBinding(lDefendSkillName, "Text", logic, "currentAction.DefenceSkill");

            BindValue(lActionAttributeValue, "currentAction.ActionAttribute", new ConvertEventHandler(AttributeToValue));
            BindValue(lActionSkillValue, "currentAction.ActionSkill", new ConvertEventHandler(SkillToValue));
            BindValue(lSkillLimitValue, "currentAction.Limit", new ConvertEventHandler(MatrixAttributeToValue));
            BindValue(lDefendAttributeValue, "currentAction.DefenceAttribute", new ConvertEventHandler(AttributeToValue));
            BindValue(lDefendSkillValue, "currentAction.DefenceSkill", new ConvertEventHandler(MatrixAttributeToValue));
            WinFormsExtensions.DoDataBinding(lActionModifier, "Text", logic, "currentAction.ActionModifier");
            WinFormsExtensions.DoDataBinding(lDefendModifier, "Text", logic, "currentAction.DefenceModifier");
            WinFormsExtensions.DoDataBinding(dpcActionDicePool, "DicePool", logic, "ActionDicePool");
            WinFormsExtensions.DoDataBinding(dpcDefendDicePool, "DicePool", logic, "DefenceDicePool");
        }

        private void BindValue(Label label, string bind, ConvertEventHandler convertEventHandler)
        {
            Binding b = new Binding("Text", logic, bind);
            b.Format += convertEventHandler;
            label.DataBindings.Add(b);
        }

        private void AttributeToValue(object sender, ConvertEventArgs cevent)
        {
            if (cevent.DesiredType != typeof(string)) return;
            cevent.Value = logic.GetTotalAttribute((string)cevent.Value).ToString();
        }

        private void MatrixAttributeToValue(object sender, ConvertEventArgs cevent)
        {
            if (cevent.DesiredType != typeof(string)) return;
            cevent.Value = logic.GetTotalMatrixAttribute((string)cevent.Value).ToString();
        }
        private void SkillToValue(object sender, ConvertEventArgs cevent)
        {
            if (cevent.DesiredType != typeof(string)) return;
            cevent.Value = logic.GetTotalSkill((string)cevent.Value).ToString();
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

            cboAttack.SelectedIndexChanged -= CbAttribute_SelectedIndexChanged;
            cboAttack.Enabled = false;
            cboAttack.BindingContext = new BindingContext();
            cboAttack.ValueMember = nameof(ListItem.Value);
            cboAttack.DisplayMember = nameof(ListItem.Name);
            cboAttack.DataSource = DataSource;
            cboAttack.SelectedIndex = 0;
            cboAttack.Visible = true;
            cboAttack.Enabled = objThis.CurrentPerson.CanSwapAttributes;
            cboAttack.SelectedIndexChanged += CbAttribute_SelectedIndexChanged;

            cboSleaze.SelectedIndexChanged -= CbAttribute_SelectedIndexChanged;
            cboSleaze.Enabled = false;
            cboSleaze.BindingContext = new BindingContext();
            cboSleaze.ValueMember = nameof(ListItem.Value);
            cboSleaze.DisplayMember = nameof(ListItem.Name);
            cboSleaze.DataSource = DataSource;
            cboSleaze.SelectedIndex = 1;
            cboSleaze.Visible = true;
            cboSleaze.Enabled = objThis.CurrentPerson.CanSwapAttributes;
            cboSleaze.SelectedIndexChanged += CbAttribute_SelectedIndexChanged;

            cboDP.SelectedIndexChanged -= CbAttribute_SelectedIndexChanged;
            cboDP.Enabled = false;
            cboDP.BindingContext = new BindingContext();
            cboDP.ValueMember = nameof(ListItem.Value);
            cboDP.DisplayMember = nameof(ListItem.Name);
            cboDP.DataSource = DataSource;
            cboDP.SelectedIndex = 2;
            cboDP.Visible = true;
            cboDP.Enabled = objThis.CurrentPerson.CanSwapAttributes;
            cboDP.SelectedIndexChanged += CbAttribute_SelectedIndexChanged;

            cboFirewall.SelectedIndexChanged -= CbAttribute_SelectedIndexChanged;
            cboFirewall.Enabled = false;
            cboFirewall.BindingContext = new BindingContext();
            cboFirewall.ValueMember = nameof(ListItem.Value);
            cboFirewall.DisplayMember = nameof(ListItem.Name);
            cboFirewall.DataSource = DataSource;
            cboFirewall.SelectedIndex = 3;
            cboFirewall.Visible = true;
            cboFirewall.Enabled = objThis.CurrentPerson.CanSwapAttributes;
            cboFirewall.SelectedIndexChanged += CbAttribute_SelectedIndexChanged;

            cboAttack.EndUpdate();
            cboSleaze.EndUpdate();
            cboDP.EndUpdate();
            cboFirewall.EndUpdate();
            cboAttack.ResumeLayout();
            cboSleaze.ResumeLayout();
            cboDP.ResumeLayout();
            cboFirewall.ResumeLayout();
        }

        private void ListSoftware_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            logic.ActivateSoftware(listSoftware.Items[e.Index].ToString(), e.NewValue == CheckState.Checked);
        }

        private void CbAttribute_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox from = (ComboBox)sender;
            Action<int> funcAttributePropertySetter;
            int oldValue;
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

        private void CbActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbActions.SelectedIndex >= 0 && cbActions.SelectedIndex < logic.Actions.Count)
                logic.CurrentActionIndex = cbActions.SelectedIndex;
        }

        private void ListCyberDecks_SelectedIndexChanged(object sender, EventArgs e)
        {
            logic.CurrentPerson = logic.Persons[listCyberDecks.SelectedIndex];
            RefreshMatrixAttributeCBOs(logic, cbAttack, cbSleaze, cbDataProc, cbFirewall);
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            int resultValue = (int)-nNoize.Value;
            if (cHotVR.Checked) resultValue += 2;
            if (cSilent.Checked) resultValue -= 2;
            logic.ActionModifier = resultValue;            
        }

    }
}
