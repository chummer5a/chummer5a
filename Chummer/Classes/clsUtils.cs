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
﻿using System.IO;
﻿using System.Linq;
﻿using System.Net;
﻿using System.Reflection;
﻿using System.Text.RegularExpressions;
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
            Regex regex = new Regex(string.Join("|", keywords.Keys));
            number = regex.Replace(number, m => keywords[m.Value].ToString(GlobalOptions.InvariantCultureInfo));

            XmlDocument objXmlDocument = new XmlDocument();
            XPathNavigator nav = objXmlDocument.CreateNavigator();
            try
            {
                XPathExpression xprValue = nav.Compile(number);
                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                if (float.TryParse(nav.Evaluate(xprValue)?.ToString(), out parsed))
                {
                    return true;
                }
            }
            catch (XPathException ex)
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
            return Process.GetCurrentProcess().ProcessName == "devenv";
        }

        public static Version GitVersion()
        {
            Version verLatestVersion = new Version();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/chummer5a/chummer5a/releases/latest");
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.Accept = "application/json";
            // Get the response.

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.

            string responseFromServer = reader.ReadToEnd();
            string[] stringSeparators = { "," };
            string[] result = responseFromServer.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in result.Where(line => line.Contains("tag_name")))
            {
                string strVersion = line.Split(':')[1];
                strVersion = strVersion.Split('}')[0].Replace("\"", string.Empty);
                strVersion = strVersion + ".0";
                Version.TryParse(strVersion, out verLatestVersion);
                break;
            }
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            return verLatestVersion;
        }

        public static int GitUpdateAvailable()
        {
            Version verCurrentversion = Assembly.GetExecutingAssembly().GetName().Version;
            int intResult = GitVersion().CompareTo(verCurrentversion);
            return intResult;
        }
        
        public static void RestartApplication(string strText = "Message_Options_Restart")
        {
            string text = LanguageManager.Instance.GetString(strText);
            string caption = LanguageManager.Instance.GetString("MessageTitle_Options_CloseForms");

            if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            // Get the parameters/arguments passed to program if any
            string arguments = string.Empty;
            arguments += GlobalOptions.Instance.MainForm.OpenCharacters.Aggregate(arguments, (current, objCharacter) => current + ("\"" + objCharacter.FileName +"\"" + " "));
            arguments = arguments.Trim();
            // Restart current application, with same arguments/parameters
            foreach (Form objForm in GlobalOptions.Instance.MainForm.MdiChildren)
            {
                objForm.Close();
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Application.StartupPath + "\\Chummer5.exe",
                Arguments = arguments
            };
            Application.Exit();
            Process.Start(startInfo);
        }
    }
}
