using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;

namespace Chummer.Controls.Skills
{
    public partial class SkillControlReadOnly : UserControl
    {
        private readonly Skill _objSkill;
        private CharacterAttrib _objAttributeActive;
        private readonly Font _fntNormal;
        private readonly Font _fntItalic;
        private readonly Font _fntNormalName;
        private readonly Font _fntItalicName;

        public SkillControlReadOnly(Skill objSkill)
        {
            if (objSkill == null)
                return;
            _objSkill = objSkill;
            InitializeComponent();
            if (objSkill.CharacterObject.Created)
                chkKarma.Parent.Controls.Remove(chkKarma);
            AdjustForDpi();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _fntNormalName = lblName.Font;
            _fntItalicName = new Font(_fntNormalName, FontStyle.Italic);
            _fntNormal = cmdAttribute.Font;
            _fntItalic = new Font(_fntNormal, FontStyle.Italic);

            lblRating.DoOneWayDataBinding("Text", objSkill, nameof(Skill.Rating));
            lblName.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplayName));
            lblSpec.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplaySpecialization));
        }

        private void SkillControlReadOnly_Load(object sender, EventArgs e)
        {
            lblRating.DoOneWayNegatableDataBinding("Visible", _objSkill, nameof(Skill.IsNativeLanguage));
            if (!_objSkill.CharacterObject.Created)
                chkKarma.DoOneWayDataBinding("Checked", _objSkill, nameof(Skill.BuyWithKarma));
            this.DoOneWayDataBinding("BackColor", _objSkill, nameof(Skill.PreferredControlColor));

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstAttributeItems))
            {
                foreach (string strLoopAttribute in AttributeSection.AttributeStrings)
                {
                    if (strLoopAttribute == "MAGAdept")
                    {
                        if (!_objSkill.CharacterObject.Settings.MysAdeptSecondMAGAttribute)
                            continue;
                        lstAttributeItems.Add(new ListItem(strLoopAttribute, LanguageManager.MAGAdeptString()));
                    }
                    else
                    {
                        string strAttributeShort = LanguageManager.GetString(
                            "String_Attribute" + strLoopAttribute + "Short", GlobalSettings.Language, false);
                        lstAttributeItems.Add(new ListItem(strLoopAttribute,
                                                           !string.IsNullOrEmpty(strAttributeShort)
                                                               ? strAttributeShort
                                                               : strLoopAttribute));
                    }
                }

                cboSelectAttribute.BeginUpdate();
                cboSelectAttribute.PopulateWithListItems(lstAttributeItems);
                cboSelectAttribute.SelectedValue = _objSkill.AttributeObject.Abbrev;
                cboSelectAttribute.EndUpdate();
            }

            _objSkill.PropertyChanged += Skill_PropertyChanged;
        }

        public void UnbindSkillControl()
        {
            _objSkill.PropertyChanged -= Skill_PropertyChanged;
            if (AttributeActive != null)
                AttributeActive.PropertyChanged -= Attribute_PropertyChanged;
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void cmdAttribute_Click(object sender, EventArgs e)
        {
            cmdAttribute.Visible = false;
            cboSelectAttribute.Visible = true;
            cboSelectAttribute.DroppedDown = true;
        }

        private void cboSelectAttribute_Closed(object sender, EventArgs e)
        {
            cmdAttribute.Visible = true;
            cboSelectAttribute.Visible = false;
            AttributeActive = _objSkill.CharacterObject.GetAttribute((string)cboSelectAttribute.SelectedValue);
            cmdAttribute.Text = cboSelectAttribute.Text;
        }

        private void lblName_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdf(_objSkill.Source + ' ' + _objSkill.DisplayPage(GlobalSettings.Language), _objSkill.CharacterObject);
        }

        private CharacterAttrib AttributeActive
        {
            get => _objAttributeActive;
            set
            {
                if (_objAttributeActive == value)
                    return;
                if (_objAttributeActive != null)
                    _objAttributeActive.PropertyChanged -= Attribute_PropertyChanged;
                _objAttributeActive = value;
                if (_objAttributeActive != null)
                    _objAttributeActive.PropertyChanged += Attribute_PropertyChanged;
                cmdAttribute.QueueThreadSafe(() => cmdAttribute.Font = _objAttributeActive == _objSkill.AttributeObject ? _fntNormal : _fntItalic);
                SkillControlReadOnly_RefreshPoolTooltipAndDisplay();
                CustomAttributeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CustomAttributeChanged;

        public bool CustomAttributeSet => AttributeActive != _objSkill.AttributeObject;

        private void SkillControlReadOnly_DpiChangedAfterParent(object sender, EventArgs e)
        {
            AdjustForDpi();
        }

        private void AdjustForDpi()
        {
            using (Graphics g = CreateGraphics())
            {
                if (lblRating != null)
                    lblRating.MinimumSize = new Size((int)(25 * g.DpiX / 96.0f), 0);
                lblModifiedRating.MinimumSize = new Size((int)(50 * g.DpiX / 96.0f), 0);
            }
        }

        /// <summary>
        /// Update the position of controls.
        /// </summary>
        /// <param name="intNewNameWidth">Width of the Name label</param>
        [UsedImplicitly]
        public void MoveControls(int intNewNameWidth)
        {
            lblName.MinimumSize = new Size(intNewNameWidth - lblName.Margin.Right - pnlAttributes.Margin.Left - pnlAttributes.Width, lblName.MinimumSize.Height);
        }

        public int NameWidth => lblName.PreferredWidth + lblName.Margin.Right + pnlAttributes.Margin.Left + pnlAttributes.Width;

        [UsedImplicitly]
        public void ResetSelectAttribute(object sender, EventArgs e)
        {
            if (!CustomAttributeSet)
                return;
            if (cboSelectAttribute != null)
            {
                cboSelectAttribute.SelectedValue = _objSkill.AttributeObject.Abbrev;
                cboSelectAttribute_Closed(sender, e);
            }
        }

        /// <summary>
        /// Refreshes the Tooltip and Displayed Dice Pool. Can be used in another Thread
        /// </summary>
        private void SkillControlReadOnly_RefreshPoolTooltipAndDisplay()
        {
            string backgroundCalcPool = _objSkill.DisplayOtherAttribute(AttributeActive.Abbrev);
            lblModifiedRating.QueueThreadSafe(() => lblModifiedRating.Text = backgroundCalcPool);
            string backgroundCalcTooltip = _objSkill.CompileDicepoolTooltip(AttributeActive.Abbrev);
            lblModifiedRating.QueueThreadSafe(() => lblModifiedRating.ToolTipText = backgroundCalcTooltip);
        }

        private void Skill_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool blnUpdateAll = false;
            //I learned something from this but i'm not sure it is a good solution
            //scratch that, i'm sure it is a bad solution. (Tooltip manager from tooltip, properties from reflection?

            //if name of changed is null it does magic to change all, otherwise it only does one.
            switch (e?.PropertyName)
            {
                case null:
                    blnUpdateAll = true;
                    goto case nameof(Skill.DisplayPool);
                case nameof(Skill.DisplayPool):
                    SkillControlReadOnly_RefreshPoolTooltipAndDisplay();
                    if (blnUpdateAll)
                        goto case nameof(Skill.Default);
                    break;
                case nameof(Skill.Default):
                    lblName.Font = !_objSkill.Default ? _fntItalicName : _fntNormalName;
                    if (blnUpdateAll)
                        goto case nameof(Skill.DefaultAttribute);
                    break;
                case nameof(Skill.DefaultAttribute):
                    if (cboSelectAttribute != null)
                    {
                        cboSelectAttribute.SelectedValue = _objSkill.AttributeObject.Abbrev;
                        cboSelectAttribute_Closed(this, EventArgs.Empty);
                    }
                    else
                    {
                        AttributeActive = _objSkill.AttributeObject;
                    }
                    if (blnUpdateAll)
                        goto case nameof(Skill.Notes);
                    break;
                case nameof(Skill.Notes):
                    if (string.IsNullOrEmpty(_objSkill.Notes))
                        cmdNotes.Enabled = false;
                    else
                    {
                        cmdNotes.Enabled = true;
                        string strTooltip = LanguageManager.GetString("Label_Notes") + Environment.NewLine
                            + Environment.NewLine + _objSkill.Notes;
                        cmdNotes.ToolTipText = strTooltip.WordWrap();
                    }
                    break;
            }
        }

        private void Attribute_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e?.PropertyName)
            {
                case null:
                case nameof(CharacterAttrib.Abbrev):
                case nameof(CharacterAttrib.TotalValue):
                    SkillControlReadOnly_RefreshPoolTooltipAndDisplay();
                    break;
            }
        }

        /// <summary>
        /// I'm not super pleased with how this works, but it's functional so w/e.
        /// The goal is for controls to retain the ability to display tooltips even while disabled. IT DOES NOT WORK VERY WELL.
        /// </summary>
        #region ButtonWithToolTip Visibility workaround

        private ButtonWithToolTip _activeButton;

        private ButtonWithToolTip ActiveButton
        {
            get => _activeButton;
            set
            {
                if (value == ActiveButton)
                    return;
                ActiveButton?.ToolTipObject.Hide(this);
                _activeButton = value;
                if (ActiveButton?.Visible == true)
                {
                    ActiveButton.ToolTipObject.Show(ActiveButton.ToolTipText, this);
                }
            }
        }

        private ButtonWithToolTip FindToolTipControl(Point pt)
        {
            return Controls.OfType<ButtonWithToolTip>().FirstOrDefault(c => c.Bounds.Contains(pt));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            ActiveButton = FindToolTipControl(e.Location);
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            ActiveButton = null;
        }

        #endregion ButtonWithToolTip Visibility workaround
    }
}
