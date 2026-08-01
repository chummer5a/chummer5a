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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public sealed partial class Character
    {
        /// <summary>
        /// Parameters describing how a reputation spend should be paid and applied.
        /// </summary>
        public sealed class ReputationSpendApplication
        {
            /// <summary>
            /// Spend definition being applied.
            /// </summary>
            public ReputationSpendDefinition Definition { get; set; }

            /// <summary>
            /// Street Cred points to permanently spend.
            /// </summary>
            public int StreetCredSpent { get; set; }

            /// <summary>
            /// Karma points to permanently spend.
            /// </summary>
            public int KarmaSpent { get; set; }

            /// <summary>
            /// Targeted contact, when required.
            /// </summary>
            public Contact TargetContact { get; set; }

            /// <summary>
            /// Targeted lifestyle, when required.
            /// </summary>
            public Lifestyle TargetLifestyle { get; set; }

            /// <summary>
            /// Faction name, when required.
            /// </summary>
            public string FactionName { get; set; } = string.Empty;

            /// <summary>
            /// Nuyen amount reduced for lifestyle discount spends.
            /// </summary>
            public decimal LifestyleDiscountNuyen { get; set; }
        }

        /// <summary>
        /// Validates whether a reputation spend can be applied with the given parameters.
        /// </summary>
        /// <param name="objApplication">Application parameters.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Empty string when valid; otherwise an error message.</returns>
        public async Task<string> ValidateReputationSpendAsync(ReputationSpendApplication objApplication,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objApplication?.Definition == null)
                return await LanguageManager.GetStringAsync("Message_ReputationSpend_Invalid", token: token)
                                            .ConfigureAwait(false);

            ReputationSpendDefinition objDefinition = objApplication.Definition;
            if (!await GetCreatedAsync(token).ConfigureAwait(false))
                return await LanguageManager.GetStringAsync("Message_ReputationSpend_CareerOnly", token: token)
                                            .ConfigureAwait(false);

            int intStreetCredSpent = Math.Max(0, objApplication.StreetCredSpent);
            int intKarmaSpent = Math.Max(0, objApplication.KarmaSpent);

            if (intStreetCredSpent > await GetTotalStreetCredAsync(token).ConfigureAwait(false))
                return await LanguageManager.GetStringAsync("Message_ReputationSpend_NotEnoughStreetCred", token: token)
                                            .ConfigureAwait(false);
            if (intKarmaSpent > await GetKarmaAsync(token).ConfigureAwait(false))
                return await LanguageManager.GetStringAsync("Message_ReputationSpend_NotEnoughKarma", token: token)
                                            .ConfigureAwait(false);

            int intRequiredPoints = await GetRequiredCostPointsAsync(objApplication, token).ConfigureAwait(false);
            if (intStreetCredSpent + intKarmaSpent < intRequiredPoints)
                return await LanguageManager.GetStringAsync("Message_ReputationSpend_InsufficientCost", token: token)
                                            .ConfigureAwait(false);

            switch (objDefinition.Target)
            {
                case ReputationSpendTarget.Contact:
                    if (objApplication.TargetContact == null)
                        return await LanguageManager.GetStringAsync("Message_ReputationSpend_SelectContact", token: token)
                                                    .ConfigureAwait(false);
                    break;
                case ReputationSpendTarget.Lifestyle:
                    if (objApplication.TargetLifestyle == null)
                        return await LanguageManager.GetStringAsync("Message_ReputationSpend_SelectLifestyle", token: token)
                                                    .ConfigureAwait(false);
                    break;
                case ReputationSpendTarget.Faction:
                    if (string.IsNullOrWhiteSpace(objApplication.FactionName))
                        return await LanguageManager.GetStringAsync("Message_ReputationSpend_SelectFaction", token: token)
                                                    .ConfigureAwait(false);
                    break;
            }

            switch (objDefinition.ActionType)
            {
                case ReputationSpendActionType.IncreaseContactLoyalty:
                {
                    int intNewLoyalty = await objApplication.TargetContact.GetLoyaltyAsync(token).ConfigureAwait(false)
                                        + objDefinition.ActionAmount;
                    if (objDefinition.MaxRating > 0 && intNewLoyalty > objDefinition.MaxRating)
                        return await LanguageManager.GetStringAsync("Message_ReputationSpend_MaxLoyalty", token: token)
                                                    .ConfigureAwait(false);
                    break;
                }
                case ReputationSpendActionType.IncreaseContactConnection:
                {
                    if (objDefinition.MinLoyalty > 0
                        && await objApplication.TargetContact.GetLoyaltyAsync(token).ConfigureAwait(false)
                        < objDefinition.MinLoyalty)
                    {
                        return await LanguageManager.GetStringAsync("Message_ReputationSpend_MinLoyalty", token: token)
                                                    .ConfigureAwait(false);
                    }

                    if (objDefinition.OncePerTarget
                        && HasContactConnectionReputationSpend(objApplication.TargetContact.InternalId))
                    {
                        return await LanguageManager.GetStringAsync("Message_ReputationSpend_ConnectionOnce", token: token)
                                                    .ConfigureAwait(false);
                    }

                    break;
                }
                case ReputationSpendActionType.IncreaseFactionReputation:
                {
                    int intNew = GetFactionReputation(objApplication.FactionName) + objDefinition.ActionAmount;
                    if (objDefinition.MaxRating > 0 && intNew > objDefinition.MaxRating)
                        return await LanguageManager.GetStringAsync("Message_ReputationSpend_MaxFaction", token: token)
                                                    .ConfigureAwait(false);
                    break;
                }
                case ReputationSpendActionType.ReduceLifestyleCost:
                {
                    if (objApplication.LifestyleDiscountNuyen <= 0)
                        return await LanguageManager.GetStringAsync("Message_ReputationSpend_InvalidDiscount", token: token)
                                                    .ConfigureAwait(false);
                    break;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Applies a reputation spend, logging expenses and mutating character state.
        /// </summary>
        /// <param name="objApplication">Application parameters.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True when applied successfully.</returns>
        public async Task<bool> ApplyReputationSpendAsync(ReputationSpendApplication objApplication,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strError = await ValidateReputationSpendAsync(objApplication, token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strError))
                return false;

            ReputationSpendDefinition objDefinition = objApplication.Definition;
            int intStreetCredSpent = Math.Max(0, objApplication.StreetCredSpent);
            int intKarmaSpent = Math.Max(0, objApplication.KarmaSpent);
            string strReason = objDefinition.DisplayName;
            string strTargetId = string.Empty;
            string strExtra = string.Empty;
            int intDelayMonths = objDefinition.DelayMonths;
            string strImprovementSourceName = objDefinition.Id;

            // Target-scoped source name (shared by typed actions and effects/bonus).
            if (objApplication.TargetContact != null)
            {
                strTargetId = objApplication.TargetContact.InternalId;
                strImprovementSourceName = objDefinition.Id + "|" + strTargetId;
            }
            else if (objApplication.TargetLifestyle != null)
            {
                strTargetId = objApplication.TargetLifestyle.InternalId;
                strImprovementSourceName = objDefinition.Id + "|" + strTargetId;
            }
            else if (!string.IsNullOrWhiteSpace(objApplication.FactionName))
            {
                strTargetId = objApplication.FactionName.Trim();
                strImprovementSourceName = objDefinition.Id + "|" + strTargetId;
            }

            // Apply effects/bonus first so a cancelled select dialog aborts before mutating the character.
            bool blnBonusApplied = false;
            if (objDefinition.BonusNode != null)
            {
                XmlDocument xmlBonusDocument = new XmlDocument { XmlResolver = null };
                XmlNode nodBonus = objDefinition.BonusNode.ToXmlNode(xmlBonusDocument);
                if (nodBonus?.HasChildNodes == true)
                {
                    if (!await ImprovementManager.CreateImprovementsAsync(
                            this, Improvement.ImprovementSource.ReputationSpend, strImprovementSourceName, nodBonus,
                            Math.Max(1, objDefinition.ActionAmount), objDefinition.DisplayName, token: token)
                        .ConfigureAwait(false))
                    {
                        return false;
                    }

                    blnBonusApplied = true;
                }
            }

            switch (objDefinition.ActionType)
            {
                case ReputationSpendActionType.ReduceNotoriety:
                    intStreetCredSpent = Math.Max(intStreetCredSpent, 2 * Math.Max(1, objDefinition.ActionAmount));
                    await ModifyBurntStreetCredAsync(intStreetCredSpent, token).ConfigureAwait(false);
                    strExtra = "burnt";
                    break;

                case ReputationSpendActionType.ManualSubtractStreetCred:
                    if (intStreetCredSpent <= 0)
                        intStreetCredSpent = Math.Max(1, objDefinition.StreetCredCost);
                    await ModifySpentStreetCredAsync(intStreetCredSpent, token).ConfigureAwait(false);
                    strExtra = "spent";
                    break;

                case ReputationSpendActionType.IncreaseContactLoyalty:
                {
                    Contact objContact = objApplication.TargetContact;
                    strTargetId = objContact.InternalId;
                    int intOldLoyalty = await objContact.GetLoyaltyAsync(token).ConfigureAwait(false);
                    int intNewLoyalty = intOldLoyalty + objDefinition.ActionAmount;
                    if (objDefinition.DelayUsesNewRating)
                        intDelayMonths = intNewLoyalty;
                    // TODO: Calendar marker for the Loyalty Rating increase delay.
                    await objContact.SetLoyaltyAsync(intNewLoyalty, token).ConfigureAwait(false);
                    strExtra = intOldLoyalty.ToString(GlobalSettings.InvariantCultureInfo) + "|"
                               + intDelayMonths.ToString(GlobalSettings.InvariantCultureInfo);
                    break;
                }

                case ReputationSpendActionType.IncreaseContactConnection:
                {
                    Contact objContact = objApplication.TargetContact;
                    strTargetId = objContact.InternalId;
                    int intOldConnection = await objContact.GetConnectionAsync(token).ConfigureAwait(false);
                    await objContact.SetConnectionAsync(intOldConnection + objDefinition.ActionAmount, token)
                                    .ConfigureAwait(false);
                    strImprovementSourceName = objDefinition.Id + "|" + objContact.InternalId;
                    ImprovementManager.CreateImprovement(this, objContact.InternalId,
                        Improvement.ImprovementSource.ReputationSpend, strImprovementSourceName,
                        Improvement.ImprovementType.ContactConnectionBoughtWithReputation, string.Empty, 1,
                        token: token);
                    ImprovementManager.Commit(this, token);
                    strExtra = intOldConnection.ToString(GlobalSettings.InvariantCultureInfo) + "|"
                               + strImprovementSourceName;
                    break;
                }

                case ReputationSpendActionType.IncreaseFactionReputation:
                {
                    string strFaction = objApplication.FactionName.Trim();
                    strTargetId = strFaction;
                    int intOld = GetFactionReputation(strFaction);
                    int intNew = intOld + objDefinition.ActionAmount;
                    if (objDefinition.DelayUsesNewRating)
                        intDelayMonths = intNew;
                    strImprovementSourceName = objDefinition.Id + "|" + strFaction;
                    SetFactionReputation(strFaction, intNew, strImprovementSourceName);
                    strExtra = intOld.ToString(GlobalSettings.InvariantCultureInfo) + "|"
                               + intDelayMonths.ToString(GlobalSettings.InvariantCultureInfo) + "|"
                               + strImprovementSourceName;
                    break;
                }

                case ReputationSpendActionType.ReduceLifestyleCost:
                {
                    Lifestyle objLifestyle = objApplication.TargetLifestyle;
                    strTargetId = objLifestyle.InternalId;
                    decimal decDiscount = objApplication.LifestyleDiscountNuyen;
                    decimal decMonthly = objLifestyle.GetTotalMonthlyCost(false);
                    if (decMonthly <= 0)
                    {
                        if (blnBonusApplied)
                        {
                            ImprovementManager.RemoveImprovements(this,
                                Improvement.ImprovementSource.ReputationSpend, strImprovementSourceName, token);
                        }

                        return false;
                    }

                    decimal decPercent = -Math.Round(decDiscount / decMonthly * 100.0m, 2, MidpointRounding.AwayFromZero);
                    strImprovementSourceName = objDefinition.Id + "|" + objLifestyle.InternalId;
                    ImprovementManager.CreateImprovement(this, objLifestyle.BaseLifestyle,
                        Improvement.ImprovementSource.ReputationSpend, strImprovementSourceName,
                        Improvement.ImprovementType.LifestyleCost, objLifestyle.InternalId, decPercent,
                        token: token);
                    ImprovementManager.Commit(this, token);
                    strExtra = decDiscount.ToString(GlobalSettings.InvariantCultureInfo) + "|"
                               + decPercent.ToString(GlobalSettings.InvariantCultureInfo) + "|"
                               + strImprovementSourceName;
                    break;
                }

                default:
                    break;
            }

            // Keep the bonus source name in Extra so undo can RemoveImprovements.
            if (blnBonusApplied)
            {
                if (string.IsNullOrEmpty(strExtra))
                    strExtra = strImprovementSourceName;
                else if (!strExtra.EndsWith(strImprovementSourceName, StringComparison.Ordinal))
                    strExtra += "|" + strImprovementSourceName;
            }

            // Pay Street Cred for non-burn / non-manual-subtract paths (those already deducted above).
            if (intStreetCredSpent > 0
                && objDefinition.ActionType != ReputationSpendActionType.ReduceNotoriety
                && objDefinition.ActionType != ReputationSpendActionType.ManualSubtractStreetCred)
            {
                await ModifySpentStreetCredAsync(intStreetCredSpent, token).ConfigureAwait(false);
            }

            if (intKarmaSpent > 0)
            {
                await ModifyKarmaAsync(-intKarmaSpent, token).ConfigureAwait(false);
                ExpenseLogEntry objKarmaExpense = new ExpenseLogEntry(this);
                objKarmaExpense.Create(-intKarmaSpent,
                    strReason + LanguageManager.GetString("String_Space", token: token)
                              + "(" + LanguageManager.GetString("String_Karma", token: token) + ")",
                    ExpenseType.Karma, DateTime.Now);
                objKarmaExpense.Undo = new ExpenseUndo().CreateKarma(KarmaExpenseType.ManualSubtract, strTargetId);
                await ExpenseEntries.AddWithSortAsync(objKarmaExpense, token: token).ConfigureAwait(false);
            }

            // Reputation expense tracks undo (including bonus-only / karma-only Custom spends).
            bool blnWriteRepExpense = intStreetCredSpent > 0
                                      || blnBonusApplied
                                      || objDefinition.ActionType == ReputationSpendActionType.Custom;
            if (blnWriteRepExpense)
            {
                ExpenseLogEntry objRepExpense = new ExpenseLogEntry(this);
                string strRepReason = strReason;
                if (intDelayMonths > 0)
                {
                    strRepReason += LanguageManager.GetString("String_Space", token: token) + "("
                                    + string.Format(GlobalSettings.CultureInfo,
                                        LanguageManager.GetString("String_ReputationSpend_DelayMonths", token: token),
                                        intDelayMonths) + ")";
                }

                objRepExpense.Create(-intStreetCredSpent, strRepReason, ExpenseType.Reputation, DateTime.Now);
                objRepExpense.Undo = new ExpenseUndo().CreateReputation(objDefinition.ExpenseType,
                    string.IsNullOrEmpty(strTargetId) ? objDefinition.Id : strTargetId,
                    intStreetCredSpent, strExtra);
                await ExpenseEntries.AddWithSortAsync(objRepExpense, token: token).ConfigureAwait(false);
            }

            return true;
        }

        /// <summary>
        /// Undoes a previously applied reputation expense.
        /// </summary>
        /// <param name="objExpense">Expense to undo.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True when undone.</returns>
        public async Task<bool> UndoReputationSpendAsync(ExpenseLogEntry objExpense, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objExpense?.Undo == null || objExpense.Type != ExpenseType.Reputation)
                return false;

            ExpenseUndo objUndo = objExpense.Undo;
            int intStreetCred = decimal.ToInt32(objUndo.Qty);
            string strObjectId = objUndo.ObjectId;
            string[] astrExtra = (objUndo.Extra ?? string.Empty).Split('|');

            switch (objUndo.ReputationType)
            {
                case ReputationExpenseType.ReduceNotoriety:
                    await ModifyBurntStreetCredAsync(-intStreetCred, token).ConfigureAwait(false);
                    // Optional effects/bonus source name is appended after "burnt".
                    if (astrExtra.Length > 1 && !string.IsNullOrEmpty(astrExtra[1]))
                    {
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ReputationSpend,
                            astrExtra[1], token);
                    }

                    break;

                case ReputationExpenseType.CustomSpend:
                {
                    await ModifySpentStreetCredAsync(-intStreetCred, token).ConfigureAwait(false);
                    string strCustomSource = astrExtra.Length > 0
                        ? astrExtra[astrExtra.Length - 1]
                        : strObjectId;
                    if (!string.IsNullOrEmpty(strCustomSource))
                    {
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ReputationSpend,
                            strCustomSource, token);
                    }

                    break;
                }

                case ReputationExpenseType.ManualSubtractStreetCred:
                    if (astrExtra.Length > 0
                        && string.Equals(astrExtra[0], "spent", StringComparison.OrdinalIgnoreCase))
                        await ModifySpentStreetCredAsync(-intStreetCred, token).ConfigureAwait(false);
                    else
                        await SetStreetCredAsync(await GetStreetCredAsync(token).ConfigureAwait(false) + intStreetCred,
                            token).ConfigureAwait(false);
                    // Optional effects/bonus source name is appended after "spent".
                    if (astrExtra.Length > 1 && !string.IsNullOrEmpty(astrExtra[1]))
                    {
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ReputationSpend,
                            astrExtra[1], token);
                    }

                    break;

                case ReputationExpenseType.ManualAddStreetCred:
                    await SetStreetCredAsync(await GetStreetCredAsync(token).ConfigureAwait(false) - intStreetCred,
                        token).ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualAddNotoriety:
                    await SetNotorietyAsync(await GetNotorietyAsync(token).ConfigureAwait(false) - intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualSubtractNotoriety:
                    await SetNotorietyAsync(await GetNotorietyAsync(token).ConfigureAwait(false) + intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualAddPublicAwareness:
                    await SetPublicAwarenessAsync(
                        await GetPublicAwarenessAsync(token).ConfigureAwait(false) - intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualSubtractPublicAwareness:
                    await SetPublicAwarenessAsync(
                        await GetPublicAwarenessAsync(token).ConfigureAwait(false) + intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualAddAstralReputation:
                    await SetAstralReputationAsync(
                        await GetAstralReputationAsync(token).ConfigureAwait(false) - intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualSubtractAstralReputation:
                    await SetAstralReputationAsync(
                        await GetAstralReputationAsync(token).ConfigureAwait(false) + intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualAddWildReputation:
                    await SetWildReputationAsync(
                        await GetWildReputationAsync(token).ConfigureAwait(false) - intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualSubtractWildReputation:
                    await SetWildReputationAsync(
                        await GetWildReputationAsync(token).ConfigureAwait(false) + intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualAddSpiritIndex:
                    await SetSpiritIndexAsync(
                        await GetSpiritIndexAsync(token).ConfigureAwait(false) - intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualSubtractSpiritIndex:
                    await SetSpiritIndexAsync(
                        await GetSpiritIndexAsync(token).ConfigureAwait(false) + intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualAddWildIndex:
                    await SetWildIndexAsync(
                        await GetWildIndexAsync(token).ConfigureAwait(false) - intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.ManualSubtractWildIndex:
                    await SetWildIndexAsync(
                        await GetWildIndexAsync(token).ConfigureAwait(false) + intStreetCred, token)
                        .ConfigureAwait(false);
                    break;

                case ReputationExpenseType.IncreaseContactLoyalty:
                {
                    Contact objContact = await Contacts.FirstOrDefaultAsync(x => x.InternalId == strObjectId, token)
                                                       .ConfigureAwait(false);
                    if (objContact != null
                        && astrExtra.Length > 0
                        && int.TryParse(astrExtra[0], NumberStyles.Integer, GlobalSettings.InvariantCultureInfo,
                            out int intOldLoyalty))
                    {
                        await objContact.SetLoyaltyAsync(intOldLoyalty, token).ConfigureAwait(false);
                    }

                    // Optional effects/bonus source name is appended as Extra[2].
                    if (astrExtra.Length > 2 && !string.IsNullOrEmpty(astrExtra[2]))
                    {
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ReputationSpend,
                            astrExtra[2], token);
                    }

                    await ModifySpentStreetCredAsync(-intStreetCred, token).ConfigureAwait(false);
                    break;
                }

                case ReputationExpenseType.IncreaseContactConnection:
                {
                    Contact objContact = await Contacts.FirstOrDefaultAsync(x => x.InternalId == strObjectId, token)
                                                       .ConfigureAwait(false);
                    if (objContact != null
                        && astrExtra.Length > 0
                        && int.TryParse(astrExtra[0], NumberStyles.Integer, GlobalSettings.InvariantCultureInfo,
                            out int intOldConnection))
                    {
                        await objContact.SetConnectionAsync(intOldConnection, token).ConfigureAwait(false);
                    }

                    string strSourceName = astrExtra.Length > 1 ? astrExtra[1] : string.Empty;
                    if (!string.IsNullOrEmpty(strSourceName))
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ReputationSpend,
                            strSourceName, token);
                    await ModifySpentStreetCredAsync(-intStreetCred, token).ConfigureAwait(false);
                    break;
                }

                case ReputationExpenseType.IncreaseFactionReputation:
                {
                    if (astrExtra.Length > 0
                        && int.TryParse(astrExtra[0], NumberStyles.Integer, GlobalSettings.InvariantCultureInfo,
                            out int intOld))
                    {
                        string strSourceName = astrExtra.Length > 2 ? astrExtra[2] : string.Empty;
                        SetFactionReputation(strObjectId, intOld, strSourceName);
                    }

                    await ModifySpentStreetCredAsync(-intStreetCred, token).ConfigureAwait(false);
                    break;
                }

                case ReputationExpenseType.ReduceLifestyleCost:
                {
                    string strSourceName = astrExtra.Length > 2 ? astrExtra[2] : string.Empty;
                    if (!string.IsNullOrEmpty(strSourceName))
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ReputationSpend,
                            strSourceName, token);
                    await ModifySpentStreetCredAsync(-intStreetCred, token).ConfigureAwait(false);
                    break;
                }
            }

            await ExpenseEntries.RemoveAsync(objExpense, token: token).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Applies a manual reputation track adjustment with an expense log entry (Karma/Nuyen-style).
        /// </summary>
        /// <param name="eTrack">Track to adjust.</param>
        /// <param name="intAmount">Signed amount (positive gains, negative losses).</param>
        /// <param name="strReason">Reason for the expense log.</param>
        /// <param name="datDate">Expense date.</param>
        /// <param name="token">Cancellation token.</param>
        public async Task ApplyManualReputationAdjustmentAsync(ReputationTrack eTrack, int intAmount, string strReason,
            DateTime datDate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (intAmount == 0)
                return;

            ReputationExpenseType eExpenseType = GetManualExpenseType(eTrack, intAmount > 0);
            switch (eTrack)
            {
                case ReputationTrack.StreetCred:
                    await SetStreetCredAsync(await GetStreetCredAsync(token).ConfigureAwait(false) + intAmount, token)
                        .ConfigureAwait(false);
                    break;
                case ReputationTrack.Notoriety:
                    await SetNotorietyAsync(await GetNotorietyAsync(token).ConfigureAwait(false) + intAmount, token)
                        .ConfigureAwait(false);
                    break;
                case ReputationTrack.PublicAwareness:
                    await SetPublicAwarenessAsync(
                        await GetPublicAwarenessAsync(token).ConfigureAwait(false) + intAmount, token)
                        .ConfigureAwait(false);
                    break;
                case ReputationTrack.AstralReputation:
                    await SetAstralReputationAsync(
                        await GetAstralReputationAsync(token).ConfigureAwait(false) + intAmount, token)
                        .ConfigureAwait(false);
                    break;
                case ReputationTrack.WildReputation:
                    await SetWildReputationAsync(
                        await GetWildReputationAsync(token).ConfigureAwait(false) + intAmount, token)
                        .ConfigureAwait(false);
                    break;
                case ReputationTrack.SpiritIndex:
                    await SetSpiritIndexAsync(
                        await GetSpiritIndexAsync(token).ConfigureAwait(false) + intAmount, token)
                        .ConfigureAwait(false);
                    break;
                case ReputationTrack.WildIndex:
                    await SetWildIndexAsync(
                        await GetWildIndexAsync(token).ConfigureAwait(false) + intAmount, token)
                        .ConfigureAwait(false);
                    break;
            }

            ExpenseLogEntry objExpense = new ExpenseLogEntry(this);
            objExpense.Create(intAmount, strReason, ExpenseType.Reputation, datDate);
            objExpense.Undo = new ExpenseUndo().CreateReputation(eExpenseType, eTrack.ToString(),
                Math.Abs(intAmount), "manual");
            await ExpenseEntries.AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);
        }

        private static ReputationExpenseType GetManualExpenseType(ReputationTrack eTrack, bool blnGain)
        {
            switch (eTrack)
            {
                case ReputationTrack.Notoriety:
                    return blnGain
                        ? ReputationExpenseType.ManualAddNotoriety
                        : ReputationExpenseType.ManualSubtractNotoriety;
                case ReputationTrack.PublicAwareness:
                    return blnGain
                        ? ReputationExpenseType.ManualAddPublicAwareness
                        : ReputationExpenseType.ManualSubtractPublicAwareness;
                case ReputationTrack.AstralReputation:
                    return blnGain
                        ? ReputationExpenseType.ManualAddAstralReputation
                        : ReputationExpenseType.ManualSubtractAstralReputation;
                case ReputationTrack.WildReputation:
                    return blnGain
                        ? ReputationExpenseType.ManualAddWildReputation
                        : ReputationExpenseType.ManualSubtractWildReputation;
                case ReputationTrack.SpiritIndex:
                    return blnGain
                        ? ReputationExpenseType.ManualAddSpiritIndex
                        : ReputationExpenseType.ManualSubtractSpiritIndex;
                case ReputationTrack.WildIndex:
                    return blnGain
                        ? ReputationExpenseType.ManualAddWildIndex
                        : ReputationExpenseType.ManualSubtractWildIndex;
                default:
                    return blnGain
                        ? ReputationExpenseType.ManualAddStreetCred
                        : ReputationExpenseType.ManualSubtractStreetCred;
            }
        }

        /// <summary>
        /// Gets the current faction reputation score for a faction name.
        /// </summary>
        /// <param name="strFactionName">Faction name.</param>
        /// <returns>Current score.</returns>
        public int GetFactionReputation(string strFactionName)
        {
            if (string.IsNullOrWhiteSpace(strFactionName))
                return 0;
            return ImprovementManager.ValueOf(this, Improvement.ImprovementType.FactionReputation,
                strImprovedName: strFactionName).StandardRound();
        }

        /// <summary>
        /// Sets faction reputation by replacing existing matching improvements.
        /// </summary>
        /// <param name="strFactionName">Faction name.</param>
        /// <param name="intValue">New score.</param>
        /// <param name="strSourceName">Source name for the improvement.</param>
        public void SetFactionReputation(string strFactionName, int intValue, string strSourceName)
        {
            if (string.IsNullOrWhiteSpace(strFactionName))
                return;

            foreach (Improvement objImprovement in Improvements
                         .Where(x => x.ImproveType == Improvement.ImprovementType.FactionReputation
                                     && x.ImprovedName == strFactionName).ToList())
            {
                ImprovementManager.RemoveImprovements(this, objImprovement.ImproveSource, objImprovement.SourceName);
            }

            if (intValue <= 0)
                return;

            string strSource = string.IsNullOrEmpty(strSourceName)
                ? "FactionReputation|" + strFactionName
                : strSourceName;
            ImprovementManager.CreateImprovement(this, strFactionName,
                Improvement.ImprovementSource.ReputationSpend, strSource,
                Improvement.ImprovementType.FactionReputation, string.Empty, intValue);
            ImprovementManager.Commit(this);
        }

        /// <summary>
        /// Whether Connection for the given contact was already raised via reputation spend.
        /// </summary>
        /// <param name="strContactId">Contact internal id.</param>
        public bool HasContactConnectionReputationSpend(string strContactId)
        {
            return Improvements.Any(x =>
                x.Enabled && x.ImproveType == Improvement.ImprovementType.ContactConnectionBoughtWithReputation
                          && x.ImprovedName == strContactId);
        }

        private async Task<int> GetRequiredCostPointsAsync(ReputationSpendApplication objApplication,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            ReputationSpendDefinition objDefinition = objApplication.Definition;
            if (objDefinition.NuyenPerPoint > 0)
            {
                if (objApplication.LifestyleDiscountNuyen <= 0)
                    return int.MaxValue;
                return (int)Math.Ceiling(objApplication.LifestyleDiscountNuyen / objDefinition.NuyenPerPoint);
            }

            if (objDefinition.CostUsesNewRating)
            {
                switch (objDefinition.ActionType)
                {
                    case ReputationSpendActionType.IncreaseContactLoyalty:
                        if (objApplication.TargetContact == null)
                            return int.MaxValue;
                        return await objApplication.TargetContact.GetLoyaltyAsync(token).ConfigureAwait(false)
                               + objDefinition.ActionAmount;
                    case ReputationSpendActionType.IncreaseFactionReputation:
                        return GetFactionReputation(objApplication.FactionName) + objDefinition.ActionAmount;
                }
            }

            if (objDefinition.AllowMixedCost && objDefinition.CostPoints > 0)
                return objDefinition.CostPoints;
            if (objDefinition.StreetCredCost > 0 || objDefinition.KarmaCost > 0)
                return objDefinition.StreetCredCost + objDefinition.KarmaCost;

            return 0;
        }
    }
}
