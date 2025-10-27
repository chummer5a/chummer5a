# Group Reference System

## Overview

The Group Reference System allows you to define common groups of XML data elements and reference them throughout your XML files. This eliminates the need to repeatedly list the same set of items (like all Ghoul quality variants) in multiple places, making your custom content easier to create and maintain.

## How It Works

The system automatically processes group references when Chummer loads your XML files. Here's the loading order:

1. **Load Base XML** (e.g., `qualities.xml`)
2. **Load groups.xml** (always loaded first to make group definitions available)
3. **Process Override Files** (`override_qualities.xml`)
4. **Process Custom Files** (`custom_qualities.xml`) 
5. **Process Amend Files** (`amend_qualities.xml`)
6. **Load Translations** (if not English)
7. **Resolve Group References** (final pass - after all custom data)

Group references are transparently expanded into individual elements after all custom data processing, so they work seamlessly with all existing functionality including the amend system.

## XML Structure

### Group Definition

Groups are defined in a separate `groups.xml` file in the `data` directory:

```xml
<?xml version="1.0" encoding="utf-8"?>
<chummer xmlns="" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:schemaLocation="http://www.w3.org/2001/XMLSchema groups.xsd">

  <groups>
    <group id="ghoul_qualities">
      <name>Ghoul Qualities</name>
      <description>All variants of the Infected: Ghoul quality</description>
      <items>
        <quality>Infected: Ghoul (Dwarf)</quality>
        <quality>Infected: Ghoul (Elf)</quality>
        <quality>Infected: Ghoul (Human)</quality>
        <quality>Infected: Ghoul (Ork)</quality>
        <quality>Infected: Ghoul (Sasquatch)</quality>
        <quality>Infected: Ghoul (Troll)</quality>
      </items>
    </group>
  </groups>
</chummer>
```

### Group Reference Usage

Use `<groupref>` elements to reference groups in your requirements:

```xml
<required>
  <oneof>
    <groupref>ghoul_qualities</groupref>
    <quality>Infected: Bandersnatch</quality>
    <quality>Infected: Dzoo-Noo-Qua</quality>
  </oneof>
</required>
```

### After Resolution

The system automatically expands group references into individual elements:

```xml
<required>
  <oneof>
    <quality>Infected: Ghoul (Dwarf)</quality>
    <quality>Infected: Ghoul (Elf)</quality>
    <quality>Infected: Ghoul (Human)</quality>
    <quality>Infected: Ghoul (Ork)</quality>
    <quality>Infected: Ghoul (Sasquatch)</quality>
    <quality>Infected: Ghoul (Troll)</quality>
    <quality>Infected: Bandersnatch</quality>
    <quality>Infected: Dzoo-Noo-Qua</quality>
  </oneof>
</required>
```

## Amend System Compatibility

The group system works seamlessly with the existing amend system. Since groups are loaded first, you can modify them through custom data files:

### Amending Group Definitions

```xml
<!-- In amend_groups.xml -->
<chummer>
  <groups>
    <group id="ghoul_qualities">
      <items>
        <quality amendoperation="addnode">Infected: Ghoul (New Variant)</quality>
      </items>
    </group>
  </groups>
</chummer>
```

### Amending Items That Use Groups

```xml
<!-- In amend_qualities.xml -->
<chummer>
  <qualities>
    <quality xpathfilter="id = 'some-quality-id'">
      <required>
        <oneof>
          <groupref amendoperation="addnode">ghoul_qualities</groupref>
        </oneof>
      </required>
    </quality>
  </qualities>
</chummer>
```

### Targeting Individual Elements After Group Expansion

Since group references are expanded after all custom data processing, you can target individual elements that were expanded from groups:

```xml
<!-- In amend_qualities.xml -->
<chummer>
  <qualities>
    <quality xpathfilter="id = 'armor-power-id'">
      <required>
        <oneof>
          <groupref>infected_qualities</groupref>
          <!-- Remove specific quality after group expansion -->
          <quality amendoperation="remove">Infected: Ghoul (Troll)</quality>
        </oneof>
      </required>
    </quality>
  </qualities>
</chummer>
```

## Benefits

1. **Reduced Duplication**: Define groups once, reference everywhere
2. **Easier Maintenance**: Add new items to a group definition instead of updating multiple files
3. **Fewer Errors**: No more missing individual items in requirement lists
4. **Better Organization**: Clear grouping of related items
5. **Amend Compatible**: Full integration with the existing amendment system
6. **Centralized Management**: All group definitions in one place (`groups.xml`)
7. **Early Loading**: Groups are loaded first, making them available for all custom data processing
8. **Transparent Expansion**: Group references are expanded after all custom data, allowing fine-grained control

## Example: Solving the Ghoul Qualities Problem

### Before (Current System)
Every time you need to reference Ghoul qualities, you must list all 6 variants:

```xml
<required>
  <oneof>
    <quality>Infected: Ghoul (Dwarf)</quality>
    <quality>Infected: Ghoul (Elf)</quality>
    <quality>Infected: Ghoul (Human)</quality>
    <quality>Infected: Ghoul (Ork)</quality>
    <quality>Infected: Ghoul (Sasquatch)</quality>
    <quality>Infected: Ghoul (Troll)</quality>
    <!-- other qualities -->
  </oneof>
</required>
```

### After (With Group System)
Define the group once in `groups.xml`, reference it everywhere:

```xml
<!-- In data/groups.xml -->
<?xml version="1.0" encoding="utf-8"?>
<chummer xmlns="" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:schemaLocation="http://www.w3.org/2001/XMLSchema groups.xsd">

  <groups>
    <group id="ghoul_qualities">
      <name>Ghoul Qualities</name>
      <description>All variants of the Infected: Ghoul quality</description>
      <items>
        <quality>Infected: Ghoul (Dwarf)</quality>
        <quality>Infected: Ghoul (Elf)</quality>
        <quality>Infected: Ghoul (Human)</quality>
        <quality>Infected: Ghoul (Ork)</quality>
        <quality>Infected: Ghoul (Sasquatch)</quality>
        <quality>Infected: Ghoul (Troll)</quality>
      </items>
    </group>
  </groups>
</chummer>

<!-- In your requirements -->
<required>
  <oneof>
    <groupref>ghoul_qualities</groupref>
    <!-- other qualities -->
  </oneof>
</required>
```

When a new Ghoul variant is added, just update the group definition - all references automatically include the new variant!

## Real-World Examples

### Magic Qualities Group

```xml
<group id="magic_qualities">
  <name>Magic Qualities</name>
  <description>Qualities related to magic and magical abilities</description>
  <items>
    <quality>Adept</quality>
    <quality>Aware</quality>
    <quality>Aspected Magician</quality>
    <quality>Enchanter</quality>
    <quality>Explorer</quality>
    <quality>Apprentice</quality>
    <quality>Magician</quality>
    <quality>Mystic Adept</quality>
  </items>
</group>
```

### All Infected Qualities Group

```xml
<group id="infected_qualities">
  <name>All Infected Qualities</name>
  <description>All variants of Infected qualities</description>
  <items>
    <groupref>ghoul_qualities</groupref>
    <groupref>vampire_qualities</groupref>
    <quality>Infected: Bandersnatch</quality>
    <quality>Infected: Banshee</quality>
    <quality>Infected: Dzoo-Noo-Qua</quality>
    <quality>Infected: Fomoraig</quality>
    <quality>Infected: Gnawer</quality>
    <quality>Infected: Goblin</quality>
    <quality>Infected: Grendel</quality>
    <quality>Infected: Harvester</quality>
    <quality>Infected: Loup-Garou</quality>
    <quality>Infected: Mutaqua</quality>
    <quality>Infected: Nosferatu</quality>
    <quality>Infected: Sukuyan (Human)</quality>
    <quality>Infected: Sukuyan (Non-Human)</quality>
    <quality>Infected: Wendigo</quality>
  </items>
</group>
```

### Weapon Mount Slots Groups

```xml
<group id="standard_rifle_mounts">
  <name>Standard Rifle Mounts</name>
  <description>Common mount points available on most rifles</description>
  <items>
    <mount>Stock</mount>
    <mount>Side</mount>
    <mount>Top</mount>
    <mount>Under</mount>
  </items>
</group>

<group id="advanced_rifle_mounts">
  <name>Advanced Rifle Mounts</name>
  <description>All mount points including barrel mounts for advanced rifles</description>
  <items>
    <groupref>standard_rifle_mounts</groupref>
    <mount>Barrel</mount>
  </items>
</group>

<group id="pistol_mounts">
  <name>Pistol Mounts</name>
  <description>Mount points typically available on pistols</description>
  <items>
    <mount>Top</mount>
    <mount>Under</mount>
  </items>
</group>
```

### Using Mount Groups in Weapons

Instead of repeatedly listing the same mount combinations:

```xml
<!-- Before: Repetitive mount listings -->
<weapon>
  <name>AK-97</name>
  <accessorymounts>
    <mount>Stock</mount>
    <mount>Side</mount>
    <mount>Top</mount>
    <mount>Under</mount>
  </accessorymounts>
</weapon>

<weapon>
  <name>M23 Assault Rifle</name>
  <accessorymounts>
    <mount>Stock</mount>
    <mount>Side</mount>
    <mount>Top</mount>
    <mount>Under</mount>
  </accessorymounts>
</weapon>
```

Use group references for cleaner, more maintainable code:

```xml
<!-- After: Using group references -->
<weapon>
  <name>AK-97</name>
  <accessorymounts>
    <groupref>standard_rifle_mounts</groupref>
  </accessorymounts>
</weapon>

<weapon>
  <name>M23 Assault Rifle</name>
  <accessorymounts>
    <groupref>standard_rifle_mounts</groupref>
  </accessorymounts>
</weapon>

<weapon>
  <name>Advanced Combat Rifle</name>
  <accessorymounts>
    <groupref>advanced_rifle_mounts</groupref>
  </accessorymounts>
</weapon>
```

## The groups.xml File

The `groups.xml` file is the central location for all group definitions. It's automatically loaded first when Chummer processes any XML file, making group definitions available throughout the entire loading process.

### File Location
- **Path**: `data/groups.xml`
- **Schema**: Uses the same XML schema as other Chummer data files
- **Loading Order**: Always loaded first, before any other XML processing

### File Structure
```xml
<?xml version="1.0" encoding="utf-8"?>
<chummer xmlns="" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:schemaLocation="http://www.w3.org/2001/XMLSchema groups.xsd">

  <groups>
    <!-- All your group definitions go here -->
  </groups>
</chummer>
```

### Custom Data Integration
You can modify groups through custom data files:
- **Override**: `override_groups.xml` - Replace entire group definitions
- **Custom**: `custom_groups.xml` - Add new group definitions
- **Amend**: `amend_groups.xml` - Modify existing group definitions

## Best Practices

1. **Use Descriptive IDs**: Choose group IDs that clearly indicate what the group contains
2. **Include Documentation**: Always provide `<name>` and `<description>` elements for your groups
3. **Group Related Items**: Only group items that are logically related
4. **Consider Hierarchy**: Use group references within groups to create hierarchical structures
5. **Test Thoroughly**: Always test your custom content to ensure group references work as expected
6. **Centralize Groups**: Keep all group definitions in `groups.xml` rather than scattered across multiple files

## Troubleshooting

- **Group Not Found**: Make sure the group ID matches exactly (case-sensitive)
- **Empty Results**: Verify that the group contains items and that the group definition is properly formatted
- **Amend Issues**: Ensure your amend files follow the correct XML structure for group modifications
