using System.ComponentModel;
using System.Windows.Forms;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace Chummer.Tests.AccessChummerState;

[Collection("AccessChummerState")]
public class ChummerUITests: IClassFixture<ChummerUIFixture>
{
    private readonly ChummerUIFixture _testFixture;
    private readonly ITestOutputHelper _testOutput;

    public ChummerUITests(ChummerUIFixture testFixture, ITestOutputHelper testOutput)
    {
        _testFixture = testFixture;
        _testOutput = testOutput;
    }

    [StaFact]
    public async Task Should_Load_Content_Async()
    {
        using var loadingBar = await Program.MainForm.DoThreadSafeFuncAsync(() =>
            Program.CreateAndShowProgressBar(Application.ProductName, Utils.BasicDataFileNames.Count));

        var cachingTasks = new List<Task>(Utils.MaxParallelBatchSize);

        foreach (var dataFileName in Utils.BasicDataFileNames)
        {
            cachingTasks.Add(CacheCommonFile(dataFileName, loadingBar));

            if (cachingTasks.Count != Utils.MaxParallelBatchSize) continue;

            var completedTask = await Task.WhenAny(cachingTasks);
            cachingTasks.Remove(completedTask);
        }

        await Task.WhenAll(cachingTasks);

        return;

        async Task CacheCommonFile(string dataFileName, ThreadSafeForm<LoadingBar> threadSafeForm)
        {
            _testOutput.WriteLine("Loading: " + dataFileName);

            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                await XmlManager.LoadXPathAsync(dataFileName, null, GlobalSettings.DefaultLanguage);
            }

            await XmlManager.LoadXPathAsync(dataFileName);

            await threadSafeForm.MyForm.PerformStepAsync(
                Application.ProductName,
                LoadingBar.ProgressBarTextPatterns.Initializing);
        }
    }

    [StaTheory]
    [MemberData(nameof(GetTestCharacterFiles))]
    public async Task Theory_Should_Load_Character_Forms_Async(FileInfo testCharacterFile)
    {
        Assert.True(ChummerTestFixture.TryLoadCharacter(testCharacterFile, out var testCharacter));

        CharacterShared? characterForm = null;
        try
        {
            characterForm = await GetCharacterFormAsync(testCharacter);

            _testOutput.WriteLine("Opening Character Form");
            await characterForm.DoThreadSafeAsync(x =>
            {
                x.MdiParent = Program.MainForm;
                x.ShowInTaskbar = false;
                x.CreateControl();
            });
        }
        catch (Win32Exception e)
        {
            _testOutput.WriteLine("Swallowed a non critical exception");
            _testOutput.WriteLine(e.Message);
        }
        finally
        {
            _testOutput.WriteLine("Closing Character Form");
            Assert.True(characterForm is not null);
            await characterForm.DoThreadSafeAsync(() => characterForm.Close());
        }


    }

    public static IEnumerable<object[]> GetTestCharacterFiles()
    {
        return ChummerTests.GetTestCharacterFiles();
    }

    private async Task<CharacterShared> GetCharacterFormAsync(Character testCharacter)
    {
        _testOutput.WriteLine("Creating Character Form: " + testCharacter.FileName);
        var characterForm = await Program.MainForm.DoThreadSafeFuncAsync(() =>
            testCharacter.Created
                ?   (CharacterShared)  new CharacterCareer(testCharacter)
                :   new CharacterCreate(testCharacter)
        );

        return characterForm;
    }


}
