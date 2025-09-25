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

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Chummer
{
    /// <summary>
    /// Interface for equipment that supports dynamic cost modifiers.
    /// Extends IHasCost to include cost modifier functionality.
    /// </summary>
    public interface IHasCostModifiers : IHasCost
    {
        /// <summary>
        /// Dictionary of enabled cost modifier checkboxes and their states.
        /// Key: Modifier name (e.g., "RangedWeaponDiscount", "Custom_Markup_10")
        /// Value: Whether the modifier is currently enabled
        /// </summary>
        Dictionary<string, bool> EnabledCostModifiers { get; set; }

        /// <summary>
        /// The equipment type string used for filtering improvements (e.g., "weapon", "armor", "gear").
        /// </summary>
        string EquipmentType { get; }

        /// <summary>
        /// Generate a detailed tooltip showing cost breakdown including base cost, modifiers, and final cost.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Formatted tooltip string</returns>
        Task<string> GetCostTooltipAsync(CancellationToken token = default);

        /// <summary>
        /// Clean up any enabled cost modifiers that are no longer valid (e.g., when improvements are removed).
        /// </summary>
        /// <param name="token">Cancellation token</param>
        void CleanupInvalidCostModifiers(CancellationToken token = default);

        /// <summary>
        /// Clean up any enabled cost modifiers that are no longer valid (async version).
        /// </summary>
        /// <param name="token">Cancellation token</param>
        Task CleanupInvalidCostModifiersAsync(CancellationToken token = default);

        /// <summary>
        /// Set markup as a custom cost modifier on this equipment.
        /// </summary>
        /// <param name="decMarkupPercentage">Markup percentage (e.g., 10 for 10% markup)</param>
        void SetMarkup(decimal decMarkupPercentage);
    }
}
