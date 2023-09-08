namespace Chummer.Tests.AccessChummerState;

// ReSharper disable once ClassNeverInstantiated.Global
public class ChummerUIFixture: SetupStaThreadFixture
{
    /// <summary>
    /// This will be run once before any test in <see cref="ChummerUITests"/> is executed
    /// </summary>
    public ChummerUIFixture()
    {
        var applicationDirectoryPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        if (applicationDirectoryPath is null)
        {
            Assert.Fail("Application Path could not be fetched");
        }

        var testFilePath = Path.Combine(applicationDirectoryPath, "TestFiles");
        var testFilesDirectory = new DirectoryInfo(testFilePath);
        TestCharacterFiles = testFilesDirectory.GetFiles("*.chum5");

        TestCharacters = new List<Character>(TestCharacterFiles.Length);
    }

    /// <summary>
    /// An initially empty list as a cache for all created Charaters
    /// </summary>
    public List<Character> TestCharacters { get; set; }

    /// <summary>
    /// All character files in the test directory
    /// </summary>
    public FileInfo[] TestCharacterFiles { get; set; }
    public IEnumerable<Character> GetTestCharacters()
    {
        foreach (var testCharacterFile in TestCharacterFiles)
        {
            var cachedCharacter = TestCharacters.FirstOrDefault(x => x.FileName == testCharacterFile.FullName);
            if (cachedCharacter is null)
            {
                if (ChummerTestFixture.TryLoadCharacter(testCharacterFile, out var character))
                {
                    TestCharacters.Add(character);
                    yield return character;
                }
                continue;
            }

            yield return cachedCharacter;
        }
    }
}
