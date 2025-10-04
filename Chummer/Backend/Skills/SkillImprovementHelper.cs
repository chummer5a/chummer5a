using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer.Backend.Skills
{
    /// <summary>
    /// Helper class for applying specialization improvements and minimum cost overrides to skills
    /// </summary>
    public static class SkillImprovementHelper
    {
        /// <summary>
        /// Applies minimum cost override improvements to a cost value
        /// </summary>
        /// <param name="character">The character to get improvements from</param>
        /// <param name="improvementType">The type of minimum cost improvement to apply</param>
        /// <param name="dictionaryKey">The skill's dictionary key</param>
        /// <param name="skillCategory">The skill's category</param>
        /// <param name="cost">The base cost to apply improvements to</param>
        /// <param name="rating">The rating for condition checking</param>
        /// <returns>The cost with minimum cost override applied</returns>
        public static int ApplyMinimumCostOverride(Character character, Improvement.ImprovementType improvementType,
            string dictionaryKey, string skillCategory, int cost, int rating)
        {
            // Check skill-specific minimum cost overrides
            int intMinOverride = ImprovementManager.GetMinimumImprovementValue(character, improvementType, dictionaryKey,
                objImprovement => (objImprovement.Maximum == 0 || rating <= objImprovement.Maximum) &&
                                objImprovement.Minimum <= rating);
            
            // Check skill category minimum cost overrides
            int intCategoryMinOverride = ImprovementManager.GetMinimumImprovementValue(character, improvementType, skillCategory,
                objImprovement => (objImprovement.Maximum == 0 || rating <= objImprovement.Maximum) &&
                                objImprovement.Minimum <= rating);
            
            // Use the minimum of both overrides
            int intFinalMinOverride = Math.Min(intMinOverride, intCategoryMinOverride);
            
            // Apply the minimum cost override if one was found
            if (intFinalMinOverride != int.MaxValue)
                return Math.Max(cost, intFinalMinOverride);
                
            return cost;
        }
        
        /// <summary>
        /// Applies minimum cost override improvements to a cost value (async version)
        /// </summary>
        /// <param name="character">The character to get improvements from</param>
        /// <param name="improvementType">The type of minimum cost improvement to apply</param>
        /// <param name="dictionaryKey">The skill's dictionary key</param>
        /// <param name="skillCategory">The skill's category</param>
        /// <param name="cost">The base cost to apply improvements to</param>
        /// <param name="rating">The rating for condition checking</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The cost with minimum cost override applied</returns>
        public static async Task<int> ApplyMinimumCostOverrideAsync(Character character, Improvement.ImprovementType improvementType,
            string dictionaryKey, string skillCategory, int cost, int rating, CancellationToken token = default)
        {
            // Check skill-specific minimum cost overrides
            int intMinOverride = await ImprovementManager.GetMinimumImprovementValueAsync(character, improvementType, dictionaryKey,
                objImprovement => (objImprovement.Maximum == 0 || rating <= objImprovement.Maximum) &&
                                objImprovement.Minimum <= rating, token).ConfigureAwait(false);
            
            // Check skill category minimum cost overrides
            int intCategoryMinOverride = await ImprovementManager.GetMinimumImprovementValueAsync(character, improvementType, skillCategory,
                objImprovement => (objImprovement.Maximum == 0 || rating <= objImprovement.Maximum) &&
                                objImprovement.Minimum <= rating, token).ConfigureAwait(false);
            
            // Use the minimum of both overrides
            int intFinalMinOverride = Math.Min(intMinOverride, intCategoryMinOverride);
            
            // Apply the minimum cost override if one was found
            if (intFinalMinOverride != int.MaxValue)
                return Math.Max(cost, intFinalMinOverride);
                
            return cost;
        }

        /// <summary>
        /// Applies specialization cost improvements to a cost value
        /// </summary>
        /// <param name="character">The character to get improvements from</param>
        /// <param name="improvementType">The type of improvement to apply</param>
        /// <param name="improvementName">The name/target of the improvement</param>
        /// <param name="cost">The base cost to apply improvements to</param>
        /// <param name="specCount">The number of specializations</param>
        /// <param name="totalBaseRating">The total base rating for condition checking</param>
        /// <param name="condition">Optional condition function to filter improvements</param>
        /// <returns>The cost with improvements applied</returns>
        public static int ApplySpecializationImprovements(Character character, Improvement.ImprovementType improvementType, 
            string improvementName, int cost, int specCount, int totalBaseRating, Func<Improvement, bool> condition = null)
        {
            // Create a combined condition that includes the base condition and rating check
            Func<Improvement, bool> combinedCondition = objImprovement =>
                (condition?.Invoke(objImprovement) ?? true) && objImprovement.Minimum <= totalBaseRating;
            
            // Apply additive improvements (multiplied by specCount)
            decimal decExtra = 0;
            foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                character, improvementType, improvementName, true))
            {
                if (combinedCondition(objImprovement))
                    decExtra += objImprovement.Value * specCount;
            }
            
            // Apply multiplier improvements
            decimal decMultiplier = 1.0m;
            foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                character, improvementType + 1, improvementName, true))
            {
                if (combinedCondition(objImprovement))
                    decMultiplier *= objImprovement.Value / 100.0m;
            }
            
            // Apply the improvements
            if (decMultiplier != 1.0m)
                return (cost * decMultiplier + decExtra).StandardRound();
            else
                return cost + decExtra.StandardRound();
        }
        
        /// <summary>
        /// Applies specialization cost improvements to a cost value (async version)
        /// </summary>
        /// <param name="character">The character to get improvements from</param>
        /// <param name="improvementType">The type of improvement to apply</param>
        /// <param name="improvementName">The name/target of the improvement</param>
        /// <param name="cost">The base cost to apply improvements to</param>
        /// <param name="specCount">The number of specializations</param>
        /// <param name="totalBaseRating">The total base rating for condition checking</param>
        /// <param name="condition">Optional condition function to filter improvements</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The cost with improvements applied</returns>
        public static async Task<int> ApplySpecializationImprovementsAsync(Character character, Improvement.ImprovementType improvementType, 
            string improvementName, int cost, int specCount, int totalBaseRating, Func<Improvement, bool> condition = null, CancellationToken token = default)
        {
            // Create a combined condition that includes the base condition and rating check
            Func<Improvement, bool> combinedCondition = objImprovement =>
                (condition?.Invoke(objImprovement) ?? true) && objImprovement.Minimum <= totalBaseRating;
            
            // Apply additive improvements (multiplied by specCount)
            decimal decExtra = 0;
            foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                character, improvementType, improvementName, true, token).ConfigureAwait(false))
            {
                if (combinedCondition(objImprovement))
                    decExtra += objImprovement.Value * specCount;
            }
            
            // Apply multiplier improvements
            decimal decMultiplier = 1.0m;
            foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                character, improvementType + 1, improvementName, true, token).ConfigureAwait(false))
            {
                if (combinedCondition(objImprovement))
                    decMultiplier *= objImprovement.Value / 100.0m;
            }
            
            // Apply the improvements
            if (decMultiplier != 1.0m)
                return (cost * decMultiplier + decExtra).StandardRound();
            else
                return cost + decExtra.StandardRound();
        }
    }
}
