# TODO for .NET6 Update
- [ ] Test the SplitButton Rework
- [ ] Text new LiveCharts
- [ ] Text Chummer.Tests
- [ ] Test CrashHandler
- [ ] Test Translator
- [ ] Test ChummerDataViewer
- [ ] Investigate this error
```
  File name: 'F:\Programming\chummer5a\Chummer.Tests\$(_TestFrameworkExtensionsRoot)Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll'
  at Microsoft.Win32.SafeHandles.SafeFileHandle.CreateFile(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
  at Microsoft.Win32.SafeHandles.SafeFileHandle.Open(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)
  at System.IO.Strategies.OSFileStreamStrategy..ctor(String path, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)
  at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share)
  at Microsoft.UpgradeAssistant.Services.AssemblyHelper.GetAssemblyAttributeValue(String assemblyPath, String attributeType) in D:\a\_work\1\s\src\Experiments\UpgradeAssistant\engine\Services\Project\AssemblyHelper.cs:line 18
  at Microsoft.UpgradeAssistant.Cli.Slices.Services.Project.ProjectService.GetReferenceTargetFrameworksAsync(String referencePath, CancellationToken cancellationToken) in D:\a\_work\1\s\src\Experiments\UpgradeAssistant\cli\Slices\Services\Project\ProjectService.cs:line 329
  at Microsoft.UpgradeAssistant.ProjectExtensions.IsCompatibleReferenceAsync(IProject project, OperationContext context, String referencePath, CancellationToken cancellationToken) in D:\a\_work\1\s\src\Experiments\UpgradeAssistant\engine\Extensions\ProjectExtensions.cs:line 104
  at Microsoft.UpgradeAssistant.Transformers.AssemblyReferenceInplaceTransformer.RunAsync(OperationContext context, SliceNode node, CancellationToken cancellationToken) in D:\a\_work\1\s\src\Experiments\UpgradeAssistant\engine\Transformers\Assembly\AssemblyReferenceInplaceTransformer.cs:line 52
  at Microsoft.UpgradeAssistant.Operations.Operation.RunTransformerAsync(OperationContext context, SliceNode node, Lazy`2 transformer, TelemetryEvent telemetryEvent, OperationLogger transformerLogger, CancellationToken cancellationToken) in D:\a\_work\1\s\src\Experiments\UpgradeAssistant\engine\Operations\Operation.cs:line 386
  at Microsoft.UpgradeAssistant.Operations.Operation.RunNodeTransformersAsync(OperationContext context, SliceNode node, SliceNodeStats nodeStats, CancellationToken cancellationToken) in D:\a\_work\1\s\src\Experiments\UpgradeAssistant\engine\Operations\Operation.cs:line 272
  info: Done
  Failed
```
- [ ] Test ChummerUpdater (especially the User-Agent Header)
- [ ] Test TextblockConverter
- [ ] System.Drawing.Icon may or may not need to replaced by System.Byte[] in CrashReporter.cs https://stackoverflow.com/questions/48068163/why-is-my-resx-file-generating-system-drawing-icon-for-upgraded-netstandard-proj
- [ ] Test Chummer.Benchmarks
- [ ] Test SamplePlugin
- [ ] Test ChummerHub.Client (Ohh god no)
- [ ] Debugger won't attach
- [ ] F:\Programming\chummer5a\Chummer\bin\Debug\net6.0-windows\Chummer5.dll missing on startup, but it is presen???
- [ ] Catastrophic failure on startup The error code is E_UNEXPECTED, or 0x8000FFFF.
- [ ] Fix crash on split button click
- [ ] Investigate all the .Designer ambigous reference errors   <Chummer>\Forms\Character Creation Forms\SelectMetatypePriority.Designer.cs:56082 Ambiguous reference:$this.Icon $this.Icon match
