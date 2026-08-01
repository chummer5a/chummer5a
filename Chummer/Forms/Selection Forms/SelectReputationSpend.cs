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
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer.Backend.Equipment;

namespace Chummer
{
    /// <summary>
    /// Career-mode selector for Street Cred / Karma reputation spends.
    /// </summary>
    public partial class SelectReputationSpend : Form
    {
        private readonly Character _objCharacter;
        private List<ReputationSpendDefinition> _lstDefinitions;
        private ReputationSpendDefinition _objSelectedDefinition;
        private readonly Character.ReputationSpendApplication _objApplication
            = new Character.ReputationSpendApplication();

        /// <summary>
        /// Creates the reputation spend selector.
        /// </summary>
        /// <param name="objCharacter">Character applying the spend.</param>
        public SelectReputationSpend(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
        }

        /// <summary>
        /// Application parameters chosen by the user.
        /// </summary>
        public Character.ReputationSpendApplication Application => _objApplication;

        private async void SelectReputationSpend_Load(object sender, EventArgs e)
        {
            _lstDefinitions = await ReputationSpendDefinition.LoadAllAsync(_objCharacter).ConfigureAwait(false);
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstItems))
            {
                foreach (ReputationSpendDefinition objDefinition in _lstDefinitions.OrderBy(x => x.Category)
                             .ThenBy(x => x.DisplayName))
                {
                    lstItems.Add(new ListItem(objDefinition.Id, objDefinition.DisplayName));
                }

                await lstSpends.PopulateWithListItemsAsync(lstItems).ConfigureAwait(false);
            }

            await RefreshTargetVisibility().ConfigureAwait(false);
            await UpdateCostControls().ConfigureAwait(false);
        }

        private async void lstSpends_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strId = await lstSpends.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            _objSelectedDefinition = _lstDefinitions?.FirstOrDefault(x => x.Id == strId);
            _objApplication.Definition = _objSelectedDefinition;
            await RefreshTargetVisibility().ConfigureAwait(false);
            await UpdateCostControls().ConfigureAwait(false);
            await RefreshNotes().ConfigureAwait(false);
        }

        private async Task RefreshNotes()
        {
            string strNotes = _objSelectedDefinition?.Notes ?? string.Empty;
            if (_objSelectedDefinition != null)
            {
                string strCost = _objSelectedDefinition.GetCostDisplay();
                strNotes = strCost + Environment.NewLine + Environment.NewLine + strNotes;
            }

            await lblNotes.DoThreadSafeAsync(x => x.Text = strNotes).ConfigureAwait(false);
        }

        private async Task RefreshTargetVisibility()
        {
            ReputationSpendTarget eTarget = _objSelectedDefinition?.Target ?? ReputationSpendTarget.None;
            await cboTarget.DoThreadSafeAsync(x => x.Visible = eTarget == ReputationSpendTarget.Contact
                                                              || eTarget == ReputationSpendTarget.Lifestyle)
                           .ConfigureAwait(false);
            await txtFaction.DoThreadSafeAsync(x => x.Visible = eTarget == ReputationSpendTarget.Faction)
                            .ConfigureAwait(false);
            await lblTarget.DoThreadSafeAsync(x =>
            {
                x.Visible = eTarget != ReputationSpendTarget.None;
                switch (eTarget)
                {
                    case ReputationSpendTarget.Contact:
                        x.Text = LanguageManager.GetString("String_Contact");
                        break;
                    case ReputationSpendTarget.Lifestyle:
                        x.Text = LanguageManager.GetString("String_Lifestyle");
                        break;
                    case ReputationSpendTarget.Faction:
                        x.Text = LanguageManager.GetString("String_ReputationSpend_Faction");
                        break;
                    default:
                        x.Text = string.Empty;
                        break;
                }
            }).ConfigureAwait(false);
            await nudDiscount.DoThreadSafeAsync(x =>
                               x.Visible = _objSelectedDefinition?.ActionType
                                           == ReputationSpendActionType.ReduceLifestyleCost)
                           .ConfigureAwait(false);
            await lblDiscount.DoThreadSafeAsync(x =>
                               x.Visible = _objSelectedDefinition?.ActionType
                                           == ReputationSpendActionType.ReduceLifestyleCost)
                           .ConfigureAwait(false);

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTargets))
            {
                switch (eTarget)
                {
                    case ReputationSpendTarget.Contact:
                        await _objCharacter.Contacts.ForEachAsync(async objContact =>
                        {
                            lstTargets.Add(new ListItem(objContact.InternalId,
                                await objContact.GetCurrentDisplayNameAsync().ConfigureAwait(false)));
                        }).ConfigureAwait(false);
                        break;
                    case ReputationSpendTarget.Lifestyle:
                        await _objCharacter.Lifestyles.ForEachAsync(async objLifestyle =>
                        {
                            lstTargets.Add(new ListItem(objLifestyle.InternalId,
                                await objLifestyle.GetCurrentDisplayNameAsync().ConfigureAwait(false)));
                        }).ConfigureAwait(false);
                        break;
                }

                await cboTarget.PopulateWithListItemsAsync(lstTargets).ConfigureAwait(false);
            }
        }

        private async Task UpdateCostControls()
        {
            bool blnMixed = _objSelectedDefinition?.AllowMixedCost == true
                            || _objSelectedDefinition?.CostUsesNewRating == true
                            || (_objSelectedDefinition?.NuyenPerPoint ?? 0) > 0;
            await nudStreetCred.DoThreadSafeAsync(x => x.Enabled = blnMixed || (_objSelectedDefinition?.StreetCredCost ?? 0) > 0)
                               .ConfigureAwait(false);
            await nudKarma.DoThreadSafeAsync(x => x.Enabled = blnMixed || (_objSelectedDefinition?.KarmaCost ?? 0) > 0)
                          .ConfigureAwait(false);

            int intStreetCred = _objSelectedDefinition?.StreetCredCost ?? 0;
            int intKarma = _objSelectedDefinition?.KarmaCost ?? 0;
            if (!blnMixed)
            {
                await nudStreetCred.DoThreadSafeAsync(x => x.Value = intStreetCred).ConfigureAwait(false);
                await nudKarma.DoThreadSafeAsync(x => x.Value = intKarma).ConfigureAwait(false);
            }
            else if (_objSelectedDefinition?.CostUsesNewRating == true)
            {
                int intPoints = await SuggestNewRatingCost().ConfigureAwait(false);
                await nudStreetCred.DoThreadSafeAsync(x => x.Value = intPoints).ConfigureAwait(false);
                await nudKarma.DoThreadSafeAsync(x => x.Value = 0).ConfigureAwait(false);
            }
            else if ((_objSelectedDefinition?.CostPoints ?? 0) > 0)
            {
                await nudStreetCred.DoThreadSafeAsync(x => x.Value = _objSelectedDefinition.CostPoints)
                                   .ConfigureAwait(false);
                await nudKarma.DoThreadSafeAsync(x => x.Value = 0).ConfigureAwait(false);
            }
        }

        private async Task<int> SuggestNewRatingCost()
        {
            if (_objSelectedDefinition == null)
                return 0;
            switch (_objSelectedDefinition.ActionType)
            {
                case ReputationSpendActionType.IncreaseContactLoyalty:
                {
                    string strId = await cboTarget.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString())
                                                  .ConfigureAwait(false);
                    Contact objContact = await _objCharacter.Contacts
                                                            .FirstOrDefaultAsync(x => x.InternalId == strId)
                                                            .ConfigureAwait(false);
                    if (objContact == null)
                        return 1;
                    return await objContact.GetLoyaltyAsync().ConfigureAwait(false)
                           + _objSelectedDefinition.ActionAmount;
                }
                case ReputationSpendActionType.IncreaseFactionReputation:
                {
                    string strFaction = await txtFaction.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
                    return _objCharacter.GetFactionReputation(strFaction) + _objSelectedDefinition.ActionAmount;
                }
                default:
                    return _objSelectedDefinition.CostPoints;
            }
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            if (_objSelectedDefinition == null)
                return;

            _objApplication.Definition = _objSelectedDefinition;
            _objApplication.StreetCredSpent =
                await nudStreetCred.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
            _objApplication.KarmaSpent =
                await nudKarma.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
            _objApplication.FactionName =
                await txtFaction.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
            _objApplication.LifestyleDiscountNuyen =
                await nudDiscount.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false);

            string strTargetId = await cboTarget.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString())
                                                .ConfigureAwait(false);
            _objApplication.TargetContact = null;
            _objApplication.TargetLifestyle = null;
            if (_objSelectedDefinition.Target == ReputationSpendTarget.Contact)
            {
                _objApplication.TargetContact = await _objCharacter.Contacts
                                                                   .FirstOrDefaultAsync(x => x.InternalId == strTargetId)
                                                                   .ConfigureAwait(false);
            }
            else if (_objSelectedDefinition.Target == ReputationSpendTarget.Lifestyle)
            {
                _objApplication.TargetLifestyle = await _objCharacter.Lifestyles
                                                                     .FirstOrDefaultAsync(x => x.InternalId == strTargetId)
                                                                     .ConfigureAwait(false);
            }

            if (_objSelectedDefinition.ActionType == ReputationSpendActionType.ReduceLifestyleCost
                && _objSelectedDefinition.NuyenPerPoint > 0)
            {
                int intRequired = (int)Math.Ceiling(_objApplication.LifestyleDiscountNuyen
                                                    / _objSelectedDefinition.NuyenPerPoint);
                if (_objApplication.StreetCredSpent + _objApplication.KarmaSpent < intRequired)
                {
                    _objApplication.StreetCredSpent = intRequired;
                    _objApplication.KarmaSpent = 0;
                }
            }

            string strError = await _objCharacter.ValidateReputationSpendAsync(_objApplication).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strError))
            {
                await Program.ShowScrollableMessageBoxAsync(this, strError, string.Empty, MessageBoxButtons.OK,
                    MessageBoxIcon.Error).ConfigureAwait(false);
                return;
            }

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }).ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cboTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_objSelectedDefinition?.CostUsesNewRating == true)
                await UpdateCostControls().ConfigureAwait(false);
        }

        private async void txtFaction_TextChanged(object sender, EventArgs e)
        {
            if (_objSelectedDefinition?.CostUsesNewRating == true)
                await UpdateCostControls().ConfigureAwait(false);
        }

        private async void nudDiscount_ValueChanged(object sender, EventArgs e)
        {
            if (_objSelectedDefinition?.NuyenPerPoint > 0)
            {
                decimal decDiscount = await nudDiscount.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false);
                int intRequired = (int)Math.Ceiling(decDiscount / _objSelectedDefinition.NuyenPerPoint);
                await nudStreetCred.DoThreadSafeAsync(x => x.Value = intRequired).ConfigureAwait(false);
                await nudKarma.DoThreadSafeAsync(x => x.Value = 0).ConfigureAwait(false);
            }
        }
    }
}
