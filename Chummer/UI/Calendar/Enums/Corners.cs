using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chummer
{
    /// <summary>
    /// Possible corners to pass to the RoundRectangle method
    /// </summary>
    [Flags()]
    public enum Corners
    {
        /// <summary>
        /// No corners
        /// </summary>
        None = 0,

        /// <summary>
        /// Northwest corner
        /// </summary>
        NorthWest = 2,

        /// <summary>
        /// Northeast corner 
        /// </summary>
        NorthEast = 4,

        /// <summary>
        /// Southeast corner 
        /// </summary>
        SouthEast = 8,

        /// <summary>
        /// Southwest corner 
        /// </summary>
        SouthWest = 16,

        /// <summary>
        /// All corners
        /// </summary>
        All = NorthWest | NorthEast | SouthEast | SouthWest,

        /// <summary>
        /// North corner
        /// </summary>
        North = NorthWest | NorthEast,

        /// <summary>
        /// South corner
        /// </summary>
        South = SouthEast | SouthWest,

        /// <summary>
        /// East Corner
        /// </summary>
        East = NorthEast | SouthEast,

        /// <summary>
        /// West corner 
        /// </summary>
        West = NorthWest | SouthWest
    }
}