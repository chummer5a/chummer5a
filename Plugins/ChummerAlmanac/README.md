# Chummer Almanac

A Chummer5a plugin that adds an "Almanac" tab to character windows.

Planned content:

- Spirit details: attributes, powers, optional powers and dice pools of a character's spirits at their Force.
- Calendar: an in-game calendar with a location setting and sunrise/sunset times.

Current state: the Spirits sub-tab lists the character's spirits and sprites and shows attributes, initiative, powers, weaknesses and skill dice pools of the selected one at its Force. Optional powers can be chosen per spirit; the choice is stored in the character's save file. The calendar is not implemented yet.

## Rules sources

The plugin follows Chummer's order of precedence for rules (see `CONTRIBUTING.md`). Attributes, initiative, powers, optional powers and skills come from Chummer's own data files (`traditions.xml`, `streams.xml`; Edge, Magic and Essence from `critters.xml` where the former lack them), so custom data and optional non-English content apply automatically.

Two rules from SR5 p. 303 are not part of those data files and are therefore set in code: spirits roll 2D6 for Initiative and (Force x 2) + 3D6 for Astral Initiative, and a spirit gets one optional power for every 3 full points of Force.

## Build

Build with Visual Studio or its MSBuild (not `dotnet build`, which cannot process Chummer's image resources) from the repository root:

```
MSBuild.exe Plugins\ChummerAlmanac\ChummerAlmanac.csproj -restore -p:Configuration=Release -p:SolutionDir=<path to repository>\
```

The build copies `ChummerAlmanac.dll` to `Chummer\bin\<Configuration>\Plugins\ChummerAlmanac\`.

## Install

Copy `ChummerAlmanac.dll` to `<Chummer folder>\Plugins\ChummerAlmanac\`, enable plugins in Chummer's options, tick "Almanac (Spirits & Calendar)" and restart Chummer.
