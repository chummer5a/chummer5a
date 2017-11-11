using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Chummer.Backend.Extensions
{
    static class PointExtensions
    {
        public static int ManhatanDistanceFrom(this Point from, Point to)
        {
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        }
    }
}
