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
        private static readonly Regex s_RgxFixedExtract = new Regex(@"FixedValues\(([^)]*)\)", RegexOptions.Compiled);

        private readonly Gear _objGear;
        private readonly string _strAttribute;
        private readonly double[] _dblFixedValues;

        /// <summary>
        ///
        /// </summary>
        /// <param name="gear"></param>
        /// <param name="attribute"></param>
        public ParameterAttribute(Gear gear, string attribute)
        {
            _objGear = gear ?? throw new ArgumentNullException(nameof(gear));
            _strAttribute = attribute ?? throw new ArgumentNullException(nameof(attribute));

            //If we have FixedValues use that
            //I wasn't to create array with rating as index for future, but
            //this is kept for backwards/laziness
            if (_strAttribute.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                //Regex to extract anything between ( ) in Param
                Match m = s_RgxFixedExtract.Match(_strAttribute);
                string strValues = m.Groups[1].Value;

                //Regex to extract anything in between [ ]
                //Not sure why i don't just split by , and remove it during
                //next phase
                MatchCollection m2 = Regex.Matches(strValues, @"\[([^\]]*)\]");

                //double junk; //Not used, tryparse needs out

                List<double> lstValues = new List<double>(m2.Count);
                foreach (Match objMatch in m2)
                {
                    if (double.TryParse(objMatch.Groups[1].Value, System.Globalization.NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out double dblValue))
                        lstValues.Add(dblValue);
                }
                _dblFixedValues = lstValues.ToArray();
            }
        }

        public Gear Gear => _objGear;

        public string AttributeString => _strAttribute;

        public double AttributeDouble
        {
            get
            {
                if (_dblFixedValues == null)
                    return 0;

                if (_objGear.Rating < 0)
                {
                    //In case of underflow return lowest
                    return _dblFixedValues[_objGear.Rating];
                }

                if (_objGear.Rating >= _dblFixedValues.Length)
                {
                    //Return highest if overflow
                    return _dblFixedValues[_dblFixedValues.Length - 1];
                }
                //Structured like this to allow easy disabling of
                //above code if IndexOutOfRangeException turns out to be
                //preferred. This is an elseif
                if (true)
                {
                    return _dblFixedValues[_objGear.Rating];
                    /**/
                }
            }
        }

        public int AttributeInt => (int)AttributeDouble;
    }
}
