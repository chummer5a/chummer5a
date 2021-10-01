using Chummer;
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
            InitializeContent();
            InitializeBinding();
            this.UpdateLightDarkMode();

            void InitializeContent()
            {
                listCyberDecks.DataSource = logic.Persons;
                listCyberDecks.DisplayMember = nameof(Gear.Name);

                listSoftware.DataSource = logic.Software;
                listSoftware.DisplayMember = nameof(Program.Name);

                foreach (var action in logic.Actions)
                    cbActions.Items.Add(action.Name);

                cbActions.SelectedIndex = 0;
            }


            void InitializeBinding()
            {
                //Attribute section
                lOverClocker.DoDataBinding("Enabled", logic, nameof(logic.OverClocker));

                rbOverAttack.DoDataBinding("Enabled", logic, nameof(logic.OverClocker));
                rbOverSleaze.DoDataBinding("Enabled", logic, nameof(logic.OverClocker));
                rbOverDataProc.DoDataBinding("Enabled", logic, nameof(logic.OverClocker));
                rbOverFirewall.DoDataBinding("Enabled", logic, nameof(logic.OverClocker));
                AddRadioCheckedBinding(rbOverAttack, logic, nameof(logic.OverClocked), nameof(logic.Attack));
                AddRadioCheckedBinding(rbOverSleaze, logic, nameof(logic.OverClocked), nameof(logic.Sleaze));
                AddRadioCheckedBinding(rbOverDataProc, logic, nameof(logic.OverClocked), nameof(logic.DataProcessing));
                AddRadioCheckedBinding(rbOverFirewall, logic, nameof(logic.OverClocked), nameof(logic.Firewall));

                lAttackMod.DoDataBinding("Text", logic, nameof(logic.AttackMod));
                lSleazeMod.DoDataBinding("Text", logic, nameof(logic.SleazeMod));
                lDataProcMod.DoDataBinding("Text", logic, nameof(logic.DataProcessingMod));
                lFirewallMod.DoDataBinding("Text", logic, nameof(logic.FirewallMod));

                lAttackRes.DoDataBinding("Text", logic, nameof(logic.TotalAttack));
                lSleazeRes.DoDataBinding("Text", logic, nameof(logic.TotalSleaze));
                lDataProcRes.DoDataBinding("Text", logic, nameof(logic.TotalDataProcessing));
                lFirewallRes.DoDataBinding("Text", logic, nameof(logic.TotalFirewall));
                //Action section
                lSkillDescription.DoDataBinding("Text", logic, nameof(logic.CurrentAction) + "." + nameof(MatrixAction.Description));
                lActionType.DoDataBinding("Text", logic, nameof(logic.CurrentAction) + "." + nameof(MatrixAction.Type));
                lActionAttributeName.DoDataBinding("Text", logic, nameof(logic.CurrentAction) + "." + nameof(MatrixAction.ActionAttribute));
                lActionSkillName.DoDataBinding("Text", logic, nameof(logic.CurrentAction) + "." + nameof(MatrixAction.ActionSkill));
                lSkillLimitName.DoDataBinding("Text", logic, nameof(logic.CurrentAction) + "." + nameof(MatrixAction.Limit));
                lDefendAttributeName.DoDataBinding("Text", logic, nameof(logic.CurrentAction) + "." + nameof(MatrixAction.DefenceAttribute));
                lDefendSkillName.DoDataBinding("Text", logic, nameof(logic.CurrentAction) + "." + nameof(MatrixAction.DefenceSkill));

                DoDataBindingWithFormatter(lActionAttributeValue, "currentAction.ActionAttribute", new ConvertEventHandler(AttributeToValue));
                DoDataBindingWithFormatter(lActionSkillValue, "currentAction.ActionSkill", new ConvertEventHandler(SkillToValue));
                DoDataBindingWithFormatter(lSkillLimitValue, "currentAction.Limit", new ConvertEventHandler(MatrixAttributeToValue));
                DoDataBindingWithFormatter(lDefendAttributeValue, "currentAction.DefenceAttribute", new ConvertEventHandler(AttributeToValue));
                DoDataBindingWithFormatter(lDefendSkillValue, "currentAction.DefenceSkill", new ConvertEventHandler(MatrixAttributeToValue));
                lActionModifier.DoDataBinding("Text", logic, "currentAction.ActionModifier");
                lDefendModifier.DoDataBinding("Text", logic, "currentAction.DefenceModifier");
                dpcActionDicePool.DoDataBinding("DicePool", logic, nameof(logic.ActionDicePool));
                dpcDefendDicePool.DoDataBinding("DicePool", logic, nameof(logic.DefenceDicePool));
            }
        }

        private void DoDataBindingWithFormatter(Control obj, string dataMember, ConvertEventHandler formatter)
        {
            Binding binding = new Binding("Text", logic, dataMember);
            binding.Format += formatter;
            obj.DataBindings.Add(binding);
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

        public void RefreshMatrixAttributeCBOs(ComboBox cboAttack, ComboBox cboSleaze, ComboBox cboDP, ComboBox cboFirewall)
        {
            if (cboAttack == null)
                throw new ArgumentNullException(nameof(cboAttack));
            if (cboSleaze == null)
                throw new ArgumentNullException(nameof(cboSleaze));
            if (cboDP == null)
                throw new ArgumentNullException(nameof(cboDP));
            if (cboFirewall == null)
                throw new ArgumentNullException(nameof(cboFirewall));

            int intBaseAttack = (logic.Attack);
            int intBaseSleaze = (logic.Sleaze);
            int intBaseDP = (logic.DataProcessing);
            int intBaseFirewall = (logic.Firewall);

            List<ListItem> DataSource = new List<ListItem>(4) {
                    new ListItem(intBaseAttack, intBaseAttack.ToString(GlobalSettings.InvariantCultureInfo)),
                    new ListItem(intBaseSleaze, intBaseSleaze.ToString(GlobalSettings.InvariantCultureInfo)),
                    new ListItem(intBaseDP, intBaseDP.ToString(GlobalSettings.InvariantCultureInfo)),
                    new ListItem(intBaseFirewall, intBaseFirewall.ToString(GlobalSettings.InvariantCultureInfo))
            };

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
            cboAttack.PopulateWithListItems(DataSource);
            cboAttack.SelectedIndex = 0;
            cboAttack.Visible = true;
            cboAttack.Enabled = logic.CurrentPerson.CanSwapAttributes;
            cboAttack.SelectedIndexChanged += CbAttribute_SelectedIndexChanged;

            cboSleaze.SelectedIndexChanged -= CbAttribute_SelectedIndexChanged;
            cboSleaze.Enabled = false;
            cboSleaze.PopulateWithListItems(DataSource);
            cboSleaze.SelectedIndex = 1;
            cboSleaze.Visible = true;
            cboSleaze.Enabled = logic.CurrentPerson.CanSwapAttributes;
            cboSleaze.SelectedIndexChanged += CbAttribute_SelectedIndexChanged;

            cboDP.SelectedIndexChanged -= CbAttribute_SelectedIndexChanged;
            cboDP.Enabled = false;
            cboDP.PopulateWithListItems(DataSource);
            cboDP.SelectedIndex = 2;
            cboDP.Visible = true;
            cboDP.Enabled = logic.CurrentPerson.CanSwapAttributes;
            cboDP.SelectedIndexChanged += CbAttribute_SelectedIndexChanged;

            cboFirewall.SelectedIndexChanged -= CbAttribute_SelectedIndexChanged;
            cboFirewall.Enabled = false;
            cboFirewall.PopulateWithListItems(DataSource);
            cboFirewall.SelectedIndex = 3;
            cboFirewall.Visible = true;
            cboFirewall.Enabled = logic.CurrentPerson.CanSwapAttributes;
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
            logic.ActivateSoftware(e.Index, e.NewValue == CheckState.Checked);
        }

        private void CbAttribute_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox from = (ComboBox)sender;
            Action<int> funcAttributePropertySetter;
            int oldValue;
            if (from == cbAttack)
            {
                oldValue = logic.Attack;
                funcAttributePropertySetter = (x => logic.Attack = x);
            }
            else if (from == cbSleaze)
            {
                oldValue = logic.Sleaze;
                funcAttributePropertySetter = (x => logic.Sleaze = x);
            }
            else if (from == cbDataProc)
            {
                oldValue = logic.DataProcessing;
                funcAttributePropertySetter = (x => logic.DataProcessing = x);
            }
            else if (from == cbFirewall)
            {
                oldValue = logic.Firewall;
                funcAttributePropertySetter = (x => logic.Firewall = x);
            }
            else return;

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

            RefreshMatrixAttributeCBOs(cbAttack, cbSleaze, cbDataProc, cbFirewall);
        }

        private void CbActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbActions.SelectedIndex >= 0 && cbActions.SelectedIndex < logic.Actions.Count)
                logic.CurrentActionIndex = cbActions.SelectedIndex;
        }

        private void ListCyberDecks_SelectedIndexChanged(object sender, EventArgs e)
        {
            logic.CurrentPerson = logic.Persons[listCyberDecks.SelectedIndex];
            RefreshMatrixAttributeCBOs(cbAttack, cbSleaze, cbDataProc, cbFirewall);
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            int resultValue = (int)-nNoize.Value;
            if (cHotVR.Checked) resultValue += 2;
            if (cSilent.Checked) resultValue -= 2;
            logic.ActionModifier = resultValue;
        }

        int hoveredIndex = -1;

        private void listSoftware_MouseMove(object sender, MouseEventArgs e)
        {
            int newHoveredIndex = listSoftware.IndexFromPoint(e.Location);
            if (hoveredIndex != newHoveredIndex)
            {
                hoveredIndex = newHoveredIndex;
                if (hoveredIndex > -1)
                {
                    listSoftware.SetToolTip(((Program)listSoftware.Items[hoveredIndex]).Description.WordWrap());
                }
            }
        }

        private void labelAction_TextChanged(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            label.Visible = !(string.IsNullOrEmpty(label.Text) || label.Text == "0");
            //label.Visible = true;
        }
    }
}
