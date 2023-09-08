# TODO for .NET7 Update
- [ ] `objTelemetry.TrackException(except);` TrackException crashes Chummer for some weird reason
    ```
    Process terminated. System.Private.CoreLib.resources couldn't be found!  Large parts of the BCL won't work!
    at System.Environment.FailFast(System.String)
    at System.Resources.ManifestBasedResourceGroveler.HandleResourceStreamMissing(System.String)
    at System.Resources.ManifestBasedResourceGroveler.GrovelForResourceSet(System.Globalization.CultureInfo, System.Collections.Generic.Dictionary`2<System.String,System.Resources.ResourceSet>, Boolean, Boolean)
    at System.Resources.ResourceManager.InternalGetResourceSet(System.Globalization.CultureInfo, Boolean, Boolean)
    at System.Resources.ResourceManager.GetResourceSet(System.Globalization.CultureInfo, Boolean, Boolean)
    at Chummer.TranslateExceptionTelemetryProcessor.TranslateExceptionMessage(System.Exception, System.Globalization.CultureInfo)
    at Chummer.TranslateExceptionTelemetryProcessor.ModifyItem(Microsoft.ApplicationInsights.Channel.ITelemetry)
    at Chummer.TranslateExceptionTelemetryProcessor.Process(Microsoft.ApplicationInsights.Channel.ITelemetry)
    at Microsoft.ApplicationInsights.Extensibility.Implementation.TelemetryProcessorChain.Process(Microsoft.ApplicationInsights.Channel.ITelemetry)
    at Microsoft.ApplicationInsights.TelemetryClient.Track(Microsoft.ApplicationInsights.Channel.ITelemetry)
    at Microsoft.ApplicationInsights.TelemetryClient.TrackException(Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry)
    at Microsoft.ApplicationInsights.TelemetryClient.TrackException(System.Exception, System.Collections.Generic.IDictionary`2<System.String,System.String>, System.Collections.Generic.IDictionary`2<System.String,Double>)
    at Chummer.Plugins.PluginControl.Initialize()
    at Chummer.Plugins.PluginControl.LoadPlugins(Chummer.CustomActivity)
    at Chummer.Program.Main()
    ```
- [x] Seems like not all the needed dlls are included in the plugin build `Could not load file or assembly 'IdentityModel, Version=6.1.0.0, Culture=neutral, PublicKeyToken=e7877f4675df049f'. The system cannot find the file specified.`
- [x] Update build files of Plugins to copy them to the correct spots
- [ ] Replace obsolete APIs
  - [ ] `LogManager.GetCurrentClassLogger()` was marked obsolete in NLog5.2, this seems to be in preparation for future major changes. Maybe we leave that be for now.
  - [ ] ChummerUpdater replace `WebClient` with the new `HttpClient`
    - The Updater heavily uses `WebClient` which is marked obsolete.
      Sadly, some APIs used don't have direct equivalents in `HttpCLient`.
      So this sounds like a rewrite at some point.
      But it should work without modification.
  - [ ] Chummer.Character replace `new TelemetryClient()` with `new TelemetryClient(TelemetryConfiguration config)` see [this](https://github.com/microsoft/ApplicationInsights-dotnet/issues/1152) Issue
    - Where do we store our telemetry info?
- [ ] Nugget Packages
  - [x] HtmlRenderer.Core and HtmlRenderer.WinForms are technically not supported in .Net 7.
    - They seem to behave well in Chummer. I think we should keep them until they either start screwing around or are replaced by Blazo Components
  - [ ] Reintroduce the NugetMissing messages Delnar pushed some time back, those got cut in the regular upkeep of this branch.
    - Those looked very much auto generated. How?
- [ ] Testing
  - [x] Test new LiveCharts
  - [x] Test Chummer.Tests -> Rewritten as xUnit tests.
    - Could not get the MSTest / xUnit hybrids to run.
    So I rewrote them in xUnit.
    Those never run on the main Thread (and Unit Test should be thread agnostic in any way) so some workarounds got included.
  - [x] Test CrashHandler -> In all the crashes I encountered, while rewriting the unit tests I encountered no problems with this bad boy.
  - [x] Test Translator
  - [x] Test ChummerDataViewer I don't got the AWS keys to test this in depth, but it at least runs. Is it even in use?
  - [ ] Test ChummerUpdater
  - [x] Test TextblockConverter (This was never functional to begin with tbh)
  - [x] Test Chummer.Benchmarks
  - [x] Test SamplePlugin
  - [ ] Test ChummerHub.Client
    - It looks good to me, besides receiving and HTTP302 when trying to log in?
    - [ ] I needed to remove the `SecurityProtocolType.Ssl3`, since it is not supported.
      I'm not certain about the ramifications if this change
  - [x] Test ChummerHub -> this runs, but ARCHON needs to really test it since I got no clue.
    - There aren't actually any real changes done here besides a push from net6 to net7, we could just roll that back

- [x] <Chummer>\bin\Debug\net7.0-windows\Chummer5.dll missing on startup. -> .Net7 changed a bit and passes its dll instead of its executable as a commandline param.

- [x] Investigate all the .Designer ambiguous reference errors <Chummer>\Forms\Character Creation Forms\SelectMetatypePriority.Designer.cs:56082 Ambiguous reference:$this.Icon $this.Icon match
    - These where caused by Microsoft.CodeAnalysis.Analyzers package and seem to be Rider specific. [YouTrack Issue](https://youtrack.jetbrains.com/issue/RIDER-98374)
    But those references aren't actually needed for us, because the analyzers are included in >.Net5
- [ ] While cleaning up the csproj files, the bootstrap parts got removed. Do we need do add them again?
