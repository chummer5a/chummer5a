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
        public static bool TryFloat(string number, out float parsed, Dictionary<string, float> keywords)
        {
            //parse to base math string
            Regex regex = new Regex(string.Join("|", keywords.Keys));
            number = regex.Replace(number, m => keywords[m.Value].ToString(GlobalOptions.InvariantCultureInfo));
            
            try
            {
                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                if (float.TryParse(CommonFunctions.EvaluateInvariantXPath(number)?.ToString(), out parsed))
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

        private static Version _objCachedGitVersion = null;
        public static Version CachedGitVersion
        {
            get
            {
                return _objCachedGitVersion;
            }
            set
            {
                _objCachedGitVersion = value;
            }
        }
        public static void DoCacheGitVersion(object sender, EventArgs e)
        {
            string strUpdateLocation = "https://api.github.com/repos/chummer5a/chummer5a/releases/latest";
            if (GlobalOptions.PreferNightlyBuilds)
            {
                strUpdateLocation = "https://api.github.com/repos/chummer5a/chummer5a/releases";
            }
            HttpWebRequest request = null;
            try
            {
                WebRequest objTemp = WebRequest.Create(strUpdateLocation);
                request = objTemp as HttpWebRequest;
            }
            catch (System.Security.SecurityException)
            {
                CachedGitVersion = null;
                return;
            }
            if (request == null)
            {
                CachedGitVersion = null;
                return;
            }
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.Accept = "application/json";
            // Get the response.

            HttpWebResponse response = null;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException)
            {
            }

            // Get the stream containing content returned by the server.
            Stream dataStream = response?.GetResponseStream();
            if (dataStream == null)
            {
                CachedGitVersion = null;
                return;
            }
            Version verLatestVersion = null;
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.

            string responseFromServer = reader.ReadToEnd();
            string[] stringSeparators = new string[] { "," };
            string[] result = responseFromServer.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            string line = result.FirstOrDefault(x => x.Contains("tag_name"));
            if (!string.IsNullOrEmpty(line))
            {
                string strVersion = line.Substring(line.IndexOf(':') + 1);
                if (strVersion.Contains('}'))
                    strVersion = strVersion.Substring(0, strVersion.IndexOf('}'));
                strVersion = strVersion.FastEscape('\"');
                // Adds zeroes if minor and/or build version are missing
                while (strVersion.Count(x => x == '.') < 2)
                {
                    strVersion = strVersion + ".0";
                }
                Version.TryParse(strVersion.TrimStart("Nightly-v"), out verLatestVersion);
            }
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            CachedGitVersion = verLatestVersion;
            return;
        }

        public static int GitUpdateAvailable()
        {
            Version verCurrentversion = Assembly.GetExecutingAssembly().GetName().Version;
            int intResult = CachedGitVersion?.CompareTo(verCurrentversion) ?? 0;
            return intResult;
        }

        /// <summary>
        /// Restarts Chummer5a.
        /// </summary>
        /// <param name="strText">Text to display in the prompt to restart. If empty, no prompt is displayed.</param>
        public static void RestartApplication(string strText = "Message_Options_Restart")
        {
            if (!string.IsNullOrEmpty(strText))
            {
                string text = LanguageManager.GetString(strText);
                string caption = LanguageManager.GetString("MessageTitle_Options_CloseForms");

                if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }
            GlobalOptions.MainForm.Cursor = Cursors.WaitCursor;
            // Get the parameters/arguments passed to program if any
            string arguments = string.Empty;
            foreach (Character objOpenCharacter in GlobalOptions.MainForm.OpenCharacters)
            {
                arguments += "\"" + objOpenCharacter.FileName + "\" ";
            }
            arguments = arguments.Trim();
            // Restart current application, with same arguments/parameters
            foreach (Form objForm in GlobalOptions.MainForm.MdiChildren)
            {
                objForm.Close();
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Application.StartupPath + "\\" + AppDomain.CurrentDomain.FriendlyName,
                Arguments = arguments
            };
            Application.Exit();
            Process.Start(startInfo);
        }
    }
}
