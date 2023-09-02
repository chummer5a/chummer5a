# TODO for .NET7 Update
- [ ] Replace obsolete APIs
  - [ ] `LogManager.GetCurrentClassLogger()` was marked obsolete in NLog5.2, this seems to be in preparation for future major changes. Maybe we leave that be for now.
  - [ ] ChummerUpdater replace `WebClient` with the new `HttpClient`
  - [ ] Chummer.Character replace `new TelemetryClient()` with `new TelemetryClient(TelemetryConfiguration config)` see [this](https://github.com/microsoft/ApplicationInsights-dotnet/issues/1152) Issue
    - Where do we store our telemetry info?
- [ ] Nugget Packages
  - [ ] HtmlRenderer.Core and HtmlRenderer.WinForms are technically not supported in .Net 7, but for use they seem to work for us.
  - [ ] Reintroduce the NugetMissing messages Delnar pushed some time back, those got cut in the regular upkeep of this branch.
- [ ] Testing
  - [ ] Test new LiveCharts
  - [ ] Test Chummer.Tests
    - [ ] Unit Tests are not working at all, xUnit runs maybe use that?
  - [ ] Test CrashHandler
  - [ ] Test Translator
  - [ ] Test ChummerDataViewer
  - [ ] Test ChummerUpdater
    - The Updater heavily uses `WebClient` which is marked obsolete. Sadly, some APIs used don't have direct equivalents in `HttpCLient`.
  - [x] Test TextblockConverter (This was never functional to begin with tbh)
  - [x] Test Chummer.Benchmarks
  - [ ] Test SamplePlugin
  - [ ] Test ChummerHub.Client
  - [ ] Test ChummerHub
    - There aren't actually any real changes done here besides a push from net6 to net7, we could just roll that back

- [x] <Chummer>\bin\Debug\net7.0-windows\Chummer5.dll missing on startup.

- [x] Investigate all the .Designer ambiguous reference errors <Chummer>\Forms\Character Creation Forms\SelectMetatypePriority.Designer.cs:56082 Ambiguous reference:$this.Icon $this.Icon match
    - These where caused by Microsoft.CodeAnalysis.Analyzers package and seem to be Rider specific. I commented them out for now [YouTrack Issue](https://youtrack.jetbrains.com/issue/RIDER-98374)
- [ ] I removed all the bootstrap stuff from the csproj. I think we don't need them, but I'm not sure
