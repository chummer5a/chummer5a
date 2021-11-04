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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BreakIfDebug()
        {
#if DEBUG
            if (Debugger.IsAttached && !IsUnitTest)
                Debugger.Break();
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
#endif
        }

        // Need this as a Lazy, otherwise it won't fire properly in the designer if we just cache it, and the check itself is also quite expensive
        private static readonly Lazy<bool> s_BlnIsRunningInVisualStudio =
            new Lazy<bool>(() => Process.GetCurrentProcess().ProcessName == "devenv");

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
            if (string.IsNullOrEmpty(strPath))
                return true;
            int intWaitInterval = Math.Max(intTimeout / DefaultSleepDuration, DefaultSleepDuration);
            while (File.Exists(strPath))
            {
                try
                {
                    File.Delete(strPath);
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
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning"));
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
                    SafeSleep(intWaitInterval);
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
        /// Restarts Chummer5a.
        /// </summary>
        /// <param name="strLanguage">Language in which to display any prompts or warnings. If empty, use Chummer's current language.</param>
        /// <param name="strText">Text to display in the prompt to restart. If empty, no prompt is displayed.</param>
        public static void RestartApplication(string strLanguage = "", string strText = "")
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
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
                    DialogResult objResult = Program.MainForm.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_UnsavedChanges", strLanguage), strCharacterName), LanguageManager.GetString("MessageTitle_UnsavedChanges", strLanguage), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (objResult)
                    {
                        case DialogResult.Yes:
                            {
                                // Attempt to save the Character. If the user cancels the Save As dialogue that may open, cancel the closing event so that changes are not lost.
                                bool blnResult = objOpenCharacterForm.SaveCharacter();
                                if (!blnResult)
                                    return;
                                // We saved a character as created, which closed the current form and added a new one
                                // This works regardless of dispose, because dispose would just set the objOpenCharacterForm pointer to null, so OpenCharacterForms would never contain it
                                if (!Program.MainForm.OpenCharacterForms.Contains(objOpenCharacterForm))
                                    i -= 1;
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
            ProcessStartInfo objStartInfo = new ProcessStartInfo
            {
                FileName = GetStartupPath + Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName,
                Arguments = sbdArguments.ToString()
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
            var tcs = new TaskCompletionSource<bool>();
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
        /// <param name="func"></param>
        /// <returns></returns>
        public static Task StartStaTask(Task func)
        {
            var tcs = new TaskCompletionSource<bool>();
            Thread thread = new Thread(RunFunction);
            async void RunFunction()
            {
                try
                {
                    tcs.SetResult(await DummyFunction());
                    // This is needed because SetResult always needs a return type
                    async Task<bool> DummyFunction()
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
            var tcs = new TaskCompletionSource<T>();
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
                    strReturn += " " + objOSInfo.ServicePack;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                strReturn = string.Empty;
            }
            return string.IsNullOrEmpty(strReturn) ? "Unknown" : strReturn;
        }
    }
}
