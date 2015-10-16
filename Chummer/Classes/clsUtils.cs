using System;
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

	}
}
