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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class CreateSpell : Form
    {
        private readonly XPathNavigator _objXmlDocument;
        private bool _blnLoading = true;
        private bool _blnSkipRefresh;
        private readonly Spell _objSpell;

        #region Control Events

        public CreateSpell(Character objCharacter)
        {
            _objSpell = new Spell(objCharacter);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objXmlDocument = objCharacter.LoadDataXPath("spells.xml");
        }

        private async void CreateSpell_Load(object sender, EventArgs e)
        {
            await lblDV.DoThreadSafeAsync(x => x.Text = 0.ToString(GlobalSettings.CultureInfo));

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstCategory))
            {
                // Populate the list of Spell Categories.
                foreach (XPathNavigator objXmlCategory in await _objXmlDocument.SelectAndCacheExpressionAsync(
                             "/chummer/categories/category"))
                {
                    string strInnerText = objXmlCategory.Value;
                    lstCategory.Add(new ListItem(strInnerText,
                                                 (await objXmlCategory.SelectSingleNodeAndCacheExpressionAsync("@translate"))?.Value
                                                 ?? strInnerText));
                }
                
                await cboCategory.PopulateWithListItemsAsync(lstCategory);
            }

            await cboCategory.DoThreadSafeAsync(x => x.SelectedIndex = 0);

            // Populate the list of Spell Types.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTypes))
            {
                lstTypes.Add(new ListItem("P", await LanguageManager.GetStringAsync("String_DescPhysical")));
                lstTypes.Add(new ListItem("M", await LanguageManager.GetStringAsync("String_DescMana")));
                await cboType.PopulateWithListItemsAsync(lstTypes);
            }

            await cboType.DoThreadSafeAsync(x => x.SelectedIndex = 0);

            // Populate the list of Ranges.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstRanges))
            {
                lstRanges.Add(new ListItem("T", await LanguageManager.GetStringAsync("String_SpellRangeTouchLong")));
                lstRanges.Add(new ListItem("LOS", await LanguageManager.GetStringAsync("String_SpellRangeLineOfSight")));
                await cboRange.PopulateWithListItemsAsync(lstRanges);
            }

            await cboRange.DoThreadSafeAsync(x => x.SelectedIndex = 0);

            // Populate the list of Durations.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstDurations))
            {
                lstDurations.Add(new ListItem("I", await LanguageManager.GetStringAsync("String_SpellDurationInstantLong")));
                lstDurations.Add(new ListItem("P", await LanguageManager.GetStringAsync("String_SpellDurationPermanentLong")));
                lstDurations.Add(new ListItem("S", await LanguageManager.GetStringAsync("String_SpellDurationSustainedLong")));
                await cboDuration.PopulateWithListItemsAsync(lstDurations);
            }

            await cboDuration.DoThreadSafeAsync(x => x.SelectedIndex = 0);

            _blnLoading = false;

            await CalculateDrain();
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString()) == "Health")
            {
                await chkArea.DoThreadSafeAsync(x =>
                {
                    x.Checked = false;
                    x.Enabled = false;
                });
            }
            else
                await chkArea.DoThreadSafeAsync(x => x.Enabled = true);

            await ChangeModifiers();
            await CalculateDrain();
        }

        private async void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            await CalculateDrain();
        }

        private async void cboRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            await CalculateDrain();
        }

        private async void cboDuration_SelectedIndexChanged(object sender, EventArgs e)
        {
            await CalculateDrain();
        }

        private async void chkModifier_CheckedChanged(object sender, EventArgs e)
        {
            await cboType.DoThreadSafeAsync(x => x.Enabled = true);
            if (_blnSkipRefresh)
                return;

            switch (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString()))
            {
                case "Combat":
                    {
                        // Direct and Indirect cannot be selected at the same time.
                        if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier2.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                                x.Enabled = false;
                            });
                            await chkModifier3.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            await nudNumberOfEffects.DoThreadSafeAsync(x => x.Enabled = false);
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier2.DoThreadSafeAsync(x => x.Enabled = true);
                            await chkModifier3.DoThreadSafeAsync(x => x.Enabled = true);
                            await nudNumberOfEffects.DoThreadSafeAsync(x => x.Enabled = true);
                        }

                        // Indirect Combat Spells must always be physical. Direct and Indirect cannot be selected at the same time.
                        if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier1.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            await cboType.DoThreadSafeAsync(x =>
                            {
                                x.SelectedValue = "P";
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier1.DoThreadSafeAsync(x => x.Enabled = true);
                        }

                        // Elemental effect spells must be Indirect (and consequently physical as well).
                        if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            await chkModifier2.DoThreadSafeAsync(x => x.Checked = true);
                        }

                        // Physical damage and Stun damage cannot be selected at the same time.
                        if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier5.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier5.DoThreadSafeAsync(x => x.Enabled = true);
                        }
                        if (await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier4.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier4.DoThreadSafeAsync(x => x.Enabled = true);
                        }

                        break;
                    }
                case "Detection":
                    {
                        // Directional, and Area cannot be selected at the same time.
                        if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier2.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier1.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }

                        // Active and Passive cannot be selected at the same time.
                        if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier5.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier5.DoThreadSafeAsync(x => x.Enabled = true);
                        }

                        if (await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier4.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier4.DoThreadSafeAsync(x => x.Enabled = true);
                        }

                        // If Extended Area is selected, Area must also be selected.
                        if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            await chkModifier1.DoThreadSafeAsync(x => x.Checked = false);
                            await chkModifier3.DoThreadSafeAsync(x => x.Checked = false);
                            await chkModifier2.DoThreadSafeAsync(x => x.Checked = true);
                        }

                        break;
                    }
                case "Health":
                    // Nothing special for Health Spells.
                    break;

                case "Illusion":
                    {
                        // Obvious and Realistic cannot be selected at the same time.
                        if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier2.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier2.DoThreadSafeAsync(x => x.Enabled = true);
                        }

                        if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier1.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier1.DoThreadSafeAsync(x => x.Enabled = true);
                        }

                        // Single-Sense and Multi-Sense cannot be selected at the same time.
                        if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier4.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier4.DoThreadSafeAsync(x => x.Enabled = true);
                        }
                        if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier3.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier3.DoThreadSafeAsync(x => x.Enabled = true);
                        }

                        break;
                    }
                case "Manipulation":
                    {
                        // Environmental, Mental, and Physical cannot be selected at the same time.
                        if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier2.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            await chkModifier3.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier1.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            await chkModifier3.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier1.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            await chkModifier2.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        await chkModifier1.DoThreadSafeAsync(x => x.Enabled = !chkModifier2.DoThreadSafeFunc(y => y.Checked) && !chkModifier3.DoThreadSafeFunc(y => y.Checked));
                        await chkModifier2.DoThreadSafeAsync(x => x.Enabled = !chkModifier1.DoThreadSafeFunc(y => y.Checked) && !chkModifier3.DoThreadSafeFunc(y => y.Checked));
                        await chkModifier3.DoThreadSafeAsync(x => x.Enabled = !chkModifier1.DoThreadSafeFunc(y => y.Checked) && !chkModifier2.DoThreadSafeFunc(y => y.Checked));

                        // Minor Change and Major Change cannot be selected at the same time.
                        if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier5.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier5.DoThreadSafeAsync(x => x.Enabled = true);
                        }
                        if (await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked))
                        {
                            _blnSkipRefresh = true;
                            await chkModifier4.DoThreadSafeAsync(x =>
                            {
                                x.Checked = false;
                                x.Enabled = false;
                            });
                            _blnSkipRefresh = false;
                        }
                        else
                        {
                            await chkModifier4.DoThreadSafeAsync(x => x.Enabled = true);
                        }

                        break;
                    }
            }

            await CalculateDrain();
        }

        private async void chkRestricted_CheckedChanged(object sender, EventArgs e)
        {
            await chkVeryRestricted.DoThreadSafeAsync(x => x.Enabled = !chkRestricted.DoThreadSafeFunc(y => y.Checked));
            await CalculateDrain();
            await txtRestriction.DoThreadSafeAsync(x =>
            {
                x.Enabled = chkRestricted.DoThreadSafeFunc(y => y.Checked) || chkVeryRestricted.DoThreadSafeFunc(y => y.Checked);
                if (!x.Enabled)
                    x.Text = string.Empty;
            });
        }

        private async void chkVeryRestricted_CheckedChanged(object sender, EventArgs e)
        {
            await chkRestricted.DoThreadSafeAsync(x => x.Enabled = !chkVeryRestricted.DoThreadSafeFunc(y => y.Checked));
            await CalculateDrain();
            await txtRestriction.DoThreadSafeAsync(x =>
            {
                x.Enabled = chkRestricted.DoThreadSafeFunc(y => y.Checked) || chkVeryRestricted.DoThreadSafeFunc(y => y.Checked);
                if (!x.Enabled)
                    x.Text = string.Empty;
            });
        }

        private async void nudNumberOfEffects_ValueChanged(object sender, EventArgs e)
        {
            await CalculateDrain();
        }

        private async void chkArea_CheckedChanged(object sender, EventArgs e)
        {
            await CalculateDrain();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            await AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion Control Events

        #region Methods

        /// <summary>
        /// Re-calculate the Drain modifiers based on the currently-selected options.
        /// </summary>
        private async ValueTask ChangeModifiers(CancellationToken token = default)
        {
            foreach (Control objControl in await flpModifiers.DoThreadSafeFuncAsync(x => x.Controls, token: token))
            {
                switch (objControl)
                {
                    case ColorableCheckBox chkCheckbox:
                    {
                        await chkCheckbox.DoThreadSafeAsync(x =>
                        {
                            x.Enabled = true;
                            x.Checked = false;
                            x.Text = string.Empty;
                            x.Tag = string.Empty;
                            x.Visible = false;
                        }, token: token);
                        break;
                    }
                    case Panel panChild:
                    {
                        await panChild.DoThreadSafeAsync(x =>
                        {
                            foreach (CheckBox chkCheckbox in x.Controls.OfType<CheckBox>())
                            {
                                chkCheckbox.Enabled = true;
                                chkCheckbox.Checked = false;
                                chkCheckbox.Text = string.Empty;
                                chkCheckbox.Tag = string.Empty;
                                chkCheckbox.Visible = false;
                            }
                        }, token: token);
                        break;
                    }
                }
            }

            await nudNumberOfEffects.DoThreadSafeAsync(x =>
            {
                x.Visible = false;
                x.Enabled = true;
            }, token: token);

            string strText;
            switch (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token))
            {
                case "Detection":
                {
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell1", token: token);
                    await chkModifier1.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell2", token: token);
                    await chkModifier2.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell3", token: token);
                    await chkModifier3.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell4", token: token);
                    await chkModifier4.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell5", token: token);
                    await chkModifier5.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell6", token: token);
                    await chkModifier6.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell7", token: token);
                    await chkModifier7.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+1";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell8", token: token);
                    await chkModifier8.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+1";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell9", token: token);
                    await chkModifier9.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell10", token: token);
                    await chkModifier10.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+4";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell11", token: token);
                    await chkModifier11.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+1";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell12", token: token);
                    await chkModifier12.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell13", token: token);
                    await chkModifier13.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+4";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell14", token: token);
                    await chkModifier14.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText;
                    }, token: token);
                    break;
                }
                case "Health":
                {
                    strText = await LanguageManager.GetStringAsync("Checkbox_HealthSpell1", token: token);
                    await chkModifier1.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_HealthSpell2", token: token);
                    await chkModifier2.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+4";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_HealthSpell3", token: token);
                    await chkModifier3.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-2";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_HealthSpell4", token: token);
                    await chkModifier4.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_HealthSpell5", token: token);
                    await chkModifier5.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-2";
                        x.Text = strText;
                    }, token: token);
                    break;
                }
                case "Illusion":
                {
                    strText = await LanguageManager.GetStringAsync("Checkbox_IllusionSpell1", token: token);
                    await chkModifier1.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-1";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_IllusionSpell2", token: token);
                    await chkModifier2.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_IllusionSpell3", token: token);
                    await chkModifier3.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-2";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_IllusionSpell4", token: token);
                    await chkModifier4.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_IllusionSpell5", token: token);
                    await chkModifier5.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText;
                    }, token: token);
                    break;
                }
                case "Manipulation":
                {
                    strText = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell1", token: token);
                    await chkModifier1.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-2";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell2", token: token);
                    await chkModifier2.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell3", token: token);
                    await chkModifier3.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell4", token: token);
                    await chkModifier4.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell5", token: token);
                    await chkModifier5.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell6", token: token);
                    await chkModifier6.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText;
                    }, token: token);
                    int intTop = await chkModifier6.DoThreadSafeFuncAsync(x => x.Top, token: token);
                    await nudNumberOfEffects.DoThreadSafeAsync(x =>
                    {
                        x.Visible = true;
                        x.Top = intTop - 1;
                    }, token: token);
                    break;
                }
                default:
                {
                    // Combat.
                    strText = await LanguageManager.GetStringAsync("Checkbox_CombatSpell1", token: token);
                    await chkModifier1.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_CombatSpell2", token: token);
                    await chkModifier2.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_CombatSpell3", token: token);
                    await chkModifier3.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_CombatSpell4", token: token);
                    await chkModifier4.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText;
                    }, token: token);
                    strText = await LanguageManager.GetStringAsync("Checkbox_CombatSpell5", token: token);
                    await chkModifier5.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-1";
                        x.Text = strText;
                    }, token: token);
                    int intTop = await chkModifier3.DoThreadSafeFuncAsync(x => x.Top, token: token);
                    await nudNumberOfEffects.DoThreadSafeAsync(x =>
                    {
                        x.Visible = true;
                        x.Top = intTop - 1;
                    }, token: token);
                    break;
                }
            }

            string strCheckBoxFormat = await LanguageManager.GetStringAsync("String_Space", token: token) + "({0})";
            foreach (Control objControl in await flpModifiers.DoThreadSafeFuncAsync(x => x.Controls, token: token))
            {
                switch (objControl)
                {
                    case CheckBox chkCheckbox:
                    {
                        await chkCheckbox.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(x.Text))
                            {
                                x.Visible = true;
                                x.Text += string.Format(GlobalSettings.CultureInfo, strCheckBoxFormat, x.Tag);
                            }
                        }, token: token);
                        break;
                    }
                    case Panel pnlControl:
                    {
                        await pnlControl.DoThreadSafeAsync(x =>
                        {
                            foreach (CheckBox chkInnerCheckbox in x.Controls.OfType<CheckBox>())
                            {
                                if (string.IsNullOrEmpty(chkInnerCheckbox.Text))
                                    continue;
                                chkInnerCheckbox.Visible = true;
                                chkInnerCheckbox.Text += string.Format(GlobalSettings.CultureInfo, strCheckBoxFormat, chkInnerCheckbox.Tag);
                            }
                        }, token: token);
                        break;
                    }
                }
            }

            if (await nudNumberOfEffects.DoThreadSafeFuncAsync(x => x.Visible, token: token))
            {
                switch (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token))
                {
                    case "Combat":
                    {
                        int intBase = await chkModifier3.DoThreadSafeFuncAsync(x => x.Left + x.Width, token: token);
                        await nudNumberOfEffects.DoThreadSafeAsync(x => x.Left = intBase + 6, token: token);
                        break;
                    }
                    case "Manipulation":
                    {
                        int intBase = await chkModifier6.DoThreadSafeFuncAsync(x => x.Left + x.Width, token: token);
                        await nudNumberOfEffects.DoThreadSafeAsync(x => x.Left = intBase + 6, token: token);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the Spell's Drain Value based on the currently-selected options.
        /// </summary>
        private async ValueTask<string> CalculateDrain(CancellationToken token = default)
        {
            if (_blnLoading)
                return string.Empty;

            int intDV = 0;

            // Type DV.
            if (await cboType.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token) != "M")
                ++intDV;

            // Range DV.
            if (await cboRange.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token) == "T")
                intDV -= 2;

            if (chkArea.Checked)
                intDV += 2;

            // Restriction DV.
            if (await chkRestricted.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                --intDV;
            if (await chkVeryRestricted.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                intDV -= 2;

            string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token);

            // Duration DV.
            // Curative Health Spells do not have a modifier for Permanent duration.
            if (await cboDuration.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token) == "P" && (strCategory != "Health" || !await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token)))
                intDV += 2;

            int intNumberOfEffects = await nudNumberOfEffects.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token);

            // Include any checked modifiers.
            foreach (CheckBox chkModifier in await flpModifiers.DoThreadSafeFuncAsync(x => x.Controls.OfType<CheckBox>(), token: token))
            {
                await chkModifier.DoThreadSafeAsync(x =>
                {
                    if (x.Visible && x.Checked)
                    {
                        if (x == chkModifier3 && strCategory == "Combat")
                            intDV += Convert.ToInt32(x.Tag.ToString(), GlobalSettings.InvariantCultureInfo)
                                     * intNumberOfEffects;
                        else if (x == chkModifier6 && strCategory == "Manipulation")
                            intDV += Convert.ToInt32(x.Tag.ToString(), GlobalSettings.InvariantCultureInfo)
                                     * intNumberOfEffects;
                        else
                            intDV += Convert.ToInt32(x.Tag.ToString(), GlobalSettings.InvariantCultureInfo);
                    }
                }, token: token);
            }
            foreach (Panel panChild in await flpModifiers.DoThreadSafeFuncAsync(x => x.Controls.OfType<Panel>(), token: token))
            {
                foreach (CheckBox chkModifier in await panChild.DoThreadSafeFuncAsync(x => x.Controls.OfType<CheckBox>(), token: token))
                {
                    await chkModifier.DoThreadSafeAsync(x =>
                    {
                        if (x.Visible && x.Checked)
                        {
                            if (x == chkModifier3 && strCategory == "Combat")
                                intDV += Convert.ToInt32(x.Tag.ToString(), GlobalSettings.InvariantCultureInfo)
                                         * intNumberOfEffects;
                            else if (x == chkModifier6 && strCategory == "Manipulation")
                                intDV += Convert.ToInt32(x.Tag.ToString(), GlobalSettings.InvariantCultureInfo)
                                         * intNumberOfEffects;
                            else
                                intDV += Convert.ToInt32(x.Tag.ToString(), GlobalSettings.InvariantCultureInfo);
                        }
                    }, token: token);
                }
            }

            string strBase;
            if (strCategory == "Health" && await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token))
            {
                // Health Spells use (Damage Value) as their base.
                strBase = '(' + await LanguageManager.GetStringAsync("String_SpellDamageValue", token: token) + ')';
            }
            else
            {
                // All other spells use (F/2) as their base.
                strBase = "(F/2)";
            }

            string strDV = intDV.ToString(GlobalSettings.InvariantCultureInfo);
            if (intDV > 0)
                strDV = '+' + strDV;
            if (intDV == 0)
                strDV = string.Empty;
            string strText = await (strBase + strDV).Replace('/', '÷').Replace('*', '×')
                                           .CheapReplaceAsync(
                                               "F", () => LanguageManager.GetStringAsync("String_SpellForce", token: token), token: token)
                                           .CheapReplaceAsync("Damage Value",
                                                              () => LanguageManager.GetStringAsync(
                                                                  "String_SpellDamageValue", token: token), token: token);
            await lblDV.DoThreadSafeAsync(x => x.Text = strText, token: token);

            return strBase + strDV;
        }

        /// <summary>
        /// Accept the values of the form.
        /// </summary>
        private async ValueTask AcceptForm(CancellationToken token = default)
        {
            string strMessage = string.Empty;
            // Make sure a name has been provided.
            if (string.IsNullOrWhiteSpace(await txtName.DoThreadSafeFuncAsync(x => x.Text, token: token)))
            {
                if (!string.IsNullOrEmpty(strMessage))
                    strMessage += Environment.NewLine;
                strMessage += await LanguageManager.GetStringAsync("Message_SpellName", token: token);
            }

            // Make sure a Restricted value if the field is enabled.
            if (txtRestriction.Enabled && string.IsNullOrWhiteSpace(await txtRestriction.DoThreadSafeFuncAsync(x => x.Text, token: token)))
            {
                if (!string.IsNullOrEmpty(strMessage))
                    strMessage += Environment.NewLine;
                strMessage += await LanguageManager.GetStringAsync("Message_SpellRestricted", token: token);
            }

            switch (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token))
            {
                // Make sure the Spell has met all of its requirements.
                case "Combat":
                    {
                        // Either Direct or Indirect must be selected.
                        if (!await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_CombatSpellRequirement1", token: token);
                        }

                        // Either Physical damage or Stun damage must be selected.
                        if (!await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_CombatSpellRequirement2", token: token);
                        }

                        break;
                    }
                case "Detection":
                    {
                        // Either Directional, Area, or Psychic must be selected.
                        if (!await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_DetectionSpellRequirement1", token: token);
                        }

                        // Either Active or Passive must be selected.
                        if (!await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_DetectionSpellRequirement2", token: token);
                        }

                        break;
                    }
                case "Health":
                    // Nothing special.
                    break;

                case "Illusion":
                    {
                        // Either Obvious or Realistic must be selected.
                        if (!await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_IllusionSpellRequirement1", token: token);
                        }

                        // Either Single-Sense or Multi-Sense must be selected.
                        if (!await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_IllusionSpellRequirement2", token: token);
                        }

                        break;
                    }
                case "Manipulation":
                    {
                        // Either Environmental, Mental, or Physical must be selected.
                        if (!await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_ManipulationSpellRequirement1", token: token);
                        }

                        // Either Minor Change or Major Change must be selected.
                        if (!await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token) && !await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_ManipulationSpellRequirement2", token: token);
                        }

                        break;
                    }
            }

            // Show the message if necessary.
            if (!string.IsNullOrEmpty(strMessage))
            {
                Program.ShowMessageBox(this, strMessage, await LanguageManager.GetStringAsync("Title_CreateSpell", token: token), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string strRange = await cboRange.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token);
            if (await chkArea.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                strRange += "(A)";

            // If we're made it this far, everything is OK, so create the Spell.
            string strDescriptors = string.Empty;
            switch (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token))
            {
                case "Detection":
                    if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Active, ";
                    if (await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Passive, ";
                    if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Directional, ";
                    if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Psychic, ";
                    if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                    {
                        if (!await chkModifier14.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                            strDescriptors += "Area, ";
                        else
                            strDescriptors += "Extended Area, ";
                    }
                    break;

                case "Health":
                    if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Negative, ";
                    break;

                case "Illusion":
                    if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Obvious, ";
                    if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Realistic, ";
                    if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Single-Sense, ";
                    if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Multi-Sense, ";
                    if (await chkArea.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Area, ";
                    break;

                case "Manipulation":
                    if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Environmental, ";
                    if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Mental, ";
                    if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Physical, ";
                    if (await chkArea.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Area, ";
                    break;

                default:
                    // Combat.
                    if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Direct, ";
                    if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Indirect, ";
                    if ((await cboRange.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token)).Contains("(A)"))
                        strDescriptors += "Area, ";
                    if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token))
                        strDescriptors += "Elemental, ";
                    break;
            }

            // Remove the trailing ", " from the Descriptors string.
            if (!string.IsNullOrEmpty(strDescriptors))
                strDescriptors = strDescriptors.Substring(0, strDescriptors.Length - 2);

            _objSpell.Name = await txtName.DoThreadSafeFuncAsync(x => x.Text, token: token);
            _objSpell.Source = "SM";
            _objSpell.Page = "159";
            _objSpell.Category = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token);
            _objSpell.Descriptors = strDescriptors;
            _objSpell.Range = strRange;
            _objSpell.Type = await cboType.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token);
            _objSpell.Limited = await chkLimited.DoThreadSafeFuncAsync(x => x.Checked, token: token);
            if (_objSpell.Category == "Combat")
                _objSpell.Damage = await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token) ? "P" : "S";
            _objSpell.DvBase = await CalculateDrain(token);
            string strExtra = await txtRestriction.DoThreadSafeFuncAsync(x => x.Text, token: token);
            if (!string.IsNullOrEmpty(strExtra))
                _objSpell.Extra = strExtra;
            _objSpell.Duration = await cboDuration.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token);

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token);
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Spell that was created in the dialogue.
        /// </summary>
        public Spell SelectedSpell => _objSpell;

        #endregion Properties
    }
}
