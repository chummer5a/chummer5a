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
 using System.Linq;
 using System.Reflection;
 using System.Runtime.CompilerServices;
 using System.Security.AccessControl;
 using System.Security.Principal;
 using System.Text;
 using System.Threading;
 using System.Threading.Tasks;
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

        // Need this as a Lazy, otherwise it won't fire properly in the designer if we just cache it, and the check itself is also quite expensive
        private static readonly Lazy<bool> s_blnIsRunningInVisualStudio =
            new Lazy<bool>(() => Process.GetCurrentProcess().ProcessName == "devenv");

        /// <summary>
        /// Returns if we are running inside Visual Studio, e.g. if we are in the designer.
        /// </summary>
        public static bool IsRunningInVisualStudio => s_blnIsRunningInVisualStudio.Value;

        /// <summary>
        /// Returns if we are in VS's Designer.
        /// WARNING! Will not work with WPF! Use in combination with Utils.IsRunningInVisualStudio for WPF controls running inside of WinForms.
        /// </summary>
        public static bool IsDesignerMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>
        /// Cached latest version of Chummer from its GitHub page.
        /// </summary>
        public static Version CachedGitVersion { get; set; }

        private static bool s_blnIsUnitTest;

        /// <summary>
        /// This property is set in the Constructor of frmChummerMain (and NO where else!)
        /// </summary>
        public static bool IsUnitTest
        {
            get => s_blnIsUnitTest;
            set
            {
                if (s_blnIsUnitTest == value)
                    return;
                s_blnIsUnitTest = value;
                s_blnIsOKToRunDoEvents = DefaultIsOKToRunDoEvents;
            }
        }

        /// <summary>
        /// Returns the actual path of the Chummer-Directory regardless of running as Unit test or not.
        /// </summary>

        public static string GetStartupPath => !IsUnitTest ? Application.StartupPath : AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        public static int GitUpdateAvailable => CachedGitVersion?.CompareTo(Assembly.GetExecutingAssembly().GetName().Version) ?? 0;

        public const int DefaultSleepDuration = 20;

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

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Task<T> StartSTATask<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
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
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Task<T> StartSTATask<T>(Task<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            Thread thread = new Thread(async () =>
            {
                try
                {
                    tcs.SetResult(await func);
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
        /// Never wait around in designer mode, we should not care about thread locking, and running in a background thread can mess up IsDesignerMode checks inside that thread
        /// </summary>
        private static bool EverDoEvents => !IsDesignerMode && !IsRunningInVisualStudio;

        /// <summary>
        /// Don't run events during unit tests, but still run in the background so that we can catch any issues caused by our setup.
        /// </summary>
        private static bool DefaultIsOKToRunDoEvents => !IsUnitTest && EverDoEvents;

        /// <summary>
        /// This member makes sure we aren't swamping the program with massive amounts of Application.DoEvents() calls
        /// </summary>
        private static bool s_blnIsOKToRunDoEvents = DefaultIsOKToRunDoEvents;

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
            {
                bool blnDoEvents = s_blnIsOKToRunDoEvents;
                try
                {
                    if (blnDoEvents)
                    {
                        s_blnIsOKToRunDoEvents = false;
                        Application.DoEvents();
                    }
                    Thread.Sleep(DefaultSleepDuration);
                }
                finally
                {
                    if (blnDoEvents)
                        s_blnIsOKToRunDoEvents = DefaultIsOKToRunDoEvents;
                }
            }
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
                foreach (Action funcToRun in afuncToRun)
                    funcToRun.Invoke();
                return;
            }
            Task[] aobjTasks = new Task[afuncToRun.Length];
            for (int i = 0; i < afuncToRun.Length; ++i)
                aobjTasks[i] = Task.Run(afuncToRun[i]);
            while (aobjTasks.Any(objTask => !objTask.IsCompleted))
            {
                bool blnDoEvents = s_blnIsOKToRunDoEvents;
                try
                {
                    if (blnDoEvents)
                    {
                        s_blnIsOKToRunDoEvents = false;
                        Application.DoEvents();
                    }
                    Thread.Sleep(DefaultSleepDuration);
                }
                finally
                {
                    if (blnDoEvents)
                        s_blnIsOKToRunDoEvents = DefaultIsOKToRunDoEvents;
                }
            }
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
            {
                bool blnDoEvents = s_blnIsOKToRunDoEvents;
                try
                {
                    if (blnDoEvents)
                    {
                        s_blnIsOKToRunDoEvents = false;
                        Application.DoEvents();
                    }
                    Thread.Sleep(DefaultSleepDuration);
                }
                finally
                {
                    if (blnDoEvents)
                        s_blnIsOKToRunDoEvents = DefaultIsOKToRunDoEvents;
                }
            }
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
                for (int i = 0; i < afuncToRun.Length; ++i)
                    aobjReturn[i] = afuncToRun[i].Invoke();
                return aobjReturn;
            }
            Task<T>[] aobjTasks = new Task<T>[afuncToRun.Length];
            for (int i = 0; i < afuncToRun.Length; ++i)
                aobjTasks[i] = Task.Run(afuncToRun[i]);
            while (aobjTasks.Any(objTask => !objTask.IsCompleted))
            {
                bool blnDoEvents = s_blnIsOKToRunDoEvents;
                try
                {
                    if (blnDoEvents)
                    {
                        s_blnIsOKToRunDoEvents = false;
                        Application.DoEvents();
                    }
                    Thread.Sleep(DefaultSleepDuration);
                }
                finally
                {
                    if (blnDoEvents)
                        s_blnIsOKToRunDoEvents = DefaultIsOKToRunDoEvents;
                }
            }
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
                objSyncTask.RunSynchronously();
                return objSyncTask.Result;
            }
            Task<T> objTask = Task.Run(funcToRun);
            while (!objTask.IsCompleted)
            {
                bool blnDoEvents = s_blnIsOKToRunDoEvents;
                try
                {
                    if (blnDoEvents)
                    {
                        s_blnIsOKToRunDoEvents = false;
                        Application.DoEvents();
                    }
                    Thread.Sleep(DefaultSleepDuration);
                }
                finally
                {
                    if (blnDoEvents)
                        s_blnIsOKToRunDoEvents = DefaultIsOKToRunDoEvents;
                }
            }
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
                for (int i = 0; i < afuncToRun.Length; ++i)
                {
                    Task<T> objSyncTask = afuncToRun[i].Invoke();
                    objSyncTask.RunSynchronously();
                    aobjReturn[i] = objSyncTask.Result;
                }
                return aobjReturn;
            }
            Task<T>[] aobjTasks = new Task<T>[afuncToRun.Length];
            for (int i = 0; i < afuncToRun.Length; ++i)
                aobjTasks[i] = Task.Run(afuncToRun[i]);
            while (aobjTasks.Any(objTask => !objTask.IsCompleted))
            {
                bool blnDoEvents = s_blnIsOKToRunDoEvents;
                try
                {
                    if (blnDoEvents)
                    {
                        s_blnIsOKToRunDoEvents = false;
                        Application.DoEvents();
                    }
                    Thread.Sleep(DefaultSleepDuration);
                }
                finally
                {
                    if (blnDoEvents)
                        s_blnIsOKToRunDoEvents = DefaultIsOKToRunDoEvents;
                }
            }
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
                objSyncTask.RunSynchronously();
                return;
            }
            Task objTask = Task.Run(funcToRun);
            while (!objTask.IsCompleted)
            {
                bool blnDoEvents = s_blnIsOKToRunDoEvents;
                try
                {
                    if (blnDoEvents)
                    {
                        s_blnIsOKToRunDoEvents = false;
                        Application.DoEvents();
                    }
                    Thread.Sleep(DefaultSleepDuration);
                }
                finally
                {
                    if (blnDoEvents)
                        s_blnIsOKToRunDoEvents = DefaultIsOKToRunDoEvents;
                }
            }
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
                foreach (Func<Task> funcToRun in afuncToRun)
                {
                    Task objSyncTask = funcToRun.Invoke();
                    objSyncTask.RunSynchronously();
                }
                return;
            }
            Task[] aobjTasks = new Task[afuncToRun.Length];
            for (int i = 0; i < afuncToRun.Length; ++i)
                aobjTasks[i] = Task.Run(afuncToRun[i]);
            while (aobjTasks.Any(objTask => !objTask.IsCompleted))
            {
                bool blnDoEvents = s_blnIsOKToRunDoEvents;
                try
                {
                    if (blnDoEvents)
                    {
                        s_blnIsOKToRunDoEvents = false;
                        Application.DoEvents();
                    }
                    Thread.Sleep(DefaultSleepDuration);
                }
                finally
                {
                    if (blnDoEvents)
                        s_blnIsOKToRunDoEvents = DefaultIsOKToRunDoEvents;
                }
            }
        }
    }
}
