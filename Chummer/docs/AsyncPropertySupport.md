# Async Method Support in XPath Conditions

This document explains how the XPath-like condition system handles async methods in Chummer5a.

## Overview

The condition evaluation system now properly supports async methods (methods that return `Task<T>`). This allows conditions to work with methods that require asynchronous operations like database lookups, file I/O, or network calls.

## How It Works

### **Async Method Detection**
The system automatically detects async methods by looking for common async method patterns:

```csharp
// Tries multiple async method patterns
string[] patterns = {
    "Get" + propertyName + "Async",    // GetCreatedAsync
    propertyName + "Async",             // CreatedAsync  
    "Is" + propertyName + "Async",      // IsCreatedAsync
    "Has" + propertyName + "Async"      // HasCreatedAsync
};
```

### **Async Method Evaluation**
When an async method is encountered:

1. **Find the Method**: Look for async methods matching common patterns
2. **Invoke the Method**: Call the async method with appropriate parameters
3. **Await the Task**: Wait for the async operation to complete
4. **Extract the Result**: Get the actual value from the completed task
5. **Use the Result**: Apply the result to the condition evaluation

```csharp
var task = asyncMethod.Invoke(targetObject, new object[] { token }) as Task;
if (task != null)
{
    await task.ConfigureAwait(false);
    return GetTaskResult(task);  // Extract the actual result
}
```

## Supported Async Property Types

### **Built-in Support**
The system has optimized handling for common types:

- `Task<string>` → String result
- `Task<bool>` → Boolean result  
- `Task<int>` → Integer result
- `Task<decimal>` → Decimal result
- `Task<double>` → Double result
- `Task<float>` → Float result

### **Generic Support**
Any other `Task<T>` type is handled using reflection:

```csharp
// Use reflection to get the Result property for other Task<T> types
var resultProperty = task.GetType().GetProperty("Result");
return resultProperty?.GetValue(task);
```

## Usage Examples

### **Character Async Methods**
```xml
<!-- Character has async method: Task<bool> GetCreatedAsync() -->
<condition>/character/created</condition>

<!-- Character has async method: Task<string> GetNameAsync() -->
<condition>/character/name = "John Doe"</condition>

<!-- Character has async method: Task<bool> IsCreatedAsync() -->
<condition>/character/created</condition>
```

### **Spell Async Methods**
```xml
<!-- Spell has async method: Task<string> GetCategoryAsync() -->
<condition>/spell/category = "Combat"</condition>

<!-- Spell has async method: Task<bool> IsAlchemicalAsync() -->
<condition>/spell/alchemical</condition>

<!-- Spell has async method: Task<string> GetRangeAsync() -->
<condition>/spell/range = "Touch"</condition>
```

### **Skill Async Methods**
```xml
<!-- Skill has async method: Task<int> GetRatingAsync() -->
<condition>/skill/rating > 3</condition>

<!-- Skill has async method: Task<string> GetAttributeAsync() -->
<condition>/skill/attribute = "AGI"</condition>

<!-- Skill has async method: Task<string> GetNameAsync() -->
<condition>/skill/name = "Firearms"</condition>
```

## Performance Considerations

### **Async vs Sync Properties**
- **Sync Properties**: Immediate evaluation, no performance overhead
- **Async Properties**: Requires awaiting, slight performance overhead
- **Mixed Usage**: System handles both seamlessly

### **Caching**
The system doesn't cache async property results, so each condition evaluation will re-execute the async operation. This is by design to ensure fresh data.

## Error Handling

### **Task Exceptions**
If an async property throws an exception:
- The condition evaluation catches the exception
- Returns `true` (allows the improvement) as a safe fallback
- Logs the error for debugging

### **Null Results**
If an async property returns `null`:
- The condition evaluation handles it gracefully
- Returns `false` for boolean conditions
- Uses appropriate fallback logic

## Implementation Details

### **GetTaskResult Method**
```csharp
private static object GetTaskResult(Task task)
{
    // Optimized handling for common types
    if (task is Task<string> stringTask)
        return stringTask.Result;
    if (task is Task<bool> boolTask)
        return boolTask.Result;
    // ... other common types
    
    // Generic handling for any Task<T>
    var resultProperty = task.GetType().GetProperty("Result");
    return resultProperty?.GetValue(task);
}
```

### **Type Safety**
The system maintains type safety by:
- Using proper generic type checking
- Handling type conversion appropriately
- Providing fallback mechanisms for unknown types

## Best Practices

### **When to Use Async Properties**
- **Database Operations**: Properties that query databases
- **File I/O**: Properties that read from files
- **Network Calls**: Properties that make API calls
- **Complex Calculations**: Properties that perform expensive computations

### **When to Use Sync Properties**
- **Simple Properties**: Direct field access
- **Cached Values**: Properties with pre-computed values
- **Performance Critical**: Properties used in tight loops

### **Naming Conventions**
- **Async Properties**: Use `Async` suffix (e.g., `NameAsync`, `RatingAsync`)
- **Clear Intent**: Make it obvious the property is async
- **Consistent**: Follow the same pattern throughout the codebase

## Examples in Practice

### **Character Creation Check**
```xml
<!-- Check if character is in creation mode (async property) -->
<condition>/character/iscreationmodeasync = false</condition>
```

### **Skill Rating Comparison**
```xml
<!-- Check if skill rating is above threshold (async property) -->
<condition>/skill/ratingasync > 5</condition>
```

### **Spell Category Filtering**
```xml
<!-- Filter spells by category (async property) -->
<condition>/spell/categoryasync = "Combat"</condition>
```

### **Complex Async Conditions**
```xml
<!-- Multiple async properties in one condition -->
<condition>/character/iscreationmodeasync = false and /skill/ratingasync > 3</condition>
```

## Troubleshooting

### **Common Issues**
1. **Task Not Awaited**: Make sure the property is properly awaited
2. **Type Mismatch**: Ensure the async property returns the expected type
3. **Null Results**: Handle cases where async properties might return null
4. **Performance**: Consider caching for frequently accessed async properties

### **Debugging**
- Use `Utils.BreakIfDebug()` to break into debugger
- Check logs for async property evaluation errors
- Verify that async properties are properly implemented
- Test with both sync and async properties

## Future Enhancements

### **Potential Improvements**
- **Caching**: Add optional caching for async properties
- **Timeout**: Add timeout handling for long-running async operations
- **Cancellation**: Better cancellation token support
- **Batch Evaluation**: Optimize multiple async property evaluations

The async property support makes the XPath-like condition system much more powerful and flexible! 🚀
