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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using NLog;

namespace Chummer
{
    public static class Utils
    {
        private static Logger Log = NLog.LogManager.GetCurrentClassLogger();
        
        public static void BreakIfDebug()
        {
#if DEBUG
            if (Debugger.IsAttached && !IsUnitTest)
                ;//               Debugger.Break();
#endif
        }

        public static bool IsRunningInVisualStudio => Process.GetCurrentProcess().ProcessName == "devenv";

        public static bool IsDesignerMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        public static Version CachedGitVersion { get; set; }

        /// <summary>
        /// This property is set in the Constructor of frmChummerMain (and NO where else!)
        /// </summary>
        public static bool IsUnitTest { get; set; }

        /// <summary>
        /// Returns the actuall path of the Chummer-Directory regardless of running as Unit test or not.
        /// </summary>

        public static string GetStartupPath
        {
            get
            {
                return !IsUnitTest ? Application.StartupPath : AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            }
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
        /// <param name="strLanguage">Language in which to display any prompts or warnings.</param>
        /// <param name="strText">Text to display in the prompt to restart. If empty, no prompt is displayed.</param>
        public static void RestartApplication(string strLanguage, string strText)
        {
            if (!string.IsNullOrEmpty(strText))
            {
                string text = LanguageManager.GetString(strText, strLanguage);
                string caption = LanguageManager.GetString("MessageTitle_Options_CloseForms", strLanguage);

                if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }
            // Need to do this here in case filenames are changed while closing forms (because a character who previously did not have a filename was saved when prompted)
            // Cannot use foreach because saving a character as created removes the current form and adds a new one
            for (int i = 0; i < Program.MainForm.OpenCharacterForms.Count; ++i)
            {
                CharacterShared objOpenCharacterForm = Program.MainForm.OpenCharacterForms[i];
                if (objOpenCharacterForm.IsDirty)
                {
                    string strCharacterName = objOpenCharacterForm.CharacterObject.CharacterName;
                    DialogResult objResult = Program.MainForm.ShowMessageBox(string.Format(LanguageManager.GetString("Message_UnsavedChanges", strLanguage), strCharacterName), LanguageManager.GetString("MessageTitle_UnsavedChanges", strLanguage), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (objResult == DialogResult.Yes)
                    {
                        // Attempt to save the Character. If the user cancels the Save As dialogue that may open, cancel the closing event so that changes are not lost.
                        bool blnResult = objOpenCharacterForm.SaveCharacter();
                        if (!blnResult)
                            return;
                        // We saved a character as created, which closed the current form and added a new one
                        // This works regardless of dispose, because dispose would just set the objOpenCharacterForm pointer to null, so OpenCharacterForms would never contain it
                        else if (!Program.MainForm.OpenCharacterForms.Contains(objOpenCharacterForm))
                            i -= 1;
                    }
                    else if (objResult == DialogResult.Cancel)
                    {
                        return;
                    }
                }
            }
            Log.Info("Restart Chummer");
            Program.MainForm.Cursor = Cursors.WaitCursor;
            // Get the parameters/arguments passed to program if any
            string arguments = string.Empty;
            foreach (CharacterShared objOpenCharacterForm in Program.MainForm.OpenCharacterForms)
            {
                arguments += '\"' + objOpenCharacterForm.CharacterObject.FileName + "\" ";
            }
            arguments = arguments.Trim();
            // Restart current application, with same arguments/parameters
            foreach (Form objForm in Program.MainForm.MdiChildren)
            {
                objForm.Close();
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = GetStartupPath + Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName,
                Arguments = arguments
            };
            Application.Exit();
            Process.Start(startInfo);
        }
    

        private static readonly Lazy<bool> _linux = new Lazy<bool>(() =>
        {
            int p = (int)Environment.OSVersion.Platform;
            return p == 4 || p == 6 || p == 128;
        });

	    public static bool IsLinux => _linux.Value;
	    public static bool IsProbablyWindows => !IsLinux;


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
			string[] stringSeparators = new string[] { "," };
			var result = responseFromServer.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

			foreach (string line in result.Where(line => line.Contains("tag_name")))
			{
				var strVersion = line.Split(':')[1];
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
        
	    public static string PascalCaseInsertSpaces(string pascalCaseName)
	    {
	         StringBuilder sb = new StringBuilder();
	        foreach (char c in pascalCaseName)
	        {
	            if (c == '_' && sb.Length != 0)
	                sb.Append(' ');
	            else
	            {
	                if (char.IsUpper(c)&& sb.Length != 0)
	                    sb.Append(' ');

	                sb.Append(c);
	            }
	        }
	        return sb.ToString();
	    }
	}
}
