# Quality Levels

## Structure

```xml
<chummer>
  <qualitygroups>
    <qualitygroup>
      <name />
      <levels>
        <level value="" />
      </levels>
    </qualitygroup>
  </qualitygroups>
</chummer>
```

The `qualitylevels.xml` file defines hierarchical quality groups where multiple levels of the same quality type can exist, but only the highest level should be applied to a character. This is particularly useful for Life Modules that can grant different levels of the same quality type that are mutually exclusive such as the SINner qualities.

## qualitygroup Node

```xml
<qualitygroup>
  <name />
  <levels>
    <level value="" />
  </levels>
</qualitygroup>
```

**name** (required): The name of the quality group. This should match the group attribute used in the `qualitylevel` bonus in Life Modules and other sources.

**levels** (required): Contains the list of levels for this quality group, ordered from lowest to highest level.

## level Node

```xml
<level value="">Quality Name</level>
```

**value** (required): The numeric level value for this quality. Higher values represent higher levels in the hierarchy.

**text content** (required): The name of the quality that corresponds to this level. This should match exactly with a quality defined in the qualities.xml file.

## Example

```xml
<chummer>
  <qualitygroups>
    <!-- SINner Quality Hierarchy -->
    <qualitygroup>
      <name>SINner</name>
      <levels>
        <level value="1">SINner (National)</level>
        <level value="2">SINner (Criminal)</level>
        <level value="3">SINner (Corporate Limited)</level>
        <level value="4">SINner (Corporate)</level>
      </levels>
    </qualitygroup>
    
    <!-- Trust Fund Quality Hierarchy -->
    <qualitygroup>
      <name>Trust Fund</name>
      <levels>
        <level value="1">Trust Fund I</level>
        <level value="2">Trust Fund II</level>
        <level value="3">Trust Fund III</level>
        <level value="4">Trust Fund IV</level>
      </levels>
    </qualitygroup>
  </qualitygroups>
</chummer>
```

## Usage in Life Modules

When a Life Module grants a quality level, it uses the `qualitylevel` bonus with a `group` attribute:

```xml
<qualitylevel group="SINner">2</qualitylevel>
```

This would grant level 2 of the SINner quality group, which corresponds to "SINner (Criminal)" in the example above.

## Related Files

- **qualities.xml**: Contains the actual quality definitions
- **bonuses.xsd**: Defines the `qualitylevel` bonus schema
- **Life Module XML files**: Use `qualitylevel` bonuses to grant quality levels
