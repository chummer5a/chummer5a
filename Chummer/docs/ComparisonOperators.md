# Condition Comparison Operators

Operators for allowlisted improvement conditions (`/type/property` and `@property`). See `XPathConditionSystem.md` for the property allowlist.

## Operators

| Operator | Meaning |
|---|---|
| `=` / `==` | Equality |
| `!=` | Inequality |
| `>` / `>=` / `<` / `<=` | Ordering (numeric or string compare) |
| `contains` | Case-insensitive substring |

Bare property (no operator) is treated as a boolean: `/spell/alchemical` ≡ true when alchemical.

## Examples

```xml
<condition>/spell/range = "Touch"</condition>
<condition>/spell/alchemical != true</condition>
<condition>/character/created = false</condition>
<condition>/skill/rating >= 3</condition>
<condition>/spell/name contains "Fire"</condition>
<condition>@alchemical = false</condition>
<condition>not(/spell/alchemical)</condition>
<condition>/spell/range = "Touch" and /spell/alchemical = false</condition>
<condition>/spell/range = "Touch" or /spell/range = "Line of Sight"</condition>
```

## Type conversion

Expected values are converted to the allowlisted property's CLR type (`bool.Parse`, `int.Parse`, etc.). Quotes around the right-hand side are optional and stripped.

```xml
<condition>/spell/alchemical = "true"</condition>
<condition>/skill/rating >= "2"</condition>
```
