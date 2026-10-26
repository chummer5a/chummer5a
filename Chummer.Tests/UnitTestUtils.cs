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

// Uncomment this define to turn on code that is useful for trying to find, diagnose, and squash memory leaks, though it will slow down unit tests considerably
//#define MEMORYTESTING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public static class UnitTestUtils
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Utils.IsUnitTest = true;
            Utils.IsUnitTestForUI = false;
            Utils.CreateSynchronizationContext();
        }

        public readonly struct CommonTestData
        {
            static CommonTestData()
            {
                TestFilesBasePathInfo = new DirectoryInfo(TestFilesBasePath);//Assuming Test is your Folder
                foreach (DirectoryInfo objOldDir in TestFilesBasePathInfo.GetDirectories("TestRun-*"))
                    Directory.Delete(objOldDir.FullName, true);
                TestPathInfo = Directory.CreateDirectory(Path.Combine(TestFilesBasePath,
                    "TestRun-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm", GlobalSettings.InvariantCultureInfo)));
                TestFileInfos = TestFilesBasePathInfo.GetFiles("*.chum5"); //Getting Text files
                Characters = new Character[TestFileInfos.Length];
            }

            private static string TestFilesBasePath { get; } =
                Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase ?? string.Empty, "TestFiles");

            private static DirectoryInfo TestFilesBasePathInfo { get; }

            public static DirectoryInfo TestPathInfo { get; }

            public static FileInfo[] TestFileInfos { get; }

            public static Character[] Characters { get; }
        }

        public static IEnumerable<Character> GetTestCharacters(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            for (int i = 0; i < CommonTestData.TestFileInfos.Length; ++i)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                FileInfo objFileInfo = CommonTestData.TestFileInfos[i];
                Debug.WriteLine("Loading " + objFileInfo.Name);
#endif
                Character objLoopCharacter = CommonTestData.Characters[i];
                if (objLoopCharacter == null)
                {
#if DEBUG
                    objLoopCharacter = LoadCharacter(objFileInfo, token: token);
#else
                    objLoopCharacter = LoadCharacter(CommonTestData.TestFileInfos[i], token: token);
#endif
                    CommonTestData.Characters[i] = objLoopCharacter;
                }

                yield return objLoopCharacter;
            }
        }

        /// <summary>
        /// Validate that a given list of Characters can be successfully loaded.
        /// </summary>
        // ReSharper disable once SuggestBaseTypeForParameter
        public static Character LoadCharacter(FileInfo objFileInfo, Character objExistingCharacter = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Character objCharacter = objExistingCharacter;
            try
            {
                Debug.WriteLine("Loading: " + objFileInfo.Name);
                if (objExistingCharacter != null)
                    objCharacter.ResetCharacter(token);
                else
                    objCharacter = new Character();
                objCharacter.FileName = objFileInfo.FullName;

                bool blnSuccess = objCharacter.Load(token: token);
                Assert.IsTrue(blnSuccess);
                Debug.WriteLine("Character loaded: " + objCharacter.Name + ", " + objFileInfo.Name);
            }
            catch (AssertFailedException e)
            {
                e = e.Demystify();
                if (objCharacter != null && objExistingCharacter == null)
                {
                    objCharacter.Dispose();
                }
                string strErrorMessage = "Could not load " + objFileInfo.FullName + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
                throw e;
            }
            catch (Exception e)
            {
                e = e.Demystify();
                if (objCharacter != null && objExistingCharacter == null)
                {
                    objCharacter.Dispose();
                }
                string strErrorMessage = "Exception while loading " + objFileInfo.FullName + ':';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
                throw e;
            }

            return objCharacter;
        }

        /// <summary>
        /// Validate that a given list of Characters can be successfully loaded.
        /// </summary>
        // ReSharper disable once SuggestBaseTypeForParameter
        public static async Task<Character> LoadCharacterAsync(FileInfo objFileInfo, Character objExistingCharacter = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Character objCharacter = objExistingCharacter;
            try
            {
                Debug.WriteLine("Loading: " + objFileInfo.Name);
                if (objExistingCharacter != null)
                    await objCharacter.ResetCharacterAsync(token).ConfigureAwait(false);
                else
                    objCharacter = new Character();
                await objCharacter.SetFileNameAsync(objFileInfo.FullName, token).ConfigureAwait(false);

                bool blnSuccess = await objCharacter.LoadAsync(token: token).ConfigureAwait(false);
                Assert.IsTrue(blnSuccess);
                Debug.WriteLine("Character loaded: " + await objCharacter.GetNameAsync(token).ConfigureAwait(false) + ", " + objFileInfo.Name);
            }
            catch (AssertFailedException e)
            {
                e = e.Demystify();
                if (objCharacter != null && objExistingCharacter == null)
                {
                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                }
                string strErrorMessage = "Could not load " + objFileInfo.FullName + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
                throw e;
            }
            catch (Exception e)
            {
                e = e.Demystify();
                if (objCharacter != null && objExistingCharacter == null)
                {
                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                }
                string strErrorMessage = "Exception while loading " + objFileInfo.FullName + ':';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
                throw e;
            }

            return objCharacter;
        }
    }
}
