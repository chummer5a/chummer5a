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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
#if DEBUG
using System.Runtime.InteropServices;
#endif
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Win32;
using NLog;

namespace Chummer
{
    public static class Utils
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BreakIfDebug()
        {
#if DEBUG
            if (Debugger.IsAttached && !IsUnitTest)
                Debugger.Break();
#else
            // Method intentionally left empty.
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BreakOnErrorIfDebug()
        {
#if DEBUG
            if (Debugger.IsAttached && !IsUnitTest)
            {
                int intErrorCode = Marshal.GetLastWin32Error();
                if (intErrorCode != 0)
                    Debugger.Break();
            }
#else
            // Method intentionally left empty.
#endif
        }

        // Need this as a Lazy, otherwise it won't fire properly in the designer if we just cache it, and the check itself is also quite expensive
        private static readonly Lazy<bool> s_BlnIsRunningInVisualStudio =
            new Lazy<bool>(() => Program.MyProcess.ProcessName == "devenv");

        /// <summary>
        /// Returns if we are running inside Visual Studio, e.g. if we are in the designer.
        /// </summary>
        public static bool IsRunningInVisualStudio => s_BlnIsRunningInVisualStudio.Value;

        /// <summary>
        /// Returns if we are in VS's Designer.
        /// WARNING! Will not work with WPF! Use in combination with Utils.IsRunningInVisualStudio for WPF controls running inside of WinForms.
        /// </summary>
        public static bool IsDesignerMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>
        /// Cached latest version of Chummer from its GitHub page.
        /// </summary>
        public static Version CachedGitVersion { get; set; }

        private static bool _blnIsUnitTest;

        /// <summary>
        /// This property is set in the Constructor of frmChummerMain (and NO where else!)
        /// </summary>
        public static bool IsUnitTest
        {
            get => _blnIsUnitTest;
            set
            {
                if (_blnIsUnitTest == value)
                    return;
                _blnIsUnitTest = value;
                _blnIsOkToRunDoEvents = DefaultIsOkToRunDoEvents;
            }
        }

        /// <summary>
        /// Returns the actual path of the Chummer-Directory regardless of running as Unit test or not.
        /// </summary>
        public static string GetStartupPath => IsUnitTest ? AppDomain.CurrentDomain.SetupInformation.ApplicationBase : Application.StartupPath;

        public static string GetAutosavesFolderPath => Path.Combine(GetStartupPath, "saves", "autosave");

        public static ReadOnlyCollection<string> BasicDataFileNames { get; } = Array.AsReadOnly(new[]
        {
            "actions.xml",
            "armor.xml",
            "bioware.xml",
            "books.xml",
            "complexforms.xml",
            "contacts.xml",
            "critters.xml",
            "critterpowers.xml",
            "cyberware.xml",
            "drugcomponents.xml",
            "echoes.xml",
            "options.xml",
            "gear.xml",
            "improvements.xml",
            "licenses.xml",
            "lifemodules.xml",
            "lifestyles.xml",
            "martialarts.xml",
            "mentors.xml",
            "metamagic.xml",
            "metatypes.xml",
            "options.xml",
            "packs.xml",
            "paragons.xml",
            "powers.xml",
            "priorities.xml",
            "programs.xml",
            "qualities.xml",
            "ranges.xml",
            "references.xml",
            "settings.xml",
            "sheets.xml",
            "skills.xml",
            "spells.xml",
            "spiritpowers.xml",
            "streams.xml",
            "traditions.xml",
            "vehicles.xml",
            "weapons.xml"
        });

        private static readonly Lazy<Version> s_ObjCurrentChummerVersion = new Lazy<Version>(() => typeof(Program).Assembly.GetName().Version);

        public static Version CurrentChummerVersion => s_ObjCurrentChummerVersion.Value;

        public static bool IsMilestoneVersion => CurrentChummerVersion.Build == 0;

        public static int GitUpdateAvailable => CachedGitVersion?.CompareTo(CurrentChummerVersion) ?? 0;

        public const int DefaultSleepDuration = 15;

        public const int SleepEmergencyReleaseMaxTicks = 1000;

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
        /// Test if the file at a given path is accessible to write operations.
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns>File is locked if True.</returns>
        public static bool IsFileLocked(string strPath)
        {
            try
            {
                using (File.Open(strPath, FileMode.Open))
                    return false;
            }
            catch (FileNotFoundException)
            {
                // File doesn't exist.
                return true;
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            catch (Exception)
            {
                BreakIfDebug();
                return true;
            }
        }

        /// <summary>
        /// Wait for an open file to be available for deletion and then delete it.
        /// </summary>
        /// <param name="strPath">File path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if the file cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <returns>True if file does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static bool SafeDeleteFile(string strPath, bool blnShowUnauthorizedAccess = false, int intTimeout = DefaultSleepDuration * 60)
        {
            return SafeDeleteFileCoreAsync(true, strPath, blnShowUnauthorizedAccess, intTimeout).GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Wait for an open file to be available for deletion and then delete it.
        /// </summary>
        /// <param name="strPath">File path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if the file cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <returns>True if file does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static Task<bool> SafeDeleteFileAsync(string strPath, bool blnShowUnauthorizedAccess = false, int intTimeout = DefaultSleepDuration * 60)
        {
            return SafeDeleteFileCoreAsync(false, strPath, blnShowUnauthorizedAccess, intTimeout);
        }

        /// <summary>
        /// Wait for an open file to be available for deletion and then delete it.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strPath">File path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if the file cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <returns>True if file does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        private static async Task<bool> SafeDeleteFileCoreAsync(bool blnSync, string strPath, bool blnShowUnauthorizedAccess, int intTimeout)
        {
            if (string.IsNullOrEmpty(strPath))
                return true;
            int intWaitInterval = Math.Max(intTimeout / DefaultSleepDuration, DefaultSleepDuration);
            while (File.Exists(strPath))
            {
                try
                {
                    if (!strPath.StartsWith(GetStartupPath, StringComparison.OrdinalIgnoreCase) && !strPath.StartsWith(GetTempPath(), StringComparison.OrdinalIgnoreCase))
                    {
                        // For safety purposes, do not allow unprompted deleting of any files outside of the Chummer folder itself
                        if (blnShowUnauthorizedAccess)
                        {
                            if (Program.ShowMessageBox(
                                    string.Format(GlobalSettings.CultureInfo,
                                        blnSync
                                            // ReSharper disable once MethodHasAsyncOverload
                                            ? LanguageManager.GetString("Message_Prompt_Delete_Existing_File")
                                            : await LanguageManager.GetStringAsync(
                                                "Message_Prompt_Delete_Existing_File"), strPath),
                                    buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Warning) != DialogResult.Yes)
                                return false;
                        }
                        else
                        {
                            BreakIfDebug();
                            return false;
                        }
                    }

                    if (blnSync)
                        File.Delete(strPath);
                    else
                        await Task.Run(() => File.Delete(strPath));
                }
                catch (PathTooLongException)
                {
                    // File path is somehow too long? File is not deleted, so return false.
                    return false;
                }
                catch (UnauthorizedAccessException)
                {
                    // We do not have sufficient privileges to delete this file.
                    if (blnShowUnauthorizedAccess)
                        Program.ShowMessageBox(blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? LanguageManager.GetString("Message_Insufficient_Permissions_Warning")
                            : await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning"));
                    return false;
                }
                catch (DirectoryNotFoundException)
                {
                    // File doesn't exist.
                    return true;
                }
                catch (FileNotFoundException)
                {
                    // File doesn't exist.
                    return true;
                }
                catch (IOException)
                {
                    //the file is unavailable because it is:
                    //still being written to
                    //or being processed by another thread
                    //or does not exist (has already been processed)
                    if (blnSync)
                        SafeSleep(intWaitInterval);
                    else
                        await SafeSleepAsync(intWaitInterval);
                    intTimeout -= intWaitInterval;
                }
                if (intTimeout < 0)
                {
                    BreakIfDebug();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Wait for an open directory to be available for deletion and then delete it.
        /// </summary>
        /// <param name="strPath">Directory path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if the directory cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static bool SafeDeleteDirectory(string strPath, bool blnShowUnauthorizedAccess = false, int intTimeout = DefaultSleepDuration * 60)
        {
            return SafeDeleteDirectoryCoreAsync(true, strPath, blnShowUnauthorizedAccess, intTimeout).GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Wait for an open directory to be available for deletion and then delete it.
        /// </summary>
        /// <param name="strPath">Directory path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if the directory cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static Task<bool> SafeDeleteDirectoryAsync(string strPath, bool blnShowUnauthorizedAccess = false, int intTimeout = DefaultSleepDuration * 60)
        {
            return SafeDeleteDirectoryCoreAsync(false, strPath, blnShowUnauthorizedAccess, intTimeout);
        }

        /// <summary>
        /// Wait for an open directory to be available for deletion and then delete it.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strPath">Directory path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if the directory cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        private static async Task<bool> SafeDeleteDirectoryCoreAsync(bool blnSync, string strPath, bool blnShowUnauthorizedAccess, int intTimeout)
        {
            if (string.IsNullOrEmpty(strPath))
                return true;
            if (!Directory.Exists(strPath))
                return true;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                SafeClearDirectory(strPath, blnShowUnauthorizedAccess: blnShowUnauthorizedAccess,
                    intTimeout: intTimeout);
            else
                await SafeClearDirectoryAsync(strPath, blnShowUnauthorizedAccess: blnShowUnauthorizedAccess,
                    intTimeout: intTimeout);
            int intWaitInterval = Math.Max(intTimeout / DefaultSleepDuration, DefaultSleepDuration);
            while (Directory.Exists(strPath))
            {
                try
                {
                    if (!strPath.StartsWith(GetStartupPath, StringComparison.OrdinalIgnoreCase) && !strPath.StartsWith(GetTempPath(), StringComparison.OrdinalIgnoreCase))
                    {
                        // For safety purposes, do not allow unprompted deleting of any files outside of the Chummer folder itself
                        if (blnShowUnauthorizedAccess)
                        {
                            if (Program.ShowMessageBox(
                                    string.Format(GlobalSettings.CultureInfo,
                                        blnSync
                                            // ReSharper disable once MethodHasAsyncOverload
                                            ? LanguageManager.GetString("Message_Prompt_Delete_Existing_File")
                                            : await LanguageManager.GetStringAsync(
                                                "Message_Prompt_Delete_Existing_File"), strPath),
                                    buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Warning) != DialogResult.Yes)
                                return false;
                        }
                        else
                        {
                            BreakIfDebug();
                            return false;
                        }
                    }

                    if (blnSync)
                        Directory.Delete(strPath, true);
                    else
                        await Task.Run(() => Directory.Delete(strPath, true));
                }
                catch (PathTooLongException)
                {
                    // File path is somehow too long? File is not deleted, so return false.
                    return false;
                }
                catch (UnauthorizedAccessException)
                {
                    // We do not have sufficient privileges to delete this file.
                    if (blnShowUnauthorizedAccess)
                        Program.ShowMessageBox(blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? LanguageManager.GetString("Message_Insufficient_Permissions_Warning")
                            : await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning"));
                    return false;
                }
                catch (DirectoryNotFoundException)
                {
                    // File doesn't exist.
                    return true;
                }
                catch (FileNotFoundException)
                {
                    // File doesn't exist.
                    return true;
                }
                catch (IOException)
                {
                    //the file is unavailable because it is:
                    //still being written to
                    //or being processed by another thread
                    //or does not exist (has already been processed)
                    if (blnSync)
                        SafeSleep(intWaitInterval);
                    else
                        await SafeSleepAsync(intWaitInterval);
                    intTimeout -= intWaitInterval;
                }
                if (intTimeout < 0)
                {
                    BreakIfDebug();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Safely deletes all files in a directory (though the directory itself remains).
        /// </summary>
        /// <param name="strPath">Directory path to clear.</param>
        /// <param name="strSearchPattern">Search pattern to use for finding files to delete. Use "*" if you wish to clear all files.</param>
        /// <param name="blnRecursive">Whether to a delete all subdirectories, too.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if a file cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static bool SafeClearDirectory(string strPath, string strSearchPattern = "*", bool blnRecursive = true, bool blnShowUnauthorizedAccess = false, int intTimeout = DefaultSleepDuration * 60)
        {
            return SafeClearDirectoryCoreAsync(true, strPath, strSearchPattern, blnRecursive, blnShowUnauthorizedAccess,
                intTimeout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Safely deletes all files in a directory (though the directory itself remains).
        /// </summary>
        /// <param name="strPath">Directory path to clear.</param>
        /// <param name="strSearchPattern">Search pattern to use for finding files to delete. Use "*" if you wish to clear all files.</param>
        /// <param name="blnRecursive">Whether to a delete all subdirectories, too.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if a file cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static Task<bool> SafeClearDirectoryAsync(string strPath, string strSearchPattern = "*", bool blnRecursive = true, bool blnShowUnauthorizedAccess = false, int intTimeout = DefaultSleepDuration * 60)
        {
            return SafeClearDirectoryCoreAsync(false, strPath, strSearchPattern, blnRecursive, blnShowUnauthorizedAccess,
                intTimeout);
        }

        /// <summary>
        /// Safely deletes all files in a directory (though the directory itself remains).
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strPath">Directory path to clear.</param>
        /// <param name="strSearchPattern">Search pattern to use for finding files to delete. Use "*" if you wish to clear all files.</param>
        /// <param name="blnRecursive">Whether to a delete all subdirectories, too.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if a file cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        private static async Task<bool> SafeClearDirectoryCoreAsync(bool blnSync, string strPath, string strSearchPattern, bool blnRecursive, bool blnShowUnauthorizedAccess, int intTimeout)
        {
            if (string.IsNullOrEmpty(strPath) || !Directory.Exists(strPath))
                return true;
            if (!strPath.StartsWith(GetStartupPath, StringComparison.OrdinalIgnoreCase)
                && !strPath.StartsWith(GetTempPath(), StringComparison.OrdinalIgnoreCase))
            {
                // For safety purposes, do not allow unprompted deleting of any files outside of the Chummer folder itself
                if (blnShowUnauthorizedAccess)
                {
                    if (Program.ShowMessageBox(
                            string.Format(GlobalSettings.Language,
                                blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? LanguageManager.GetString("Message_Prompt_Delete_Existing_File")
                                    : await LanguageManager.GetStringAsync("Message_Prompt_Delete_Existing_File"),
                                strPath),
                            buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Warning)
                        != DialogResult.Yes)
                        return false;
                }
                else
                {
                    BreakIfDebug();
                    return false;
                }
            }
            string[] astrFilesToDelete = Directory.GetFiles(strPath, strSearchPattern,
                                                            blnRecursive
                                                                ? SearchOption.AllDirectories
                                                                : SearchOption.TopDirectoryOnly);
            if (blnSync)
            {
                object objSuccessLock = new object();
                bool blnReturn = true;
                Parallel.ForEach(astrFilesToDelete, () => true, (strToDelete, x, y) =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    return SafeDeleteFile(strToDelete, false, intTimeout);
                }, blnLoop =>
                {
                    lock (objSuccessLock)
                    {
                        if (!blnLoop)
                            // ReSharper disable once AccessToModifiedClosure
                            blnReturn = false;
                    }
                });
                return blnReturn;
            }

            Task<bool>[] atskSuccesses = new Task<bool>[astrFilesToDelete.Length];
            for (int i = 0; i < astrFilesToDelete.Length; i++)
            {
                string strToDelete = astrFilesToDelete[i];
                atskSuccesses[i] = SafeDeleteFileAsync(strToDelete, false, intTimeout);
            }
            foreach (Task<bool> x in atskSuccesses)
            {
                if (!await x)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Restarts Chummer5a.
        /// </summary>
        /// <param name="strLanguage">Language in which to display any prompts or warnings. If empty, use Chummer's current language.</param>
        /// <param name="strText">Text to display in the prompt to restart. If empty, no prompt is displayed.</param>
        public static async ValueTask RestartApplication(string strLanguage = "", string strText = "")
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            if (!string.IsNullOrEmpty(strText))
            {
                string text = await LanguageManager.GetStringAsync(strText, strLanguage);
                string caption = await LanguageManager.GetStringAsync("MessageTitle_Options_CloseForms", strLanguage);

                if (Program.ShowMessageBox(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
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
                    DialogResult objResult = Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_UnsavedChanges", strLanguage), strCharacterName), await LanguageManager.GetStringAsync("MessageTitle_UnsavedChanges", strLanguage), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (objResult)
                    {
                        case DialogResult.Yes:
                            {
                                // Attempt to save the Character. If the user cancels the Save As dialogue that may open, cancel the closing event so that changes are not lost.
                                bool blnResult = await objOpenCharacterForm.SaveCharacter();
                                if (!blnResult)
                                    return;
                                // We saved a character as created, which closed the current form and added a new one
                                // This works regardless of dispose, because dispose would just set the objOpenCharacterForm pointer to null, so OpenCharacterForms would never contain it
                                if (!Program.MainForm.OpenCharacterForms.Contains(objOpenCharacterForm))
                                    --i;
                                break;
                            }
                        case DialogResult.Cancel:
                            return;
                    }
                }
            }
            Log.Info("Restart Chummer");
            Application.UseWaitCursor = true;
            // Get the parameters/arguments passed to program if any
            string strArguments;
            using (new FetchSafelyFromPool<StringBuilder>(StringBuilderPool, out StringBuilder sbdArguments))
            {
                foreach (CharacterShared objOpenCharacterForm in Program.MainForm.OpenCharacterForms)
                {
                    sbdArguments.Append('\"').Append(objOpenCharacterForm.CharacterObject.FileName).Append("\" ");
                }

                if (sbdArguments.Length > 0)
                    --sbdArguments.Length;
                // Restart current application, with same arguments/parameters
                foreach (Form objForm in Program.MainForm.MdiChildren)
                {
                    objForm.Close();
                }

                strArguments = sbdArguments.ToString();
            }
            ProcessStartInfo objStartInfo = new ProcessStartInfo
            {
                FileName = GetStartupPath + Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName,
                Arguments = strArguments
            };
            Application.Exit();
            objStartInfo.Start();
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Task StartStaTask(Action func)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Thread thread = new Thread(() =>
            {
                try
                {
                    tcs.SetResult(DummyFunction());
                    // This is needed because SetResult always needs a return type
                    bool DummyFunction()
                    {
                        func.Invoke();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Task<T> StartStaTask<T>(Func<T> func)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            Thread thread = new Thread(() =>
            {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Task StartStaTask(Task func)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Thread thread = new Thread(RunFunction);
            async void RunFunction()
            {
                try
                {
                    tcs.SetResult(await DummyFunction());
                    // This is needed because SetResult always needs a return type
                    async ValueTask<bool> DummyFunction()
                    {
                        await func;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Task<T> StartStaTask<T>(Task<T> func)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            Thread thread = new Thread(RunFunction);
            async void RunFunction()
            {
                try
                {
                    tcs.SetResult(await func);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep with the default sleep duration done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// Because async functions don't lock threads, it does not need to manually call events anyway.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable SafeSleepAsync()
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return SafeSleepAsync(DefaultSleepDuration);
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// Because async functions don't lock threads, it does not need to manually call events anyway.
        /// </summary>
        /// <param name="intDurationMilliseconds">Duration to wait in milliseconds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable SafeSleepAsync(int intDurationMilliseconds)
        {
            return Task.Delay(intDurationMilliseconds).ConfigureAwait(false);
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep with the default sleep duration done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// </summary>
        /// <param name="blnForceDoEvents">Force running of events. Useful for unit tests where running events is normally disabled.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeSleep(bool blnForceDoEvents = false)
        {
            SafeSleep(DefaultSleepDuration, blnForceDoEvents);
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// </summary>
        /// <param name="intDurationMilliseconds">Duration to wait in milliseconds.</param>
        /// <param name="blnForceDoEvents">Force running of events. Useful for unit tests where running events is normally disabled.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeSleep(int intDurationMilliseconds, bool blnForceDoEvents = false)
        {
            for (; intDurationMilliseconds > 0; intDurationMilliseconds -= DefaultSleepDuration)
            {
                Thread.Sleep(intDurationMilliseconds);
                if (!EverDoEvents)
                    return;
                bool blnDoEvents = blnForceDoEvents || _blnIsOkToRunDoEvents;
                try
                {
                    if (blnDoEvents)
                    {
                        _blnIsOkToRunDoEvents = false;
                        Application.DoEvents();
                    }
                }
                finally
                {
                    if (blnDoEvents)
                        _blnIsOkToRunDoEvents = DefaultIsOkToRunDoEvents;
                }
            }
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// </summary>
        /// <param name="objTimeSpan">Duration to wait. If 0 or less milliseconds, DefaultSleepDuration is used instead.</param>
        /// <param name="blnForceDoEvents">Force running of events. Useful for unit tests where running events is normally disabled.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeSleep(TimeSpan objTimeSpan, bool blnForceDoEvents = false)
        {
            SafeSleep(objTimeSpan.Milliseconds, blnForceDoEvents);
        }

        /// <summary>
        /// Never wait around in designer mode, we should not care about thread locking, and running in a background thread can mess up IsDesignerMode checks inside that thread
        /// </summary>
        private static bool EverDoEvents => Program.IsMainThread && !IsDesignerMode && !IsRunningInVisualStudio;

        /// <summary>
        /// Don't run events during unit tests, but still run in the background so that we can catch any issues caused by our setup.
        /// </summary>
        private static bool DefaultIsOkToRunDoEvents => !IsUnitTest && EverDoEvents;

        /// <summary>
        /// This member makes sure we aren't swamping the program with massive amounts of Application.DoEvents() calls
        /// </summary>
        private static bool _blnIsOkToRunDoEvents = DefaultIsOkToRunDoEvents;

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(Action funcToRun)
        {
            if (!EverDoEvents)
            {
                funcToRun.Invoke();
                return;
            }
            Task objTask = Task.Run(funcToRun);
            while (!objTask.IsCompleted)
                SafeSleep();
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(params Action[] afuncToRun)
        {
            if (!EverDoEvents)
            {
                Parallel.Invoke(afuncToRun);
                return;
            }
            Task objTask = Task.Run(() => Parallel.Invoke(afuncToRun));
            while (!objTask.IsCompleted)
                SafeSleep();
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RunWithoutThreadLock<T>(Func<T> funcToRun)
        {
            if (!EverDoEvents)
            {
                return funcToRun.Invoke();
            }
            Task<T> objTask = Task.Run(funcToRun);
            while (!objTask.IsCompleted)
                SafeSleep();
            return objTask.Result;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] RunWithoutThreadLock<T>(params Func<T>[] afuncToRun)
        {
            T[] aobjReturn = new T[afuncToRun.Length];
            if (!EverDoEvents)
            {
                Parallel.For(0, afuncToRun.Length, i => aobjReturn[i] = afuncToRun[i].Invoke());
                return aobjReturn;
            }
            Task<T>[] aobjTasks = new Task<T>[afuncToRun.Length];
            for (int i = 0; i < afuncToRun.Length; ++i)
                aobjTasks[i] = Task.Run(afuncToRun[i]);
            Task<T[]> objTask = Task.Run(() => Task.WhenAll(aobjTasks));
            while (!objTask.IsCompleted)
                SafeSleep();
            for (int i = 0; i < afuncToRun.Length; ++i)
                aobjReturn[i] = aobjTasks[i].Result;
            return aobjReturn;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RunWithoutThreadLock<T>(Func<Task<T>> funcToRun)
        {
            if (!EverDoEvents)
            {
                Task<T> objSyncTask = funcToRun.Invoke();
                if (objSyncTask.Status == TaskStatus.Created)
                    objSyncTask.RunSynchronously();
                return objSyncTask.Result;
            }
            Task<T> objTask = Task.Run(funcToRun);
            while (!objTask.IsCompleted)
                SafeSleep();
            return objTask.Result;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] RunWithoutThreadLock<T>(params Func<Task<T>>[] afuncToRun)
        {
            T[] aobjReturn = new T[afuncToRun.Length];
            if (!EverDoEvents)
            {
                Parallel.For(0, afuncToRun.Length, i =>
                {
                    Task<T> objSyncTask = afuncToRun[i].Invoke();
                    if (objSyncTask.Status == TaskStatus.Created)
                        objSyncTask.RunSynchronously();
                    aobjReturn[i] = objSyncTask.Result;
                });
                return aobjReturn;
            }
            Task<T>[] aobjTasks = new Task<T>[afuncToRun.Length];
            for (int i = 0; i < afuncToRun.Length; ++i)
                aobjTasks[i] = Task.Run(afuncToRun[i]);
            Task<T[]> objTask = Task.Run(() => Task.WhenAll(aobjTasks));
            while (!objTask.IsCompleted)
                SafeSleep();
            for (int i = 0; i < afuncToRun.Length; ++i)
                aobjReturn[i] = aobjTasks[i].Result;
            return aobjReturn;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(Func<Task> funcToRun)
        {
            if (!EverDoEvents)
            {
                Task objSyncTask = funcToRun.Invoke();
                if (objSyncTask.Status == TaskStatus.Created)
                    objSyncTask.RunSynchronously();
                return;
            }
            Task objTask = Task.Run(funcToRun);
            while (!objTask.IsCompleted)
                SafeSleep();
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(params Func<Task>[] afuncToRun)
        {
            if (!EverDoEvents)
            {
                Parallel.ForEach(afuncToRun, funcToRun =>
                {
                    Task objSyncTask = funcToRun.Invoke();
                    if (objSyncTask.Status == TaskStatus.Created)
                        objSyncTask.RunSynchronously();
                });
                return;
            }
            Task[] aobjTasks = new Task[afuncToRun.Length];
            for (int i = 0; i < afuncToRun.Length; ++i)
                aobjTasks[i] = Task.Run(afuncToRun[i]);
            Task objTask = Task.Run(() => Task.WhenAll(aobjTasks));
            while (!objTask.IsCompleted)
                SafeSleep();
        }

        private static readonly Lazy<string> _strHumanReadableOSVersion = new Lazy<string>(GetHumanReadableOSVersion);

        public static string HumanReadableOSVersion => _strHumanReadableOSVersion.Value;

        /// <summary>
        /// Gets a human-readable version of the current Environment's Windows version.
        /// It will return something like "Windows XP" or "Windows 7" or "Windows 10" for Windows XP, Windows 7, and Windows 10.
        /// </summary>
        /// <returns></returns>
        private static string GetHumanReadableOSVersion()
        {
            string strReturn = string.Empty;
            try
            {
                //Get Operating system information.
                OperatingSystem objOSInfo = Environment.OSVersion;
                //Get version information about the os.
                Version objOSInfoVersion = objOSInfo.Version;

                switch (objOSInfo.Platform)
                {
                    case PlatformID.Win32Windows:
                        //This is a pre-NT version of Windows
                        switch (objOSInfoVersion.Minor)
                        {
                            case 0:
                                strReturn = "Windows 95";
                                break;

                            case 10:
                                strReturn = objOSInfoVersion.Revision.ToString() == "2222A" ? "Windows 98SE" : "Windows 98";
                                break;

                            case 90:
                                strReturn = "Windows ME";
                                break;
                        }

                        break;

                    case PlatformID.Win32NT:
                        switch (objOSInfoVersion.Major)
                        {
                            case 3:
                                strReturn = "Windows NT 3.51";
                                break;

                            case 4:
                                strReturn = "Windows NT 4.0";
                                break;

                            case 5:
                                strReturn = objOSInfoVersion.Minor == 0 ? "Windows 2000" : "Windows XP";
                                break;

                            case 6:
                                switch (objOSInfoVersion.Minor)
                                {
                                    case 0:
                                        strReturn = "Windows Vista";
                                        break;

                                    case 1:
                                        strReturn = "Windows 7";
                                        break;

                                    case 2:
                                        strReturn = "Windows 8";
                                        break;

                                    default:
                                        strReturn = "Windows 8.1";
                                        break;
                                }
                                break;

                            case 10:
                                strReturn = "Windows 10";
                                break;

                            case 11:
                                strReturn = "Windows 11";
                                break;
                        }

                        break;
                    case PlatformID.Win32S:
                        strReturn = "Legacy Windows 16-bit Compatibility Layer";
                        break;
                    case PlatformID.WinCE:
                        strReturn = "Windows Embedded Compact " + objOSInfoVersion.Major + ".0";
                        break;
                    case PlatformID.Unix:
                        strReturn = "Unix Kernel " + objOSInfoVersion;
                        break;
                    case PlatformID.Xbox:
                        strReturn = "Xbox 360";
                        break;
                    case PlatformID.MacOSX:
                        strReturn = "macOS with Darwin Kernel " + objOSInfoVersion;
                        break;
                    default:
                        BreakIfDebug();
                        strReturn = objOSInfo.VersionString;
                        break;
                }
                //Make sure we actually got something in our OS check
                //We don't want to just return " Service Pack 2" or " 32-bit"
                //That information is useless without the OS version.
                if (strReturn.StartsWith("Windows") && !string.IsNullOrEmpty(objOSInfo.ServicePack))
                {
                    //Append service pack to the OS name.  i.e. "Windows XP Service Pack 3"
                    strReturn += ' ' + objOSInfo.ServicePack;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                strReturn = string.Empty;
            }
            return string.IsNullOrEmpty(strReturn) ? "Unknown" : strReturn;
        }

        public static void SetupWebBrowserRegistryKeys()
        {
            int intInternetExplorerVersionKey = GlobalSettings.EmulatedBrowserVersion * 1000;
            string strChummerExeName = AppDomain.CurrentDomain.FriendlyName;
            try
            {
                using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey(
                           "Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true))
                    objRegistry?.SetValue(strChummerExeName, intInternetExplorerVersionKey, RegistryValueKind.DWord);

                using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey(
                           "Software\\WOW6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true))
                    objRegistry?.SetValue(strChummerExeName, intInternetExplorerVersionKey, RegistryValueKind.DWord);

                // These two needed to have WebBrowser control obey DPI settings for Chummer
                using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey(
                           "Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_96DPI_PIXEL", true))
                    objRegistry?.SetValue(strChummerExeName, 1, RegistryValueKind.DWord);

                using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey(
                           "Software\\WOW6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_96DPI_PIXEL", true))
                    objRegistry?.SetValue(strChummerExeName, 1, RegistryValueKind.DWord);
            }
            catch (UnauthorizedAccessException)
            {
                // Swallow this
            }
            catch (IOException)
            {
                // Swallow this
            }
            catch (SecurityException)
            {
                // Swallow this
            }
        }

        private static string s_strTempPath = string.Empty;

        /// <summary>
        /// Gets a temporary file folder that is exclusive to Chummer and therefore can be manipulated at will without worrying about interfering with anything else.
        /// Basically, like Path.GetTempPath(), but safer.
        /// </summary>
        public static string GetTempPath()
        {
            if (string.IsNullOrEmpty(s_strTempPath))
                s_strTempPath = Path.Combine(Path.GetTempPath(), "Chummer");
            if (!Directory.Exists(s_strTempPath))
                Directory.CreateDirectory(s_strTempPath);
            return s_strTempPath;
        }

        private static readonly DefaultObjectPoolProvider s_ObjObjectPoolProvider = new DefaultObjectPoolProvider();

        /// <summary>
        /// Memory Pool for empty StringBuilder objects. A bit slower up-front than a simple allocation, but reduces memory allocations, which saves on CPU used for Garbage Collection.
        /// </summary>
        [CLSCompliant(false)]
        public static ObjectPool<StringBuilder> StringBuilderPool { get; }
            = s_ObjObjectPoolProvider.CreateStringBuilderPool();

        /// <summary>
        /// Memory Pool for empty lists of ListItems. A bit slower up-front than a simple allocation, but reduces memory allocations when used a lot, which saves on CPU used for Garbage Collection.
        /// </summary>
        [CLSCompliant(false)]
        public static ObjectPool<List<ListItem>> ListItemListPool { get; }
            = s_ObjObjectPoolProvider.Create(new CollectionPooledObjectPolicy<List<ListItem>, ListItem>());

        /// <summary>
        /// Memory Pool for empty hash sets of strings. A bit slower up-front than a simple allocation, but reduces memory allocations when used a lot, which saves on CPU used for Garbage Collection.
        /// </summary>
        [CLSCompliant(false)]
        public static ObjectPool<HashSet<string>> StringHashSetPool { get; }
            = s_ObjObjectPoolProvider.Create(new CollectionPooledObjectPolicy<HashSet<string>, string>());

        /// <summary>
        /// Memory Pool for empty dictionaries used for processing multiple property changed. A bit slower up-front than a simple allocation, but reduces memory allocations when used a lot, which saves on CPU used for Garbage Collection.
        /// </summary>
        [CLSCompliant(false)]
        public static ObjectPool<Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>>
            DictionaryForMultiplePropertyChangedPool { get; }
            = s_ObjObjectPoolProvider.Create(
                new CollectionPooledObjectPolicy<Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>,
                    KeyValuePair<INotifyMultiplePropertyChanged, HashSet<string>>>());
    }
}
