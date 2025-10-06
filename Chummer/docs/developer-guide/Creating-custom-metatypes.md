# Creating Custom Metatypes

This guide explains how to create custom metatypes in Chummer5a.

## Overview

Metatypes in Chummer5a are defined in the `metatypes.xml` file and control the base attributes and characteristics of character races.

## File Location

The metatypes data is stored in:
- **File**: `Chummer/data/metatypes.xml`
- **Schema**: `Chummer/data/metatypes.xsd`

## Basic Structure

Here's the basic structure for a metatype entry:

```xml
<metatype>
  <id>unique-guid-here</id>
  <name>Metatype Name</name>
  <body>3</body>
  <agility>3</agility>
  <reaction>3</reaction>
  <strength>3</strength>
  <willpower>3</willpower>
  <logic>3</logic>
  <intuition>3</intuition>
  <charisma>3</charisma>
  <edge>3</edge>
  <source>SR5</source>
  <page>123</page>
</metatype>
```

## Required Fields

### Core Attributes
- **id**: Unique GUID for the metatype
- **name**: Display name of the metatype
- **body**: Base Body attribute
- **agility**: Base Agility attribute
- **reaction**: Base Reaction attribute
- **strength**: Base Strength attribute
- **willpower**: Base Willpower attribute
- **logic**: Base Logic attribute
- **intuition**: Base Intuition attribute
- **charisma**: Base Charisma attribute
- **edge**: Base Edge attribute

### Source Information
- **source**: Source book abbreviation (e.g., "SR5", "CF", "RF")
- **page**: Page number in the source book

## Optional Fields

### Physical Characteristics
- **height**: Average height range
- **weight**: Average weight range
- **lifespan**: Average lifespan

### Special Abilities
- **special**: Special abilities or traits
- **notes**: Additional notes or restrictions

### Availability
- **avail**: Availability rating
- **cost**: Cost in nuyen (if applicable)

## Example

```xml
<metatype>
  <id>12345678-1234-1234-1234-123456789abc</id>
  <name>Custom Metatype</name>
  <body>4</body>
  <agility>3</agility>
  <reaction>3</reaction>
  <strength>4</strength>
  <willpower>3</willpower>
  <logic>2</logic>
  <intuition>3</intuition>
  <charisma>2</charisma>
  <edge>3</edge>
  <source>Custom</source>
  <page>1</page>
  <special>Natural armor +1</special>
  <notes>This is a custom metatype for homebrew campaigns</notes>
</metatype>
```

## Step-by-Step Process

### 1. Plan Your Metatype
- Decide on base attributes
- Determine any special abilities
- Consider balance implications
- Document source and page references

### 2. Generate GUID
- Use a GUID generator to create a unique identifier
- Ensure the GUID is properly formatted
- Never reuse existing GUIDs

### 3. Add to XML File
- Open `Chummer/data/metatypes.xml`
- Add your new metatype entry
- Follow the existing structure and formatting
- Validate the XML syntax

### 4. Update Project File
- Open `Chummer.sln` in Visual Studio
- Ensure the metatypes.xml file is included
- Set "Copy to Output Directory" to "Copy if newer"

### 5. Test Your Metatype
- Build and run Chummer5a
- Create a new character
- Verify your metatype appears in the list
- Test character creation with your metatype
- Verify all attributes and abilities work correctly

## Best Practices

### Attribute Balance
- Keep base attributes reasonable (typically 2-4)
- Consider the impact on character creation
- Balance against existing metatypes
- Test with different character concepts

### Naming Conventions
- Use clear, descriptive names
- Follow existing naming patterns
- Avoid special characters or symbols
- Keep names concise but informative

### Documentation
- Include source information
- Add notes for special abilities
- Document any restrictions or requirements
- Keep track of changes and updates

### Testing
- Test with various character builds
- Verify attribute calculations
- Check special abilities
- Ensure compatibility with other systems

## Common Issues

### XML Validation Errors
- Check that all required fields are present
- Verify XML syntax is correct
- Ensure GUIDs are properly formatted
- Validate against the XSD schema

### Metatype Not Appearing
- Verify the file is included in the project
- Check that "Copy to Output Directory" is set correctly
- Ensure the XML structure matches the schema
- Verify the GUID is unique

### Attribute Calculation Issues
- Check that all attributes are properly defined
- Verify the XML structure is correct
- Ensure no conflicting entries exist
- Test with different character builds

## Advanced Features

### Special Abilities
You can add special abilities to metatypes:

```xml
<special>Natural armor +1, Low-light vision</special>
```

### Restrictions
You can add restrictions or requirements:

```xml
<notes>Requires GM approval, not suitable for all campaigns</notes>
```

### Source Integration
Ensure proper source attribution:

```xml
<source>Custom Campaign</source>
<page>Homebrew Rules</page>
```

## Resources

- [GUID Generator](https://guidgenerator.com/)
- [XML Schema Documentation](https://www.w3.org/XML/Schema)
- [Chummer5a Data Structure Guide](../wiki/README.md)
- [Shadowrun 5th Edition Core Rulebook](https://www.shadowruntabletop.com/)
