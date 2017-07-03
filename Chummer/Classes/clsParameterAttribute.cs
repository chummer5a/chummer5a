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
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    /// <summary>
    /// This class allows to read more advanced Values out of an XmlNode
    /// Such as "FixedValues([523],[42],[421])" or "Rating * 3"
    /// 
    /// Provided it doesn't contain "FixedValues" the expression is evaulated
    /// at runtime as a mathematical expression, allowing stuff such as
    /// "Rating * Rating * 4", "Rating * 5 + 2" or "(Rating / 2) * 4000"
    /// <c>Expressions are evaluated in the order of expression NOT as defined
    /// by mathematics. That means "1 + 0 * 10" evaluates to 10</c> <i>so far</i>
    /// </summary>
    class ParameterAttribute
    {
         //Keep a single regex to not create one for each class.
        //This might not be thread save if winforms ever gets multithreaded
        private static Regex FixedExtract = new Regex(@"FixedValues\(([^)]*)\)");
        private Gear _gear;
        private String _attribute;
        private double[] fixedDoubles;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gear"></param>
        /// <param name="attribute"></param>
        public ParameterAttribute(Gear gear, String attribute)
        {
            
            _gear = gear;
            _attribute = attribute;

            //If we have FixedValues use that
            //I wan't to create array with rating as index for future, but
            //this is keept for backwards/laziness
            if (_attribute.StartsWith("FixedValues"))
            {
                //Regex to extracxt anything between ( ) in Param
                Match m = FixedExtract.Match(_attribute);
                String vals = m.Groups[1].Value;

                //Regex to extract anything inbetween [ ]
                //Not sure why i don't just split by , and remove it durring 
                //next phase
                MatchCollection m2 = Regex.Matches(vals, @"\[([^\]]*)\]");

                double junk; //Not used, tryparse needs out

                //LINQ magic to cast matchcollection to the double[]
                fixedDoubles = (from val in m2.Cast<Match>()
                    where double.TryParse(val.Groups[1].Value, out junk)
                    select double.Parse(val.Groups[1].Value)).ToArray();
            }
            else
            {
                
            }

            
        }

        public Gear Gear
        {
            get { return _gear; }
        }

        public String AttributeString
        {
            get { return _attribute; }
        }

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
                        //prefered. This is an elseif
                        if(true)
                        {
                            return fixedDoubles[_gear.Rating];
                    /**/}
                }
                else
                {
                    return 0;
                }
            }
        }

        public int AttributeInt
        {
            get { return (int) AttributeDouble; }
        }


    }
}
