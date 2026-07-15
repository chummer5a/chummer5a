# XPath-Like Condition System

Allowlisted, typed condition predicates for improvements. Custom data can use these without code changes for listed properties; new predicates require a small typed addition in `ImprovementManager`.

## Syntax

### XPath-like

```xml
<condition>/character/created</condition>
<condition>/spell/alchemical</condition>
<condition>/spell/range = "Touch"</condition>
<condition>/spell/name != "Fireball"</condition>
<condition>not(/spell/alchemical)</condition>
<condition>/spell/alchemical = false and /spell/range = "Touch"</condition>
```

### `@property` (same allowlist, type inferred from the evaluation target)

```xml
<condition>@alchemical</condition>
<condition>@alchemical = false</condition>
<condition>not(@alchemical)</condition>
```

### Legacy (still supported)

```xml
<condition>career</condition>
<condition>create</condition>
<condition>Pistols</condition>  <!-- skill specialization name -->
```

## Supported properties (allowlist)

### Character (`/character/...`)

| Property | Type |
|---|---|
| `created` | bool (career mode) |
| `name` | string |
| `metatype` | string |
| `metavariant` | string |

### Spell (`/spell/...`)

| Property | Type |
|---|---|
| `alchemical` | bool |
| `name` | string |
| `category` | string |
| `type` | string |
| `range` | string |
| `extended` | bool |
| `limited` | bool |

### Skill (`/skill/...`)

| Property | Type |
|---|---|
| `name` | string |
| `category` / `skillcategory` | string |
| `attribute` | string |
| `rating` | int |

### Skill group (`/skillgroup/...`)

| Property | Type |
|---|---|
| `name` | string |
| `rating` | int |

Unknown `/type/property` keys evaluate as **false**. Wrong object type for the path also fails the leaf (composites follow normal and/or/not rules). Completely unrecognized non-XPath leaves fall back to legacy handling, then default to **true** for backward compatibility with specialization-style conditions.

## Operators

`=`, `!=`, `>`, `<`, `>=`, `<=`, `contains` (case-insensitive substring).

See `ComparisonOperators.md` for examples.

## Adding a property for custom creators

1. Add a `case` in `TryGetConditionValue` and `TryGetConditionValueAsync` (typed getters only — never `Task.Result` / reflection invoke).
2. Document it in this file.

## Examples

```xml
<!-- Power focus dice: not for alchemical preparations -->
<spellcategory>
  <name>Combat</name>
  <val>Rating</val>
  <condition>not(/spell/alchemical)</condition>
</spellcategory>

<!-- Career-only karma cost tweak -->
<skillcategorykarmacost>
  <name>Academic</name>
  <val>-1</val>
  <min>3</min>
  <condition>/character/created</condition>
</skillcategorykarmacost>
```
