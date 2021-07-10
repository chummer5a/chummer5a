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
using System.Text.RegularExpressions;
using Chummer.Backend.Equipment;

namespace Chummer
{
    /// <summary>
    /// This class allows to read more advanced Values out of an XmlNode
    /// Such as "FixedValues([523],[42],[421])" or "Rating * 3"
    ///
    /// Provided it doesn't contain "FixedValues" the expression is evaluated
    /// at runtime as a mathematical expression, allowing stuff such as
    /// "Rating * Rating * 4", "Rating * 5 + 2" or "(Rating / 2) * 4000"
    /// <c>Expressions are evaluated in the order of expression NOT as defined
    /// by mathematics. That means "1 + 0 * 10" evaluates to 10</c> <i>so far</i>
    /// </summary>
    public sealed class ParameterAttribute
    {
        //Keep a single regex to not create one for each class.
        //This might not be thread save if winforms ever gets multithreaded
        private static readonly Regex FixedExtract = new Regex(@"FixedValues\(([^)]*)\)");

        private readonly Gear _gear;
        private readonly string _attribute;
        private readonly double[] fixedDoubles;

        /// <summary>
        ///
        /// </summary>
        /// <param name="gear"></param>
        /// <param name="attribute"></param>
        public ParameterAttribute(Gear gear, string attribute)
        {
            _gear = gear ?? throw new ArgumentNullException(nameof(gear));
            _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));

            //If we have FixedValues use that
            //I wasn't to create array with rating as index for future, but
            //this is kept for backwards/laziness
            if (_attribute.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                //Regex to extract anything between ( ) in Param
                Match m = FixedExtract.Match(_attribute);
                string vals = m.Groups[1].Value;

                //Regex to extract anything in between [ ]
                //Not sure why i don't just split by , and remove it during
                //next phase
                MatchCollection m2 = Regex.Matches(vals, @"\[([^\]]*)\]");

                //double junk; //Not used, tryparse needs out

                List<double> lstValues = new List<double>(m2.Count);
                foreach (Match objMatch in m2)
                {
                    if (double.TryParse(objMatch.Groups[1].Value, System.Globalization.NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out double dblValue))
                        lstValues.Add(dblValue);
                }
                fixedDoubles = lstValues.ToArray();
            }
        }

        public Gear Gear => _gear;

        public string AttributeString => _attribute;

        public double AttributeDouble
        {
            get
            {
                if (fixedDoubles != null)
                {
                    if (_gear.Rating < 0)
                    {
                        //In case of underflow return lowest
                        return fixedDoubles[_gear.Rating];
                    }
                    else if (_gear.Rating >= fixedDoubles.Length)
                    {
                        //Return highest if overflow
                        return fixedDoubles[fixedDoubles.Length - 1];
                    }
                    else  //Structured like this to allow easy disabling of
                          //above code if IndexOutOfRangeException turns out to be
                          //preferred. This is an elseif
                        if (true)
                    {
                        return fixedDoubles[_gear.Rating];
                        /**/
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        public int AttributeInt => (int)AttributeDouble;
    }
}
