# XPath-Like Condition System

This document describes the XPath-like condition evaluation system for improvements in Chummer5a.

## Overview

The condition system now supports XPath-like syntax that is more intuitive and eliminates the need for multiple type-specific methods. All improvement conditions can now use a single, unified evaluation system.

## Syntax

### XPath-Like Syntax

#### Basic Property Access
```xml
<condition>/character/created</condition>        <!-- Character is in career mode -->
<condition>/spell/alchemical</condition>         <!-- Spell is alchemical -->
<condition>/skill/name</condition>               <!-- Skill has a name -->
```

#### Property Comparisons
```xml
<condition>/spell/range = "Touch"</condition>   <!-- Spell has Touch range -->
<condition>/spell/name != "Fireball"</condition> <!-- Spell is not Fireball -->
<condition>/character/name = "John"</condition> <!-- Character name is John -->
```

#### Complex Conditions
```xml
<condition>not(/spell/alchemical)</condition>   <!-- Spell is not alchemical -->
<condition>/spell/alchemical = false and /spell/range = "Touch"</condition>
```

### Legacy Syntax (Still Supported)

#### Character Conditions
```xml
<condition>career</condition>    <!-- Only during career mode -->
<condition>create</condition>    <!-- Only during character creation -->
```

#### Skill Conditions
```xml
<condition>Pistols</condition>   <!-- Only when skill has Pistols specialization -->
<condition>Combat</condition>    <!-- Only when skill has Combat specialization -->
```

#### Old Property Syntax
```xml
<condition>@alchemical = false</condition>
<condition>@range = "Touch"</condition>
<condition>not(@alchemical)</condition>
```

## Supported Object Types

### Character Properties
- `/character/created` - Boolean indicating if character is in career mode
- `/character/name` - String character name
- `/character/metatype` - String character metatype
- `/character/metavariant` - String character metavariant

### Spell Properties
- `/spell/alchemical` - Boolean indicating if the spell is alchemical
- `/spell/name` - String name of the spell
- `/spell/range` - String range of the spell
- `/spell/category` - String category of the spell
- `/spell/type` - String type of the spell
- `/spell/source` - String source book
- `/spell/page` - String page number
- `/spell/extended` - Boolean indicating if the spell is extended
- `/spell/limited` - Boolean indicating if the spell is limited
- `/spell/barehandedadept` - Boolean indicating if the spell is for bare-handed adepts

### Skill Properties
- `/skill/name` - String skill name
- `/skill/category` - String skill category
- `/skill/attribute` - String linked attribute
- `/skill/rating` - Integer skill rating
- `/skill/karma` - Integer karma spent on skill

### SkillGroup Properties
- `/skillgroup/name` - String skill group name
- `/skillgroup/rating` - Integer skill group rating

### Gear Properties
- `/gear/name` - String gear name
- `/gear/category` - String gear category
- `/gear/rating` - Integer gear rating
- `/gear/avail` - String availability

### Cyberware Properties (includes Bioware)
- `/cyberware/name` - String cyberware name
- `/cyberware/category` - String cyberware category
- `/cyberware/rating` - Integer cyberware rating
- `/cyberware/grade` - String cyberware grade
- `/bioware/name` - String bioware name (same as cyberware)
- `/bioware/category` - String bioware category (same as cyberware)

## Usage Examples

### Spellcasting Focus (Exclude Alchemical)
```xml
<bonus unique="combatspellfocus">
  <spellcategory>
    <name>Combat</name>
    <val>Rating</val>
    <condition>not(/spell/alchemical)</condition>
  </spellcategory>
</bonus>
```

### Touch-Only Focus
```xml
<bonus unique="touchfocus">
  <spellcategory>
    <name>Combat</name>
    <val>Rating</val>
    <condition>/spell/range = "Touch"</condition>
  </spellcategory>
</bonus>
```

### Character Creation Only
```xml
<bonus>
  <attribute>
    <name>BOD</name>
    <val>1</val>
    <condition>/character/created = false</condition>
  </attribute>
</bonus>
```

### Skill Specialization Bonus
```xml
<bonus>
  <skill>
    <name>Firearms</name>
    <val>2</val>
    <condition>Pistols</condition>  <!-- Legacy specialization condition -->
  </skill>
</bonus>
```

### Complex Spell Condition
```xml
<bonus unique="selectivefocus">
  <spellcategory>
    <name>Combat</name>
    <val>Rating</val>
    <condition>/spell/alchemical = false and /spell/range != "Self" and /spell/name != "Fireball"</condition>
  </spellcategory>
</bonus>
```

## API Usage

### Single Unified Method

Instead of multiple type-specific methods, there's now just one:

```csharp
// Async version
await ConditionEvaluator.EvaluateImprovementConditionAsync(improvement, targetObject, token)

// Synchronous version  
ConditionEvaluator.EvaluateImprovementCondition(improvement, targetObject)
```

### Migration from Old System

**Before (Multiple Methods):**
```csharp
// Different methods for different types
await ConditionEvaluator.EvaluateSpellImprovementConditionAsync(improvement, spell, token)
await ConditionEvaluator.EvaluateSkillImprovementConditionAsync(improvement, skill, token)
await ConditionEvaluator.EvaluateCharacterImprovementConditionAsync(improvement, character, token)
```

**After (Single Method):**
```csharp
// One method for all types
await ConditionEvaluator.EvaluateImprovementConditionAsync(improvement, targetObject, token)
```

## Benefits

1. **Unified API**: Single method for all improvement types
2. **Intuitive Syntax**: XPath-like syntax is more readable
3. **Type Safety**: Automatic type checking and conversion
4. **Backward Compatible**: All legacy conditions still work
5. **Extensible**: Easy to add new object types and properties
6. **Performance**: Optimized for common use cases

## Advanced Features

### Logical Operations
```xml
<condition>/spell/alchemical = false and /spell/range = "Touch"</condition>
<condition>/spell/range = "Touch" or /spell/range = "Line of Sight"</condition>
<condition>not(/spell/alchemical)</condition>
```

### Comparison Operators
```xml
<condition>/skill/rating > 3</condition>
<condition>/skill/rating >= 2</condition>
<condition>/skill/rating < 5</condition>
<condition>/skill/rating <= 4</condition>
<condition>/spell/name contains "Fire"</condition>
```

### Complex Conditions
```xml
<condition>not(/spell/alchemical) and (/spell/range = "Touch" or /spell/range = "Line of Sight")</condition>
<condition>/spell/category = "Combat" and /spell/range != "Self" and not(/spell/extended)</condition>
```

## Error Handling

- **Graceful Fallback**: If a condition cannot be evaluated, it defaults to `true` (allows the improvement)
- **Type Safety**: Automatic type conversion with fallback to string comparison
- **Logging**: Errors are logged for debugging purposes
- **Backward Compatibility**: Legacy conditions continue to work unchanged

## Performance

- **Synchronous Evaluation**: Simple conditions are evaluated synchronously for better performance
- **Async Support**: Complex conditions with async properties are handled properly
- **Caching**: Property access is optimized for repeated evaluations
- **Minimal Overhead**: Simple conditions have virtually no performance impact
