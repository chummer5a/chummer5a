# Chummer Almanac

A Chummer5a plugin that adds an "Almanac" tab to character windows.

The tab has two parts:

- **Spirits**: lists the character's spirits and sprites and shows attributes, initiative, powers, weaknesses and skill dice pools of the selected one at its Force. Optional powers can be chosen per spirit. If the time of summoning is entered, the tab shows when the services of an unbound spirit expire.
- **Calendar**: the current in-game date and time, a month view, a location (a list of cities or free coordinates with a time zone), sunrise and sunset, the phase of the moon and a note for each day.

Everything the user enters is stored in the character's save file, inside the plugin's own element.

## Rules sources

The plugin follows Chummer's order of precedence for rules (see `CONTRIBUTING.md`). Attributes, initiative, powers, optional powers and skills come from Chummer's own data files (`traditions.xml`, `streams.xml`; Edge, Magic and Essence from `critters.xml` where the former lack them), so custom data and optional non-English content apply automatically.

Three rules are not part of those data files and are therefore set in code:

- Spirits roll 2D6 for Initiative and (Force x 2) + 3D6 for Astral Initiative (SR5 p. 303).
- A spirit gets one optional power for every 3 full points of Force (SR5 p. 303).
- The services of an unbound spirit expire at the next sunrise or sunset, whichever comes first (SR5 core rulebook, section "Spirit Services").

## Sun and moon

Sunrise and sunset are calculated offline with the NOAA solar calculator formulas (after Jean Meeus, "Astronomical Algorithms") and are accurate to about a minute outside the polar regions. Daylight saving time follows the rules Windows has for the chosen time zone. The phase of the moon is an approximation that can be off by about half a day.

## Build

Build with Visual Studio or its MSBuild (not `dotnet build`, which cannot process Chummer's image resources) from the repository root:

```
MSBuild.exe Plugins\ChummerAlmanac\ChummerAlmanac.csproj -restore -p:Configuration=Release -p:SolutionDir=<path to repository>\
```

The build copies `ChummerAlmanac.dll` to `Chummer\bin\<Configuration>\Plugins\ChummerAlmanac\`.

## Install

Copy `ChummerAlmanac.dll` to `<Chummer folder>\Plugins\ChummerAlmanac\`, enable plugins in Chummer's options, tick "Almanac (Spirits & Calendar)" and restart Chummer.
