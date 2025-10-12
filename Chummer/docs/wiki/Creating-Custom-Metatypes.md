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
  <karma>0</karma>
  <category>Metahuman</category>
  <bodmin>1</bodmin>
  <bodmax>6</bodmax>
  <bodaug>10</bodaug>
  <agimin>1</agimin>
  <agimax>6</agimax>
  <agiaug>10</agiaug>
  <reamin>1</reamin>
  <reamax>6</reamax>
  <reaaug>10</reaaug>
  <strmin>1</strmin>
  <strmax>6</strmax>
  <straug>10</straug>
  <chamin>1</chamin>
  <chamax>6</chamax>
  <chaaug>10</chaaug>
  <intmin>1</intmin>
  <intmax>6</intmax>
  <intaug>10</intaug>
  <logmin>1</logmin>
  <logmax>6</logmax>
  <logaug>10</logaug>
  <wilmin>1</wilmin>
  <wilmax>6</wilmax>
  <wilaug>10</wilaug>
  <edgmin>1</edgmin>
  <edgmax>6</edgmax>
  <edgaug>10</edgaug>
  <source>SR5</source>
  <page>123</page>
</metatype>
```

## Required Fields

### Core Attributes
- **id**: Unique GUID for the metatype
- **name**: Display name of the metatype
- **karma**: Karma cost for the metatype (0 for free)
- **category**: Category (Metahuman, Metavariant, Metasapient, Shapeshifter)

### Attribute Limits
For each attribute (Body, Agility, Reaction, Strength, Charisma, Intuition, Logic, Willpower, Edge):
- **[attr]min**: Minimum attribute value
- **[attr]max**: Maximum attribute value  
- **[attr]aug**: Maximum augmented attribute value

### Source Information
- **source**: Source book abbreviation (e.g., "SR5", "CF", "RF")
- **page**: Page number in the source book

## Example

Here's a valid example of a custom metatype:

```xml
<metatype>
  <id>12345678-1234-1234-1234-123456789abc</id>
  <name>Custom Metatype</name>
  <karma>0</karma>
  <category>Metahuman</category>
  <bodmin>2</bodmin>
  <bodmax>7</bodmax>
  <bodaug>10</bodaug>
  <agimin>1</agimin>
  <agimax>6</agimax>
  <agiaug>10</agiaug>
  <reamin>1</reamin>
  <reamax>6</reamax>
  <reaaug>10</reaaug>
  <strmin>2</strmin>
  <strmax>7</strmax>
  <straug>10</straug>
  <chamin>1</chamin>
  <chamax>5</chamax>
  <chaaug>10</chaaug>
  <intmin>1</intmin>
  <intmax>6</intmax>
  <intaug>10</intaug>
  <logmin>1</logmin>
  <logmax>6</logmax>
  <logaug>10</logaug>
  <wilmin>1</wilmin>
  <wilmax>6</wilmax>
  <wilaug>10</wilaug>
  <edgmin>1</edgmin>
  <edgmax>6</edgmax>
  <edgaug>10</edgaug>
  <source>Custom</source>
  <page>1</page>
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
- Create a custom data file, ie `custom_metatypes.xml`
- Add your new metatype entry
- Follow the existing structure and formatting
- Validate the XML syntax

### 4. Test Your Metatype
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
You can add special abilities to metatypes using the bonus system:

```xml
<bonus>
 <armor>2</armor>
</bonus>
```

### Source Integration
Ensure proper source attribution. All content in Chummer5a requires a source to be defined. If you don't want to create a custom book, it is recommended to use juse the SR5 source:

```xml
<source>SR5</source>
<page>0</page>
```

## Resources

- [GUID Generator](https://guidgenerator.com/)
- [XML Schema Documentation](https://www.w3.org/XML/Schema)
- [Shadowrun 5th Edition Core Rulebook](https://www.shadowruntabletop.com/)
