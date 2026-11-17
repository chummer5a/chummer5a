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
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    // Isolate all WinForms-specific unit tests to this class
    [TestClass]
    public class UserInterfaceTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod, Timeout(30 * 1000, CooperativeCancellation = true)]
        public void DoBasicStartup()
        {
            // First, populate our MRU lists with test data to keep things consistent and make sure things like the character roster work properly
            string[] astrStickyMru = new string[GlobalSettings.MaxMruSize];
            string[] astrMru = new string[GlobalSettings.MaxMruSize];
            int i = 0;
            for (; i < GlobalSettings.MaxMruSize; ++i)
            {
                astrStickyMru[i] = UnitTestUtils.CommonTestData.TestFileInfos[i].FullName;
            }
            for (; i < GlobalSettings.MaxMruSize * 2; ++i)
            {
                astrMru[i - GlobalSettings.MaxMruSize] = UnitTestUtils.CommonTestData.TestFileInfos[i].FullName;
            }
            List<string> lstOldFavoriteCharacters = new List<string>(GlobalSettings.FavoriteCharacters);
            List<string> lstOldMostRecentlyUsedCharacters = new List<string>(GlobalSettings.MostRecentlyUsedCharacters);
            string strOldWatchFolder = GlobalSettings.CharacterRosterPath;
            try
            {
                GlobalSettings.FavoriteCharacters.Clear();
                GlobalSettings.FavoriteCharacters.AddRange(astrStickyMru);
                GlobalSettings.MostRecentlyUsedCharacters.Clear();
                GlobalSettings.MostRecentlyUsedCharacters.AddRange(astrMru);
                GlobalSettings.CharacterRosterPath = UnitTestUtils.CommonTestData.TestPathInfo.FullName;

                // Now load the UI, including the Master Index and Character Roster
                bool blnOldHideMasterIndex = GlobalSettings.HideMasterIndex;
                bool blnOldHideCharacterRoster = GlobalSettings.HideCharacterRoster;
                bool blnOldAutoUpdate = GlobalSettings.AutomaticUpdate;
                try
                {
                    GlobalSettings.HideMasterIndex = false;
                    GlobalSettings.HideCharacterRoster = false;
                    GlobalSettings.AutomaticUpdate = false;
                    ChummerMainForm frmOldMainForm = Program.MainForm;
                    ChummerMainForm frmTestForm = null;
                    // Try-finally pattern necessary in order prevent weird exceptions from disposal of MdiChildren
                    try
                    {
                        Utils.IsUnitTestForUI = true;
                        frmTestForm = Utils.RunOnMainThread(() => new ChummerMainForm(true, true)
                        {
                            ShowInTaskbar =
                                false // This lets the form be "shown" in unit tests (to actually have it show, ShowDialog() needs to be used, but that forces the test to be interactive)
                        }, token: TestContext.CancellationToken);
                        Program.MainForm = frmTestForm; // Set program Main form to Unit test version
                        frmTestForm.DoThreadSafe(x =>
                        {
                            x.Show(); // We don't actually want to display the main form, so Show() is used (ShowDialog() would actually display it).
#if DEBUG
                    x.SendToBack();
#endif
                        }, TestContext.CancellationToken);
                        while (!frmTestForm.IsFinishedLoading) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                        {
                            Utils.SafeSleep(TestContext.CancellationToken);
                        }
                    }
                    finally
                    {
                        try
                        {
                            frmTestForm?.DoThreadSafe(x => x.Close(), TestContext.CancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this
                        }
                        catch (Exception e)
                        {
                            e = e.Demystify();
                            string strErrorMessage = "Encountered (non-fatal) exception while disposing of main form." + Environment.NewLine
                                + e.Message;
                            Debug.WriteLine(strErrorMessage);
                            Console.WriteLine(strErrorMessage);
                            Utils.BreakIfDebug();
                        }
                        Program.MainForm = frmOldMainForm;
                        Utils.IsUnitTestForUI = false;
                    }
                }
                finally
                {
                    GlobalSettings.HideMasterIndex = blnOldHideMasterIndex;
                    GlobalSettings.HideCharacterRoster = blnOldHideCharacterRoster;
                    GlobalSettings.AutomaticUpdate = blnOldAutoUpdate;
                }
            }
            finally
            {
                GlobalSettings.FavoriteCharacters.Clear();
                GlobalSettings.FavoriteCharacters.AddRange(lstOldFavoriteCharacters);
                GlobalSettings.MostRecentlyUsedCharacters.Clear();
                GlobalSettings.MostRecentlyUsedCharacters.AddRange(lstOldMostRecentlyUsedCharacters);
                GlobalSettings.CharacterRosterPath = strOldWatchFolder;
            }
        }

        [TestMethod, Timeout(30 * 60 * 1000, CooperativeCancellation = true)]
        public void LoadAllCharacterForms()
        {
            Utils.SafelyRunSynchronously(t => UnitTestUtils.LoadAllTestCharactersAsync(token: t), TestContext.CancellationToken);

            // First, populate our MRU lists with test data to keep things consistent and make sure things like the character roster work properly
            string[] astrStickyMru = new string[GlobalSettings.MaxMruSize];
            string[] astrMru = new string[GlobalSettings.MaxMruSize];
            int i = 0;
            for (; i < GlobalSettings.MaxMruSize; ++i)
            {
                astrStickyMru[i] = UnitTestUtils.CommonTestData.TestFileInfos[i].FullName;
            }
            for (; i < GlobalSettings.MaxMruSize * 2; ++i)
            {
                astrMru[i - GlobalSettings.MaxMruSize] = UnitTestUtils.CommonTestData.TestFileInfos[i].FullName;
            }
            List<string> lstOldFavoriteCharacters = new List<string>(GlobalSettings.FavoriteCharacters);
            List<string> lstOldMostRecentlyUsedCharacters = new List<string>(GlobalSettings.MostRecentlyUsedCharacters);
            string strOldWatchFolder = GlobalSettings.CharacterRosterPath;
            try
            {
                GlobalSettings.FavoriteCharacters.Clear();
                GlobalSettings.FavoriteCharacters.AddRange(astrStickyMru);
                GlobalSettings.MostRecentlyUsedCharacters.Clear();
                GlobalSettings.MostRecentlyUsedCharacters.AddRange(astrMru);
                GlobalSettings.CharacterRosterPath = UnitTestUtils.CommonTestData.TestPathInfo.FullName;

                // Now load the UI, including the Master Index and Character Roster
                bool blnOldHideMasterIndex = GlobalSettings.HideMasterIndex;
                bool blnOldHideCharacterRoster = GlobalSettings.HideCharacterRoster;
                bool blnOldAutoUpdate = GlobalSettings.AutomaticUpdate;
                try
                {
                    GlobalSettings.HideMasterIndex = false;
                    GlobalSettings.HideCharacterRoster = false;
                    GlobalSettings.AutomaticUpdate = false;
                    ChummerMainForm frmOldMainForm = Program.MainForm;
                    ChummerMainForm frmTestForm = null;
                    // Try-finally pattern necessary in order prevent weird exceptions from disposal of MdiChildren
                    try
                    {
                        Utils.IsUnitTestForUI = true;
                        frmTestForm = Utils.RunOnMainThread(() => new ChummerMainForm(true, true)
                        {
                            ShowInTaskbar =
                                false // This lets the form be "shown" in unit tests (to actually have it show, ShowDialog() needs to be used, but that forces the test to be interactive)
                        }, token: TestContext.CancellationToken);
                        Program.MainForm = frmTestForm; // Set program Main form to Unit test version
                        frmTestForm.DoThreadSafe(x =>
                        {
                            x.Show(); // We don't actually want to display the main form, so Show() is used (ShowDialog() would actually display it).
#if DEBUG
                    x.SendToBack();
#endif
                        }, TestContext.CancellationToken);
                        while (!frmTestForm.IsFinishedLoading) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                        {
                            Utils.SafeSleep(TestContext.CancellationToken);
                        }

                        Debug.WriteLine("Main form loaded");
                        foreach (Character objCharacter in UnitTestUtils.GetTestCharacters(TestContext.CancellationToken))
                        {
                            string strFileName = Path.GetFileName(objCharacter.FileName) ?? "Unknown";
                            Debug.WriteLine("Checking " + strFileName);
                            string strDummyFileName = Path.Combine(UnitTestUtils.CommonTestData.TestPathInfo.FullName,
                                "(UnitTest07Dummy) "
                                + Path.GetFileNameWithoutExtension(objCharacter.FileName)
                                + ".txt");
                            using (File.Create(strDummyFileName, byte.MaxValue,
                                       FileOptions
                                           .DeleteOnClose)) // Create this so that we can track how far along the Unit Test is even if we don't have a debugger attached
                            {
                                try
                                {
                                    bool blnFormClosed = false;
                                    bool blnCreated = objCharacter.Created;
                                    // ReSharper disable once AccessToDisposedClosure
                                    CharacterShared frmCharacterForm = Program.MainForm.DoThreadSafeFunc(
                                        () => blnCreated
                                            // ReSharper disable once AccessToDisposedClosure
                                            ? (CharacterShared)new CharacterCareer(objCharacter)
                                            // ReSharper disable once AccessToDisposedClosure
                                            : new CharacterCreate(objCharacter), TestContext.CancellationToken);
                                    try
                                    {
                                        frmCharacterForm.DoThreadSafe(x =>
                                        {
                                            x.FormClosed += (sender, args) => blnFormClosed = true;
                                            x.MdiParent = frmTestForm;
                                            x.ShowInTaskbar = false;
                                            x.Show(); // We don't actually want to display the main form, so Show() is used (ShowDialog() would actually display it).
#if DEBUG
                                    x.SendToBack();
#endif
                                        }, TestContext.CancellationToken);
                                        while
                                            (!frmCharacterForm
                                                .IsFinishedInitializing) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                                        {
                                            Utils.SafeSleep(TestContext.CancellationToken);
                                        }
                                    }
                                    finally
                                    {
                                        try
                                        {
                                            frmCharacterForm.DoThreadSafe(x => x.Close(), TestContext.CancellationToken);
                                            while
                                                (!blnFormClosed &&
                                                 !frmCharacterForm
                                                     .IsDisposed) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                                            {
                                                Utils.SafeSleep(TestContext.CancellationToken);
                                            }
                                        }
                                        catch (OperationCanceledException)
                                        {
                                            //swallow this
                                        }
                                        catch (ApplicationException e)
                                        {
                                            e = e.Demystify();
                                            string strErrorMessage
                                                = "Encountered (non-fatal) exception while disposing of character form."
                                                  + Environment.NewLine
                                                  + e.Message;
                                            Debug.WriteLine(strErrorMessage);
                                            Console.WriteLine(strErrorMessage);
                                            Program.OpenCharacters.Remove(objCharacter);
                                        }
                                        catch (InvalidOperationException e)
                                        {
                                            e = e.Demystify();
                                            string strErrorMessage
                                                = "Encountered (non-fatal) exception while disposing of character form."
                                                  + Environment.NewLine
                                                  + e.Message;
                                            Debug.WriteLine(strErrorMessage);
                                            Console.WriteLine(strErrorMessage);
                                            Program.OpenCharacters.Remove(objCharacter);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    e = e.Demystify();
                                    string strErrorMessage
                                        = "Exception while loading form for " + strFileName + ":";
                                    strErrorMessage += Environment.NewLine + e;
                                    Debug.WriteLine(strErrorMessage);
                                    Console.WriteLine(strErrorMessage);
                                    throw;
                                }
                            }
                        }
                    }
                    finally
                    {
                        try
                        {
                            frmTestForm?.DoThreadSafe(x => x.Close(), TestContext.CancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this
                        }
                        catch (Exception e)
                        {
                            string strErrorMessage = "Encountered (non-fatal) exception while disposing of main form." + Environment.NewLine
                                + e.Message;
                            Debug.WriteLine(strErrorMessage);
                            Console.WriteLine(strErrorMessage);
                            Utils.BreakIfDebug();
                        }
                        Program.MainForm = frmOldMainForm;
                        Utils.IsUnitTestForUI = false;
                    }
                }
                finally
                {
                    GlobalSettings.HideMasterIndex = blnOldHideMasterIndex;
                    GlobalSettings.HideCharacterRoster = blnOldHideCharacterRoster;
                    GlobalSettings.AutomaticUpdate = blnOldAutoUpdate;
                }
            }
            finally
            {
                GlobalSettings.FavoriteCharacters.Clear();
                GlobalSettings.FavoriteCharacters.AddRange(lstOldFavoriteCharacters);
                GlobalSettings.MostRecentlyUsedCharacters.Clear();
                GlobalSettings.MostRecentlyUsedCharacters.AddRange(lstOldMostRecentlyUsedCharacters);
                GlobalSettings.CharacterRosterPath = strOldWatchFolder;
            }
        }
    }
}
