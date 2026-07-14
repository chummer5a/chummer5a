# Comparison Operators in XPath Conditions

This document explains how to use comparison operators in the XPath-like condition system.

## Overview

The XPath-like condition system supports various comparison operators for evaluating conditions against object properties. You can use these operators in both XPath syntax and property syntax.

## Supported Operators

### **Equality Operators**
- `=` - Equal to
- `!=` - Not equal to

### **Ordering Operators**
- `>` - Greater than
- `<` - Less than
- `>=` - Greater than or equal to
- `<=` - Less than or equal to

### **String Operators**
- `contains` - String contains (case-insensitive)

## XPath Syntax Examples

### **Basic Comparisons**
```xml
<!-- Equal to -->
<condition>/skill/rating = 3</condition>
<condition>/spell/range = "Touch"</condition>
<condition>/character/name = "John Doe"</condition>

<!-- Not equal to -->
<condition>/spell/alchemical != true</condition>
<condition>/skill/name != "Firearms"</condition>
<condition>/character/created != false</condition>
```

### **Numeric Comparisons**
```xml
<!-- Greater than -->
<condition>/skill/rating > 3</condition>
<condition>/spell/force > 2</condition>

<!-- Less than -->
<condition>/skill/rating < 5</condition>
<condition>/spell/force < 6</condition>

<!-- Greater than or equal -->
<condition>/skill/rating >= 4</condition>
<condition>/spell/force >= 3</condition>

<!-- Less than or equal -->
<condition>/skill/rating <= 6</condition>
<condition>/spell/force <= 5</condition>
```

### **String Comparisons**
```xml
<!-- String contains -->
<condition>/spell/name contains "Fire"</condition>
<condition>/character/name contains "John"</condition>
<condition>/skill/name contains "Combat"</condition>
```

## Property Syntax Examples

### **Using @property syntax**
```xml
<!-- Equal to -->
<condition>@rating = 3</condition>
<condition>@range = "Touch"</condition>
<condition>@name = "John Doe"</condition>

<!-- Not equal to -->
<condition>@alchemical != true</condition>
<condition>@name != "Firearms"</condition>

<!-- Greater than -->
<condition>@rating > 3</condition>
<condition>@force > 2</condition>

<!-- Less than -->
<condition>@rating < 5</condition>
<condition>@force < 6</condition>

<!-- String contains -->
<condition>@name contains "Fire"</condition>
<condition>@name contains "Combat"</condition>
```

## Complex Conditions

### **Multiple Conditions with AND/OR**
```xml
<!-- AND conditions -->
<condition>/skill/rating > 3 and /skill/name contains "Combat"</condition>
<condition>/spell/range = "Touch" and /spell/alchemical = false</condition>

<!-- OR conditions -->
<condition>/skill/rating > 5 or /skill/name = "Firearms"</condition>
<condition>/spell/range = "Touch" or /spell/range = "Line of Sight"</condition>

<!-- Complex combinations -->
<condition>/skill/rating > 3 and (/skill/name contains "Combat" or /skill/name contains "Firearms")</condition>
```

### **Negation with NOT**
```xml
<!-- NOT conditions -->
<condition>not(/spell/alchemical)</condition>
<condition>not(/skill/rating < 3)</condition>
<condition>not(/character/name contains "Test")</condition>
```

## Real-World Examples

### **Spellcasting Focus Conditions**
```xml
<!-- Exclude alchemical spells -->
<condition>not(/spell/alchemical)</condition>

<!-- Only Touch spells -->
<condition>/spell/range = "Touch"</condition>

<!-- Combat spells with Force > 2 -->
<condition>/spell/category = "Combat" and /spell/force > 2</condition>

<!-- Exclude specific spells -->
<condition>/spell/name != "Fireball"</condition>
```

### **Skill Conditions**
```xml
<!-- High-level skills only -->
<condition>/skill/rating > 4</condition>

<!-- Combat-related skills -->
<condition>/skill/name contains "Combat"</condition>

<!-- Specific skill types -->
<condition>/skill/name = "Firearms" or /skill/name = "Blades"</condition>
```

### **Character Conditions**
```xml
<!-- Career mode only -->
<condition>/character/created = true</condition>

<!-- Character creation only -->
<condition>/character/created = false</condition>

<!-- Specific character names -->
<condition>/character/name contains "John"</condition>
```

## Type Handling

### **Automatic Type Conversion**
The system automatically converts string values to the appropriate type:

```xml
<!-- Numeric comparisons -->
<condition>/skill/rating > "3"</condition>  <!-- String "3" converted to int -->
<condition>/spell/force >= "2"</condition>  <!-- String "2" converted to int -->

<!-- Boolean comparisons -->
<condition>/spell/alchemical = "true"</condition>   <!-- String "true" converted to bool -->
<condition>/character/created = "false"</condition> <!-- String "false" converted to bool -->
```

### **String Comparisons**
String comparisons are case-insensitive for the `contains` operator:

```xml
<!-- These are equivalent -->
<condition>/spell/name contains "fire"</condition>
<condition>/spell/name contains "FIRE"</condition>
<condition>/spell/name contains "Fire"</condition>
```

## Performance Considerations

### **Operator Precedence**
Operators are processed in order of specificity:
1. `!=`, `>=`, `<=` (multi-character operators)
2. `=`, `>`, `<` (single-character operators)
3. `contains` (string operator)

### **Type Safety**
- **Numeric Types**: `int`, `decimal`, `double`, `float` support ordering operators
- **String Types**: Support equality and `contains` operators
- **Boolean Types**: Support equality operators only
- **Fallback**: Unknown types fall back to string comparison

## Error Handling

### **Invalid Comparisons**
If a comparison cannot be performed:
- **Type Mismatch**: Returns `false`
- **Invalid Operator**: Falls back to equality comparison
- **Missing Property**: Returns `false`

### **Safe Fallbacks**
```xml
<!-- These will safely return false if comparison fails -->
<condition>/skill/rating > "invalid"</condition>
<condition>/spell/name > 123</condition>
<condition>/character/created contains "text"</condition>
```

## Best Practices

### **Use Appropriate Operators**
```xml
<!-- ✅ Good - Numeric comparison -->
<condition>/skill/rating > 3</condition>

<!-- ✅ Good - String comparison -->
<condition>/spell/name contains "Fire"</condition>

<!-- ✅ Good - Boolean comparison -->
<condition>/spell/alchemical = false</condition>

<!-- ❌ Avoid - String comparison on numeric -->
<condition>/skill/rating contains "3"</condition>
```

### **Combine Conditions Effectively**
```xml
<!-- ✅ Good - Clear and readable -->
<condition>/spell/range = "Touch" and /spell/alchemical = false</condition>

<!-- ✅ Good - Logical grouping -->
<condition>/skill/rating > 3 and (/skill/name contains "Combat" or /skill/name contains "Firearms")</condition>
```

The comparison operators make the XPath-like condition system much more powerful and flexible! 🎯
