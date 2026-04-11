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

### 4. Add to the Priority Table (if you use Priority-based character creation)
If you want your metatype to appear in the Priority table, you must also amend `priorities.xml`. This is done with an Amend file because you are changing existing Priority rows rather than replacing the whole file.

**What you are editing in `priorities.xml`**
- Each Priority row lives under `<priorities><priority>`.
- Metatype choices for that row are under `<metatypes><metatype>`.
- The `name` must match the metatype name from `metatypes.xml`.
- The `value` is the Special Attribute points granted at that priority.
- The `karma` is the Karma cost for that metatype. This may be a negative value to provide karma, 0 for no additional cost, or a positive value to have the metatype cost karma.

**Create an Amend file**
- File name: `amend_priorities.xml`
- Location: your custom data folder (see the Custom Data Files guide)
- For nodes with children, recursion is the default behavior; include the parent nodes leading to what you want to change so the amend system can follow that path.

**Example: Add a custom metatype to the “A - Any metatype” row**

```xml
<?xml version="1.0" encoding="utf-8"?>
<chummer>
  <priorities>
    <priority xpathfilter="name='A - Any metatype'">
      <metatypes>
        <metatype amendoperation="addnode">
          <name>Custom Metatype</name>
          <value>7</value>
          <karma>0</karma>
          <metavariants>
            <metavariant>
              <name>Custom Variant</name>
              <value>7</value>
              <karma>5</karma>
            </metavariant>
          </metavariants>
        </metatype>
      </metatypes>
    </priority>
  </priorities>
</chummer>
```

**Example: Add a custom metavariant to an existing metatype**
This keeps the base metatype (like Human) and adds a new metavariant option under it in the Priority table:

```xml
<?xml version="1.0" encoding="utf-8"?>
<chummer>
  <priorities>
    <priority xpathfilter="name='A - Any metatype'">
      <metatypes>
        <metatype xpathfilter="name='Human'">
          <metavariants>
            <metavariant amendoperation="addnode">
              <name>Custom Human Variant</name>
              <value>9</value>
              <karma>5</karma>
            </metavariant>
          </metavariants>
        </metatype>
      </metatypes>
    </priority>
  </priorities>
</chummer>
```

If you skip this step, your metatype can still be used in Karma-based creation, but it will not show up in the Priority table.

### 5. Test Your Metatype
- Create a new character using a settings file that includes your custom data file
- Verify your metatype appears in the list
- Test character creation with your metatype
- Verify all attributes and abilities work correctly


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
