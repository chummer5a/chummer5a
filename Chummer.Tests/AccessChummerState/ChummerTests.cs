using System.Globalization;
using System.Xml.Schema;
using Org.XmlUnit.Builder;
using Org.XmlUnit.Diff;
using Xunit.Abstractions;

namespace Chummer.Tests.xUnit.AccessChummerState;

[Collection("AccessChummerState")]
public class ChummerTests: IClassFixture<ChummerTestFixture>
{
    private readonly ChummerTestFixture _testFixture;
    private readonly ITestOutputHelper _testOutput;

    public ChummerTests(ChummerTestFixture testFixture, ITestOutputHelper testOutput)
    {
        _testFixture = testFixture;
        _testOutput = testOutput;
    }

    [StaTheory]
    [MemberData(nameof(GetTestCharacterFiles))]
    public void Should_Load_And_Save_Character_As_Chum5lz(FileInfo testCharacterFile)
    {
        Utils.CreateSynchronizationContext();
        Assert.True(ChummerTestFixture.TryLoadCharacter(testCharacterFile, out var character));

        var oldCompressionPreset = GlobalSettings.Chum5lzCompressionLevel;
        try
        {
            var saveFilePath = Path.Combine(_testFixture.WorkingDirectory.FullName, testCharacterFile.Name);
            if (!saveFilePath.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
            {
                if (saveFilePath.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                    saveFilePath += "lz";
                else
                    saveFilePath += ".chum5lz";
            }
            Assert.True(ChummerTestFixture.TrySaveCharacter(character, saveFilePath));
            Assert.True(ChummerTestFixture.TryLoadCharacter(new FileInfo(saveFilePath), out var reloadedCharacter));
            reloadedCharacter.Dispose();
        }
        finally
        {
            GlobalSettings.Chum5lzCompressionLevel = oldCompressionPreset;
        }
    }

    [StaTheory]
    [MemberData(nameof(GetTestCharacterFiles))]
    public void Load_And_Save_Is_Deterministic(FileInfo testCharacterFile)
    {
        Assert.True(ChummerTestFixture.TryLoadCharacter(testCharacterFile, out var character));
        var firstSavePath = Path.Combine(_testFixture.WorkingDirectory.FullName, "First_Save_" + testCharacterFile.Name);
        Assert.True(ChummerTestFixture.TrySaveCharacter(character, firstSavePath));

        // Load and Save it again
        Assert.True(ChummerTestFixture.TryLoadCharacter(new FileInfo(firstSavePath), out character));
        var secondSavePath =
            Path.Combine(_testFixture.WorkingDirectory.FullName, "Second_Save_" + testCharacterFile.Name);
        Assert.True(ChummerTestFixture.TrySaveCharacter(character, secondSavePath));

        // We don't need that anymore
        character.Dispose();

        // Check to see that character after first load cycle is consistent with character after second
        using var firstSaveFileStream =
            File.Open(firstSavePath, FileMode.Open, FileAccess.Read);

        using var secondSaveFileStream =
            File.Open(secondSavePath, FileMode.Open, FileAccess.Read);
        try
        {
            var myDiff = DiffBuilder
                .Compare(firstSaveFileStream)
                .WithTest(secondSaveFileStream)
                .CheckForIdentical()
                .WithNodeFilter(x =>
                    x.Name !=
                    "mugshot") // image loading and unloading is not going to be deterministic due to compression algorithms
                .WithNodeMatcher(
                    new DefaultNodeMatcher(
                        ElementSelectors.Or(
                            ElementSelectors.ByNameAndText,
                            ElementSelectors.ByName)))
                .IgnoreWhitespace()
                .Build();
            foreach (var diff in myDiff.Differences)
            {
                _testOutput.WriteLine(diff.Comparison.ToString());
            }

            Assert.False(myDiff.HasDifferences(), myDiff.ToString());
        }
        catch (XmlSchemaException e)
        {
            Assert.Fail("Unexpected validation failure: " + e.Message);
        }
    }



    [StaTheory]
    [MemberData(nameof(GetTestCharacterFileLanguageProduct))]
    public async Task Load_Then_Print_Async(FileInfo testCharacterFile, FileInfo languageFileInfo)
    {
        _testOutput.WriteLine("Character File: " + testCharacterFile.Name);
        _testOutput.WriteLine("Language File: " + languageFileInfo.Name);
        Assert.True(ChummerTestFixture.TryLoadCharacter(testCharacterFile, out var character));

        var languageName = Path.GetFileNameWithoutExtension(languageFileInfo.FullName);
        var exportPath = Path.Combine(_testFixture.WorkingDirectory.FullName,
            languageName + "_" + testCharacterFile.Name);

        CultureInfo cultureInfo;
        try
        {
            cultureInfo = new CultureInfo(languageName);
        }
        catch (CultureNotFoundException)
        {
            _testOutput.WriteLine("Fallback to invariant culture for " + languageName);
            cultureInfo = CultureInfo.InvariantCulture;
        }

        var xmlDocument = await character.GenerateExportXml(cultureInfo, languageName);

        await using var fileStream = new FileStream(exportPath, FileMode.Create, FileAccess.Write, FileShare.None);
        xmlDocument.Save(fileStream);
    }


    /// <summary>
    /// Gets an object array of all the test character files in a static manner to be used as MemberData
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> GetTestCharacterFiles()
    {
        var applicationDirectoryPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        if (applicationDirectoryPath is null)
        {
            Assert.Fail("Application Path could not be fetched");
        }

        var testFilePath = Path.Combine(applicationDirectoryPath, "TestFiles");
        var testFilesDirectory = new DirectoryInfo(testFilePath);
        var testCharacterFiles = testFilesDirectory.GetFiles("*.chum5");

        foreach (var testCharacterFile in testCharacterFiles)
        {
            yield return new object[] {testCharacterFile};
        }
    }

    public static IEnumerable<object[]> GetTestCharacterFileLanguageProduct()
    {
        var languageFiles = Directory.EnumerateFiles(Path.Combine(Utils.GetStartupPath, "lang"), "*.xml");
        var characterFiles = GetTestCharacterFiles();

        foreach (var languageFile in languageFiles)
        {
            if (languageFile.Contains("data"))
            {
                continue;
            }
            foreach (var characterFile in characterFiles)
            {
                yield return new [] {characterFile.First(), new FileInfo(languageFile)};
            }
        }
    }
}

