using System;
using System.Globalization;
using System.Threading;

namespace Chummer.Backend.Static
{
    /// <summary>
    /// Context object for cost processing operations that encapsulates all the parameters needed for comprehensive cost processing. Saves having a large number of parameters in methods, easier to expand on.
    /// </summary>
    public class CostProcessingContext
    {
        /// <summary>
        /// The cost string to process
        /// </summary>
        public string CostString { get; set; } = string.Empty;

        /// <summary>
        /// The rating to use for calculations
        /// </summary>
        public int Rating { get; set; } = 1;

        /// <summary>
        /// Cost modifiers to apply
        /// </summary>
        public CostModifiers Modifiers { get; set; } = new CostModifiers();

        /// <summary>
        /// The number format string for display
        /// </summary>
        public string NumberFormat { get; set; } = "N0";

        /// <summary>
        /// The currency symbol
        /// </summary>
        public string CurrencySymbol { get; set; } = "¥";

        /// <summary>
        /// The culture info for formatting
        /// </summary>
        public CultureInfo Culture { get; set; } = GlobalSettings.CultureInfo;

        /// <summary>
        /// Character object for Variable cost prompting (optional)
        /// </summary>
        public Character Character { get; set; }

        /// <summary>
        /// Display name for Variable cost prompting (optional)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Whether this is for a select form (skip Variable prompting)
        /// </summary>
        public bool ForSelectForm { get; set; } = false;

        /// <summary>
        /// Whether to skip cost prompting
        /// </summary>
        public bool SkipCost { get; set; } = false;

        /// <summary>
        /// Whether to skip showing selection forms
        /// </summary>
        public bool SkipSelectForms { get; set; } = false;

        /// <summary>
        /// Whether improvements are being created
        /// </summary>
        public bool CreateImprovements { get; set; } = false;

        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = default;

        /// <summary>
        /// Creates a new CostProcessingContext with default values
        /// </summary>
        public CostProcessingContext()
        {
        }

        /// <summary>
        /// Creates a new CostProcessingContext with the specified cost string and rating
        /// </summary>
        /// <param name="costString">The cost string to process</param>
        /// <param name="rating">The rating to use for calculations</param>
        public CostProcessingContext(string costString, int rating = 1)
        {
            CostString = costString;
            Rating = rating;
        }

        /// <summary>
        /// Creates a new CostProcessingContext for selection forms
        /// </summary>
        /// <param name="costString">The cost string to process</param>
        /// <param name="rating">The rating to use for calculations</param>
        /// <param name="modifiers">Cost modifiers to apply</param>
        /// <param name="character">Character object for context</param>
        /// <param name="displayName">Display name for the item</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static CostProcessingContext ForSelectionForm(string costString, int rating, CostModifiers modifiers, 
            Character character, string displayName, CancellationToken cancellationToken = default)
        {
            return new CostProcessingContext(costString, rating)
            {
                Modifiers = modifiers,
                Character = character,
                DisplayName = displayName,
                ForSelectForm = true,
                CancellationToken = cancellationToken
            };
        }

        /// <summary>
        /// Creates a new CostProcessingContext for equipment creation
        /// </summary>
        /// <param name="costString">The cost string to process</param>
        /// <param name="rating">The rating to use for calculations</param>
        /// <param name="character">Character object for context</param>
        /// <param name="displayName">Display name for the item</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static CostProcessingContext ForEquipmentCreation(string costString, int rating, 
            Character character, string displayName, CancellationToken cancellationToken = default)
        {
            return new CostProcessingContext(costString, rating)
            {
                Character = character,
                DisplayName = displayName,
                ForSelectForm = false,
                CancellationToken = cancellationToken
            };
        }
    }
}
