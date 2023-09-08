using System.Diagnostics.CodeAnalysis;

namespace Chummer.Tests.AccessChummerState;

public class ChummerTestFixture: SetupStaThreadFixture
{
    public ChummerTestFixture()
    {
        Utils.IsUnitTest = true;

        var applicationDirectoryPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        if (applicationDirectoryPath is null)
        {
            Assert.Fail("Application Path could not be fetched");
        }

        var testFilePath = Path.Combine(applicationDirectoryPath, "TestFiles");
        var testFilesDirectory = new DirectoryInfo(testFilePath);

        // Clean up old test runs
        foreach (var oldDirectory in testFilesDirectory.GetDirectories(nameof(ChummerTests) + "-TestRun-*"))
        {
            Directory.Delete(oldDirectory.FullName, true);
        }

        var testWorkingDirectoryPath = Path.Combine(testFilePath, nameof(ChummerTests) + "-TestRun-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm", GlobalSettings.InvariantCultureInfo));
        WorkingDirectory = Directory.CreateDirectory(testWorkingDirectoryPath);

        // Preload language files for all tests
        var languageFiles = Directory.EnumerateFiles(Path.Combine(Utils.GetStartupPath, "lang"), "*.xml");
        foreach (var languageFile in languageFiles)
        {
            if (languageFile.Contains("data"))
            {
                // we actually want the language names not the files, so we discard all the languageName_data.xml files
                continue;
            }
            LanguageManager.LoadLanguage(Path.GetFileNameWithoutExtension(languageFile));
        }
    }

    public DirectoryInfo WorkingDirectory { get; set; }


    public static bool TryLoadCharacter(FileInfo testCharacterFile, [NotNullWhen(returnValue: true)] out Character? character)
    {
        character = new Character()
        {
            FileName = testCharacterFile.FullName
        };
        var success = character.Load();

        if (!success)
        {
            character.Dispose();
            character = null;
        }

        return success;
    }

    public static bool TrySaveCharacter(Character character, string path)
    {
        character.Save(path, false);
        return true;
    }
}
