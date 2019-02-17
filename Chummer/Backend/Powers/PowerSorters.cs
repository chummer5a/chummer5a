using System;
using System.Collections.Generic;

namespace Chummer.Backend.Powers
{
    class PowerSorter : IComparer<Power>
    {
        private readonly Comparison<Power> _comparison;
        
        public PowerSorter(Comparison<Power> comparison)
        {
            if(comparison == null) throw new ArgumentNullException(nameof(comparison));

            _comparison = comparison;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(Power x, Power y)
        {
            return _comparison(x, y);
        }
    }
}
