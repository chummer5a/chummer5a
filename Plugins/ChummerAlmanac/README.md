# Chummer Almanac

A Chummer5a plugin that adds an "Almanac" tab to character windows.

Planned content:

- Spirit details: attributes, powers, optional powers and dice pools of a character's spirits at their Force.
- Calendar: an in-game calendar with a location setting and sunrise/sunset times.

Current state: skeleton only. The tab shows the plugin version, the character name and the number of spirits and sprites.

## Rules sources

The plugin follows Chummer's order of precedence for rules (see `CONTRIBUTING.md`). Values come from Chummer's own data files, so custom data and optional non-English content apply automatically. Nothing rules-related is hard-coded in the plugin.

## Build

Build with Visual Studio or its MSBuild (not `dotnet build`, which cannot process Chummer's image resources) from the repository root:

```
MSBuild.exe Plugins\ChummerAlmanac\ChummerAlmanac.csproj -restore -p:Configuration=Release -p:SolutionDir=<path to repository>\
```

The build copies `ChummerAlmanac.dll` to `Chummer\bin\<Configuration>\Plugins\ChummerAlmanac\`.

## Install

Copy `ChummerAlmanac.dll` to `<Chummer folder>\Plugins\ChummerAlmanac\`, enable plugins in Chummer's options, tick "Almanac (Spirits & Calendar)" and restart Chummer.
