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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Chummer
{
    /// <summary>
    /// Target kind required when applying a reputation spend.
    /// </summary>
    public enum ReputationSpendTarget
    {
        /// <summary>
        /// No additional target selection is required.
        /// </summary>
        None = 0,

        /// <summary>
        /// A contact must be selected.
        /// </summary>
        Contact,

        /// <summary>
        /// A lifestyle must be selected.
        /// </summary>
        Lifestyle,

        /// <summary>
        /// A faction name must be provided.
        /// </summary>
        Faction
    }

    /// <summary>
    /// Explicit action performed by a reputation spend beyond generic bonus nodes.
    /// </summary>
    public enum ReputationSpendActionType
    {
        /// <summary>
        /// Customdata-only or bonus-driven spend with no hardcoded action.
        /// </summary>
        Custom = 0,

        /// <summary>
        /// Permanently reduce Street Cred (manual modifier).
        /// </summary>
        ManualSubtractStreetCred,

        /// <summary>
        /// Burn Street Cred to reduce Notoriety (legacy burn behavior).
        /// </summary>
        ReduceNotoriety,

        /// <summary>
        /// Increase a contact's Loyalty rating.
        /// </summary>
        IncreaseContactLoyalty,

        /// <summary>
        /// Increase a contact's Connection rating.
        /// </summary>
        IncreaseContactConnection,

        /// <summary>
        /// Increase faction reputation.
        /// </summary>
        IncreaseFactionReputation,

        /// <summary>
        /// Reduce lifestyle payment cost for the current lifestyle.
        /// </summary>
        ReduceLifestyleCost
    }

    /// <summary>
    /// Definition of a Street Cred / Karma reputation spend loaded from reputationspends.xml.
    /// </summary>
    public sealed class ReputationSpendDefinition
    {
        /// <summary>
        /// Creates an empty reputation spend definition.
        /// </summary>
        public ReputationSpendDefinition()
        {
        }

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public string Id { get; private set; } = string.Empty;

        /// <summary>
        /// Display name.
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Translated display name when available.
        /// </summary>
        public string DisplayName { get; private set; } = string.Empty;

        /// <summary>
        /// Category used for filtering.
        /// </summary>
        public string Category { get; private set; } = string.Empty;

        /// <summary>
        /// Source book abbreviation.
        /// </summary>
        public string Source { get; private set; } = string.Empty;

        /// <summary>
        /// Source page.
        /// </summary>
        public string Page { get; private set; } = string.Empty;

        /// <summary>
        /// Descriptive notes shown in the selector.
        /// </summary>
        public string Notes { get; private set; } = string.Empty;

        /// <summary>
        /// Fixed Street Cred cost. Zero when cost is calculated dynamically.
        /// </summary>
        public int StreetCredCost { get; private set; }

        /// <summary>
        /// Fixed Karma cost. Zero when cost is calculated dynamically.
        /// </summary>
        public int KarmaCost { get; private set; }

        /// <summary>
        /// When true, Street Cred and Karma may be combined to pay CostPoints.
        /// </summary>
        public bool AllowMixedCost { get; private set; }

        /// <summary>
        /// Total points that must be paid when AllowMixedCost is true (or when CostUsesNewRating is true).
        /// </summary>
        public int CostPoints { get; private set; }

        /// <summary>
        /// When true, cost equals the new rating of the targeted score (loyalty, faction reputation, etc.).
        /// </summary>
        public bool CostUsesNewRating { get; private set; }

        /// <summary>
        /// Yen reduced per Street Cred/Karma point for lifestyle discount spends.
        /// </summary>
        public int NuyenPerPoint { get; private set; }

        /// <summary>
        /// Maximum rating that may be reached via this spend.
        /// </summary>
        public int MaxRating { get; private set; }

        /// <summary>
        /// Minimum Loyalty required on the targeted contact (Connection spends).
        /// </summary>
        public int MinLoyalty { get; private set; }

        /// <summary>
        /// Months of delay before the change takes effect (informational / pending tracking).
        /// </summary>
        public int DelayMonths { get; private set; }

        /// <summary>
        /// When true, DelayMonths equals the new rating after the spend.
        /// </summary>
        public bool DelayUsesNewRating { get; private set; }

        /// <summary>
        /// When true, this spend may only be used once per target.
        /// </summary>
        public bool OncePerTarget { get; private set; }

        /// <summary>
        /// Target selection required by this spend.
        /// </summary>
        public ReputationSpendTarget Target { get; private set; }

        /// <summary>
        /// Explicit action performed by this spend.
        /// </summary>
        public ReputationSpendActionType ActionType { get; private set; }

        /// <summary>
        /// Flat amount applied by the action (e.g. notoriety reduced by 1).
        /// </summary>
        public int ActionAmount { get; private set; } = 1;

        /// <summary>
        /// Mapped expense type used for undo.
        /// </summary>
        public ReputationExpenseType ExpenseType { get; private set; }

        /// <summary>
        /// Optional bonus XML fragment for improvement-based effects.
        /// </summary>
        public XPathNavigator BonusNode { get; private set; }

        /// <summary>
        /// Loads a definition from an XML node.
        /// </summary>
        /// <param name="xmlNode">Node to load.</param>
        /// <returns>True if the node was valid.</returns>
        public bool Load(XPathNavigator xmlNode)
        {
            if (xmlNode == null)
                return false;

            Id = xmlNode.SelectSingleNodeAndCacheExpression("id")?.Value ?? string.Empty;
            Name = xmlNode.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty;
            DisplayName = xmlNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
            Category = xmlNode.SelectSingleNodeAndCacheExpression("category")?.Value ?? string.Empty;
            Source = xmlNode.SelectSingleNodeAndCacheExpression("source")?.Value ?? string.Empty;
            Page = xmlNode.SelectSingleNodeAndCacheExpression("page")?.Value ?? string.Empty;
            Notes = xmlNode.SelectSingleNodeAndCacheExpression("notes")?.Value
                    ?? xmlNode.SelectSingleNodeAndCacheExpression("altnotes")?.Value
                    ?? string.Empty;

            XPathNavigator xmlCost = xmlNode.SelectSingleNodeAndCacheExpression("cost");
            if (xmlCost != null)
            {
                StreetCredCost = ParseInt(xmlCost.SelectSingleNodeAndCacheExpression("streetcred")?.Value);
                KarmaCost = ParseInt(xmlCost.SelectSingleNodeAndCacheExpression("karma")?.Value);
                CostPoints = ParseInt(xmlCost.SelectSingleNodeAndCacheExpression("points")?.Value);
                AllowMixedCost = xmlCost.SelectSingleNodeAndCacheExpression("allowmixed") != null
                                 || xmlCost.SelectSingleNodeAndCacheExpression("points") != null;
                CostUsesNewRating = xmlCost.SelectSingleNodeAndCacheExpression("usenewrating") != null;
                NuyenPerPoint = ParseInt(xmlCost.SelectSingleNodeAndCacheExpression("nuyenperpoint")?.Value);
            }

            XPathNavigator xmlRules = xmlNode.SelectSingleNodeAndCacheExpression("rules");
            if (xmlRules != null)
            {
                MaxRating = ParseInt(xmlRules.SelectSingleNodeAndCacheExpression("maxrating")?.Value);
                MinLoyalty = ParseInt(xmlRules.SelectSingleNodeAndCacheExpression("minloyalty")?.Value);
                DelayMonths = ParseInt(xmlRules.SelectSingleNodeAndCacheExpression("delaymonths")?.Value);
                DelayUsesNewRating = xmlRules.SelectSingleNodeAndCacheExpression("delayusesnewrating") != null;
                OncePerTarget = xmlRules.SelectSingleNodeAndCacheExpression("oncepertarget") != null;
            }

            string strTarget = xmlNode.SelectSingleNodeAndCacheExpression("targets/target")?.Value ?? "none";
            Target = Enum.TryParse(strTarget, true, out ReputationSpendTarget eTarget)
                ? eTarget
                : ReputationSpendTarget.None;

            XPathNavigator xmlAction = xmlNode.SelectSingleNodeAndCacheExpression("effects/action");
            if (xmlAction != null)
            {
                string strType = xmlAction.GetAttribute("type", string.Empty);
                if (string.IsNullOrEmpty(strType))
                    strType = "Custom";
                ActionType = Enum.TryParse(strType, true, out ReputationSpendActionType eAction)
                    ? eAction
                    : ReputationSpendActionType.Custom;
                string strAmount = xmlAction.GetAttribute("amount", string.Empty);
                ActionAmount = string.IsNullOrEmpty(strAmount) ? 1 : ParseInt(strAmount);
            }

            BonusNode = xmlNode.SelectSingleNodeAndCacheExpression("effects/bonus");

            ExpenseType = MapExpenseType(ActionType);
            return !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Name);
        }

        private static int ParseInt(string strValue)
        {
            return int.TryParse(strValue, NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out int intReturn)
                ? intReturn
                : 0;
        }

        private static ReputationExpenseType MapExpenseType(ReputationSpendActionType eAction)
        {
            switch (eAction)
            {
                case ReputationSpendActionType.ManualSubtractStreetCred:
                    return ReputationExpenseType.ManualSubtractStreetCred;
                case ReputationSpendActionType.ReduceNotoriety:
                    return ReputationExpenseType.ReduceNotoriety;
                case ReputationSpendActionType.IncreaseContactLoyalty:
                    return ReputationExpenseType.IncreaseContactLoyalty;
                case ReputationSpendActionType.IncreaseContactConnection:
                    return ReputationExpenseType.IncreaseContactConnection;
                case ReputationSpendActionType.IncreaseFactionReputation:
                    return ReputationExpenseType.IncreaseFactionReputation;
                case ReputationSpendActionType.ReduceLifestyleCost:
                    return ReputationExpenseType.ReduceLifestyleCost;
                default:
                    return ReputationExpenseType.CustomSpend;
            }
        }

        /// <summary>
        /// Loads all reputation spend definitions for a character's enabled custom data.
        /// </summary>
        /// <param name="objCharacter">Character whose settings drive data loading.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Loaded definitions.</returns>
        public static async Task<List<ReputationSpendDefinition>> LoadAllAsync(Character objCharacter,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));

            List<ReputationSpendDefinition> lstReturn = new List<ReputationSpendDefinition>();
            XPathNavigator xmlRoot = await objCharacter.LoadDataXPathAsync("reputationspends.xml", token: token)
                                                       .ConfigureAwait(false);
            if (xmlRoot == null)
                return lstReturn;

            foreach (XPathNavigator xmlNode in xmlRoot.SelectAndCacheExpression("/chummer/reputationspends/reputationspend"))
            {
                ReputationSpendDefinition objDefinition = new ReputationSpendDefinition();
                if (objDefinition.Load(xmlNode))
                    lstReturn.Add(objDefinition);
            }

            return lstReturn;
        }

        /// <summary>
        /// Formats the fixed cost for display.
        /// </summary>
        public string GetCostDisplay(CultureInfo objCulture = null)
        {
            objCulture = objCulture ?? GlobalSettings.CultureInfo;
            if (CostUsesNewRating)
                return LanguageManager.GetString("String_ReputationSpend_CostNewRating");
            if (NuyenPerPoint > 0)
            {
                return string.Format(objCulture,
                    LanguageManager.GetString("String_ReputationSpend_CostPerNuyen"),
                    NuyenPerPoint.ToString("#,0", objCulture));
            }

            if (AllowMixedCost && CostPoints > 0)
            {
                return string.Format(objCulture,
                    LanguageManager.GetString("String_ReputationSpend_CostMixed"),
                    CostPoints.ToString(objCulture));
            }

            List<string> lstParts = new List<string>(2);
            if (StreetCredCost > 0)
            {
                lstParts.Add(StreetCredCost.ToString(objCulture) + LanguageManager.GetString("String_Space")
                             + LanguageManager.GetString("String_StreetCred"));
            }

            if (KarmaCost > 0)
            {
                lstParts.Add(KarmaCost.ToString(objCulture) + LanguageManager.GetString("String_Space")
                             + LanguageManager.GetString("String_Karma"));
            }

            return lstParts.Count > 0
                ? string.Join(LanguageManager.GetString("String_Space") + "+" + LanguageManager.GetString("String_Space"),
                    lstParts)
                : LanguageManager.GetString("String_None");
        }
    }
}
