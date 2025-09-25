using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using Chummer.Backend.Equipment;
using System.Xml.XPath;

namespace Chummer.Backend.Static
{
    /// <summary>
    /// Static class for handling all cost processing operations including Variable costs, cost modifiers, and cost parsing.
    /// 
    /// This class provides a unified method for processing all non-standard costs for equipment items:
    /// - Variable costs (e.g., "Variable(100-500)")
    /// - FixedValues expressions (e.g., "FixedValues(100,200,300)")
    /// - XPath expressions (e.g., "Rating * 50")
    /// - Simple numeric costs (e.g., "250")
    /// 
    /// Usage Examples:
    /// 
    /// 1. Simple cost processing for selection forms:
    /// <code>
    /// var modifiers = new CostModifiers { MarkupMultiplier = 1.2m };
    /// var (display, cost, success) = await CostProcessing.ProcessCostComprehensiveAsync(
    ///     "Variable(100-500)", 1, modifiers, "N0", "¥", CultureInfo.CurrentCulture,
    ///     character, "Weapon", true, false, false, false, token);
    /// </code>
    /// 
    /// 2. Using the context object for cleaner code:
    /// <code>
    /// var context = CostProcessingContext.ForSelectionForm("Variable(100-500)", 1, modifiers, character, "Weapon", token);
    /// var (display, cost, success) = await CostProcessing.ProcessCostComprehensiveAsync(context);
    /// </code>
    /// 
    /// 3. Using extension methods:
    /// <code>
    /// var (display, cost, success) = await "Variable(100-500)".ProcessCostForSelectionFormAsync(1, modifiers, character, "Weapon", token);
    /// </code>
    /// </summary>
    public static class CostProcessing
    {
        // Cache for common cost processing results to avoid repeated calculations
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string display, decimal cost, bool success)> _costCache =
            new System.Collections.Concurrent.ConcurrentDictionary<string, (string, decimal, bool)>();

        // Cache for cost modifiers to avoid repeated object creation
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, CostModifiers> _modifierCache =
            new System.Collections.Concurrent.ConcurrentDictionary<string, CostModifiers>();
        /// <summary>
        /// Prompts the user for a Variable cost value and returns the selected cost or cancellation signal.
        /// </summary>
        /// <param name="strCost">The cost string to process (should start with "Variable(")</param>
        /// <param name="objCharacter">Character object for context</param>
        /// <param name="strDisplayName">Display name for the item</param>
        /// <param name="blnForSelectForm">Whether this is being called from a selection form</param>
        /// <param name="blnSkipCost">Whether to skip cost prompting</param>
        /// <param name="blnSkipSelectForms">Whether to skip showing selection forms</param>
        /// <param name="blnCreateImprovements">Whether improvements are being created</param>
        /// <param name="strUpdatedCost">The updated cost string (output parameter)</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>True if the user cancelled, false if successful</returns>
        public static bool PromptForVariableCost(string strCost, Character objCharacter, string strDisplayName,
            bool blnForSelectForm, bool blnSkipCost, bool blnSkipSelectForms, bool blnCreateImprovements,
            out string strUpdatedCost, CancellationToken token = default)
        {
            var result = PromptForVariableCostInternal(strCost, objCharacter, strDisplayName, blnForSelectForm,
                blnSkipCost, blnSkipSelectForms, blnCreateImprovements, true, token).GetAwaiter().GetResult();
            strUpdatedCost = result.strUpdatedCost;
            return result.blnCancelled;
        }

        /// <summary>
        /// Asynchronously prompts the user for a Variable cost value and returns the selected cost or cancellation signal.
        /// </summary>
        /// <param name="strCost">The cost string to process (should start with "Variable(")</param>
        /// <param name="objCharacter">Character object for context</param>
        /// <param name="strDisplayName">Display name for the item</param>
        /// <param name="blnForSelectForm">Whether this is being called from a selection form</param>
        /// <param name="blnSkipCost">Whether to skip cost prompting</param>
        /// <param name="blnSkipSelectForms">Whether to skip showing selection forms</param>
        /// <param name="blnCreateImprovements">Whether improvements are being created</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Tuple containing cancellation status and updated cost string</returns>
        public static async Task<(bool blnCancelled, string strUpdatedCost)> PromptForVariableCostAsync(string strCost, Character objCharacter, string strDisplayName,
            bool blnForSelectForm, bool blnSkipCost, bool blnSkipSelectForms, bool blnCreateImprovements,
            CancellationToken token = default)
        {
            return await PromptForVariableCostInternal(strCost, objCharacter, strDisplayName, blnForSelectForm,
                blnSkipCost, blnSkipSelectForms, blnCreateImprovements, false, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Internal method that handles both sync and async Variable cost prompting.
        /// </summary>
        private static async Task<(bool blnCancelled, string strUpdatedCost)> PromptForVariableCostInternal(string strCost, Character objCharacter, string strDisplayName,
            bool blnForSelectForm, bool blnSkipCost, bool blnSkipSelectForms, bool blnCreateImprovements, bool blnSync,
            CancellationToken token = default)
        {
            string strUpdatedCost = strCost;

            // Check for a Variable Cost.
            if (blnForSelectForm || blnSkipCost || !strCost.StartsWith("Variable(", StringComparison.Ordinal))
                return (false, strUpdatedCost);

            string strFirstHalf = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
            string strSecondHalf = string.Empty;
            int intHyphenIndex = strFirstHalf.IndexOf('-');
            if (intHyphenIndex != -1)
            {
                if (intHyphenIndex + 1 < strFirstHalf.Length)
                    strSecondHalf = strFirstHalf.Substring(intHyphenIndex + 1);
                strFirstHalf = strFirstHalf.Substring(0, intHyphenIndex);
            }

            if (blnSkipSelectForms)
            {
                strUpdatedCost = strFirstHalf;
                return (false, strUpdatedCost);
            }

            decimal decMin = decimal.MinValue;
            decimal decMax = decimal.MaxValue;
            if (intHyphenIndex != -1)
            {
                decimal.TryParse(strFirstHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);
                decimal.TryParse(strSecondHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMax);
            }
            else
                decimal.TryParse(strFirstHalf.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);

            // Validate that we have meaningful bounds
            bool blnValidRange = decMin != decimal.MinValue || decMax != decimal.MaxValue;

            if (!blnValidRange)
            {
                strUpdatedCost = strFirstHalf;
                return (false, strUpdatedCost);
            }

            if (decMax > 1000000)
                decMax = 1000000;

            // Get description and decimal places
            string strDescription;
            int intDecimalPlaces;

            if (blnSync)
            {
                strDescription = string.Format(
                    GlobalSettings.CultureInfo,
                    LanguageManager.GetString("String_SelectVariableCost", token: token),
                    strDisplayName);
                intDecimalPlaces = objCharacter.Settings.MaxNuyenDecimals;
            }
            else
            {
                strDescription = string.Format(
                    GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("String_SelectVariableCost", token: token).ConfigureAwait(false),
                    strDisplayName);
                intDecimalPlaces = await (await objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMaxNuyenDecimalsAsync(token).ConfigureAwait(false);
            }

            // Show the form
            if (blnSync)
            {
                using (ThreadSafeForm<SelectNumber> frmPickNumber
                       // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                       = ThreadSafeForm<SelectNumber>.Get(() => new SelectNumber(intDecimalPlaces)
                       {
                           Minimum = decMin,
                           Maximum = decMax,
                           Description = strDescription,
                           AllowCancel = false
                       }))
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    if (frmPickNumber.ShowDialogSafe(objCharacter, token) == DialogResult.Cancel)
                        return (true, strUpdatedCost);

                    strUpdatedCost = frmPickNumber.MyForm.SelectedValue.ToString(GlobalSettings.InvariantCultureInfo);
                }
            }
            else
            {
                using (ThreadSafeForm<SelectNumber> frmPickNumber
                       = await ThreadSafeForm<SelectNumber>.GetAsync(() => new SelectNumber(intDecimalPlaces)
                       {
                           Minimum = decMin,
                           Maximum = decMax,
                           Description = strDescription,
                           AllowCancel = false
                       }, token).ConfigureAwait(false))
                {
                    if (await frmPickNumber.ShowDialogSafeAsync(objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                        return (true, strUpdatedCost);

                    strUpdatedCost = frmPickNumber.MyForm.SelectedValue.ToString(GlobalSettings.InvariantCultureInfo);
                }
            }

            return (false, strUpdatedCost);
        }

        /// <summary>
        /// Formats a Variable cost string for display purposes.
        /// Converts "Variable(min-max)" to "min¥-max¥" or "Variable(min+)" to "min¥+"
        /// </summary>
        /// <param name="strCost">The cost string to format</param>
        /// <param name="strFormat">The number format string</param>
        /// <param name="strNuyenSymbol">The currency symbol</param>
        /// <param name="objCulture">The culture info for formatting</param>
        /// <returns>Formatted cost string for display</returns>
        public static string FormatVariableCostForDisplay(string strCost, string strFormat, string strNuyenSymbol, CultureInfo objCulture)
        {
            return FormatVariableCostForDisplay(strCost, strFormat, strNuyenSymbol, objCulture, new CostModifiers());
        }

        /// <summary>
        /// Formats a Variable cost string for display purposes with cost modifiers.
        /// Converts "Variable(min-max)" to "min¥-max¥" or "Variable(min+)" to "min¥+" with modifiers applied
        /// </summary>
        /// <param name="strCost">The cost string to format</param>
        /// <param name="strFormat">The number format string</param>
        /// <param name="strNuyenSymbol">The currency symbol</param>
        /// <param name="objCulture">The culture info for formatting</param>
        /// <param name="decMarkupMultiplier">Markup multiplier (e.g., 1.2 for 20% markup)</param>
        /// <param name="decBlackMarketMultiplier">Black market discount multiplier (e.g., 0.9 for 10% discount)</param>
        /// <param name="decOtherMultiplier">Other cost multiplier (e.g., quantity, stolen, etc.)</param>
        /// <returns>Formatted cost string for display with modifiers applied</returns>
        public static string FormatVariableCostForDisplay(string strCost, string strFormat, string strNuyenSymbol, CultureInfo objCulture,
            decimal decMarkupMultiplier, decimal decBlackMarketMultiplier, decimal decOtherMultiplier)
        {
            return FormatVariableCostForDisplay(strCost, strFormat, strNuyenSymbol, objCulture,
                new CostModifiers(decMarkupMultiplier, decBlackMarketMultiplier, decOtherMultiplier));
        }

        /// <summary>
        /// Formats a Variable cost string for display purposes with comprehensive cost modifiers.
        /// Converts "Variable(min-max)" to "min¥-max¥" or "Variable(min+)" to "min¥+" with modifiers applied
        /// </summary>
        /// <param name="strCost">The cost string to format</param>
        /// <param name="strFormat">The number format string</param>
        /// <param name="strNuyenSymbol">The currency symbol</param>
        /// <param name="objCulture">The culture info for formatting</param>
        /// <param name="objModifiers">Cost modifiers to apply</param>
        /// <returns>Formatted cost string for display with modifiers applied</returns>
        public static string FormatVariableCostForDisplay(string strCost, string strFormat, string strNuyenSymbol, CultureInfo objCulture,
            CostModifiers objModifiers)
        {
            if (!strCost.StartsWith("Variable(", StringComparison.Ordinal))
                return strCost;

            string strFirstHalf = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
            string strSecondHalf = string.Empty;
            int intHyphenIndex = strFirstHalf.IndexOf('-');
            if (intHyphenIndex != -1)
            {
                if (intHyphenIndex + 1 < strFirstHalf.Length)
                    strSecondHalf = strFirstHalf.Substring(intHyphenIndex + 1);
                strFirstHalf = strFirstHalf.Substring(0, intHyphenIndex);
            }

            decimal decMin;
            decimal decMax = decimal.MaxValue;
            if (intHyphenIndex != -1)
            {
                decimal.TryParse(strFirstHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);
                decimal.TryParse(strSecondHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMax);
            }
            else
                decimal.TryParse(strFirstHalf.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);

            // Apply all cost modifiers
            decimal decTotalMultiplier = objModifiers.GetTotalMultiplier();
            decMin *= decTotalMultiplier;
            if (decMax != decimal.MaxValue)
                decMax *= decTotalMultiplier;

            if (decMax == decimal.MaxValue)
                return decMin.ToString(strFormat, objCulture) + strNuyenSymbol + '+';
            else
                return decMin.ToString(strFormat, objCulture) + strNuyenSymbol + '-' + decMax.ToString(strFormat, objCulture) + strNuyenSymbol;
        }

        /// <summary>
        /// Processes a string into a decimal based on logical processing with equipment-specific replacements.
        /// This is the centralized version of ProcessRatingStringAsDec that was duplicated across equipment classes.
        /// </summary>
        /// <param name="strExpression">The expression to process</param>
        /// <param name="funcRating">Function to get the rating</param>
        /// <param name="blnIsSuccess">Whether the processing was successful</param>
        /// <param name="blnForRange">Whether this is for range calculation</param>
        /// <param name="objCharacter">Character object for equipment-specific replacements</param>
        /// <param name="objEquipment">Equipment object for equipment-specific replacements</param>
        /// <returns>The processed decimal value</returns>
        public static decimal ProcessRatingStringAsDec(string strExpression, Func<int> funcRating, out bool blnIsSuccess, bool blnForRange = false, Character objCharacter = null, object objEquipment = null)
        {
            blnIsSuccess = true;
            if (string.IsNullOrEmpty(strExpression))
                return 0;
            strExpression = strExpression.ProcessFixedValuesString(funcRating).TrimStart('+');
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                blnIsSuccess = false;
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        int intRating = funcRating();
                        sbdValue.Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));

                        if (blnForRange)
                        {
                            sbdValue.Replace("{MinRating}", "1");
                            sbdValue.Replace("MinRating", "1");
                        }
                        else
                        {
                            sbdValue.Replace("{MinRating}", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                            sbdValue.Replace("MinRating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                        }

                        // Handle equipment-specific replacements
                        if (objCharacter != null && objEquipment != null)
                        {
                            // Handle Physical/Missile replacements (for weapons)
                            if (strExpression.Contains("Physical") || strExpression.Contains("Missile"))
                            {
                                string strPhysicalLimit;
                                if (objEquipment is Weapon objWeapon && objWeapon.ParentVehicle != null)
                                {
                                    strPhysicalLimit = objWeapon.ParentVehicle.TotalHandling;
                                    int intSlashIndex = strPhysicalLimit.IndexOf('/');
                                    if (intSlashIndex != -1)
                                        strPhysicalLimit = strPhysicalLimit.Substring(0, intSlashIndex);
                                }
                                else
                                    strPhysicalLimit = objCharacter.LimitPhysical.ToString(GlobalSettings.InvariantCultureInfo);

                                sbdValue.Replace("{Physical}", strPhysicalLimit);
                                sbdValue.Replace("Physical", strPhysicalLimit);
                                sbdValue.Replace("{Missile}", strPhysicalLimit);
                                sbdValue.Replace("Missile", strPhysicalLimit);
                            }

                            // Handle Mental replacements (for gear)
                            if (strExpression.Contains("Mental"))
                            {
                                string strMentalLimit = objCharacter.LimitMental.ToString(GlobalSettings.InvariantCultureInfo);
                                sbdValue.Replace("{Mental}", strMentalLimit);
                                sbdValue.Replace("Mental", strMentalLimit);
                            }

                            // Handle Social replacements (for gear)
                            if (strExpression.Contains("Social"))
                            {
                                string strSocialLimit = objCharacter.LimitSocial.ToString(GlobalSettings.InvariantCultureInfo);
                                sbdValue.Replace("{Social}", strSocialLimit);
                                sbdValue.Replace("Social", strSocialLimit);
                            }
                        }

                        (bool blnIsSuccess2, object objProcess) = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                        if (blnIsSuccess2)
                        {
                            blnIsSuccess = true;
                            return Convert.ToDecimal((double)objProcess);
                        }
                    }
                }
                return decValue;
            }
            return decValue;
        }

        /// <summary>
        /// Asynchronous version of ProcessRatingStringAsDec with equipment-specific replacements.
        /// </summary>
        /// <param name="strExpression">The expression to process</param>
        /// <param name="funcRating">Function to get the rating</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="objCharacter">Character object for equipment-specific replacements</param>
        /// <param name="objEquipment">Equipment object for equipment-specific replacements</param>
        /// <returns>Tuple containing the processed decimal value and success status</returns>
        public static async Task<(decimal decValue, bool blnIsSuccess)> ProcessRatingStringAsDecAsync(string strExpression, Func<Task<int>> funcRating, Character objCharacter = null, object objEquipment = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnIsSuccess = true;
            if (string.IsNullOrEmpty(strExpression))
                return (0, true);
            strExpression = await strExpression.ProcessFixedValuesStringAsync(funcRating, token).ConfigureAwait(false);
            strExpression = strExpression.TrimStart('+');
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                blnIsSuccess = false;
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        int intRating = await funcRating().ConfigureAwait(false);
                        sbdValue.Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{MinRating}", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("MinRating", intRating.ToString(GlobalSettings.InvariantCultureInfo));

                        // Handle equipment-specific replacements
                        if (objCharacter != null && objEquipment != null)
                        {
                            // Handle Physical/Missile replacements (for weapons)
                            if (strExpression.Contains("Physical") || strExpression.Contains("Missile"))
                            {
                                string strPhysicalLimit;
                                if (objEquipment is Weapon objWeapon && objWeapon.ParentVehicle != null)
                                {
                                    strPhysicalLimit = objWeapon.ParentVehicle.TotalHandling;
                                    int intSlashIndex = strPhysicalLimit.IndexOf('/');
                                    if (intSlashIndex != -1)
                                        strPhysicalLimit = strPhysicalLimit.Substring(0, intSlashIndex);
                                }
                                else
                                    strPhysicalLimit = objCharacter.LimitPhysical.ToString(GlobalSettings.InvariantCultureInfo);

                                sbdValue.Replace("{Physical}", strPhysicalLimit);
                                sbdValue.Replace("Physical", strPhysicalLimit);
                                sbdValue.Replace("{Missile}", strPhysicalLimit);
                                sbdValue.Replace("Missile", strPhysicalLimit);
                            }

                            // Handle Mental replacements (for gear)
                            if (strExpression.Contains("Mental"))
                            {
                                string strMentalLimit = objCharacter.LimitMental.ToString(GlobalSettings.InvariantCultureInfo);
                                sbdValue.Replace("{Mental}", strMentalLimit);
                                sbdValue.Replace("Mental", strMentalLimit);
                            }

                            // Handle Social replacements (for gear)
                            if (strExpression.Contains("Social"))
                            {
                                string strSocialLimit = objCharacter.LimitSocial.ToString(GlobalSettings.InvariantCultureInfo);
                                sbdValue.Replace("{Social}", strSocialLimit);
                                sbdValue.Replace("Social", strSocialLimit);
                            }
                        }

                        (bool blnIsSuccess2, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(sbdValue.ToString(), token).ConfigureAwait(false);
                        if (blnIsSuccess2)
                        {
                            blnIsSuccess = true;
                            return (Convert.ToDecimal((double)objProcess), blnIsSuccess);
                        }
                    }
                }
                return (decValue, blnIsSuccess);
            }
            return (decValue, blnIsSuccess);
        }

        /// <summary>
        /// Comprehensive cost processing that handles all cost types (Variable, FixedValues, XPath expressions, etc.)
        /// and returns both the display string and calculated cost value using a context object.
        /// </summary>
        /// <param name="context">The cost processing context containing all parameters</param>
        /// <returns>Tuple containing display string, calculated cost value, and success status</returns>
        public static async Task<(string strDisplayCost, decimal decCalculatedCost, bool blnIsSuccess)> ProcessCostComprehensiveAsync(CostProcessingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Modifiers == null)
                context.Modifiers = new CostModifiers();

            if (context.Culture == null)
                context.Culture = GlobalSettings.CultureInfo;

            if (string.IsNullOrEmpty(context.NumberFormat))
                context.NumberFormat = "N0";

            if (string.IsNullOrEmpty(context.CurrencySymbol))
                context.CurrencySymbol = "¥";

            return await ProcessCostComprehensiveAsync(context.CostString, context.Rating, context.Modifiers,
                context.NumberFormat, context.CurrencySymbol, context.Culture, context.Character,
                context.DisplayName, context.ForSelectForm, context.SkipCost, context.SkipSelectForms,
                context.CreateImprovements, context.CancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Comprehensive cost processing that handles all cost types (Variable, FixedValues, XPath expressions, etc.)
        /// and returns both the display string and calculated cost value.
        /// </summary>
        /// <param name="strCost">The cost string to process</param>
        /// <param name="intRating">The rating to use for calculations</param>
        /// <param name="objModifiers">Cost modifiers to apply</param>
        /// <param name="strFormat">The number format string for display</param>
        /// <param name="strNuyenSymbol">The currency symbol</param>
        /// <param name="objCulture">The culture info for formatting</param>
        /// <param name="objCharacter">Character object for Variable cost prompting (optional)</param>
        /// <param name="strDisplayName">Display name for Variable cost prompting (optional)</param>
        /// <param name="blnForSelectForm">Whether this is for a select form (skip Variable prompting)</param>
        /// <param name="blnSkipCost">Whether to skip cost prompting</param>
        /// <param name="blnSkipSelectForms">Whether to skip showing selection forms</param>
        /// <param name="blnCreateImprovements">Whether improvements are being created</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Tuple containing display string, calculated cost value, and success status</returns>
        public static async Task<(string strDisplayCost, decimal decCalculatedCost, bool blnIsSuccess)> ProcessCostComprehensiveAsync(
            string strCost, int intRating, CostModifiers objModifiers, string strFormat, string strNuyenSymbol,
            CultureInfo objCulture, Character objCharacter = null, string strDisplayName = null,
            bool blnForSelectForm = false, bool blnSkipCost = false, bool blnSkipSelectForms = false,
            bool blnCreateImprovements = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(strCost))
                return (string.Empty, 0, true);

            // Create cache key for non-interactive cost processing (no character prompting)
            if (objCharacter == null || blnForSelectForm)
            {
                string strCacheKey = $"{strCost}|{intRating}|{objModifiers.GetTotalMultiplier()}|{strFormat}|{strNuyenSymbol}|{objCulture.Name}";
                if (_costCache.TryGetValue(strCacheKey, out var cachedResult))
                    return cachedResult;
            }

            // Handle Variable costs
            if (strCost.StartsWith("Variable(", StringComparison.Ordinal))
            {
                // If we have a character and display name, prompt for Variable cost
                if (objCharacter != null && !string.IsNullOrEmpty(strDisplayName))
                {
                    var (blnCancelled, strUpdatedCost) = await PromptForVariableCostAsync(strCost, objCharacter, strDisplayName,
                        blnForSelectForm, blnSkipCost, blnSkipSelectForms, blnCreateImprovements, token).ConfigureAwait(false);

                    if (blnCancelled)
                        return (strCost, 0, false);

                    strCost = strUpdatedCost;
                }

                // Format for display with modifiers
                string strDisplayCost = FormatVariableCostForDisplay(strCost, strFormat, strNuyenSymbol, objCulture, objModifiers);

                // Extract calculated cost with modifiers applied
                string strFirstHalf = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                int intHyphenIndex = strFirstHalf.IndexOf('-');
                if (intHyphenIndex != -1)
                    strFirstHalf = strFirstHalf.Substring(0, intHyphenIndex);
                decimal.TryParse(strFirstHalf.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decMin);
                decimal decVariableCalculatedCost = decMin * objModifiers.GetTotalMultiplier();

                var variableResult = (strDisplayCost, decVariableCalculatedCost, true);
                // Cache the result for non-interactive processing
                if (objCharacter == null || blnForSelectForm)
                {
                    string strCacheKey = $"{strCost}|{intRating}|{objModifiers.GetTotalMultiplier()}|{strFormat}|{strNuyenSymbol}|{objCulture.Name}";
                    _costCache.TryAdd(strCacheKey, variableResult);
                }
                return variableResult;
            }

            // Handle other cost types (FixedValues, XPath expressions, etc.)
            string strProcessedCost = strCost.ProcessFixedValuesString(intRating).TrimStart('+');

            if (strProcessedCost.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                // This is an XPath expression that needs processing
                if (strProcessedCost.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strProcessedCost);
                        sbdValue.Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{MinRating}", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("MinRating", intRating.ToString(GlobalSettings.InvariantCultureInfo));

                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(sbdValue.ToString(), token).ConfigureAwait(false);
                        if (blnIsSuccess)
                        {
                            decimal decXPathCalculatedCost = Convert.ToDecimal((double)objProcess) * objModifiers.GetTotalMultiplier();
                            string strXPathDisplayCost = decXPathCalculatedCost.ToString(strFormat, objCulture) + strNuyenSymbol;
                            var xpathResult = (strXPathDisplayCost, decXPathCalculatedCost, true);
                            // Cache the result for non-interactive processing
                            if (objCharacter == null || blnForSelectForm)
                            {
                                string strCacheKey = $"{strCost}|{intRating}|{objModifiers.GetTotalMultiplier()}|{strFormat}|{strNuyenSymbol}|{objCulture.Name}";
                                _costCache.TryAdd(strCacheKey, xpathResult);
                            }
                            return xpathResult;
                        }
                    }
                }
                return (strCost, 0, false);
            }

            // Simple numeric cost
            decimal decNumericCalculatedCost = decValue * objModifiers.GetTotalMultiplier();
            string strNumericDisplayCost = decNumericCalculatedCost.ToString(strFormat, objCulture) + strNuyenSymbol;
            var numericResult = (strNumericDisplayCost, decNumericCalculatedCost, true);
            // Cache the result for non-interactive processing
            if (objCharacter == null || blnForSelectForm)
            {
                string strCacheKey = $"{strCost}|{intRating}|{objModifiers.GetTotalMultiplier()}|{strFormat}|{strNuyenSymbol}|{objCulture.Name}";
                _costCache.TryAdd(strCacheKey, numericResult);
            }
            return numericResult;
        }

        /// <summary>
        /// Synchronous version of comprehensive cost processing.
        /// </summary>
        public static (string strDisplayCost, decimal decCalculatedCost, bool blnIsSuccess) ProcessCostComprehensive(
            string strCost, int intRating, CostModifiers objModifiers, string strFormat, string strNuyenSymbol,
            CultureInfo objCulture, Character objCharacter = null, string strDisplayName = null,
            bool blnForSelectForm = false, bool blnSkipCost = false, bool blnSkipSelectForms = false,
            bool blnCreateImprovements = false, CancellationToken token = default)
        {
            return ProcessCostComprehensiveAsync(strCost, intRating, objModifiers, strFormat, strNuyenSymbol,
                objCulture, objCharacter, strDisplayName, blnForSelectForm, blnSkipCost, blnSkipSelectForms,
                blnCreateImprovements, token).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Processes a string expression that may need XPath evaluation with equipment-specific replacements.
        /// This is a centralized version of the common pattern used across equipment classes.
        /// </summary>
        /// <param name="strExpression">The expression to process</param>
        /// <param name="objCharacter">Character object for equipment-specific replacements</param>
        /// <param name="objEquipment">Equipment object for equipment-specific replacements</param>
        /// <param name="funcProcessAttributes">Optional function to process additional attributes</param>
        /// <returns>Tuple containing the processed decimal value and success status</returns>
        public static (decimal decValue, bool blnIsSuccess) ProcessExpressionWithXPath(string strExpression, Character objCharacter = null, object objEquipment = null, Action<StringBuilder, string> funcProcessAttributes = null)
        {
            if (string.IsNullOrEmpty(strExpression))
                return (0, true);

            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);

                        // Handle equipment-specific replacements
                        if (objCharacter != null && objEquipment != null)
                        {
                            // Handle Physical/Missile replacements (for weapons)
                            if (strExpression.Contains("Physical") || strExpression.Contains("Missile"))
                            {
                                string strPhysicalLimit;
                                if (objEquipment is Weapon objWeapon && objWeapon.ParentVehicle != null)
                                {
                                    strPhysicalLimit = objWeapon.ParentVehicle.TotalHandling;
                                    int intSlashIndex = strPhysicalLimit.IndexOf('/');
                                    if (intSlashIndex != -1)
                                        strPhysicalLimit = strPhysicalLimit.Substring(0, intSlashIndex);
                                }
                                else
                                    strPhysicalLimit = objCharacter.LimitPhysical.ToString(GlobalSettings.InvariantCultureInfo);

                                sbdValue.Replace("{Physical}", strPhysicalLimit);
                                sbdValue.Replace("Physical", strPhysicalLimit);
                                sbdValue.Replace("{Missile}", strPhysicalLimit);
                                sbdValue.Replace("Missile", strPhysicalLimit);
                            }

                            // Handle Mental replacements (for gear)
                            if (strExpression.Contains("Mental"))
                            {
                                string strMentalLimit = objCharacter.LimitMental.ToString(GlobalSettings.InvariantCultureInfo);
                                sbdValue.Replace("{Mental}", strMentalLimit);
                                sbdValue.Replace("Mental", strMentalLimit);
                            }

                            // Handle Social replacements (for gear)
                            if (strExpression.Contains("Social"))
                            {
                                string strSocialLimit = objCharacter.LimitSocial.ToString(GlobalSettings.InvariantCultureInfo);
                                sbdValue.Replace("{Social}", strSocialLimit);
                                sbdValue.Replace("Social", strSocialLimit);
                            }
                        }

                        // Allow custom attribute processing
                        funcProcessAttributes?.Invoke(sbdValue, strExpression);

                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                        if (blnIsSuccess)
                            return (Convert.ToDecimal((double)objProcess), true);
                    }
                }
                return (decValue, false);
            }
            return (decValue, true);
        }

        /// <summary>
        /// Asynchronous version of ProcessExpressionWithXPath.
        /// </summary>
        /// <param name="strExpression">The expression to process</param>
        /// <param name="objCharacter">Character object for equipment-specific replacements</param>
        /// <param name="objEquipment">Equipment object for equipment-specific replacements</param>
        /// <param name="funcProcessAttributes">Optional async function to process additional attributes</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Tuple containing the processed decimal value and success status</returns>
        public static async Task<(decimal decValue, bool blnIsSuccess)> ProcessExpressionWithXPathAsync(string strExpression, Character objCharacter = null, object objEquipment = null, Func<StringBuilder, string, Task> funcProcessAttributes = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(strExpression))
                return (0, true);

            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);

                        // Handle equipment-specific replacements
                        if (objCharacter != null && objEquipment != null)
                        {
                            // Handle Physical/Missile replacements (for weapons)
                            if (strExpression.Contains("Physical") || strExpression.Contains("Missile"))
                            {
                                string strPhysicalLimit;
                                if (objEquipment is Weapon objWeapon && objWeapon.ParentVehicle != null)
                                {
                                    strPhysicalLimit = objWeapon.ParentVehicle.TotalHandling;
                                    int intSlashIndex = strPhysicalLimit.IndexOf('/');
                                    if (intSlashIndex != -1)
                                        strPhysicalLimit = strPhysicalLimit.Substring(0, intSlashIndex);
                                }
                                else
                                    strPhysicalLimit = objCharacter.LimitPhysical.ToString(GlobalSettings.InvariantCultureInfo);

                                sbdValue.Replace("{Physical}", strPhysicalLimit);
                                sbdValue.Replace("Physical", strPhysicalLimit);
                                sbdValue.Replace("{Missile}", strPhysicalLimit);
                                sbdValue.Replace("Missile", strPhysicalLimit);
                            }

                            // Handle Mental replacements (for gear)
                            if (strExpression.Contains("Mental"))
                            {
                                string strMentalLimit = objCharacter.LimitMental.ToString(GlobalSettings.InvariantCultureInfo);
                                sbdValue.Replace("{Mental}", strMentalLimit);
                                sbdValue.Replace("Mental", strMentalLimit);
                            }

                            // Handle Social replacements (for gear)
                            if (strExpression.Contains("Social"))
                            {
                                string strSocialLimit = objCharacter.LimitSocial.ToString(GlobalSettings.InvariantCultureInfo);
                                sbdValue.Replace("{Social}", strSocialLimit);
                                sbdValue.Replace("Social", strSocialLimit);
                            }
                        }

                        // Allow custom attribute processing
                        if (funcProcessAttributes != null)
                            await funcProcessAttributes(sbdValue, strExpression).ConfigureAwait(false);

                        (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(sbdValue.ToString(), token).ConfigureAwait(false);
                        if (blnIsSuccess)
                            return (Convert.ToDecimal((double)objProcess), true);
                    }
                }
                return (decValue, false);
            }
            return (decValue, true);
        }

        /// <summary>
        /// Convert a multiplier value to a percentage change text (e.g., 0.90 -> "-10%", 1.10 -> "+10%").
        /// </summary>
        /// <param name="decMultiplier">The multiplier value (e.g., 0.90 for 10% discount, 1.10 for 10% markup)</param>
        /// <returns>Formatted percentage change string with appropriate sign</returns>
        public static string GetPercentageChangeText(decimal decMultiplier)
        {
            decimal decPercentageChange = (decMultiplier - 1.0m) * 100.0m;
            string strSign = decPercentageChange >= 0 ? "+" : "";
            return $"{strSign}{decPercentageChange:F0}%";
        }

        #region Cost Modifier Improvement Methods

        /// <summary>
        /// Apply cost modifier improvements to a CostModifiers instance.
        /// </summary>
        internal static CostModifiers ApplyCostModifierImprovements(Character objCharacter, string strEquipmentType, CostModifiers objBaseModifiers = null, bool blnUserChoiceOnly = false)
        {
            if (objCharacter == null)
                return objBaseModifiers ?? new CostModifiers();

            List<Improvement> lstImprovements = objCharacter.GetCostModifierImprovements(strEquipmentType, blnUserChoiceOnly);
            if (lstImprovements.Count == 0)
                return objBaseModifiers ?? new CostModifiers();

            var objResult = objBaseModifiers?.Clone() ?? new CostModifiers();

            foreach (Improvement objImprovement in lstImprovements)
            {
                (string strModifierName, decimal decValue) = objImprovement.GetCostModifier();
                if (!string.IsNullOrEmpty(strModifierName))
                {
                    objResult.SetCustomModifier(strModifierName, decValue);
                }
            }

            return objResult;
        }

        /// <summary>
        /// Apply cost modifier improvements to a CostModifiers instance.
        /// </summary>
        internal static async Task<CostModifiers> ApplyCostModifierImprovementsAsync(Character objCharacter, string strEquipmentType, CostModifiers objBaseModifiers = null, bool blnUserChoiceOnly = false, CancellationToken token = default)
        {
            if (objCharacter == null)
                return objBaseModifiers ?? new CostModifiers();

            List<Improvement> lstImprovements = await objCharacter.GetCostModifierImprovementsAsync(strEquipmentType, blnUserChoiceOnly, token).ConfigureAwait(false);
            if (lstImprovements.Count == 0)
                return objBaseModifiers ?? new CostModifiers();

            var objResult = objBaseModifiers?.Clone() ?? new CostModifiers();

            foreach (Improvement objImprovement in lstImprovements)
            {
                (string strModifierName, decimal decValue) = await objImprovement.GetCostModifierAsync(token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strModifierName))
                {
                    objResult.SetCustomModifier(strModifierName, decValue);
                }
            }

            return objResult;
        }

        #endregion Cost Modifier Improvement Methods
    }

    /// <summary>
    /// Represents cost modifiers that can be applied to Variable costs.
    /// Supports both strongly-typed modifiers for known types and arbitrary custom modifiers for extensibility.
    /// 
    /// Usage Examples:
    /// <code>
    /// // Strongly-typed modifiers (recommended for known types)
    /// var modifiers = new CostModifiers
    /// {
    ///     MarkupMultiplier = 1.2m,        // 20% markup
    ///     BlackMarketMultiplier = 0.9m,   // 10% black market discount
    ///     MadeManMultiplier = 0.9m        // 10% Made Man discount
    /// };
    /// 
    /// // Custom modifiers (for extensibility)
    /// modifiers["CorporateDiscount"] = 0.85m;  // 15% corporate discount
    /// modifiers["BulkPurchase"] = 0.8m;        // 20% bulk discount
    /// 
    /// // Or using helper methods
    /// modifiers.SetCustomModifier("LoyaltyProgram", 0.95m);
    /// 
    /// // Total multiplier includes all modifiers
    /// decimal totalMultiplier = modifiers.GetTotalMultiplier();
    /// </code>
    /// </summary>
    public class CostModifiers
    {
        /// <summary>
        /// Markup multiplier (e.g., 1.2 for 20% markup)
        /// </summary>
        public decimal MarkupMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Black market discount multiplier (e.g., 0.9 for 10% discount)
        /// </summary>
        public decimal BlackMarketMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Made Man quality discount multiplier (e.g., 0.9 for 10% discount on stolen/restricted goods)
        /// </summary>
        public decimal MadeManMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Quantity multiplier (e.g., 2.0 for double quantity)
        /// </summary>
        public decimal QuantityMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Stolen item multiplier (e.g., 0.5 for 50% of normal cost)
        /// </summary>
        public decimal StolenMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Used item multiplier (e.g., 0.7 for 70% of normal cost)
        /// </summary>
        public decimal UsedMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Accessory multiplier (for weapon accessories)
        /// </summary>
        public decimal AccessoryMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Doubled cost modification multiplier (for weapon accessories)
        /// </summary>
        public decimal DoubledCostMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Genetech cost modifier (for cyberware)
        /// </summary>
        public decimal GenetechMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Additional custom multiplier
        /// </summary>
        public decimal CustomMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Custom modifiers dictionary for arbitrary cost modifications
        /// </summary>
        private readonly Dictionary<string, decimal> _customModifiers = new Dictionary<string, decimal>();

        /// <summary>
        /// Indexer for accessing custom modifiers by name
        /// </summary>
        /// <param name="modifierName">Name of the custom modifier</param>
        /// <returns>Value of the custom modifier (defaults to 1.0 if not found)</returns>
        public decimal this[string modifierName]
        {
            get => _customModifiers.TryGetValue(modifierName, out decimal value) ? value : 1.0m;
            set => _customModifiers[modifierName] = value;
        }

        /// <summary>
        /// Default constructor with no modifiers
        /// </summary>
        public CostModifiers()
        {
        }

        /// <summary>
        /// Constructor with basic modifiers
        /// </summary>
        /// <param name="decMarkupMultiplier">Markup multiplier</param>
        /// <param name="decBlackMarketMultiplier">Black market discount multiplier</param>
        /// <param name="decOtherMultiplier">Other cost multiplier</param>
        public CostModifiers(decimal decMarkupMultiplier, decimal decBlackMarketMultiplier, decimal decOtherMultiplier)
        {
            MarkupMultiplier = decMarkupMultiplier;
            BlackMarketMultiplier = decBlackMarketMultiplier;
            CustomMultiplier = decOtherMultiplier;
        }

        /// <summary>
        /// Gets the total combined multiplier from all modifiers
        /// </summary>
        /// <returns>Total multiplier</returns>
        public decimal GetTotalMultiplier()
        {
            decimal total = MarkupMultiplier * BlackMarketMultiplier * MadeManMultiplier * QuantityMultiplier * StolenMultiplier *
                           UsedMultiplier * AccessoryMultiplier * DoubledCostMultiplier * GenetechMultiplier * CustomMultiplier;

            // Apply custom modifiers
            foreach (decimal customModifier in _customModifiers.Values)
                total *= customModifier;

            return total;
        }

        /// <summary>
        /// Creates a copy of this CostModifiers instance
        /// </summary>
        /// <returns>New CostModifiers instance with same values</returns>
        public CostModifiers Clone()
        {
            var clone = new CostModifiers
            {
                MarkupMultiplier = MarkupMultiplier,
                BlackMarketMultiplier = BlackMarketMultiplier,
                MadeManMultiplier = MadeManMultiplier,
                QuantityMultiplier = QuantityMultiplier,
                StolenMultiplier = StolenMultiplier,
                UsedMultiplier = UsedMultiplier,
                AccessoryMultiplier = AccessoryMultiplier,
                DoubledCostMultiplier = DoubledCostMultiplier,
                GenetechMultiplier = GenetechMultiplier,
                CustomMultiplier = CustomMultiplier
            };

            // Copy custom modifiers
            foreach (var kvp in _customModifiers)
                clone._customModifiers[kvp.Key] = kvp.Value;

            return clone;
        }

        /// <summary>
        /// Adds or updates a custom modifier
        /// </summary>
        /// <param name="modifierName">Name of the custom modifier</param>
        /// <param name="value">Value of the modifier</param>
        public void SetCustomModifier(string modifierName, decimal value)
        {
            _customModifiers[modifierName] = value;
        }

        /// <summary>
        /// Gets a custom modifier value, or returns the default if not found
        /// </summary>
        /// <param name="modifierName">Name of the custom modifier</param>
        /// <param name="defaultValue">Default value to return if modifier not found</param>
        /// <returns>Value of the custom modifier or default value</returns>
        public decimal GetCustomModifier(string modifierName, decimal defaultValue = 1.0m)
        {
            return _customModifiers.TryGetValue(modifierName, out decimal value) ? value : defaultValue;
        }

        /// <summary>
        /// Removes a custom modifier
        /// </summary>
        /// <param name="modifierName">Name of the custom modifier to remove</param>
        /// <returns>True if the modifier was removed, false if it didn't exist</returns>
        public bool RemoveCustomModifier(string modifierName)
        {
            return _customModifiers.Remove(modifierName);
        }

        /// <summary>
        /// Checks if a custom modifier exists
        /// </summary>
        /// <param name="modifierName">Name of the custom modifier</param>
        /// <returns>True if the modifier exists, false otherwise</returns>
        public bool HasCustomModifier(string modifierName)
        {
            return _customModifiers.ContainsKey(modifierName);
        }

        /// <summary>
        /// Gets all custom modifier names
        /// </summary>
        /// <returns>Collection of custom modifier names</returns>
        public IEnumerable<string> GetCustomModifierNames()
        {
            return _customModifiers.Keys;
        }

        /// <summary>
        /// Gets all custom modifiers as key-value pairs
        /// </summary>
        /// <returns>Collection of custom modifier key-value pairs</returns>
        public IEnumerable<KeyValuePair<string, decimal>> GetCustomModifiers()
        {
            return _customModifiers;
        }

        /// <summary>
        /// Clears all custom modifiers
        /// </summary>
        public void ClearCustomModifiers()
        {
            _customModifiers.Clear();
        }
    }

    /// <summary>
    /// Shared utility methods for XPath evaluation in selection forms.
    /// </summary>
    public static class XPathEvaluation
    {
        /// <summary>
        /// Check if an improvement matches a specific equipment item by evaluating its XPath filter against the equipment's XML node.
        /// This is a generic method that can be used by all selection forms.
        /// </summary>
        /// <param name="objImprovement">The improvement to check</param>
        /// <param name="objXmlDocument">The XML document containing the equipment data</param>
        /// <param name="strSelectedId">The ID of the currently selected equipment item (from the list's SelectedValue)</param>
        /// <param name="strEquipmentType">The type of equipment (e.g., "weapon", "armor", "gear") for debug output</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>True if the improvement matches the selected equipment, false otherwise</returns>
        public static async Task<bool> MatchesSpecificEquipmentAsync(Improvement objImprovement, XmlDocument objXmlDocument, 
            string strSelectedId, string strEquipmentType, CancellationToken token = default)
        {
            if (objImprovement == null || objXmlDocument == null || string.IsNullOrEmpty(strSelectedId))
                return false;

            // Get the equipment filter XPath from the improvement's Target property
            string strEquipmentFilter = objImprovement.Target;
            if (string.IsNullOrEmpty(strEquipmentFilter))
            {
                System.Diagnostics.Debug.WriteLine($"Improvement '{objImprovement.ImprovedName}' has no equipment filter, applying to all {strEquipmentType}");
                return true; // No filter means it applies to all equipment types
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Testing XPath '{strEquipmentFilter}' against {strEquipmentType} ID {strSelectedId} for improvement '{objImprovement.ImprovedName}'");

                // Test the XPath against the entire document to see if it selects this equipment
                System.Diagnostics.Debug.WriteLine($"Testing XPath '{strEquipmentFilter}' against entire document to see if {strEquipmentType} matches");
                
                // Get all nodes that match the XPath filter
                XmlNodeList objMatchingNodes = objXmlDocument.SelectNodes(strEquipmentFilter);
                if (objMatchingNodes == null)
                {
                    System.Diagnostics.Debug.WriteLine($"XPath '{strEquipmentFilter}' returned null");
                    return false;
                }
                
                // Check if any of the matching nodes is our specific equipment
                bool blnMatches = false;
                foreach (XmlNode objNode in objMatchingNodes)
                {
                    if (objNode["id"]?.InnerText == strSelectedId)
                    {
                        blnMatches = true;
                        break;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"XPath '{strEquipmentFilter}' matched {objMatchingNodes.Count} nodes, {strEquipmentType} {strSelectedId} included: {blnMatches}");
                return blnMatches;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error evaluating equipment filter XPath '{strEquipmentFilter}' for improvement {objImprovement.SourceName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if an improvement matches a specific equipment item by evaluating its XPath filter against the equipment's XML node.
        /// This overload accepts XPathNavigator for forms that use XPathNavigator instead of XmlDocument.
        /// </summary>
        /// <param name="objImprovement">The improvement to check</param>
        /// <param name="objXPathNavigator">The XPath navigator containing the equipment data</param>
        /// <param name="strSelectedId">The ID of the currently selected equipment item (from the list's SelectedValue)</param>
        /// <param name="strEquipmentType">The type of equipment (e.g., "weapon", "armor", "gear") for debug output</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>True if the improvement matches the selected equipment, false otherwise</returns>
        public static async Task<bool> MatchesSpecificEquipmentAsync(Improvement objImprovement, XPathNavigator objXPathNavigator, 
            string strSelectedId, string strEquipmentType, CancellationToken token = default)
        {
            if (objImprovement == null || objXPathNavigator == null || string.IsNullOrEmpty(strSelectedId))
                return false;

            // Get the equipment filter XPath from the improvement's Target property
            string strEquipmentFilter = objImprovement.Target;
            if (string.IsNullOrEmpty(strEquipmentFilter))
            {
                System.Diagnostics.Debug.WriteLine($"Improvement '{objImprovement.ImprovedName}' has no equipment filter, applying to all {strEquipmentType}");
                return true; // No filter means it applies to all equipment types
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Testing XPath '{strEquipmentFilter}' against {strEquipmentType} ID {strSelectedId} for improvement '{objImprovement.ImprovedName}'");

                // Test the XPath against the entire document to see if it selects this equipment
                System.Diagnostics.Debug.WriteLine($"Testing XPath '{strEquipmentFilter}' against entire document to see if {strEquipmentType} matches");
                
                // Get all nodes that match the XPath filter
                XPathNodeIterator objMatchingNodes = objXPathNavigator.Select(strEquipmentFilter);
                if (objMatchingNodes == null)
                {
                    System.Diagnostics.Debug.WriteLine($"XPath '{strEquipmentFilter}' returned null");
                    return false;
                }
                
                // Check if any of the matching nodes is our specific equipment
                bool blnMatches = false;
                int intCount = 0;
                while (objMatchingNodes.MoveNext())
                {
                    intCount++;
                    XPathNavigator objNode = objMatchingNodes.Current;
                    if (objNode != null)
                    {
                        // Get the id element value
                        XPathNavigator objIdNode = objNode.SelectSingleNode("id");
                        if (objIdNode != null && objIdNode.Value == strSelectedId)
                        {
                            blnMatches = true;
                            break;
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"XPath '{strEquipmentFilter}' matched {intCount} nodes, {strEquipmentType} {strSelectedId} included: {blnMatches}");
                return blnMatches;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error evaluating equipment filter XPath '{strEquipmentFilter}' for improvement {objImprovement.SourceName}: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Shared utility class for managing dynamic cost modifier checkboxes in selection forms.
    /// </summary>
    public static class DynamicCostModifierCheckboxes
    {
        /// <summary>
        /// Create and manage dynamic checkboxes for CostModifierUserChoice improvements.
        /// </summary>
        /// <param name="objCharacter">The character object</param>
        /// <param name="strEquipmentType">The equipment type (e.g., "weapon", "armor", "gear")</param>
        /// <param name="objXmlDocument">The XML document containing the equipment data</param>
        /// <param name="strSelectedId">The ID of the currently selected equipment item</param>
        /// <param name="dicDynamicCostModifierCheckboxes">Dictionary to store the checkboxes</param>
        /// <param name="flpCheckBoxes">FlowLayoutPanel to add checkboxes to</param>
        /// <param name="funcUpdateCostModifiers">Function to call when checkboxes change</param>
        /// <param name="funcUpdateInfo">Function to call when checkboxes change</param>
        /// <param name="objGenericToken">Cancellation token for async operations</param>
        /// <param name="token">Cancellation token</param>
        public static async Task CreateDynamicCostModifierCheckboxesAsync(
            Character objCharacter,
            string strEquipmentType,
            XmlDocument objXmlDocument,
            string strSelectedId,
            Dictionary<string, ColorableCheckBox> dicDynamicCostModifierCheckboxes,
            FlowLayoutPanel flpCheckBoxes,
            Func<CancellationToken, Task> funcUpdateCostModifiers,
            Func<CancellationToken, Task> funcUpdateInfo,
            CancellationToken objGenericToken,
            CancellationToken token = default)
        {
            try
            {
                // Get all CostModifierUserChoice improvements that apply to this equipment type
                List<Improvement> lstAllImprovements = await objCharacter.GetCostModifierImprovementsAsync(strEquipmentType, blnUserChoiceOnly: true, token: token).ConfigureAwait(false);
                
                System.Diagnostics.Debug.WriteLine($"Found {lstAllImprovements.Count} total CostModifierUserChoice improvements for {strEquipmentType}");
                
                // Filter improvements to only those that match the specific selected equipment
                List<Improvement> lstImprovements = new List<Improvement>();
                foreach (Improvement objImprovement in lstAllImprovements)
                {
                    bool blnMatches = await XPathEvaluation.MatchesSpecificEquipmentAsync(objImprovement, objXmlDocument, strSelectedId, strEquipmentType, token).ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"Improvement '{objImprovement.ImprovedName}' (Target: '{objImprovement.Target}') matches specific {strEquipmentType}: {blnMatches}");
                    if (blnMatches)
                    {
                        lstImprovements.Add(objImprovement);
                    }
                }
                
                // Remove existing dynamic checkboxes that are no longer needed
                List<string> lstCheckboxesToRemove = new List<string>();
                foreach (string strModifierName in dicDynamicCostModifierCheckboxes.Keys)
                {
                    if (!lstImprovements.Any(x => x.ImprovedName == strModifierName))
                    {
                        lstCheckboxesToRemove.Add(strModifierName);
                    }
                }
                
                foreach (string strModifierName in lstCheckboxesToRemove)
                {
                    if (dicDynamicCostModifierCheckboxes.TryGetValue(strModifierName, out ColorableCheckBox objCheckbox))
                    {
                        await objCheckbox.DoThreadSafeAsync(x => x.Dispose(), token: token).ConfigureAwait(false);
                        dicDynamicCostModifierCheckboxes.Remove(strModifierName);
                    }
                }
                
                // Create or update checkboxes for current improvements
                System.Diagnostics.Debug.WriteLine($"Creating checkboxes for {lstImprovements.Count} improvements");
                
                // Only create checkboxes if there are improvements
                if (lstImprovements.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No improvements found, skipping checkbox creation");
                    return;
                }
                
                foreach (Improvement objImprovement in lstImprovements)
                {
                    string strModifierName = objImprovement.ImprovedName;
                    if (string.IsNullOrEmpty(strModifierName))
                    {
                        System.Diagnostics.Debug.WriteLine($"Skipping improvement with empty modifier name: {objImprovement.SourceName}");
                        continue;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Processing improvement: {strModifierName} (Value: {objImprovement.Value})");
                    
                    if (!dicDynamicCostModifierCheckboxes.TryGetValue(strModifierName, out ColorableCheckBox objCheckbox))
                    {
                        // Create new checkbox
                        System.Diagnostics.Debug.WriteLine($"Creating new checkbox for: {strModifierName}");
                        objCheckbox = new ColorableCheckBox
                        {
                            Anchor = AnchorStyles.Left,
                            AutoSize = true,
                            DefaultColorScheme = true,
                            Location = new System.Drawing.Point(228, 4),
                            Margin = new Padding(3, 4, 3, 4),
                            Name = $"chk{strModifierName}",
                            Size = new System.Drawing.Size(163, 17),
                            TabIndex = 70 + dicDynamicCostModifierCheckboxes.Count,
                            Tag = $"Checkbox_{strModifierName}",
                            Text = $"{strModifierName} ({CostProcessing.GetPercentageChangeText(objImprovement.Value)})",
                            UseVisualStyleBackColor = true,
                            Visible = false
                        };
                        
                        // Add event handler
                        objCheckbox.CheckedChanged += async (sender, e) =>
                        {
                            try
                            {
                                // Update the equipment's stored cost modifiers
                                await funcUpdateCostModifiers(objGenericToken).ConfigureAwait(false);
                                // Update the equipment info display
                                await funcUpdateInfo(objGenericToken).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                //swallow this
                            }
                        };
                        
                        // Add to form and dictionary
                        await flpCheckBoxes.DoThreadSafeAsync(x => x.Controls.Add(objCheckbox), token: token).ConfigureAwait(false);
                        dicDynamicCostModifierCheckboxes[strModifierName] = objCheckbox;
                    }
                    
                    // Update checkbox text and visibility
                    string strDisplayText = $"{strModifierName} ({CostProcessing.GetPercentageChangeText(objImprovement.Value)})";
                    System.Diagnostics.Debug.WriteLine($"Setting checkbox visible for: {strModifierName}");
                    await objCheckbox.DoThreadSafeAsync(x =>
                    {
                        x.Text = strDisplayText;
                        x.Visible = true;
                        System.Diagnostics.Debug.WriteLine($"Checkbox {strModifierName} visibility set to: {x.Visible}");
                    }, token: token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                //Log.Warn(ex, "Error creating dynamic cost modifier checkboxes");
            }
        }

        /// <summary>
        /// Create and manage dynamic checkboxes for CostModifierUserChoice improvements (XPathNavigator overload).
        /// </summary>
        public static async Task CreateDynamicCostModifierCheckboxesAsync(
            Character objCharacter,
            string strEquipmentType,
            XPathNavigator objXPathNavigator,
            string strSelectedId,
            Dictionary<string, ColorableCheckBox> dicDynamicCostModifierCheckboxes,
            FlowLayoutPanel flpCheckBoxes,
            Func<CancellationToken, Task> funcUpdateCostModifiers,
            Func<CancellationToken, Task> funcUpdateInfo,
            CancellationToken objGenericToken,
            CancellationToken token = default)
        {
            try
            {
                // Get all CostModifierUserChoice improvements that apply to this equipment type
                List<Improvement> lstAllImprovements = await objCharacter.GetCostModifierImprovementsAsync(strEquipmentType, blnUserChoiceOnly: true, token: token).ConfigureAwait(false);
                
                System.Diagnostics.Debug.WriteLine($"Found {lstAllImprovements.Count} total CostModifierUserChoice improvements for {strEquipmentType}");
                
                // Filter improvements to only those that match the specific selected equipment
                List<Improvement> lstImprovements = new List<Improvement>();
                foreach (Improvement objImprovement in lstAllImprovements)
                {
                    bool blnMatches = await XPathEvaluation.MatchesSpecificEquipmentAsync(objImprovement, objXPathNavigator, strSelectedId, strEquipmentType, token).ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"Improvement '{objImprovement.ImprovedName}' (Target: '{objImprovement.Target}') matches specific {strEquipmentType}: {blnMatches}");
                    if (blnMatches)
                    {
                        lstImprovements.Add(objImprovement);
                    }
                }
                
                // Remove existing dynamic checkboxes that are no longer needed
                List<string> lstCheckboxesToRemove = new List<string>();
                foreach (string strModifierName in dicDynamicCostModifierCheckboxes.Keys)
                {
                    if (!lstImprovements.Any(x => x.ImprovedName == strModifierName))
                    {
                        lstCheckboxesToRemove.Add(strModifierName);
                    }
                }
                
                foreach (string strModifierName in lstCheckboxesToRemove)
                {
                    if (dicDynamicCostModifierCheckboxes.TryGetValue(strModifierName, out ColorableCheckBox objCheckbox))
                    {
                        await objCheckbox.DoThreadSafeAsync(x => x.Dispose(), token: token).ConfigureAwait(false);
                        dicDynamicCostModifierCheckboxes.Remove(strModifierName);
                    }
                }
                
                // Create or update checkboxes for current improvements
                System.Diagnostics.Debug.WriteLine($"Creating checkboxes for {lstImprovements.Count} improvements");
                
                // Only create checkboxes if there are improvements
                if (lstImprovements.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No improvements found, skipping checkbox creation");
                    return;
                }
                
                foreach (Improvement objImprovement in lstImprovements)
                {
                    string strModifierName = objImprovement.ImprovedName;
                    if (string.IsNullOrEmpty(strModifierName))
                    {
                        System.Diagnostics.Debug.WriteLine($"Skipping improvement with empty modifier name: {objImprovement.SourceName}");
                        continue;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Processing improvement: {strModifierName} (Value: {objImprovement.Value})");
                    
                    if (!dicDynamicCostModifierCheckboxes.TryGetValue(strModifierName, out ColorableCheckBox objCheckbox))
                    {
                        // Create new checkbox
                        System.Diagnostics.Debug.WriteLine($"Creating new checkbox for: {strModifierName}");
                        objCheckbox = new ColorableCheckBox
                        {
                            Anchor = AnchorStyles.Left,
                            AutoSize = true,
                            DefaultColorScheme = true,
                            Location = new System.Drawing.Point(228, 4),
                            Margin = new Padding(3, 4, 3, 4),
                            Name = $"chk{strModifierName}",
                            Size = new System.Drawing.Size(163, 17),
                            TabIndex = 70 + dicDynamicCostModifierCheckboxes.Count,
                            Tag = $"Checkbox_{strModifierName}",
                            Text = $"{strModifierName} ({CostProcessing.GetPercentageChangeText(objImprovement.Value)})",
                            UseVisualStyleBackColor = true,
                            Visible = false
                        };
                        
                        // Add event handler
                        objCheckbox.CheckedChanged += async (sender, e) =>
                        {
                            try
                            {
                                // Update the equipment's stored cost modifiers
                                await funcUpdateCostModifiers(objGenericToken).ConfigureAwait(false);
                                // Update the equipment info display
                                await funcUpdateInfo(objGenericToken).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                //swallow this
                            }
                        };
                        
                        // Add to form and dictionary
                        await flpCheckBoxes.DoThreadSafeAsync(x => x.Controls.Add(objCheckbox), token: token).ConfigureAwait(false);
                        dicDynamicCostModifierCheckboxes[strModifierName] = objCheckbox;
                    }
                    
                    // Update checkbox text and visibility
                    string strDisplayText = $"{strModifierName} ({CostProcessing.GetPercentageChangeText(objImprovement.Value)})";
                    System.Diagnostics.Debug.WriteLine($"Setting checkbox visible for: {strModifierName}");
                    await objCheckbox.DoThreadSafeAsync(x =>
                    {
                        x.Text = strDisplayText;
                        x.Visible = true;
                        System.Diagnostics.Debug.WriteLine($"Checkbox {strModifierName} visibility set to: {x.Visible}");
                    }, token: token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                //Log.Warn(ex, "Error creating dynamic cost modifier checkboxes");
            }
        }

        /// <summary>
        /// Get the current state of dynamic cost modifier checkboxes.
        /// </summary>
        /// <param name="dicDynamicCostModifierCheckboxes">Dictionary containing the checkboxes</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Dictionary of modifier names and their checked states</returns>
        public static async Task<Dictionary<string, bool>> GetDynamicCostModifierStatesAsync(
            Dictionary<string, ColorableCheckBox> dicDynamicCostModifierCheckboxes,
            CancellationToken token = default)
        {
            Dictionary<string, bool> dicStates = new Dictionary<string, bool>();
            
            foreach (KeyValuePair<string, ColorableCheckBox> kvp in dicDynamicCostModifierCheckboxes)
            {
                bool blnChecked = await kvp.Value.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                dicStates[kvp.Key] = blnChecked;
            }
            
            return dicStates;
        }

        /// <summary>
        /// Clean up dynamic cost modifier checkboxes.
        /// </summary>
        /// <param name="dicDynamicCostModifierCheckboxes">Dictionary containing the checkboxes</param>
        /// <param name="token">Cancellation token</param>
        public static async Task CleanupDynamicCostModifierCheckboxesAsync(
            Dictionary<string, ColorableCheckBox> dicDynamicCostModifierCheckboxes,
            CancellationToken token = default)
        {
            foreach (KeyValuePair<string, ColorableCheckBox> kvp in dicDynamicCostModifierCheckboxes)
            {
                await kvp.Value.DoThreadSafeAsync(x => x.Dispose(), token: token).ConfigureAwait(false);
            }
            dicDynamicCostModifierCheckboxes.Clear();
        }
    }
}
