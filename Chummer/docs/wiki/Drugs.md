# drugs.xml and drugcomponents.xml

drugs.xml (and custom_drugs.xml, see [Custom Data Files](Custom-Data-Files)) contains premade drugs and drug grades. drugcomponents.xml (and custom_drugcomponents.xml) contains the components used to build custom drugs.

`DrugsData` only names these files and looks up nodes. It does not define extra fields. Catalog lookup is `/chummer/drugs/drug` by id, then by name. A catalog id that is missing from drugs.xml is loaded from a `drugs/drug` entry in drugcomponents.xml when one exists.

# Structure

```xml
<chummer>
  <grades>
    <grade />
  </grades>
  <drugs>
    <drug />
  </drugs>
</chummer>
```

drugcomponents.xml:

```xml
<chummer>
  <categories>
    <category />
  </categories>
  <drugcomponents>
    <drugcomponent />
  </drugcomponents>
</chummer>
```

[**grade**](#grade "grade") nodes describe drug grades.

[**drug**](#drug "drug") nodes describe premade drugs.

[**drugcomponent**](#drugcomponent "drugcomponent") nodes describe custom-drug components.

## <a id="grade"></a>Grade Node

Grades are loaded by the same grade loader used for cyberware, with the drug-specific fields below. Essence and device rating are not read for drug grades.

**id** (required): GUID. Grades are matched by this id when a source id is known, otherwise by `name`.

**name** (required): the name of the Grade, such as _Standard_ or _Pharmaceutical_.

**cost** (optional): decimal cost multiplier. When omitted, the multiplier stays `0`.

**addictionthreshold** (optional): integer added to the drug's addiction threshold for this grade. Read only when the grade source is a drug.

**avail** (optional): integer Availability adjustment. The stock drug grades omit this field.

**source** (optional): the code for the Sourcebook that this entry comes from. See [books.xml](Books "books.xml").

## <a id="drug"></a>Drug Node

**id** (required): GUID stored as the drug's source id.

**name** (required): the name of the drug.

**category** (optional): category name. The value `Custom Drug` is rewritten to `Custom Drugs`.

**source** / **page** (optional): book reference. See [books.xml](Books "books.xml").

**availability** (optional): Availability string. When `availability` is absent, `avail` is read instead.

**cost** (optional): decimal cost. This is not a formula field.

**rating** (optional): integer addiction rating. A rating chosen in the selection window replaces this value when it is greater than `0`.

**threshold** (optional): integer addiction threshold before the grade adjustment.

**speed** (optional): integer onset speed. When this element is present, that integer is used as the speed and the default is not added. When it is omitted, speed is `9` plus any speed values on attached component effects.

**duration** (optional): duration amount. When the text needs calculation, attribute placeholders are processed and the result is evaluated as a number. Component duration bonuses and the `DrugDuration` and `DrugDurationMultiplier` improvements are applied after that. The stock entries use expressions such as `(6-{BOD})*3600`.

**timescale** is not read when a catalog drug is added, so the displayed unit stays Instant. A saved drug does read `timescale`. Accepted values are `Instant` or `Immediate`, `Second` or `Seconds`, `CombatTurn` or `CombatTurns`, `Minute` or `Minutes`, `Hour` or `Hours`, and `Day` or `Days`. Any other text is Instant.

**bonus** (optional): effect block for this premade drug. When `bonus` is present, a drug component with the same id is not attached. When `bonus` is absent and drugcomponents.xml has a component with the same id, that component is attached and supplies the effects. See [**Catalog Bonus**](#catalog_bonus "Catalog Bonus").

**required** / **forbidden** (optional): selection rules evaluated by the shared requirements check. See [Conditions](Conditions).

**hide** / **ignoresourcedisabled** (optional): sourcebook-filter visibility. An empty `hide` element removes the drug from filtered selection lists.

The element `vectors` appears in the stock file and is not read.

## <a id="catalog_bonus"></a>Catalog Bonus

`DrugBonusCompiler` turns a catalog `bonus` into an Improvement Manager bonus. These children are recognized:

**attribute**: `name` is the attribute short name, such as `REA`. `value` is the modifier. `val` is accepted when `value` is absent. This becomes a `specificattribute` improvement.

**limit**: `name` must be `Physical`, `Mental`, or `Social`. `value` is the modifier. These become `physicallimit`, `mentallimit`, and `sociallimit`. Any other limit name is dropped.

**initiative** / **initiativedice**: copied through as improvement elements.

**quality**: the text is a quality name from [qualities.xml](Qualities "qualities.xml"). Optional attributes `rating`, `select`, and `forced` are kept. When `rating` is missing, custom-component compilation uses `1`. Quality nodes are applied as qualities rather than left inside the bonus.

**specificattribute**, **physicallimit**, **mentallimit**, and **sociallimit** are copied through. Any other child element is copied through unchanged.

## <a id="drugcomponent"></a>Drug Component Node

**id** (required): GUID. A component whose id matches a catalog drug is eligible to be attached to that drug when the catalog entry has no `bonus`.

**name** (required): the name of the component.

**category** (optional): must match a `categories/category` entry when you want it grouped in the custom-drug builder. The saved category `Custom Drug` is rewritten to `Custom Drugs`.

**availability** (optional): Availability string. Defaults to `0` when omitted.

**cost** (optional): cost string.

**level** (optional): integer level selected for this component. Defaults to `0`.

**limit** (optional): integer maximum level. Defaults to `1` when omitted.

**rating** (optional): integer addiction rating contributed by the component.

**threshold** (optional): integer addiction threshold contributed by the component.

**source** / **page** (optional): book reference. See [books.xml](Books "books.xml").

**effects** (optional): one or more `effect` children. See [**Effect**](#effect "Effect").

## <a id="effect"></a>Effect Node

**level** (optional): integer level at which this effect applies. The component's active effect is the one for its current level.

**attribute**: `name` plus integer `value`. Stored as an attribute modifier.

**limit**: `name` plus integer `value`. Stored as a limit modifier. Custom-drug compilation only emits `Physical`, `Mental`, and `Social`.

**quality**: inner XML of a quality reference. `rating`, `select`, and `forced` attributes are preserved when the custom drug is compiled. A missing `rating` becomes `1`.

**specificskill**: copied onto the compiled bonus as-is.

**info**: text shown with the drug. The element's text is stored.

**initiative** / **initiativedice** / **crashdamage** / **speed** / **duration** (optional): integer text. `duration` and `speed` are added to the drug totals. `crashdamage` is summed from components only; catalog bonus nodes do not set crash damage.

An effect child with any other name is ignored and a warning is logged.
