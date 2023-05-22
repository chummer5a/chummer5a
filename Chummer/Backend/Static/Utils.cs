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
using System.Drawing;
using System.IO;
using System.Linq;
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
using System.Xml;
using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using NLog;
using Microsoft.IO;
using Chummer.Forms;

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
            if (Debugger.IsAttached)
                Debugger.Break();
#else
            // Method intentionally left empty.
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BreakOnErrorIfDebug()
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                int intErrorCode = Marshal.GetLastWin32Error();
                if (intErrorCode != 0)
                    Debugger.Break();
            }
#else
            // Method intentionally left empty.
#endif
        }

        private static SynchronizationContext MySynchronizationContext { get; set; }

        private static JoinableTaskContext MyJoinableTaskContext { get; set; }

        public static JoinableTaskContext CreateSynchronizationContext()
        {
            if (Program.IsMainThread)
            {
                using (new DummyForm()) // New Form needs to be created (or Application.Run() called) before Synchronization.Current is set
                {
                    MySynchronizationContext = SynchronizationContext.Current;
                    return MyJoinableTaskContext
                        = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
                }
            }

            return default;
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
                bool blnOldIsOkToRunDoEvents = DefaultIsOkToRunDoEvents;
                _blnIsUnitTest = value;
                if (!value)
                    IsUnitTestForUI = false;
                bool blnNewIsOkToRunDoEvents = DefaultIsOkToRunDoEvents;
                if (blnOldIsOkToRunDoEvents == blnNewIsOkToRunDoEvents)
                    return;
                if (blnNewIsOkToRunDoEvents)
                    Interlocked.Increment(ref _intIsOkToRunDoEvents);
                else
                    Interlocked.Decrement(ref _intIsOkToRunDoEvents);
            }
        }

        private static bool _blnIsUnitTestForUI;

        /// <summary>
        /// This property is set in the Constructor of frmChummerMain (and NO where else!)
        /// </summary>
        public static bool IsUnitTestForUI
        {
            get => _blnIsUnitTestForUI;
            set
            {
                if (_blnIsUnitTestForUI == value)
                    return;
                bool blnOldIsOkToRunDoEvents = DefaultIsOkToRunDoEvents;
                _blnIsUnitTestForUI = value;
                if (value)
                    _blnIsUnitTest = true;
                bool blnNewIsOkToRunDoEvents = DefaultIsOkToRunDoEvents;
                if (blnOldIsOkToRunDoEvents == blnNewIsOkToRunDoEvents)
                    return;
                if (blnNewIsOkToRunDoEvents)
                    Interlocked.Increment(ref _intIsOkToRunDoEvents);
                else
                    Interlocked.Decrement(ref _intIsOkToRunDoEvents);
            }
        }

        private static readonly LockingDictionary<Icon, Bitmap> s_dicCachedIconBitmaps = new LockingDictionary<Icon, Bitmap>(10);

        /// <summary>
        /// Dictionary assigning icons to singly-initialized instances of their bitmaps.
        /// Mainly intended for SystemIcons.
        /// </summary>
        public static Bitmap GetCachedIconBitmap(Icon objIcon)
        {
            return s_dicCachedIconBitmaps.AddOrGet(objIcon, x => x.ToBitmap());
        }

        /// <summary>
        /// Dictionary assigning icons to singly-initialized instances of their bitmaps.
        /// Mainly intended for SystemIcons.
        /// </summary>
        public static Task<Bitmap> GetCachedIconBitmapAsync(Icon objIcon, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return s_dicCachedIconBitmaps.AddOrGetAsync(objIcon, x => x.ToBitmap(), token).AsTask();
        }

        private static readonly LockingDictionary<Icon, Bitmap> s_dicStockIconBitmapsForSystemIcons = new LockingDictionary<Icon, Bitmap>(10);

        /// <summary>
        /// Dictionary assigning Windows stock icons' bitmaps to SystemIcons equivalents.
        /// Needed where the graphics used in dialog windows in newer versions of windows are different from those in SystemIcons.
        /// </summary>
        public static Bitmap GetStockIconBitmapsForSystemIcon(Icon objIcon)
        {
            return s_dicStockIconBitmapsForSystemIcons.AddOrGet(objIcon, x =>
            {
                if (x == SystemIcons.Application)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_APPLICATION).ToBitmap();
                }

                if (x == SystemIcons.Asterisk || x == SystemIcons.Information)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_INFO).ToBitmap();
                }

                if (x == SystemIcons.Error || x == SystemIcons.Hand)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_ERROR).ToBitmap();
                }

                if (x == SystemIcons.Exclamation || x == SystemIcons.Warning)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_WARNING).ToBitmap();
                }

                if (x == SystemIcons.Question)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_HELP).ToBitmap();
                }

                if (x == SystemIcons.Shield)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_SHIELD).ToBitmap();
                }

                if (x == SystemIcons.WinLogo)
                {
                    return SystemIcons.WinLogo.ToBitmap();
                }

                throw new ArgumentOutOfRangeException(nameof(objIcon));
            });
        }

        /// <summary>
        /// Dictionary assigning Windows stock icons' bitmaps to SystemIcons equivalents.
        /// Needed where the graphics used in dialog windows in newer versions of windows are different from those in SystemIcons.
        /// </summary>
        public static Task<Bitmap> GetStockIconBitmapsForSystemIconAsync(Icon objIcon, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return s_dicStockIconBitmapsForSystemIcons.AddOrGetAsync(objIcon, x =>
            {
                if (x == SystemIcons.Application)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_APPLICATION).ToBitmap();
                }

                if (x == SystemIcons.Asterisk || x == SystemIcons.Information)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_INFO).ToBitmap();
                }

                if (x == SystemIcons.Error || x == SystemIcons.Hand)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_ERROR).ToBitmap();
                }

                if (x == SystemIcons.Exclamation || x == SystemIcons.Warning)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_WARNING).ToBitmap();
                }

                if (x == SystemIcons.Question)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_HELP).ToBitmap();
                }

                if (x == SystemIcons.Shield)
                {
                    return NativeMethods.GetStockIcon(NativeMethods.SHSTOCKICONID.SIID_SHIELD).ToBitmap();
                }

                if (x == SystemIcons.WinLogo)
                {
                    return SystemIcons.WinLogo.ToBitmap();
                }

                throw new ArgumentOutOfRangeException(nameof(objIcon));
            }, token).AsTask();
        }

        /// <summary>
        /// Maximum amount of tasks to run in parallel, useful to use with batching to avoid overloading the task scheduler.
        /// </summary>
        public static int MaxParallelBatchSize { get; } = Environment.ProcessorCount * 2;

        public static int DefaultPoolSize { get; } = Math.Max(MaxParallelBatchSize, 32);

        private static readonly Lazy<string> s_strGetStartupPath = new Lazy<string>(
            () => IsUnitTest ? AppDomain.CurrentDomain.SetupInformation.ApplicationBase : Application.StartupPath);

        private static readonly Lazy<string> s_strGetAutosavesFolderPath
            = new Lazy<string>(() => Path.Combine(GetStartupPath, "saves", "autosave"));

        private static readonly Lazy<string> s_strGetDataFolderPath
            = new Lazy<string>(() => Path.Combine(GetStartupPath, "data"));

        private static readonly Lazy<string> s_strGetLiveCustomDataFolderPath
            = new Lazy<string>(() => Path.Combine(GetStartupPath, "livecustomdata"));

        private static readonly Lazy<string> s_strGetPacksFolderPath
            = new Lazy<string>(() => Path.Combine(GetStartupPath, "packs"));

        private static readonly Lazy<string> s_strGetLanguageFolderPath
            = new Lazy<string>(() => Path.Combine(GetStartupPath, "lang"));

        private static readonly Lazy<string> s_strGetSettingsFolderPath
            = new Lazy<string>(() => Path.Combine(GetStartupPath, "settings"));

        /// <summary>
        /// Returns the actual path of the Chummer-Directory regardless of running as Unit test or not.
        /// </summary>
        public static string GetStartupPath => s_strGetStartupPath.Value;

        public static string GetAutosavesFolderPath => s_strGetAutosavesFolderPath.Value;

        public static string GetLiveCustomDataFolderPath => s_strGetLiveCustomDataFolderPath.Value;

        public static string GetDataFolderPath => s_strGetDataFolderPath.Value;

        public static string GetPacksFolderPath => s_strGetPacksFolderPath.Value;

        public static string GetLanguageFolderPath => s_strGetLanguageFolderPath.Value;

        public static string GetSettingsFolderPath => s_strGetSettingsFolderPath.Value;

        private static readonly Lazy<JoinableTaskFactory> s_objJoinableTaskFactory
            = new Lazy<JoinableTaskFactory>(() => IsRunningInVisualStudio
                                                ? new JoinableTaskFactory(new JoinableTaskContext())
                                                : new JoinableTaskFactory(
                                                    MyJoinableTaskContext ?? CreateSynchronizationContext()));

        public static JoinableTaskFactory JoinableTaskFactory => s_objJoinableTaskFactory.Value;

        private static readonly Lazy<string[]> s_astrBasicDataFileNames = new Lazy<string[]>(() =>
        {
            List<string> lstFiles = new List<string>(byte.MaxValue);
            foreach (string strFile in Directory.EnumerateFiles(GetDataFolderPath, "*.xml").Select(Path.GetFileName))
            {
                if (string.IsNullOrEmpty(strFile)
                    || strFile.StartsWith("amend_", StringComparison.OrdinalIgnoreCase)
                    || strFile.StartsWith("custom_", StringComparison.OrdinalIgnoreCase)
                    || strFile.StartsWith("override_", StringComparison.OrdinalIgnoreCase))
                    continue;
                lstFiles.Add(strFile);
            }
            return lstFiles.ToArray();
        });

        public static ReadOnlyCollection<string> BasicDataFileNames => Array.AsReadOnly(s_astrBasicDataFileNames.Value);

        /// <summary>
        /// Attempts to find any custom data files located in the base data directory (where they would be ignored) and move them to an appropriate directory.
        /// PACKS will go into packs, character settings will go into settings, everything else will go into livecustomdata.
        /// </summary>
        /// <param name="blnShowErrors">If true, an error message will be displayed every time the method runs into an exception.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static void MoveMisplacedCustomDataFiles(bool blnShowErrors = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<string> lstToMove = new List<string>();
            foreach (string strFilePath in Directory.EnumerateFiles(GetDataFolderPath, "*.xml"))
            {
                token.ThrowIfCancellationRequested();
                string strFile = Path.GetFileName(strFilePath);
                if (strFile.StartsWith("amend_", StringComparison.OrdinalIgnoreCase)
                    || strFile.StartsWith("custom_", StringComparison.OrdinalIgnoreCase)
                    || strFile.StartsWith("override_", StringComparison.OrdinalIgnoreCase))
                {
                    lstToMove.Add(strFilePath);
                }
            }

            foreach (string strFilePath in lstToMove)
            {
                token.ThrowIfCancellationRequested();
                string strFile = Path.GetFileName(strFilePath);
                string strDestinationFolder;
                if (strFile.EndsWith("_packs.xml", StringComparison.OrdinalIgnoreCase))
                    strDestinationFolder = GetPacksFolderPath;
                else if (strFile.EndsWith("_settings.xml", StringComparison.OrdinalIgnoreCase))
                    strDestinationFolder = GetSettingsFolderPath;
                else
                    strDestinationFolder = GetLiveCustomDataFolderPath;

                if (!Directory.Exists(strDestinationFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(strDestinationFolder);
                    }
                    catch (Exception ex)
                    {
                        if (blnShowErrors)
                            Program.ShowScrollableMessageBox(ex.ToString(), icon: MessageBoxIcon.Error);
                        continue;
                    }
                }
                token.ThrowIfCancellationRequested();
                string strDestinationPath = Path.Combine(strDestinationFolder, "*.xml");
                if (File.Exists(strDestinationPath))
                {
                    if (blnShowErrors)
                        Program.ShowScrollableMessageBox(
                            string.Format(GlobalSettings.CultureInfo,
                                          LanguageManager.GetString("Message_DuplicateFile", token: token), strFile,
                                          strDestinationFolder),
                            LanguageManager.GetString("MessageTitle_DuplicateFile", token: token),
                            icon: MessageBoxIcon.Error);
                    continue;
                }
                token.ThrowIfCancellationRequested();
                try
                {
                    Directory.Move(strFilePath, strDestinationPath);
                }
                catch (Exception ex)
                {
                    if (blnShowErrors)
                        Program.ShowScrollableMessageBox(ex.ToString(), icon: MessageBoxIcon.Error);
                }
            }
        }

        private static readonly Lazy<Version> s_ObjCurrentChummerVersion = new Lazy<Version>(() => typeof(Program).Assembly.GetName().Version);

        public static Version CurrentChummerVersion => s_ObjCurrentChummerVersion.Value;

        public static bool IsMilestoneVersion => CurrentChummerVersion.Build == 0;

        public static int GitUpdateAvailable => CachedGitVersion?.CompareTo(CurrentChummerVersion) ?? 0;

        public const int DefaultSleepDuration = 15;

        public const int SleepEmergencyReleaseMaxTicks = 60000 / DefaultSleepDuration; // 1 minute in ticks

        public const int WaitEmergencyReleaseMaxTicks = 1800000 / DefaultSleepDuration; // 30 minutes in ticks

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
            catch
            {
                BreakIfDebug();
                return true;
            }
        }

        /// <summary>
        /// Wait for an open directory to be available for deletion and then delete it.
        /// </summary>
        /// <param name="strPath">Directory path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if the directory cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static bool SafeDeleteDirectory(string strPath, bool blnShowUnauthorizedAccess = false, int intTimeout = DefaultSleepDuration * 60, CancellationToken token = default)
        {
            return SafelyRunSynchronously(() => SafeDeleteDirectoryCoreAsync(true, strPath, blnShowUnauthorizedAccess, intTimeout, token), token);
        }

        /// <summary>
        /// Wait for an open directory to be available for deletion and then delete it.
        /// </summary>
        /// <param name="strPath">Directory path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if the directory cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static Task<bool> SafeDeleteDirectoryAsync(string strPath, bool blnShowUnauthorizedAccess = false, int intTimeout = DefaultSleepDuration * 60, CancellationToken token = default)
        {
            return SafeDeleteDirectoryCoreAsync(false, strPath, blnShowUnauthorizedAccess, intTimeout, token);
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
        /// <param name="token">Cancellation token to use</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        private static async Task<bool> SafeDeleteDirectoryCoreAsync(bool blnSync, string strPath, bool blnShowUnauthorizedAccess, int intTimeout, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath))
                return true;
            if (!Directory.Exists(strPath))
                return true;
            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                if (!SafeClearDirectory(strPath, blnShowUnauthorizedAccess: blnShowUnauthorizedAccess,
                                        intTimeout: intTimeout, token: token))
                    return false;
            }
            else if (!await SafeClearDirectoryAsync(strPath, blnShowUnauthorizedAccess: blnShowUnauthorizedAccess,
                                                    intTimeout: intTimeout, token: token).ConfigureAwait(false))
                return false;
            int intWaitInterval = Math.Max(intTimeout / DefaultSleepDuration, DefaultSleepDuration);
            while (Directory.Exists(strPath))
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    if (!strPath.StartsWith(GetStartupPath, StringComparison.OrdinalIgnoreCase) && !strPath.StartsWith(GetTempPath(), StringComparison.OrdinalIgnoreCase))
                    {
                        token.ThrowIfCancellationRequested();
                        // For safety purposes, do not allow unprompted deleting of any files outside of the Chummer folder itself
                        if (blnShowUnauthorizedAccess)
                        {
                            if (Program.ShowScrollableMessageBox(
                                    string.Format(GlobalSettings.CultureInfo,
                                        blnSync
                                            // ReSharper disable once MethodHasAsyncOverload
                                            ? LanguageManager.GetString("Message_Prompt_Delete_Existing_File", token: token)
                                            : await LanguageManager.GetStringAsync(
                                                "Message_Prompt_Delete_Existing_File", token: token).ConfigureAwait(false), strPath),
                                    buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Warning) != DialogResult.Yes)
                                return false;
                        }
                        else
                        {
                            BreakIfDebug();
                            return false;
                        }
                    }
                    token.ThrowIfCancellationRequested();
                    // Need these two to handle disposal of file handles
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    token.ThrowIfCancellationRequested();
                    if (blnSync)
                        Directory.Delete(strPath, true);
                    else
                        await Task.Run(() => Directory.Delete(strPath, true), token).ConfigureAwait(false);
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
                        Program.ShowScrollableMessageBox(blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? LanguageManager.GetString("Message_Insufficient_Permissions_Warning", token: token)
                            : await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning", token: token).ConfigureAwait(false));
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
                        SafeSleep(intWaitInterval, token);
                    else
                        await SafeSleepAsync(intWaitInterval, token).ConfigureAwait(false);
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
        /// <param name="token">Cancellation token to use</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static bool SafeClearDirectory(string strPath, string strSearchPattern = "*", bool blnRecursive = true, bool blnShowUnauthorizedAccess = false, int intTimeout = DefaultSleepDuration * 60, CancellationToken token = default)
        {
            return SafelyRunSynchronously(() => SafeClearDirectoryCoreAsync(
                                              true, strPath, strSearchPattern, blnRecursive, blnShowUnauthorizedAccess,
                                              intTimeout, token), token);
        }

        /// <summary>
        /// Safely deletes all files in a directory (though the directory itself remains).
        /// </summary>
        /// <param name="strPath">Directory path to clear.</param>
        /// <param name="strSearchPattern">Search pattern to use for finding files to delete. Use "*" if you wish to clear all files.</param>
        /// <param name="blnRecursive">Whether to a delete all subdirectories, too.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether or not to show a message if a file cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static Task<bool> SafeClearDirectoryAsync(string strPath, string strSearchPattern = "*", bool blnRecursive = true, bool blnShowUnauthorizedAccess = false, int intTimeout = DefaultSleepDuration * 60, CancellationToken token = default)
        {
            return SafeClearDirectoryCoreAsync(false, strPath, strSearchPattern, blnRecursive, blnShowUnauthorizedAccess,
                intTimeout, token);
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
        /// <param name="token">Cancellation token to use</param>
        /// <returns>True if directory does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        private static async Task<bool> SafeClearDirectoryCoreAsync(bool blnSync, string strPath, string strSearchPattern, bool blnRecursive, bool blnShowUnauthorizedAccess, int intTimeout, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath) || !Directory.Exists(strPath))
                return true;
            if (!strPath.StartsWith(GetStartupPath, StringComparison.OrdinalIgnoreCase)
                && !strPath.StartsWith(GetTempPath(), StringComparison.OrdinalIgnoreCase))
            {
                // For safety purposes, do not allow unprompted deleting of any files outside of the Chummer folder itself
                if (blnShowUnauthorizedAccess)
                {
                    if (Program.ShowScrollableMessageBox(
                            string.Format(GlobalSettings.Language,
                                blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? LanguageManager.GetString("Message_Prompt_Delete_Existing_File", token: token)
                                    : await LanguageManager.GetStringAsync("Message_Prompt_Delete_Existing_File", token: token).ConfigureAwait(false),
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
            token.ThrowIfCancellationRequested();
            string[] astrFilesToDelete = Directory.GetFiles(strPath, strSearchPattern,
                                                            blnRecursive
                                                                ? SearchOption.AllDirectories
                                                                : SearchOption.TopDirectoryOnly);
            token.ThrowIfCancellationRequested();
            if (blnSync)
            {
                int intReturn = 1;
                RunWithoutThreadLock(() =>
                {
                    Parallel.ForEach(astrFilesToDelete, () => true,
                                     (strToDelete, x, y) => FileExtensions.SafeDelete(strToDelete, false, intTimeout, token),
                                     blnLoop =>
                                     {
                                         if (!blnLoop)
                                             Interlocked.Exchange(ref intReturn, 0);
                                     });
                }, token);
                return intReturn > 0;
            }

            Task<bool>[] atskSuccesses = new Task<bool>[astrFilesToDelete.Length];
            for (int i = 0; i < astrFilesToDelete.Length; i++)
            {
                string strToDelete = astrFilesToDelete[i];
                atskSuccesses[i] = FileExtensions.SafeDeleteAsync(strToDelete, false, intTimeout, token);
            }
            foreach (Task<bool> x in atskSuccesses)
            {
                if (!await x.ConfigureAwait(false))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Restarts Chummer5a.
        /// </summary>
        /// <param name="strLanguage">Language in which to display any prompts or warnings. If empty, use Chummer's current language.</param>
        /// <param name="strText">Text to display in the prompt to restart. If empty, no prompt is displayed.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async ValueTask RestartApplication(string strLanguage = "", string strText = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            if (!string.IsNullOrEmpty(strText))
            {
                string text = await LanguageManager.GetStringAsync(strText, strLanguage, token: token).ConfigureAwait(false);
                string caption
                    = await LanguageManager.GetStringAsync("MessageTitle_Options_CloseForms", strLanguage, token: token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (Program.ShowScrollableMessageBox(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    != DialogResult.Yes)
                    return;
            }

            // Need to do this here in case file names are changed while closing forms (because a character who previously did not have a filename was saved when prompted)
            // Cannot use foreach because saving a character as created removes the current form and adds a new one
            ThreadSafeObservableCollection<CharacterShared> lstToProcess = Program.MainForm.OpenCharacterEditorForms;
            if (lstToProcess != null)
            {
                for (int i = await lstToProcess.GetCountAsync(token).ConfigureAwait(false); i >= 0; --i)
                {
                    token.ThrowIfCancellationRequested();
                    CharacterShared objOpenCharacterForm;
                    try
                    {
                        objOpenCharacterForm = await lstToProcess.GetValueAtAsync(i, token).ConfigureAwait(false);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // Swallow this, we've closed the form in between the loop and us trying to get the form at the index
                        continue;
                    }

                    if (objOpenCharacterForm?.IsDirty == true)
                    {
                        string strCharacterName = await objOpenCharacterForm.CharacterObject
                                                                            .GetCharacterNameAsync(token)
                                                                            .ConfigureAwait(false);
                        if (Program.ShowScrollableMessageBox(
                                string.Format(GlobalSettings.CultureInfo,
                                              await LanguageManager.GetStringAsync(
                                                                       "Message_UnsavedChanges", strLanguage,
                                                                       token: token)
                                                                   .ConfigureAwait(false), strCharacterName),
                                await LanguageManager
                                      .GetStringAsync("MessageTitle_UnsavedChanges", strLanguage, token: token)
                                      .ConfigureAwait(false),
                                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        try
                        {
                            // Attempt to save the Character. If the user cancels the Save As dialogue that may open, cancel the closing event so that changes are not lost.
                            bool blnResult = await objOpenCharacterForm.SaveCharacter(token: token)
                                                                       .ConfigureAwait(false);
                            if (!blnResult)
                                return;
                        }
                        catch (OperationCanceledException)
                        {
                            if (token.IsCancellationRequested)
                                throw; // Do not throw if cancellation is from the form's internal generic token (because the form is already closing)
                        }
                    }
                }
            }

            Log.Info("Restart Chummer");
            string strFileName;
            string strArguments = string.Empty;
            Application.UseWaitCursor = true;
            try
            {
                // Get the parameters/arguments passed to program if any
                if (lstToProcess != null)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(StringBuilderPool, out StringBuilder sbdArguments))
                    {
                        foreach (CharacterShared objOpenCharacterForm in lstToProcess)
                        {
                            string strLoopFileName = objOpenCharacterForm.CharacterObject?.FileName ?? string.Empty;
                            if (!string.IsNullOrEmpty(strLoopFileName))
                                sbdArguments.Append('\"').Append(strLoopFileName).Append("\" ");
                        }

                        if (sbdArguments.Length > 0)
                            --sbdArguments.Length;

                        strArguments = sbdArguments.ToString();
                    }
                }

                strFileName = GetStartupPath + Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName;
                // Restart current application, with same arguments/parameters
                foreach (Form objForm in Program.MainForm.MdiChildren)
                {
                    await objForm.DoThreadSafeAsync(x => x.Close(), token: token).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                Application.UseWaitCursor = false;
                throw;
            }
            // Sending restart command asynchronously to MySynchronizationContext so that tasks can properly clean up before restart
#pragma warning disable VSTHRD001
            MySynchronizationContext.Post(x =>
            {
                (string strMyFileName, string strMyArguments) = (Tuple<string, string>) x;
                ProcessStartInfo objStartInfo = new ProcessStartInfo
                {
                    FileName = strMyFileName,
                    Arguments = strMyArguments
                };
                try
                {
                    Application.Exit();
                }
                finally
                {
                    objStartInfo.Start();
                }
            }, new Tuple<string, string>(strFileName, strArguments));
#pragma warning restore VSTHRD001
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        [Obsolete("Avoid if possible and use RunOnMainThread instead, which not only guarantees STA mode, but also guarantees that the thread will never be destroyed over the lifetime of the program.")]
        public static Task StartStaTask(Action func)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Thread thread = new Thread(() =>
            {
                try
                {
                    func.Invoke();
                    // This is needed because SetResult always needs a return type
                    tcs.TrySetResult(true);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        [Obsolete("Avoid if possible and use RunOnMainThread instead, which not only guarantees STA mode, but also guarantees that the thread will never be destroyed over the lifetime of the program.")]
        public static Task<T> StartStaTask<T>(Func<T> func)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            Thread thread = new Thread(() =>
            {
                try
                {
                    tcs.TrySetResult(func());
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        [Obsolete("Avoid if possible and use RunOnMainThread instead, which not only guarantees STA mode, but also guarantees that the thread will never be destroyed over the lifetime of the program.")]
        public static Task StartStaTask(Task func)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Thread thread = new Thread(RunFunction);
            async void RunFunction()
            {
                try
                {
                    await func.ConfigureAwait(false);
                    // This is needed because SetResult always needs a return type
                    tcs.TrySetResult(true);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            }
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        [Obsolete("Avoid if possible and use RunOnMainThread instead, which not only guarantees STA mode, but also guarantees that the thread will never be destroyed over the lifetime of the program.")]
        public static Task<T> StartStaTask<T>(Task<T> func)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            Thread thread = new Thread(RunFunction);
            async void RunFunction()
            {
                try
                {
                    tcs.TrySetResult(await func.ConfigureAwait(false));
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            }
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        [Obsolete("Avoid if possible and use RunOnMainThread instead, which not only guarantees STA mode, but also guarantees that the thread will never be destroyed over the lifetime of the program.")]
        public static Task StartStaTask(Action func, CancellationToken token)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            CancellationTokenRegistration objRegistration = token.Register(x => ((TaskCompletionSource<bool>)x).TrySetCanceled(token), tcs);
            Thread thread = new Thread(() =>
            {
                try
                {
                    func.Invoke();
                    // This is needed because SetResult always needs a return type
                    tcs.TrySetResult(true);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
                finally
                {
                    objRegistration.Dispose();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        [Obsolete("Avoid if possible and use RunOnMainThread instead, which not only guarantees STA mode, but also guarantees that the thread will never be destroyed over the lifetime of the program.")]
        public static Task<T> StartStaTask<T>(Func<T> func, CancellationToken token)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            CancellationTokenRegistration objRegistration = token.Register(x => ((TaskCompletionSource<bool>)x).TrySetCanceled(token), tcs);
            Thread thread = new Thread(() =>
            {
                try
                {
                    tcs.TrySetResult(func());
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
                finally
                {
                    objRegistration.Dispose();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        [Obsolete("Avoid if possible and use RunOnMainThread instead, which not only guarantees STA mode, but also guarantees that the thread will never be destroyed over the lifetime of the program.")]
        public static Task StartStaTask(Task func, CancellationToken token)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            CancellationTokenRegistration objRegistration = token.Register(x => ((TaskCompletionSource<bool>)x).TrySetCanceled(token), tcs);
            Thread thread = new Thread(RunFunction);
            async void RunFunction()
            {
                try
                {
                    await func.ConfigureAwait(false);
                    // This is needed because SetResult always needs a return type
                    tcs.TrySetResult(true);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
                finally
                {
                    objRegistration.Dispose();
                }
            }
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Start a task in a single-threaded apartment (STA) mode, which a lot of UI methods need.
        /// </summary>
        [Obsolete("Avoid if possible and use RunOnMainThread instead, which not only guarantees STA mode, but also guarantees that the thread will never be destroyed over the lifetime of the program.")]
        public static Task<T> StartStaTask<T>(Task<T> func, CancellationToken token)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            CancellationTokenRegistration objRegistration = token.Register(x => ((TaskCompletionSource<bool>)x).TrySetCanceled(token), tcs);
            Thread thread = new Thread(RunFunction);
            async void RunFunction()
            {
                try
                {
                    tcs.TrySetResult(await func.ConfigureAwait(false));
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
                finally
                {
                    objRegistration.Dispose();
                }
            }
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static void RunOnMainThread(Action func, JoinableTaskCreationOptions eOptions = JoinableTaskCreationOptions.None)
        {
            if (Program.IsMainThread)
                func.Invoke();
            else
            {
                JoinableTaskFactory.Run(async () =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync();
                    func.Invoke();
                }, eOptions);
            }
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static T RunOnMainThread<T>(Func<T> func, JoinableTaskCreationOptions eOptions = JoinableTaskCreationOptions.None)
        {
            return Program.IsMainThread
                ? func.Invoke()
                : JoinableTaskFactory.Run(async () =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync();
                    return func.Invoke();
                }, eOptions);
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static void RunOnMainThread(Action func, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (Program.IsMainThread)
                func.Invoke();
            else
            {
                JoinableTaskFactory.Run(async () =>
                {
                    token.ThrowIfCancellationRequested();
                    await JoinableTaskFactory.SwitchToMainThreadAsync();
                    func.Invoke();
                });
            }
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static T RunOnMainThread<T>(Func<T> func, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Program.IsMainThread
                ? func.Invoke()
                : JoinableTaskFactory.Run(async () =>
                {
                    token.ThrowIfCancellationRequested();
                    await JoinableTaskFactory.SwitchToMainThreadAsync();
                    return func.Invoke();
                });
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static void RunOnMainThread(Func<Task> func, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            JoinableTaskFactory.Run(func);
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static T RunOnMainThread<T>(Func<Task<T>> func, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return JoinableTaskFactory.Run(func);
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static void RunOnMainThread(Action func, JoinableTaskCreationOptions eOptions, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (Program.IsMainThread)
                func.Invoke();
            else
            {
                JoinableTaskFactory.Run(async () =>
                {
                    token.ThrowIfCancellationRequested();
                    await JoinableTaskFactory.SwitchToMainThreadAsync();
                    func.Invoke();
                }, eOptions);
            }
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static T RunOnMainThread<T>(Func<T> func, JoinableTaskCreationOptions eOptions, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Program.IsMainThread
                ? func.Invoke()
                : JoinableTaskFactory.Run(async () =>
                {
                    token.ThrowIfCancellationRequested();
                    await JoinableTaskFactory.SwitchToMainThreadAsync();
                    return func.Invoke();
                }, eOptions);
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static void RunOnMainThread(Func<Task> func, JoinableTaskCreationOptions eOptions = JoinableTaskCreationOptions.None, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            JoinableTaskFactory.Run(func, eOptions);
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static T RunOnMainThread<T>(Func<Task<T>> func, JoinableTaskCreationOptions eOptions = JoinableTaskCreationOptions.None, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return JoinableTaskFactory.Run(func, eOptions);
        }

        /// <summary>
        /// Run code on the main (UI) thread in an awaitable, asynchronous fashion.
        /// </summary>
        public static Task RunOnMainThreadAsync(Action func)
        {
            return JoinableTaskFactory.RunAsync(async () =>
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();
                func.Invoke();
            }).Task;
        }

        /// <summary>
        /// Run code on the main (UI) thread in an awaitable, asynchronous fashion.
        /// </summary>
        public static Task<T> RunOnMainThreadAsync<T>(Func<T> func)
        {
            return JoinableTaskFactory.RunAsync(async () =>
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();
                return func.Invoke();
            }).Task;
        }

        /// <summary>
        /// Run code on the main (UI) thread in an awaitable, asynchronous fashion.
        /// </summary>
        public static Task RunOnMainThreadAsync(Action func, CancellationToken token)
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled(token)
                : JoinableTaskFactory.RunAsync(async () =>
                {
                    token.ThrowIfCancellationRequested();
                    await JoinableTaskFactory.SwitchToMainThreadAsync(token);
                    func.Invoke();
                }).Task;
        }

        /// <summary>
        /// Run code on the main (UI) thread in an awaitable, asynchronous fashion.
        /// </summary>
        public static Task<T> RunOnMainThreadAsync<T>(Func<T> func, CancellationToken token)
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<T>(token)
                : JoinableTaskFactory.RunAsync(async () =>
                {
                    token.ThrowIfCancellationRequested();
                    await JoinableTaskFactory.SwitchToMainThreadAsync(token);
                    return func.Invoke();
                }).Task;
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static Task RunOnMainThreadAsync(Func<Task> func, CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled(token) : JoinableTaskFactory.RunAsync(func).Task;
        }

        /// <summary>
        /// Run code on the main (UI) thread in a synchronous fashion.
        /// </summary>
        public static Task<T> RunOnMainThreadAsync<T>(Func<Task<T>> func, CancellationToken token = default)
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<T>(token)
                : JoinableTaskFactory.RunAsync(func).Task;
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep with the default sleep duration done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// Because async functions don't lock threads, it does not need to manually call events anyway.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task SafeSleepAsync()
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return SafeSleepAsync(DefaultSleepDuration);
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep with the default sleep duration done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// Because async functions don't lock threads, it does not need to manually call events anyway.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task SafeSleepAsync(CancellationToken token)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return SafeSleepAsync(DefaultSleepDuration, token);
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// Because async functions don't lock threads, it does not need to manually call events anyway.
        /// </summary>
        /// <param name="intDurationMilliseconds">Duration to wait in milliseconds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task SafeSleepAsync(int intDurationMilliseconds)
        {
            return Task.Delay(intDurationMilliseconds);
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// Because async functions don't lock threads, it does not need to manually call events anyway.
        /// </summary>
        /// <param name="intDurationMilliseconds">Duration to wait in milliseconds.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task SafeSleepAsync(int intDurationMilliseconds, CancellationToken token)
        {
            return Task.Delay(intDurationMilliseconds, token);
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// </summary>
        /// <param name="objTimeSpan">Duration to wait. If 0 or less milliseconds, DefaultSleepDuration is used instead.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task SafeSleepAsync(TimeSpan objTimeSpan)
        {
            return SafeSleepAsync(objTimeSpan.Milliseconds);
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// </summary>
        /// <param name="objTimeSpan">Duration to wait. If 0 or less milliseconds, DefaultSleepDuration is used instead.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task SafeSleepAsync(TimeSpan objTimeSpan, CancellationToken token)
        {
            return SafeSleepAsync(objTimeSpan.Milliseconds, token);
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
        /// Syntactic sugar for Thread.Sleep with the default sleep duration done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// </summary>
        /// <param name="token">Cancellation token to use.</param>
        /// <param name="blnForceDoEvents">Force running of events. Useful for unit tests where running events is normally disabled.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeSleep(CancellationToken token, bool blnForceDoEvents = false)
        {
            SafeSleep(DefaultSleepDuration, token, blnForceDoEvents);
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
            if (!EverDoEvents)
            {
                if (Program.IsMainThread)
                    JoinableTaskFactory.Run(() => Task.Delay(intDurationMilliseconds),
                                            intDurationMilliseconds > 1000
                                                ? JoinableTaskCreationOptions.LongRunning
                                                : JoinableTaskCreationOptions.None);
                else
                    Thread.Sleep(intDurationMilliseconds);
                return;
            }

            int i = intDurationMilliseconds;
            for (; i >= DefaultSleepDuration; i -= DefaultSleepDuration)
            {
                if (Program.IsMainThread)
                    JoinableTaskFactory.Run(() => Task.Delay(DefaultSleepDuration));
                else
                    Thread.Sleep(DefaultSleepDuration);
                DoEventsSafe(blnForceDoEvents);
            }
            if (i > 0)
            {
                if (Program.IsMainThread)
                    JoinableTaskFactory.Run(() => Task.Delay(i));
                else
                    Thread.Sleep(i);
                DoEventsSafe(blnForceDoEvents);
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
        /// Syntactic sugar for Thread.Sleep done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// </summary>
        /// <param name="intDurationMilliseconds">Duration to wait in milliseconds.</param>
        /// <param name="token">Cancellation token to use.</param>
        /// <param name="blnForceDoEvents">Force running of events. Useful for unit tests where running events is normally disabled.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeSleep(int intDurationMilliseconds, CancellationToken token, bool blnForceDoEvents = false)
        {
            int i = intDurationMilliseconds;
            for (; i >= DefaultSleepDuration; i -= DefaultSleepDuration)
            {
                token.ThrowIfCancellationRequested();
                if (Program.IsMainThread)
                    JoinableTaskFactory.Run(() => Task.Delay(DefaultSleepDuration, token));
                else
                    Thread.Sleep(DefaultSleepDuration);
                if (EverDoEvents)
                {
                    token.ThrowIfCancellationRequested();
                    DoEventsSafe(blnForceDoEvents);
                }
            }
            if (i > 0)
            {
                token.ThrowIfCancellationRequested();
                if (Program.IsMainThread)
                    JoinableTaskFactory.Run(() => Task.Delay(i, token));
                else
                    Thread.Sleep(i);
                if (EverDoEvents)
                {
                    token.ThrowIfCancellationRequested();
                    DoEventsSafe(blnForceDoEvents);
                }
            }
        }

        /// <summary>
        /// Syntactic sugar for Thread.Sleep done in a way that makes sure the application will run queued up events afterwards.
        /// This means that this method can (in theory) be put in a loop without it ever causing the UI thread to get locked.
        /// </summary>
        /// <param name="objTimeSpan">Duration to wait. If 0 or less milliseconds, DefaultSleepDuration is used instead.</param>
        /// <param name="token">Cancellation token to use.</param>
        /// <param name="blnForceDoEvents">Force running of events. Useful for unit tests where running events is normally disabled.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeSleep(TimeSpan objTimeSpan, CancellationToken token, bool blnForceDoEvents = false)
        {
            SafeSleep(objTimeSpan.Milliseconds, token, blnForceDoEvents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DoEventsSafe(bool blnForceDoEvents = false)
        {
            try
            {
                int intIsOkToRunDoEvents = Interlocked.Decrement(ref _intIsOkToRunDoEvents);
                if (blnForceDoEvents || intIsOkToRunDoEvents == 0)
                {
                    Application.DoEvents();
                }
            }
            finally
            {
                Interlocked.Increment(ref _intIsOkToRunDoEvents);
            }
        }

        /// <summary>
        /// Never wait around in designer mode, we should not care about thread locking, and running in a background thread can mess up IsDesignerMode checks inside that thread
        /// </summary>
        public static bool EverDoEvents => Program.IsMainThread && !IsDesignerMode && !IsRunningInVisualStudio;

        /// <summary>
        /// Don't run events during unit tests, but still run in the background so that we can catch any issues caused by our setup.
        /// </summary>
        private static bool DefaultIsOkToRunDoEvents => (!IsUnitTest || IsUnitTestForUI) && EverDoEvents;

        /// <summary>
        /// This member makes sure we aren't swamping the program with massive amounts of Application.DoEvents() calls
        /// </summary>
        private static int _intIsOkToRunDoEvents = DefaultIsOkToRunDoEvents.ToInt32();

#pragma warning disable VSTHRD002
#pragma warning disable VSTHRD104 // Offer async methods
        /// <summary>
        /// Syntactic sugar for synchronously running an async task in a way that uses the Main Thread's JoinableTaskFactory where possible.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to run.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafelyRunSynchronously(Func<Task> funcToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Program.IsMainThread)
                JoinableTaskFactory.Run(funcToRun, JoinableTaskCreationOptions.LongRunning);
            else
                funcToRun.Invoke().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Syntactic sugar for synchronously running an async task in a way that uses the Main Thread's JoinableTaskFactory where possible.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to run.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SafelyRunSynchronously<T>(Func<Task<T>> funcToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Program.IsMainThread
                ? JoinableTaskFactory.Run(funcToRun, JoinableTaskCreationOptions.LongRunning)
                : funcToRun.Invoke().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Syntactic sugar for synchronously running an async task in a way that uses the Main Thread's JoinableTaskFactory where possible.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Code to run.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafelyRunSynchronously(IEnumerable<Func<Task>> afuncToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Program.IsMainThread)
            {
                foreach (Func<Task> funcToRun in afuncToRun)
                {
                    token.ThrowIfCancellationRequested();
                    JoinableTaskFactory.Run(funcToRun, JoinableTaskCreationOptions.LongRunning);
                }
            }
            else
            {
                foreach (Func<Task> funcToRun in afuncToRun)
                {
                    token.ThrowIfCancellationRequested();
                    funcToRun.Invoke().GetAwaiter().GetResult();
                }
            }
        }

        /// <summary>
        /// Syntactic sugar for synchronously running an async task in a way that uses the Main Thread's JoinableTaskFactory where possible.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Code to run.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] SafelyRunSynchronously<T>(IReadOnlyCollection<Func<Task<T>>> afuncToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intCount = afuncToRun.Count;
            T[] aobjReturn = new T[intCount];
            int i = 0;
            if (Program.IsMainThread)
            {
                foreach (Func<Task<T>> funcToRun in afuncToRun)
                {
                    token.ThrowIfCancellationRequested();
                    aobjReturn[i++] = JoinableTaskFactory.Run(funcToRun, JoinableTaskCreationOptions.LongRunning);
                }
            }
            else
            {
                foreach (Func<Task<T>> funcToRun in afuncToRun)
                {
                    token.ThrowIfCancellationRequested();
                    aobjReturn[i++] = funcToRun.Invoke().GetAwaiter().GetResult();
                }
            }
            return aobjReturn;
        }

        /// <summary>
        /// Syntactic sugar for synchronously running an async task in a way that uses the Main Thread's JoinableTaskFactory where possible.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Code to run.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafelyRunSynchronously(params Func<Task>[] afuncToRun)
        {
            SafelyRunSynchronously(afuncToRun, default);
        }

        /// <summary>
        /// Syntactic sugar for synchronously running an async task in a way that uses the Main Thread's JoinableTaskFactory where possible.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Code to run.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] SafelyRunSynchronously<T>(params Func<Task<T>>[] afuncToRun)
        {
            return SafelyRunSynchronously(afuncToRun, default);
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(Action funcToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!EverDoEvents || (Program.IsMainThread && _intIsOkToRunDoEvents < 1))
            {
                funcToRun.Invoke();
                return;
            }
            Task objTask = Task.Run(funcToRun, token);
            while (!objTask.IsCompleted)
                SafeSleep(token);
            if (objTask.Exception != null)
                throw objTask.Exception;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(Action<CancellationToken> funcToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!EverDoEvents || (Program.IsMainThread && _intIsOkToRunDoEvents < 1))
            {
                funcToRun.Invoke(token);
                return;
            }
            Task objTask = Task.Run(() => funcToRun(token), token);
            while (!objTask.IsCompleted)
                SafeSleep(token);
            if (objTask.Exception != null)
                throw objTask.Exception;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(params Action[] afuncToRun)
        {
            RunWithoutThreadLock(afuncToRun, default);
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(Action[] afuncToRun, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!EverDoEvents || (Program.IsMainThread && _intIsOkToRunDoEvents < 1))
            {
                if (token == CancellationToken.None)
                    Parallel.Invoke(afuncToRun);
                else
                {
                    Parallel.ForEach(afuncToRun, (x, y) =>
                    {
                        if (token.IsCancellationRequested || y.ShouldExitCurrentIteration)
                            y.Stop();
                        x.Invoke();
                    });
                    token.ThrowIfCancellationRequested();
                }
                return;
            }

            Task objTask = token == CancellationToken.None
                ? Task.Run(() => Parallel.Invoke(afuncToRun), token)
                : Task.Run(() =>
                {
                    Parallel.ForEach(afuncToRun, (x, y) =>
                    {
                        if (token.IsCancellationRequested || y.ShouldExitCurrentIteration)
                            y.Stop();
                        x.Invoke();
                    });
                    token.ThrowIfCancellationRequested();
                }, token);
            while (!objTask.IsCompleted)
                SafeSleep(token);
            if (objTask.Exception != null)
                throw objTask.Exception;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RunWithoutThreadLock<T>(Func<T> funcToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!EverDoEvents || (Program.IsMainThread && _intIsOkToRunDoEvents < 1))
            {
                return funcToRun.Invoke();
            }
            Task<T> objTask = Task.Run(funcToRun, token);
            while (!objTask.IsCompleted)
                SafeSleep(token);
            if (objTask.Exception != null)
                throw objTask.Exception;
            return objTask.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RunWithoutThreadLock<T>(Func<CancellationToken, T> funcToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!EverDoEvents || (Program.IsMainThread && _intIsOkToRunDoEvents < 1))
            {
                return funcToRun.Invoke(token);
            }
            Task<T> objTask = Task.Run(() => funcToRun(token), token);
            while (!objTask.IsCompleted)
                SafeSleep(token);
            if (objTask.Exception != null)
                throw objTask.Exception;
            return objTask.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] RunWithoutThreadLock<T>(params Func<T>[] afuncToRun)
        {
            return RunWithoutThreadLock(afuncToRun, default);
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] RunWithoutThreadLock<T>(IReadOnlyList<Func<T>> afuncToRun, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            int intLength = afuncToRun.Count;
            T[] aobjReturn = new T[intLength];
            if (!EverDoEvents || (Program.IsMainThread && _intIsOkToRunDoEvents < 1))
            {
                Parallel.For(0, intLength, (i, y) =>
                {
                    if (token.IsCancellationRequested || y.ShouldExitCurrentIteration)
                        y.Stop();
                    aobjReturn[i] = afuncToRun[i].Invoke();
                });
                token.ThrowIfCancellationRequested();
                return aobjReturn;
            }
            Task<T>[] aobjTasks = new Task<T>[MaxParallelBatchSize];
            int intCounter = 0;
            int intOffset = 0;
            for (int i = 0; i < intLength; ++i)
            {
                aobjTasks[intCounter++] = Task.Run(afuncToRun[i], token);
                if (intCounter != MaxParallelBatchSize)
                    continue;
                Task<T[]> tskLoop = Task.Run(() => Task.WhenAll(aobjTasks), token);
                while (!tskLoop.IsCompleted)
                    SafeSleep(token);
                if (tskLoop.Exception != null)
                    throw tskLoop.Exception;
                for (int j = 0; j < MaxParallelBatchSize; ++j)
                    aobjReturn[i] = aobjTasks[j].GetAwaiter().GetResult();
                intOffset += MaxParallelBatchSize;
                intCounter = 0;
            }
            int intFinalBatchSize = intLength % MaxParallelBatchSize;
            if (intFinalBatchSize != 0)
            {
                Task<T[]> objTask = Task.Run(() => Task.WhenAll(aobjTasks), token);
                while (!objTask.IsCompleted)
                    SafeSleep(token);
                if (objTask.Exception != null)
                    throw objTask.Exception;
                for (int j = 0; j < intFinalBatchSize; ++j)
                    aobjReturn[intOffset + j] = aobjTasks[j].GetAwaiter().GetResult();
            }
            return aobjReturn;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RunWithoutThreadLock<T>(Func<Task<T>> funcToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Program.IsMainThread && _intIsOkToRunDoEvents < 1)
            {
                return JoinableTaskFactory.Run(funcToRun, JoinableTaskCreationOptions.LongRunning);
            }
            if (!EverDoEvents)
            {
                return funcToRun.Invoke().GetAwaiter().GetResult();
            }
            Task<T> objTask = Task.Run(funcToRun, token);
            while (!objTask.IsCompleted)
                SafeSleep(token);
            if (objTask.Exception != null)
                throw objTask.Exception;
            return objTask.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RunWithoutThreadLock<T>(Func<CancellationToken, Task<T>> funcToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Program.IsMainThread && _intIsOkToRunDoEvents < 1)
            {
                return JoinableTaskFactory.Run(() => funcToRun(token), JoinableTaskCreationOptions.LongRunning);
            }
            if (!EverDoEvents)
            {
                return funcToRun.Invoke(token).GetAwaiter().GetResult();
            }
            Task<T> objTask = Task.Run(() => funcToRun(token), token);
            while (!objTask.IsCompleted)
                SafeSleep(token);
            if (objTask.Exception != null)
                throw objTask.Exception;
            return objTask.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] RunWithoutThreadLock<T>(params Func<Task<T>>[] afuncToRun)
        {
            return RunWithoutThreadLock(Array.AsReadOnly(afuncToRun), default);
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] RunWithoutThreadLock<T>(IReadOnlyList<Func<Task<T>>> afuncToRun, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            int intLength = afuncToRun.Count;
            T[] aobjReturn = new T[intLength];
            if (Program.IsMainThread && _intIsOkToRunDoEvents < 1)
            {
                Parallel.For(0, intLength, (i, y) =>
                {
                    if (token.IsCancellationRequested || y.ShouldExitCurrentIteration)
                        y.Stop();
                    aobjReturn[i] = JoinableTaskFactory.Run(afuncToRun[i], JoinableTaskCreationOptions.LongRunning);
                });
                token.ThrowIfCancellationRequested();
                return aobjReturn;
            }
            if (!EverDoEvents)
            {
                Parallel.For(0, intLength, (i, y) =>
                {
                    if (token.IsCancellationRequested || y.ShouldExitCurrentIteration)
                        y.Stop();
                    Task<T> objSyncTask = afuncToRun[i].Invoke();
                    if (objSyncTask.Status == TaskStatus.Created)
                        objSyncTask.RunSynchronously();
                    if (objSyncTask.Exception != null)
                        throw objSyncTask.Exception;
                    aobjReturn[i] = objSyncTask.GetAwaiter().GetResult();
                });
                token.ThrowIfCancellationRequested();
                return aobjReturn;
            }
            Task<T>[] aobjTasks = new Task<T>[MaxParallelBatchSize];
            int intCounter = 0;
            int intOffset = 0;
            for (int i = 0; i < intLength; ++i)
            {
                aobjTasks[intCounter++] = Task.Run(afuncToRun[i], token);
                if (intCounter != MaxParallelBatchSize)
                    continue;
                Task<T[]> tskLoop = Task.Run(() => Task.WhenAll(aobjTasks), token);
                while (!tskLoop.IsCompleted)
                    SafeSleep(token);
                if (tskLoop.Exception != null)
                    throw tskLoop.Exception;
                for (int j = 0; j < MaxParallelBatchSize; ++j)
                    aobjReturn[i] = aobjTasks[j].GetAwaiter().GetResult();
                intOffset += MaxParallelBatchSize;
                intCounter = 0;
            }
            int intFinalBatchSize = intLength % MaxParallelBatchSize;
            if (intFinalBatchSize != 0)
            {
                Task<T[]> objTask = Task.Run(() => Task.WhenAll(aobjTasks), token);
                while (!objTask.IsCompleted)
                    SafeSleep(token);
                if (objTask.Exception != null)
                    throw objTask.Exception;
                for (int j = 0; j < intFinalBatchSize; ++j)
                    aobjReturn[intOffset + j] = aobjTasks[j].GetAwaiter().GetResult();
            }
            return aobjReturn;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(Func<Task> funcToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Program.IsMainThread && _intIsOkToRunDoEvents < 1)
            {
                JoinableTaskFactory.Run(funcToRun, JoinableTaskCreationOptions.LongRunning);
                return;
            }
            if (!EverDoEvents)
            {
                Task objSyncTask = funcToRun.Invoke();
                if (objSyncTask.Status == TaskStatus.Created)
                    objSyncTask.RunSynchronously();
                if (objSyncTask.Exception != null)
                    throw objSyncTask.Exception;
                return;
            }
            Task objTask = Task.Run(funcToRun, token);
            while (!objTask.IsCompleted)
                SafeSleep(token);
            if (objTask.Exception != null)
                throw objTask.Exception;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for code to complete while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="funcToRun">Code to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(Func<CancellationToken, Task> funcToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Program.IsMainThread && _intIsOkToRunDoEvents < 1)
            {
                JoinableTaskFactory.Run(() => funcToRun(token), JoinableTaskCreationOptions.LongRunning);
                return;
            }
            if (!EverDoEvents)
            {
                Task objSyncTask = funcToRun.Invoke(token);
                if (objSyncTask.Status == TaskStatus.Created)
                    objSyncTask.RunSynchronously();
                if (objSyncTask.Exception != null)
                    throw objSyncTask.Exception;
                return;
            }
            Task objTask = Task.Run(() => funcToRun.Invoke(token), token);
            while (!objTask.IsCompleted)
                SafeSleep(token);
            if (objTask.Exception != null)
                throw objTask.Exception;
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(params Func<Task>[] afuncToRun)
        {
            RunWithoutThreadLock(Array.AsReadOnly(afuncToRun), default);
        }

        /// <summary>
        /// Syntactic sugar for synchronously waiting for codes to complete in parallel while still allowing queued invocations to go through.
        /// Warning: much clumsier and slower than just using awaits inside of an async method. Use those instead if possible.
        /// </summary>
        /// <param name="afuncToRun">Codes to wait for.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunWithoutThreadLock(IEnumerable<Func<Task>> afuncToRun, CancellationToken token)
        {
            if (Program.IsMainThread && _intIsOkToRunDoEvents < 1)
            {
                Parallel.ForEach(afuncToRun, (funcToRun, y) =>
                {
                    if (token.IsCancellationRequested || y.ShouldExitCurrentIteration)
                        y.Stop();
                    JoinableTaskFactory.Run(funcToRun, JoinableTaskCreationOptions.LongRunning);
                });
                token.ThrowIfCancellationRequested();
                return;
            }
            if (!EverDoEvents)
            {
                Parallel.ForEach(afuncToRun, (funcToRun, y) =>
                {
                    if (token.IsCancellationRequested || y.ShouldExitCurrentIteration)
                        y.Stop();
                    Task objSyncTask = funcToRun.Invoke();
                    if (objSyncTask.Status == TaskStatus.Created)
                        objSyncTask.RunSynchronously();
                    if (objSyncTask.Exception != null)
                        throw objSyncTask.Exception;
                });
                token.ThrowIfCancellationRequested();
                return;
            }
            List<Task> lstTasks = new List<Task>(MaxParallelBatchSize);
            int intCounter = 0;
            foreach (Func<Task> funcToRun in afuncToRun)
            {
                lstTasks.Add(Task.Run(funcToRun, token));
                if (++intCounter != MaxParallelBatchSize)
                    continue;
                Task tskLoop = Task.Run(() => Task.WhenAll(lstTasks), token);
                while (!tskLoop.IsCompleted)
                    SafeSleep(token);
                if (tskLoop.Exception != null)
                    throw tskLoop.Exception;
                lstTasks.Clear();
                intCounter = 0;
            }
            Task objTask = Task.Run(() => Task.WhenAll(lstTasks), token);
            while (!objTask.IsCompleted)
                SafeSleep(token);
            if (objTask.Exception != null)
                throw objTask.Exception;
        }
#pragma warning restore VSTHRD104 // Offer async methods
#pragma warning restore VSTHRD002

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
                if (strReturn.StartsWith("Windows", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(objOSInfo.ServicePack))
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

        private static readonly XmlWriterSettings _objStandardXmlWriterSettings = new XmlWriterSettings
        { Async = true, Encoding = Encoding.UTF8, Indent = true, IndentChars = "\t" };

        public static XmlWriter GetStandardXmlWriter(Stream output)
        {
            return XmlWriter.Create(output, _objStandardXmlWriterSettings);
        }

        private static readonly XmlWriterSettings _objXslTransformXmlWriterSettings = new XmlWriterSettings
        { Encoding = Encoding.UTF8, Indent = true, IndentChars = "\t", CheckCharacters = false, ConformanceLevel = ConformanceLevel.Fragment };

        public static XmlWriter GetXslTransformXmlWriter(Stream output)
        {
            return XmlWriter.Create(output, _objXslTransformXmlWriterSettings);
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

        private static readonly DefaultObjectPoolProvider s_ObjObjectPoolProvider = new DefaultObjectPoolProvider
        {
            MaximumRetained = DefaultPoolSize
        };

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
        public static SafeObjectPool<List<ListItem>> ListItemListPool { get; }
            = new SafeObjectPool<List<ListItem>>(Math.Max(MaxParallelBatchSize, 64), () => new List<ListItem>(), x => x.Clear());

        /// <summary>
        /// Memory Pool for empty hash sets of strings. A bit slower up-front than a simple allocation, but reduces memory allocations when used a lot, which saves on CPU used for Garbage Collection.
        /// </summary>
        [CLSCompliant(false)]
        public static SafeObjectPool<HashSet<string>> StringHashSetPool { get; }
            = new SafeObjectPool<HashSet<string>>(() => new HashSet<string>(), x => x.Clear());

        /// <summary>
        /// Memory Pool for empty dictionaries used for processing multiple property changed. A bit slower up-front than a simple allocation, but reduces memory allocations when used a lot, which saves on CPU used for Garbage Collection.
        /// </summary>
        [CLSCompliant(false)]
        public static SafeObjectPool<Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>>
            DictionaryForMultiplePropertyChangedPool { get; }
            = new SafeObjectPool<Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>>(
                () => new Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>(), x => x.Clear());

        /// <summary>
        /// Memory Pool for SemaphoreSlim with one allowed semaphore that is used for async-friendly thread safety stuff. A bit slower up-front than a simple allocation, but reduces memory allocations when used a lot, which saves on CPU used for Garbage Collection.
        /// WARNING! This will end up being a DisposableObjectPool, which can have weird behaviors (e.g. disposal-then-reuse) if used in SemaphoreSlim members in classes that stick around! Avoid using this if possible for those cases.
        /// </summary>
        [CLSCompliant(false)]
        public static SafeDisposableObjectPool<DebuggableSemaphoreSlim> SemaphorePool { get; }
            = new SafeDisposableObjectPool<DebuggableSemaphoreSlim>(Math.Max(MaxParallelBatchSize, 256), () => new DebuggableSemaphoreSlim());

        /// <summary>
        /// RecyclableMemoryStreamManager to be used for all RecyclableMemoryStream constructors.
        /// </summary>
        public static RecyclableMemoryStreamManager MemoryStreamManager { get; } = new RecyclableMemoryStreamManager();
    }
}
