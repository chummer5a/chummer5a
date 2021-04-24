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
﻿using System.IO;
using System.Reflection;
 using System.Security.AccessControl;
 using System.Security.Principal;
 using System.Text;
 using System.Windows.Forms;
 using NLog;

namespace Chummer
{
    public static class Utils
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static void BreakIfDebug()
        {
#if DEBUG
            if (Debugger.IsAttached && !IsUnitTest)
                Debugger.Break();
#endif
        }

        public static bool IsRunningInVisualStudio => Process.GetCurrentProcess().ProcessName == "devenv"; // Cannot cache this, otherwise it won't fire when the Designer is running

        public static bool IsDesignerMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime || IsRunningInVisualStudio;

        public static Version CachedGitVersion { get; set; }

        /// <summary>
        /// This property is set in the Constructor of frmChummerMain (and NO where else!)
        /// </summary>
        public static bool IsUnitTest { get; set; }

        /// <summary>
        /// Returns the actual path of the Chummer-Directory regardless of running as Unit test or not.
        /// </summary>

        public static string GetStartupPath => !IsUnitTest ? Application.StartupPath : AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        public static int GitUpdateAvailable => CachedGitVersion?.CompareTo(Assembly.GetExecutingAssembly().GetName().Version) ?? 0;

        /// <summary>
        /// Can the current user context write to a given file path?
        /// </summary>
        /// <param name="strPath">File path to evaluate.</param>
        /// <returns></returns>
        public static bool CanWriteToPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath))
                return false;
            try
            {
                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                DirectorySecurity security = Directory.GetAccessControl(Path.GetDirectoryName(strPath) ?? throw new ArgumentOutOfRangeException(nameof(strPath)));
                AuthorizationRuleCollection authRules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                foreach (FileSystemAccessRule accessRule in authRules)
                {
                    if (!(accessRule.IdentityReference is SecurityIdentifier objIdentifier) || !principal.IsInRole(objIdentifier))
                        continue;
                    if ((FileSystemRights.WriteData & accessRule.FileSystemRights) !=
                        FileSystemRights.WriteData) continue;
                    switch (accessRule.AccessControlType)
                    {
                        case AccessControlType.Allow:
                            return true;
                        case AccessControlType.Deny:
                            //Deny usually overrides any Allow
                            return false;
                    }
                }
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
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

                if (Program.MainForm.ShowMessageBox(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }
            // Need to do this here in case file names are changed while closing forms (because a character who previously did not have a filename was saved when prompted)
            // Cannot use foreach because saving a character as created removes the current form and adds a new one
            for (int i = 0; i < Program.MainForm.OpenCharacterForms.Count; ++i)
            {
                CharacterShared objOpenCharacterForm = Program.MainForm.OpenCharacterForms[i];
                if (objOpenCharacterForm.IsDirty)
                {
                    string strCharacterName = objOpenCharacterForm.CharacterObject.CharacterName;
                    DialogResult objResult = Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_UnsavedChanges", strLanguage), strCharacterName), LanguageManager.GetString("MessageTitle_UnsavedChanges", strLanguage), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
            Application.UseWaitCursor = true;
            // Get the parameters/arguments passed to program if any
            StringBuilder sbdArguments = new StringBuilder();
            foreach (CharacterShared objOpenCharacterForm in Program.MainForm.OpenCharacterForms)
            {
                sbdArguments.Append('\"' + objOpenCharacterForm.CharacterObject.FileName + "\" ");
            }
            if (sbdArguments.Length > 0)
                sbdArguments.Length -= 1;
            // Restart current application, with same arguments/parameters
            foreach (Form objForm in Program.MainForm.MdiChildren)
            {
                objForm.Close();
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = GetStartupPath + Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName,
                Arguments = sbdArguments.ToString()
            };
            Application.Exit();
            Process.Start(startInfo);
        }
    }
}
