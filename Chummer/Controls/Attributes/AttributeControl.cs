/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using Chummer.Properties;

namespace Chummer.UI.Attributes
{
    public partial class AttributeControl : UserControl
    {
        public event EventHandler ValueChanged;

        private readonly string _strAttributeName;
        private int _oldBase;
        private int _oldKarma;
        private readonly Character _objCharacter;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly BindingSource _dataSource;

        private readonly NumericUpDownEx nudKarma;
        private readonly NumericUpDownEx nudBase;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ButtonWithToolTip cmdBurnEdge;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ButtonWithToolTip cmdImproveATT;

        public AttributeControl(CharacterAttrib attribute)
        {
            if (attribute == null)
                return;
            _strAttributeName = attribute.Abbrev;
            _objCharacter = attribute.CharacterObject;
            _dataSource = _objCharacter.AttributeSection.GetAttributeBindingByName(AttributeName);

            InitializeComponent();

            Disposed += (sender, args) => UnbindAttributeControl();

            SuspendLayout();
            try
            {
                // To make sure that the initial load formats the name column properly, we need to set the attribute name in the constructor
                lblName.Text = attribute.DisplayNameFormatted;
                //Display
                if (_objCharacter.Created)
                {
                    cmdImproveATT = new ButtonWithToolTip
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Padding = new Padding(1),
                        MinimumSize = new Size(24, 24),
                        ImageDpi96 = Resources.add_16,
                        ImageDpi192 = Resources.add_32,
                        Name = "cmdImproveATT",
                        UseVisualStyleBackColor = true
                    };
                    cmdImproveATT.Click += cmdImproveATT_Click;
                    flpRight.Controls.Add(cmdImproveATT);
                    if (AttributeName == "EDG")
                    {
                        cmdBurnEdge = new ButtonWithToolTip
                        {
                            Anchor = AnchorStyles.Right,
                            AutoSize = true,
                            AutoSizeMode = AutoSizeMode.GrowAndShrink,
                            Padding = new Padding(1),
                            MinimumSize = new Size(24, 24),
                            ImageDpi96 = Resources.fire_16,
                            ImageDpi192 = Resources.fire_32,
                            Name = "cmdBurnEdge",
                            ToolTipText = LanguageManager.GetString("Tip_CommonBurnEdge"),
                            UseVisualStyleBackColor = true
                        };
                        cmdBurnEdge.Click += cmdBurnEdge_Click;
                        flpRight.Controls.Add(cmdBurnEdge);
                    }
                }
                else
                {
                    using (EnterReadLock.Enter(AttributeObject))
                    {
                        while (AttributeObject.KarmaMaximum < 0 && AttributeObject.Base > 0)
                            --AttributeObject.Base;
                        // Very rough fix for when Karma values somehow exceed KarmaMaximum after loading in. This shouldn't happen in the first place, but this ad-hoc patch will help fix crashes.
                        if (AttributeObject.Karma > AttributeObject.KarmaMaximum)
                            AttributeObject.Karma = AttributeObject.KarmaMaximum;
                    }

                    nudKarma = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = GlobalSettings.InterceptMode,
                        Margin = new Padding(3, 0, 3, 0),
                        Maximum = new decimal(new[] {99, 0, 0, 0}),
                        MinimumSize = new Size(35, 0),
                        Name = "nudKarma"
                    };
                    nudKarma.BeforeValueIncrement += nudKarma_BeforeValueIncrement;
                    nudKarma.ValueChanged += nudKarma_ValueChanged;
                    nudBase = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = GlobalSettings.InterceptMode,
                        Margin = new Padding(3, 0, 3, 0),
                        Maximum = new decimal(new[] {99, 0, 0, 0}),
                        MinimumSize = new Size(35, 0),
                        Name = "nudBase"
                    };
                    nudBase.BeforeValueIncrement += nudBase_BeforeValueIncrement;
                    nudBase.ValueChanged += nudBase_ValueChanged;

                    flpRight.Controls.Add(nudKarma);
                    flpRight.Controls.Add(nudBase);
                }

                this.UpdateLightDarkMode();
                this.TranslateWinForm();
            }
            finally
            {
                ResumeLayout();
            }
        }

        private async void AttributeControl_Load(object sender, EventArgs e)
        {
            await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
            try
            {
                //Display
                await lblName.DoOneWayDataBindingAsync("Text", _dataSource,
                                                       nameof(CharacterAttrib.DisplayNameFormatted)).ConfigureAwait(false);
                await lblValue.DoOneWayDataBindingAsync("Text", _dataSource, nameof(CharacterAttrib.DisplayValue)).ConfigureAwait(false);
                await lblLimits.DoOneWayDataBindingAsync("Text", _dataSource,
                                                         nameof(CharacterAttrib.AugmentedMetatypeLimits)).ConfigureAwait(false);
                await lblValue.DoOneWayDataBindingAsync("ToolTipText", _dataSource, nameof(CharacterAttrib.ToolTip)).ConfigureAwait(false);
                if (await _objCharacter.GetCreatedAsync().ConfigureAwait(false))
                {
                    await cmdImproveATT.DoOneWayDataBindingAsync("ToolTipText", _dataSource,
                                                                 nameof(CharacterAttrib.UpgradeToolTip)).ConfigureAwait(false);
                    await cmdImproveATT.DoOneWayDataBindingAsync("Enabled", _dataSource,
                                                                 nameof(CharacterAttrib.CanUpgradeCareer)).ConfigureAwait(false);
                }
                else
                {
                    using (await EnterReadLock.EnterAsync(AttributeObject).ConfigureAwait(false))
                    {
                        while (await AttributeObject.GetBaseAsync().ConfigureAwait(false) > 0 &&
                               await AttributeObject.GetKarmaMaximumAsync().ConfigureAwait(false) < 0)
                        {
                            await AttributeObject.ModifyBaseAsync(-1).ConfigureAwait(false);
                        }

                        // Very rough fix for when Karma values somehow exceed KarmaMaximum after loading in. This shouldn't happen in the first place, but this ad-hoc patch will help fix crashes.
                        int intKarmaMaximum = await AttributeObject.GetKarmaMaximumAsync().ConfigureAwait(false);
                        if (await AttributeObject.GetKarmaAsync().ConfigureAwait(false) > intKarmaMaximum)
                            await AttributeObject.SetKarmaAsync(intKarmaMaximum).ConfigureAwait(false);
                    }

                    await nudBase.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacter,
                                                                 nameof(Character
                                                                            .EffectiveBuildMethodUsesPriorityTables),
                                                                 x => x.GetEffectiveBuildMethodUsesPriorityTablesAsync()
                                                                       .AsTask()).ConfigureAwait(false);
                    await nudBase.DoOneWayDataBindingAsync("Maximum", _dataSource,
                                                           nameof(CharacterAttrib.PriorityMaximum)).ConfigureAwait(false);
                    await nudBase.DoOneWayDataBindingAsync("Enabled", _dataSource,
                                                           nameof(CharacterAttrib.BaseUnlocked)).ConfigureAwait(false);
                    await nudKarma.DoOneWayDataBindingAsync("Maximum", _dataSource,
                                                            nameof(CharacterAttrib.KarmaMaximum)).ConfigureAwait(false);
                    await nudBase.DoDataBindingAsync("Value", _dataSource, nameof(CharacterAttrib.Base)).ConfigureAwait(false);
                    await nudKarma.DoDataBindingAsync("Value", _dataSource, nameof(CharacterAttrib.Karma)).ConfigureAwait(false);
                }
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout(true)).ConfigureAwait(false);
            }
        }

        public void UpdateWidths(int intNameWidth, int intNudKarmaWidth, int intValueWidth, int intLimitsWidth)
        {
            tlpMain.DoThreadSafe(x => x.SuspendLayout());
            try
            {
                if (intNameWidth >= 0)
                {
                    lblName.DoThreadSafe(x =>
                    {
                        if (x.MinimumSize.Width > intNameWidth)
                            x.MinimumSize = new Size(intNameWidth, x.MinimumSize.Height);
                        if (x.MaximumSize.Width != intNameWidth)
                            x.MaximumSize = new Size(intNameWidth, x.MinimumSize.Height);
                        if (x.MinimumSize.Width < intNameWidth)
                            x.MinimumSize = new Size(intNameWidth, x.MinimumSize.Height);
                    });
                }

                if (intNudKarmaWidth >= 0 && nudBase?.DoThreadSafeFunc(x => x.Visible) == true)
                {
                    nudKarma?.DoThreadSafe(x =>
                    {
                        if (x.Visible)
                        {
                            x.Margin = new Padding(
                                x.Margin.Right + Math.Max(intNudKarmaWidth - x.Width, 0),
                                x.Margin.Top,
                                x.Margin.Right,
                                x.Margin.Bottom);
                        }
                    });
                }

                if (intValueWidth >= 0)
                {
                    lblValue.DoThreadSafe(x =>
                    {
                        if (x.MinimumSize.Width > intValueWidth)
                            x.MinimumSize = new Size(intValueWidth, x.MinimumSize.Height);
                        if (x.MaximumSize.Width != intValueWidth)
                            x.MaximumSize = new Size(intValueWidth, x.MinimumSize.Height);
                        if (x.MinimumSize.Width < intValueWidth)
                            x.MinimumSize = new Size(intValueWidth, x.MinimumSize.Height);
                    });
                }

                if (intLimitsWidth >= 0)
                {
                    lblLimits.DoThreadSafe(x =>
                    {
                        if (x.MinimumSize.Width > intLimitsWidth)
                            x.MinimumSize = new Size(intLimitsWidth, x.MinimumSize.Height);
                        if (x.MaximumSize.Width != intLimitsWidth)
                            x.MaximumSize = new Size(intLimitsWidth, x.MinimumSize.Height);
                        if (x.MinimumSize.Width < intLimitsWidth)
                            x.MinimumSize = new Size(intLimitsWidth, x.MinimumSize.Height);
                    });
                }
            }
            finally
            {
                tlpMain.DoThreadSafe(x => x.ResumeLayout());
            }
        }

        private void UnbindAttributeControl()
        {
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private async void cmdImproveATT_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                CharacterAttrib objAttribute = await GetAttributeObjectAsync().ConfigureAwait(false);
                using (await EnterReadLock.EnterAsync(objAttribute).ConfigureAwait(false))
                {
                    int intUpgradeKarmaCost = await objAttribute.GetUpgradeKarmaCostAsync().ConfigureAwait(false);

                    if (intUpgradeKarmaCost == -1) return; //TODO: more descriptive
                    if (intUpgradeKarmaCost > await _objCharacter.GetKarmaAsync().ConfigureAwait(false))
                    {
                        Program.ShowScrollableMessageBox(
                            await LanguageManager.GetStringAsync("Message_NotEnoughKarma").ConfigureAwait(false),
                            await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma").ConfigureAwait(false),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string confirmstring = string.Format(GlobalSettings.CultureInfo,
                                                         await LanguageManager
                                                               .GetStringAsync("Message_ConfirmKarmaExpense")
                                                               .ConfigureAwait(false),
                                                         await objAttribute.GetDisplayNameFormattedAsync()
                                                                           .ConfigureAwait(false),
                                                         await objAttribute.GetValueAsync().ConfigureAwait(false) + 1,
                                                         intUpgradeKarmaCost);
                    if (!await CommonFunctions.ConfirmKarmaExpenseAsync(confirmstring).ConfigureAwait(false))
                        return;

                    await objAttribute.Upgrade().ConfigureAwait(false);
                }

                await this.DoThreadSafeAsync(x => x.ValueChanged?.Invoke(this, e)).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void nudBase_ValueChanged(object sender, EventArgs e)
        {
            int intValue = await ((NumericUpDownEx)sender).DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
            if (Interlocked.Exchange(ref _oldBase, intValue) == intValue)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                CharacterAttrib objAttribute = await GetAttributeObjectAsync().ConfigureAwait(false);
                using (await EnterReadLock.EnterAsync(objAttribute).ConfigureAwait(false))
                {
                    if (!await CanBeMetatypeMax(
                                Math.Max(
                                    await nudKarma.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false) +
                                    await objAttribute.GetFreeBaseAsync().ConfigureAwait(false)
                                    + await objAttribute.GetRawMinimumAsync().ConfigureAwait(false) +
                                    await objAttribute.GetAttributeValueModifiersAsync().ConfigureAwait(false),
                                    await objAttribute.GetTotalMinimumAsync().ConfigureAwait(false)) + intValue)
                            .ConfigureAwait(false))
                    {
                        await nudBase.DoThreadSafeAsync(x =>
                        {
                            decimal newValue = Math.Max(x.Value - 1, 0);
                            if (newValue > x.Maximum)
                            {
                                newValue = x.Maximum;
                            }

                            if (newValue < x.Minimum)
                            {
                                newValue = x.Minimum;
                            }

                            x.Value = newValue;
                        }).ConfigureAwait(false);
                        return;
                    }
                }

                await this.DoThreadSafeAsync(x => x.ValueChanged?.Invoke(this, e)).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void nudKarma_ValueChanged(object sender, EventArgs e)
        {
            int intValue = await ((NumericUpDownEx)sender).DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
            int intOldKarma = Interlocked.Exchange(ref _oldKarma, intValue);
            if (intOldKarma == intValue)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                CharacterAttrib objAttribute = await GetAttributeObjectAsync().ConfigureAwait(false);
                using (await EnterReadLock.EnterAsync(objAttribute).ConfigureAwait(false))
                {
                    if (!await CanBeMetatypeMax(
                                Math.Max(
                                    await nudBase.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false) +
                                    await objAttribute.GetFreeBaseAsync().ConfigureAwait(false)
                                    + await objAttribute.GetRawMinimumAsync().ConfigureAwait(false) +
                                    await objAttribute.GetAttributeValueModifiersAsync().ConfigureAwait(false),
                                    await objAttribute.GetTotalMinimumAsync().ConfigureAwait(false)) + intValue)
                            .ConfigureAwait(false))
                    {
                        // It's possible that the attribute maximum was reduced by an improvement, so confirm the appropriate value to bounce up/down to.
                        int intKarmaMaximum = await objAttribute.GetKarmaMaximumAsync().ConfigureAwait(false);
                        if (intOldKarma > intKarmaMaximum)
                        {
                            if (Interlocked.CompareExchange(ref _oldKarma, intKarmaMaximum - 1, intValue) == intValue)
                                intValue = intKarmaMaximum - 1;
                            intOldKarma = intKarmaMaximum - 1;
                        }

                        if (intOldKarma < 0)
                        {
                            Interlocked.CompareExchange(ref _oldKarma, 0, intValue);
                            int intOldKarmaLocal = intOldKarma;
                            await nudBase.DoThreadSafeAsync(x =>
                            {
                                decimal newValue = Math.Max(x.Value - intOldKarmaLocal, 0);
                                if (newValue > x.Maximum)
                                {
                                    newValue = x.Maximum;
                                }

                                if (newValue < x.Minimum)
                                {
                                    newValue = x.Minimum;
                                }

                                x.Value = newValue;
                            }).ConfigureAwait(false);
                            intOldKarma = 0;
                        }

                        await nudKarma.DoThreadSafeAsync(x => x.Value = intOldKarma).ConfigureAwait(false);
                        return;
                    }
                }

                await this.DoThreadSafeAsync(x => x.ValueChanged?.Invoke(this, e)).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Is the attribute allowed to reach the Metatype Maximum?
        /// SR5 66: Characters at character creation may only have 1 Mental or Physical
        /// attribute at their natural maximum limit; the special attributes of Magic, Edge,
        /// and Resonance are not included in this limitation.
        /// </summary>
        private async ValueTask<bool> CanBeMetatypeMax(int intValue, CancellationToken token = default)
        {
            CharacterAttrib objAttribute = await GetAttributeObjectAsync(token).ConfigureAwait(false);
            using (await EnterReadLock.EnterAsync(objAttribute, token).ConfigureAwait(false))
            {
                int intTotalMaximum = await objAttribute.GetTotalMaximumAsync(token).ConfigureAwait(false);
                if (intValue < intTotalMaximum || intTotalMaximum == 0)
                    return true;

                if (await _objCharacter.AttributeSection.CanRaiseAttributeToMetatypeMax(objAttribute, token).ConfigureAwait(false))
                    return true;

                Program.ShowScrollableMessageBox(
                    string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Message_AttributeMaximum", token: token).ConfigureAwait(false),
                        _objCharacter.Settings.MaxNumberMaxAttributesCreate),
                    await LanguageManager.GetStringAsync("MessageTitle_Attribute", token: token).ConfigureAwait(false), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return false;
            }
        }

        public string AttributeName => _strAttributeName;

        private CharacterAttrib _objCachedCharacterAttrib;

        public CharacterAttrib AttributeObject =>
            _objCachedCharacterAttrib ?? (_objCachedCharacterAttrib =
                _objCharacter.AttributeSection.GetAttributeByName(AttributeName));

        public async ValueTask<CharacterAttrib> GetAttributeObjectAsync(CancellationToken token = default) =>
            _objCachedCharacterAttrib ?? (_objCachedCharacterAttrib =
                await _objCharacter.AttributeSection.GetAttributeByNameAsync(AttributeName, token).ConfigureAwait(false));

        [UsedImplicitly]
        public int NameWidth => lblName.DoThreadSafeFunc(x => x.PreferredWidth);

        [UsedImplicitly]
        public Task<int> GetNameWidthAsync(CancellationToken token = default) => lblName.DoThreadSafeFuncAsync(x => x.PreferredWidth, token);

        private async void cmdBurnEdge_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                // Edge cannot go below 1.
                CharacterAttrib objAttribute = await GetAttributeObjectAsync().ConfigureAwait(false);
                using (await EnterReadLock.EnterAsync(objAttribute).ConfigureAwait(false))
                {
                    if (await objAttribute.GetValueAsync().ConfigureAwait(false) <= 0)
                    {
                        Program.ShowScrollableMessageBox(
                            await LanguageManager.GetStringAsync("Message_CannotBurnEdge").ConfigureAwait(false),
                            await LanguageManager.GetStringAsync("MessageTitle_CannotBurnEdge").ConfigureAwait(false),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        return;
                    }

                    // Verify that the user wants to Burn a point of Edge.
                    if (Program.ShowScrollableMessageBox(
                            await LanguageManager.GetStringAsync("Message_BurnEdge").ConfigureAwait(false),
                            await LanguageManager.GetStringAsync("MessageTitle_BurnEdge").ConfigureAwait(false),
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == DialogResult.No)
                        return;

                    await objAttribute.Degrade(1).ConfigureAwait(false);
                }

                await this.DoThreadSafeAsync(x => x.ValueChanged?.Invoke(this, e)).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void nudBase_BeforeValueIncrement(object sender, CancelEventArgs e)
        {
            if (nudBase.Value + Math.Max(nudKarma.Value, 0) != AttributeObject.TotalMaximum || nudKarma.Value == nudKarma.Minimum)
                return;
            if (nudKarma.Value - nudBase.Increment >= 0)
            {
                nudKarma.Value -= nudBase.Increment;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void nudKarma_BeforeValueIncrement(object sender, CancelEventArgs e)
        {
            if (nudBase.Value + nudKarma.Value != AttributeObject.TotalMaximum || nudBase.Value == nudBase.Minimum)
                return;
            if (nudBase.Value - nudKarma.Increment >= 0)
            {
                nudBase.Value -= nudKarma.Increment;
            }
            else
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// I'm not super pleased with how this works, but it's functional so w/e.
        /// The goal is for controls to retain the ability to display tooltips even while disabled. IT DOES NOT WORK VERY WELL.
        /// </summary>

        #region ButtonWithToolTip Visibility workaround

        private ButtonWithToolTip _activeButton;

        protected ButtonWithToolTip ActiveButton
        {
            get => _activeButton;
            set
            {
                ButtonWithToolTip objOldValue = Interlocked.Exchange(ref _activeButton, value);
                if (objOldValue == value)
                    return;
                objOldValue?.ToolTipObject.Hide(this);
                if (value?.Visible == true)
                {
                    value.ToolTipObject.Show(value.ToolTipText, this);
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
