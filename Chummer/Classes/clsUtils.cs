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
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
	static class Utils
	{
		//someday this should parse into an abstract syntax tree, but this hack
		//have worked for a few years, and will work a few years more
		public static bool TryFloat(string number, out float parsed, Dictionary<string, float> keywords )
		{
			//parse to base math string
			try
			{
				Regex regex = new Regex(String.Join("|", keywords.Keys));
				number = regex.Replace(number, m => keywords[m.Value].ToString(System.Globalization.CultureInfo.InvariantCulture));

				XmlDocument objXmlDocument = new XmlDocument();
				XPathNavigator nav = objXmlDocument.CreateNavigator();
				XPathExpression xprValue = nav.Compile(number);

				// Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
				if (float.TryParse(nav.Evaluate(xprValue).ToString(), out parsed))
				{
					return true;
				}
			}
			catch (Exception ex)
			{	
				Log.Exception(ex);
			}

			parsed = 0;
			return false;
		}

		public static void BreakIfDebug()
		{
			if (Debugger.IsAttached)
				Debugger.Break();
			
		}

		public static bool IsRunningInVisualStudio()
		{
			return System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv";
		}

	}
}
