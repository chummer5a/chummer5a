# Custom Data Files

This document explains how to work with custom data files in Chummer5a.

## Overview

Chummer5a uses XML files to store game data. These files are located in the `Chummer/data/` directory and contain information about:

- Armor
- Bioware
- Cyberware
- Gear
- Spells
- Metatypes
- And much more

## File Structure

Each data type has its own XML file with a corresponding XSD schema file. The schema files define the structure and validation rules for the data.

### Example: Armor Data

```xml
<armor>
  <armor>
    <id>guid-here</id>
    <name>Armor Name</name>
    <armor>12</armor>
    <capacity>0</capacity>
    <avail>2</avail>
    <cost>1000</cost>
    <source>SR5</source>
    <page>123</page>
  </armor>
</armor>
```

## Adding Custom Content

### 1. Create the Data Entry

1. Open the appropriate XML file in `Chummer/data/`
2. Add a new entry following the existing pattern
3. Generate a unique GUID for the `id` field
4. Fill in all required fields according to the XSD schema

### 2. Update the Project File

1. Open `Chummer.sln` in Visual Studio
2. Right-click on the data file in Solution Explorer
3. Select "Add > Existing Item"
4. Set "Copy to Output Directory" to "Copy if newer"

### 3. Test Your Changes

1. Build the project
2. Run Chummer5a
3. Verify your custom content appears correctly
4. Test all functionality related to your content

## Data Types

### Armor
- **File**: `armor.xml`
- **Schema**: `armor.xsd`
- **Required fields**: id, name, armor, capacity, avail, cost, source, page

### Bioware
- **File**: `bioware.xml`
- **Schema**: `bioware.xsd`
- **Required fields**: id, name, grade, essence, capacity, avail, cost, source, page

### Cyberware
- **File**: `cyberware.xml`
- **Schema**: `cyberware.xsd`
- **Required fields**: id, name, grade, essence, capacity, avail, cost, source, page

### Gear
- **File**: `gear.xml`
- **Schema**: `gear.xsd`
- **Required fields**: id, name, avail, cost, source, page

### Spells
- **File**: `spells.xml`
- **Schema**: `spells.xsd`
- **Required fields**: id, name, category, type, range, duration, drain, source, page

### Metatypes
- **File**: `metatypes.xml`
- **Schema**: `metatypes.xsd`
- **Required fields**: id, name, body, agility, reaction, strength, willpower, logic, intuition, charisma, edge, source, page

## Best Practices

1. **Follow the Schema** - Always validate your XML against the XSD schema
2. **Use Unique GUIDs** - Generate new GUIDs for each entry
3. **Test Thoroughly** - Verify all functionality works with your custom content
4. **Document Changes** - Keep track of what you've added or modified
5. **Backup Original Files** - Always keep backups of the original data files

## Troubleshooting

### Common Issues

1. **XML Validation Errors**
   - Check that all required fields are present
   - Verify XML syntax is correct
   - Ensure GUIDs are properly formatted

2. **Content Not Appearing**
   - Verify the file is included in the project
   - Check that "Copy to Output Directory" is set correctly
   - Ensure the XML structure matches the schema

3. **Performance Issues**
   - Large data files can slow down loading
   - Consider splitting very large files
   - Optimize XML structure for better performance

## Resources

- [XML Schema Documentation](https://www.w3.org/XML/Schema)
- [GUID Generator](https://guidgenerator.com/)
- [XML Validation Tools](https://www.xmlvalidation.com/)
- [Chummer5a Data Structure Guide](../wiki/README.md)
