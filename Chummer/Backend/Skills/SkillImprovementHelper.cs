using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer.Backend.Skills
{
    /// <summary>
    /// Helper class for applying specialization improvements to skills
    /// </summary>
    public static class SkillImprovementHelper
    {
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
            decimal decExtra = 0;
            decimal decMultiplier = 1.0m;
            
            // Apply additive improvements
            foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                character, improvementType, improvementName, true))
            {
                if (condition?.Invoke(objImprovement) ?? true)
                {
                    if (objImprovement.Minimum <= totalBaseRating)
                        decExtra += objImprovement.Value * specCount;
                }
            }
            
            // Apply multiplier improvements
            foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                character, improvementType + 1, improvementName, true))
            {
                if (condition?.Invoke(objImprovement) ?? true)
                {
                    if (objImprovement.Minimum <= totalBaseRating)
                        decMultiplier *= objImprovement.Value / 100.0m;
                }
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
            decimal decExtra = 0;
            decimal decMultiplier = 1.0m;
            
            // Apply additive improvements
            foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                character, improvementType, improvementName, true, token).ConfigureAwait(false))
            {
                if (condition?.Invoke(objImprovement) ?? true)
                {
                    if (objImprovement.Minimum <= totalBaseRating)
                        decExtra += objImprovement.Value * specCount;
                }
            }
            
            // Apply multiplier improvements
            foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                character, improvementType + 1, improvementName, true, token).ConfigureAwait(false))
            {
                if (condition?.Invoke(objImprovement) ?? true)
                {
                    if (objImprovement.Minimum <= totalBaseRating)
                        decMultiplier *= objImprovement.Value / 100.0m;
                }
            }
            
            // Apply the improvements
            if (decMultiplier != 1.0m)
                return (cost * decMultiplier + decExtra).StandardRound();
            else
                return cost + decExtra.StandardRound();
        }
    }
}