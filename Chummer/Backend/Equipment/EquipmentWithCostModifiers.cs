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
using Chummer.Backend.Static;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Base class for equipment that supports dynamic cost modifiers.
    /// Provides shared implementation for cost modifier functionality.
    /// </summary>
    public abstract class EquipmentWithCostModifiers : IHasCostModifiers
    {
        protected Dictionary<string, bool> _dicEnabledCostModifiers = new Dictionary<string, bool>();

        /// <summary>
        /// The character object that owns this equipment.
        /// </summary>
        public virtual Character CharacterObject { get; } = null;

        /// <summary>
        /// The equipment type string used for filtering improvements (e.g., "weapon", "armor", "gear").
        /// </summary>
        public abstract string EquipmentType { get; }

        /// <summary>
        /// Dictionary of enabled cost modifier checkboxes and their states.
        /// </summary>
        public Dictionary<string, bool> EnabledCostModifiers
        {
            get => _dicEnabledCostModifiers;
            set => _dicEnabledCostModifiers = value ?? new Dictionary<string, bool>();
        }

        /// <summary>
        /// Get the base cost of this equipment (without modifiers).
        /// Must be implemented by derived classes.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Base cost</returns>
        protected abstract Task<decimal> GetBaseCostAsync(CancellationToken token = default);

        /// <summary>
        /// Get the total cost of this equipment (with modifiers applied).
        /// This method applies cost modifiers to the base cost calculated by GetBaseCostAsync.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Total cost</returns>
        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            decimal decBaseCost = await GetBaseCostAsync(token).ConfigureAwait(false);
            return await ApplyCostModifiersAsync(decBaseCost, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the total cost of this equipment (synchronous version).
        /// </summary>
        public decimal TotalCost => GetTotalCostAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Apply cost modifiers to a base cost value.
        /// This is the unified cost modifier application logic used by all equipment.
        /// </summary>
        /// <param name="decBaseCost">The base cost to apply modifiers to</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Final cost with modifiers applied</returns>
        protected async Task<decimal> ApplyCostModifiersAsync(decimal decBaseCost, CancellationToken token = default)
        {
            // Apply modifiers
            var objModifiers = new CostModifiers { BlackMarketMultiplier = this is ICanBlackMarketDiscount objBlackMarket && objBlackMarket.DiscountCost ? 0.9m : 1.0m };
            
            // Apply stored cost modifiers (including markup and dynamic improvements)
            foreach (KeyValuePair<string, bool> kvp in _dicEnabledCostModifiers)
            {
                if (kvp.Value)
                {
                    if (kvp.Key.StartsWith("Custom_"))
                    {
                        // Extract the modifier value from the key (e.g., "Custom_Markup_10" -> 10)
                        string strModifierValue = kvp.Key.Substring(kvp.Key.LastIndexOf('_') + 1);
                        if (decimal.TryParse(strModifierValue, out decimal decModifierValue))
                        {
                            if (kvp.Key.Contains("Markup"))
                            {
                                objModifiers.SetCustomModifier(kvp.Key, decModifierValue / 100.0m + 1.0m);
                            }
                            else
                            {
                                objModifiers.SetCustomModifier(kvp.Key, decModifierValue);
                            }
                        }
                    }
                    else
                    {
                        // Get the improvement to find the modifier value
                        List<Improvement> lstImprovements = await CharacterObject.GetCostModifierImprovementsAsync(EquipmentType, blnUserChoiceOnly: true, token: token).ConfigureAwait(false);
                        Improvement objImprovement = lstImprovements.FirstOrDefault(x => x.ImprovedName == kvp.Key);
                        if (objImprovement != null)
                        {
                            objModifiers.SetCustomModifier(kvp.Key, objImprovement.Value);
                        }
                    }
                }
            }
            
            return decBaseCost * objModifiers.GetTotalMultiplier();
        }

        /// <summary>
        /// Generate a detailed tooltip showing cost breakdown including base cost, modifiers, and final cost.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Formatted tooltip string</returns>
        public virtual async Task<string> GetCostTooltipAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            
            // Get base cost (without modifiers)
            decimal decBaseCost = await GetBaseCostAsync(token).ConfigureAwait(false);
            
            // Get final cost (with modifiers)
            decimal decFinalCost = await GetTotalCostAsync(token).ConfigureAwait(false);
            
            // Format costs
            string strFormat = await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false))
                .GetNuyenFormatAsync(token).ConfigureAwait(false);
            string strNuyen = await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
            
            string strBaseCost = decBaseCost.ToString(strFormat, GlobalSettings.CultureInfo) + strNuyen;
            string strFinalCost = decFinalCost.ToString(strFormat, GlobalSettings.CultureInfo) + strNuyen;
            
            // If no modifiers applied, just show the base cost
            if (Math.Abs(decBaseCost - decFinalCost) < 0.01m)
            {
                return strBaseCost;
            }
            
            // Build modifier list
            List<string> lstModifiers = new List<string>();
            
            // Check for Black Market Discount (if applicable)
            if (this is ICanBlackMarketDiscount objBlackMarket && objBlackMarket.DiscountCost)
            {
                lstModifiers.Add("-10% Black Market Discount");
            }
            
            // Check for stored cost modifiers
            foreach (KeyValuePair<string, bool> kvp in _dicEnabledCostModifiers)
            {
                if (kvp.Value) // If this modifier was enabled
                {
                    if (kvp.Key.StartsWith("Custom_"))
                    {
                        // Extract the modifier value from the key (e.g., "Custom_Markup_10" -> 10)
                        string strModifierValue = kvp.Key.Substring(kvp.Key.LastIndexOf('_') + 1);
                        if (decimal.TryParse(strModifierValue, out decimal decModifierValue))
                        {
                            if (kvp.Key.Contains("Markup"))
                            {
                                lstModifiers.Add($"+{decModifierValue}% Markup");
                            }
                            else
                            {
                                // Extract modifier name (e.g., "Custom_RangedWeaponDiscount_10" -> "RangedWeaponDiscount")
                                string strModifierName = kvp.Key.Substring(7, kvp.Key.LastIndexOf('_') - 7);
                                decimal decPercentage = (decModifierValue - 1.0m) * 100.0m;
                                string strSign = decPercentage >= 0 ? "+" : "";
                                lstModifiers.Add($"{strSign}{decPercentage:F0}% {strModifierName}");
                            }
                        }
                    }
                    else
                    {
                        // Get the improvement to find the modifier value
                        List<Improvement> lstImprovements = await CharacterObject.GetCostModifierImprovementsAsync(EquipmentType, blnUserChoiceOnly: true, token: token).ConfigureAwait(false);
                        Improvement objImprovement = lstImprovements.FirstOrDefault(x => x.ImprovedName == kvp.Key);
                        if (objImprovement != null)
                        {
                            decimal decPercentage = (objImprovement.Value - 1.0m) * 100.0m;
                            string strSign = decPercentage >= 0 ? "+" : "";
                            lstModifiers.Add($"{strSign}{decPercentage:F0}% {kvp.Key}");
                        }
                    }
                }
            }
            
            // Build tooltip string
            if (lstModifiers.Count == 0)
            {
                return strBaseCost;
            }
            
            string strModifierList = string.Join(", ", lstModifiers);
            return $"{strBaseCost}\n[{strModifierList}]\n= {strFinalCost}";
        }

        /// <summary>
        /// Clean up any enabled cost modifiers that are no longer valid (e.g., when improvements are removed).
        /// </summary>
        /// <param name="token">Cancellation token</param>
        public virtual void CleanupInvalidCostModifiers(CancellationToken token = default)
        {
            if (_dicEnabledCostModifiers.Count == 0)
                return;

            // Get all currently valid cost modifier improvements for this equipment type
            List<Improvement> lstValidImprovements = CharacterObject.GetCostModifierImprovements(EquipmentType, blnUserChoiceOnly: true, token: token);
            HashSet<string> setValidModifierNames = new HashSet<string>(lstValidImprovements.Select(x => x.ImprovedName));

            // Remove any enabled modifiers that are no longer valid
            List<string> lstModifiersToRemove = new List<string>();
            foreach (string strModifierName in _dicEnabledCostModifiers.Keys)
            {
                if (!setValidModifierNames.Contains(strModifierName) && !strModifierName.StartsWith("Custom_"))
                {
                    lstModifiersToRemove.Add(strModifierName);
                }
            }

            foreach (string strModifierName in lstModifiersToRemove)
            {
                _dicEnabledCostModifiers.Remove(strModifierName);
            }
        }

        /// <summary>
        /// Clean up any enabled cost modifiers that are no longer valid (async version).
        /// </summary>
        /// <param name="token">Cancellation token</param>
        public virtual async Task CleanupInvalidCostModifiersAsync(CancellationToken token = default)
        {
            if (_dicEnabledCostModifiers.Count == 0)
                return;

            // Get all currently valid cost modifier improvements for this equipment type
            List<Improvement> lstValidImprovements = await CharacterObject.GetCostModifierImprovementsAsync(EquipmentType, blnUserChoiceOnly: true, token: token).ConfigureAwait(false);
            HashSet<string> setValidModifierNames = new HashSet<string>(lstValidImprovements.Select(x => x.ImprovedName));

            // Remove any enabled modifiers that are no longer valid
            List<string> lstModifiersToRemove = new List<string>();
            foreach (string strModifierName in _dicEnabledCostModifiers.Keys)
            {
                if (!setValidModifierNames.Contains(strModifierName) && !strModifierName.StartsWith("Custom_"))
                {
                    lstModifiersToRemove.Add(strModifierName);
                }
            }

            foreach (string strModifierName in lstModifiersToRemove)
            {
                _dicEnabledCostModifiers.Remove(strModifierName);
            }
        }

        /// <summary>
        /// Set markup as a custom cost modifier on this equipment.
        /// </summary>
        /// <param name="decMarkupPercentage">Markup percentage (e.g., 10 for 10% markup)</param>
        public virtual void SetMarkup(decimal decMarkupPercentage)
        {
            // Remove any existing markup
            var lstKeysToRemove = _dicEnabledCostModifiers.Keys.Where(x => x.StartsWith("Custom_Markup_")).ToList();
            foreach (string strKey in lstKeysToRemove)
            {
                _dicEnabledCostModifiers.Remove(strKey);
            }
            
            // Add new markup if percentage > 0
            if (decMarkupPercentage > 0)
            {
                string strMarkupKey = $"Custom_Markup_{decMarkupPercentage}";
                _dicEnabledCostModifiers[strMarkupKey] = true;
            }
        }
    }
}
