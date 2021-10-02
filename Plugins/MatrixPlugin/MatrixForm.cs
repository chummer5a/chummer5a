using Chummer;
using Chummer.Backend.Equipment;
using Chummer.UI.Shared.Components;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MatrixPlugin
{
    public partial class MatrixForm : Form
    {
        private readonly MatrixLogic _logic;

        public MatrixForm(MatrixLogic logic)
        {
            _logic = logic;

            InitializeComponent();
            InitializeContent();
            InitializeBinding();
            this.UpdateLightDarkMode();

            void InitializeContent()
            {
                listCyberDecks.DataSource = _logic.Persons;
                listCyberDecks.DisplayMember = nameof(Gear.Name);

                listSoftware.DataSource = _logic.Programs;
                listSoftware.DisplayMember = nameof(Program.Name);

                foreach (var action in _logic.Actions)
                    cbActions.Items.Add(action.Name);

                cbActions.SelectedIndex = 0;
            }

            void InitializeBinding()
            {
                //Attribute section
                lOverClocker.DoOneWayDataBinding("Enabled", _logic, nameof(MatrixLogic.OverClocker));

                rbOverAttack.DoDataBinding("Enabled", _logic, nameof(MatrixLogic.OverClocker));
                rbOverSleaze.DoDataBinding("Enabled", _logic, nameof(MatrixLogic.OverClocker));
                rbOverDataProc.DoDataBinding("Enabled", _logic, nameof(MatrixLogic.OverClocker));
                rbOverFirewall.DoDataBinding("Enabled", _logic, nameof(MatrixLogic.OverClocker));
                AddRadioCheckedBinding(rbOverAttack, _logic, nameof(MatrixLogic.OverClocked), nameof(MatrixLogic.Attack));
                AddRadioCheckedBinding(rbOverSleaze, _logic, nameof(MatrixLogic.OverClocked), nameof(MatrixLogic.Sleaze));
                AddRadioCheckedBinding(rbOverDataProc, _logic, nameof(MatrixLogic.OverClocked), nameof(MatrixLogic.DataProcessing));
                AddRadioCheckedBinding(rbOverFirewall, _logic, nameof(MatrixLogic.OverClocked), nameof(MatrixLogic.Firewall));

                lAttackMod.DoDataBinding("Text", _logic, nameof(MatrixLogic.AttackMod));
                lSleazeMod.DoDataBinding("Text", _logic, nameof(MatrixLogic.SleazeMod));
                lDataProcMod.DoDataBinding("Text", _logic, nameof(MatrixLogic.DataProcessingMod));
                lFirewallMod.DoDataBinding("Text", _logic, nameof(MatrixLogic.FirewallMod));

                lAttackRes.DoDataBinding("Text", _logic, nameof(MatrixLogic.TotalAttack));
                lSleazeRes.DoDataBinding("Text", _logic, nameof(MatrixLogic.TotalSleaze));
                lDataProcRes.DoDataBinding("Text", _logic, nameof(MatrixLogic.TotalDataProcessing));
                lFirewallRes.DoDataBinding("Text", _logic, nameof(MatrixLogic.TotalFirewall));
                //Action section
                lSkillDescription.DoDataBinding("Text", _logic, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.Description));
                lActionType.DoDataBinding("Text", _logic, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.Type));
                lActionAttributeName.DoDataBinding("Text", _logic, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.ActionAttribute));
                lActionSkillName.DoDataBinding("Text", _logic, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.ActionSkill));
                lSkillLimitName.DoDataBinding("Text", _logic, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.Limit));
                lDefendAttributeName.DoDataBinding("Text", _logic, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.DefenceAttribute));
                lDefendSkillName.DoDataBinding("Text", _logic, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.DefenceSkill));

                DoDataBindingWithFormatter(lActionAttributeValue, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.ActionAttribute), new ConvertEventHandler(AttributeToValue));
                DoDataBindingWithFormatter(lActionSkillValue, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.ActionSkill), new ConvertEventHandler(SkillToValue));
                DoDataBindingWithFormatter(lSkillLimitValue, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.Limit), new ConvertEventHandler(MatrixAttributeToValue));
                DoDataBindingWithFormatter(lDefendAttributeValue, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.DefenceAttribute), new ConvertEventHandler(AttributeToValue));
                DoDataBindingWithFormatter(lDefendSkillValue, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.DefenceSkill), new ConvertEventHandler(MatrixAttributeToValue));
                lActionModifier.DoDataBinding("Text", _logic, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.ActionModifier));
                lDefendModifier.DoDataBinding("Text", _logic, nameof(MatrixLogic.CurrentAction) + "." + nameof(MatrixAction.DefenceModifier));
                dpcActionDicePool.DoDataBinding(nameof(DicePoolControl.DicePool), _logic, nameof(MatrixLogic.ActionDicePool));
                dpcDefendDicePool.DoDataBinding(nameof(DicePoolControl.DicePool), _logic, nameof(MatrixLogic.DefenceDicePool));
            }
        }

        private void DoDataBindingWithFormatter(Control obj, string dataMember, ConvertEventHandler formatter)
        {
            Binding binding = new Binding("Text", _logic, dataMember);
            binding.Format += formatter;
            obj.DataBindings.Add(binding);
        }

        private void AttributeToValue(object sender, ConvertEventArgs cevent)
        {
            if (cevent.DesiredType != typeof(string)) return;
            cevent.Value = _logic.GetTotalAttribute((string)cevent.Value).ToString();
        }

        private void MatrixAttributeToValue(object sender, ConvertEventArgs cevent)
        {
            if (cevent.DesiredType != typeof(string)) return;
            cevent.Value = _logic.GetTotalMatrixAttribute((string)cevent.Value).ToString();
        }
        private void SkillToValue(object sender, ConvertEventArgs cevent)
        {
            if (cevent.DesiredType != typeof(string)) return;
            cevent.Value = _logic.GetTotalSkill((string)cevent.Value).ToString();
        }

        private void AddRadioCheckedBinding(RadioButton radio, object dataSource, string dataMember, string trueValue)
        {
            var binding = new Binding(nameof(RadioButton.Checked), dataSource, dataMember, true, DataSourceUpdateMode.OnPropertyChanged);
            binding.Parse += (s, a) => { if ((bool)a.Value) a.Value = trueValue; _logic.OverClocked = trueValue; };
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

            int intBaseAttack = _logic.Attack;
            int intBaseSleaze = _logic.Sleaze;
            int intBaseDP = _logic.DataProcessing;
            int intBaseFirewall = _logic.Firewall;

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
            cboAttack.Enabled = _logic.CurrentPerson.CanSwapAttributes;
            cboAttack.SelectedIndexChanged += CbAttribute_SelectedIndexChanged;

            cboSleaze.SelectedIndexChanged -= CbAttribute_SelectedIndexChanged;
            cboSleaze.Enabled = false;
            cboSleaze.PopulateWithListItems(DataSource);
            cboSleaze.SelectedIndex = 1;
            cboSleaze.Visible = true;
            cboSleaze.Enabled = _logic.CurrentPerson.CanSwapAttributes;
            cboSleaze.SelectedIndexChanged += CbAttribute_SelectedIndexChanged;

            cboDP.SelectedIndexChanged -= CbAttribute_SelectedIndexChanged;
            cboDP.Enabled = false;
            cboDP.PopulateWithListItems(DataSource);
            cboDP.SelectedIndex = 2;
            cboDP.Visible = true;
            cboDP.Enabled = _logic.CurrentPerson.CanSwapAttributes;
            cboDP.SelectedIndexChanged += CbAttribute_SelectedIndexChanged;

            cboFirewall.SelectedIndexChanged -= CbAttribute_SelectedIndexChanged;
            cboFirewall.Enabled = false;
            cboFirewall.PopulateWithListItems(DataSource);
            cboFirewall.SelectedIndex = 3;
            cboFirewall.Visible = true;
            cboFirewall.Enabled = _logic.CurrentPerson.CanSwapAttributes;
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
            _logic.Programs[e.Index].IsActive = e.NewValue == CheckState.Checked;
        }

        private void CbAttribute_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox from = (ComboBox)sender;
            Action<int> funcAttributePropertySetter;
            int oldValue;

            if (from == cbAttack)
            {
                oldValue = _logic.Attack;
                funcAttributePropertySetter = (x => _logic.Attack = x);
            }
            else if (from == cbSleaze)
            {
                oldValue = _logic.Sleaze;
                funcAttributePropertySetter = (x => _logic.Sleaze = x);
            }
            else if (from == cbDataProc)
            {
                oldValue = _logic.DataProcessing;
                funcAttributePropertySetter = (x => _logic.DataProcessing = x);
            }
            else if (from == cbFirewall)
            {
                oldValue = _logic.Firewall;
                funcAttributePropertySetter = (x => _logic.Firewall = x);
            }
            else return;

            if (from.SelectedItem.ToString() == cbAttack.Items[cbAttack.SelectedIndex].ToString() && cbAttack != from)
            {
                funcAttributePropertySetter.Invoke(_logic.Attack);
                _logic.Attack = oldValue;
            }
            else if (from.SelectedItem.ToString() == cbSleaze.Items[cbSleaze.SelectedIndex].ToString() && cbSleaze != from)
            {
                funcAttributePropertySetter.Invoke(_logic.Sleaze);
                _logic.Sleaze = oldValue;
            }
            else if (from.SelectedItem.ToString() == cbDataProc.Items[cbDataProc.SelectedIndex].ToString() && cbDataProc != from)
            {
                funcAttributePropertySetter.Invoke(_logic.DataProcessing);
                _logic.DataProcessing = oldValue;
            }
            else if (from.SelectedItem.ToString() == cbFirewall.Items[cbFirewall.SelectedIndex].ToString() && cbFirewall != from)
            {
                funcAttributePropertySetter.Invoke(_logic.Firewall);
                _logic.Firewall = oldValue;
            }

            RefreshMatrixAttributeCBOs(cbAttack, cbSleaze, cbDataProc, cbFirewall);
        }

        private void CbActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbActions.SelectedIndex >= 0 && cbActions.SelectedIndex < _logic.Actions.Count)
                _logic.CurrentActionIndex = cbActions.SelectedIndex;
        }

        private void ListCyberDecks_SelectedIndexChanged(object sender, EventArgs e)
        {
            _logic.CurrentPerson = _logic.Persons[listCyberDecks.SelectedIndex];
            RefreshMatrixAttributeCBOs(cbAttack, cbSleaze, cbDataProc, cbFirewall);
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            int resultValue = (int)-nNoize.Value;
            if (cHotVR.Checked) resultValue += 2;
            if (cSilent.Checked) resultValue -= 2;
            _logic.ActionModifier = resultValue;
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
        }
    }
}
