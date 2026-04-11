# Custom Data Files

Players and GMs can enter Custom, Override, or Amend data by creating additional data files. If you try to customize data by editing original data files, you are likely to lose your work each time you receive a Chummer update, and it will also be hard to sync these changes between multiple people. Custom, Override, and Amend files should be saved with UTF-8 encoding.

## Data File Location

There are two options for where you can keep your custom data files:

1. You can create a folder in Chummer's `customdata` subdirectory with a unique name, like Dave Home Campaign, to hold your Custom, Override, and Amend data files. Folders stored in the `customdata` subdirectory are automatically detected and added to the list of custom data rulesets in Chummer when the program is started, though you will still need to enable them manually. However, be warned that putting your folder there can cause it to get deleted when upgrading or changing Chummer versions, so keep a backup handy.
2. You can create a folder anywhere on your computer to hold your Custom, Override, and Amend data files. If you place the folder inside your Chummer directory somewhere, the program will remember its location relative to the Chummer directory (which means it can keep track of it should you move your folder around), otherwise, the program will remember its absolute location. Should you choose this option, you will need to manually add your folder to Chummer's list of custom rulesets: go to Tools -> Global Settings, click the Custom Data Directories tab, and click the Add Directory button.

## Enabling and Disabling Custom Data Files

To use any folder containing custom data, you will need to create a custom Setting. Go into Tools -> Character Settings. Choose a Setting Name you wish to base your setting on and then click the Custom Data tab, and then check the checkboxes next to the custom rulesets you wish to use. You can also adjust the load order of your enabled custom rulesets in this tab, in case you have multiple custom rulesets that affect each other's content. Click the Save As... button and name your custom settings. If you already have a character, you can change what setting it uses by going to Special -> Change Settings File. You can also automatically enable singular custom data files by placing them directly into Chummer's `data` directory, though doing so means you cannot enable or disable the file from within Chummer's options menu, and you could accidentally lose the file when upgrading or changing Chummer versions.

## Custom Data File Types

Chummer processes custom data files in a specific order: **Override files are processed first**, then **Custom files**, and finally **Amend files**. This order is important when you have multiple types of files affecting the same content.

All custom data files must be wrapped in a `<chummer>` root element and follow the same XML structure as the base data files. The file names must follow the pattern `[prefix]_[datafile].xml`, where `[prefix]` is `custom_`, `override_`, or `amend_`, and `[datafile]` corresponds to the base data file name (e.g., `cyberware.xml`, `qualities.xml`, `weapons.xml`).

### Custom Files

Custom files are designed for when you wish to add new content without any special bells or whistles. Their file names must start with `custom_` and end with `_[datafile].xml`, where `[datafile]` corresponds to the type of data being added by the given Custom file. For example, if you wanted to add a new piece of cyberware for a Flashlight retinal implant, you could create a new file called `custom_cyberware.xml`, `custom_flashlighteyes_cyberware.xml`, or `custom_coolstuff_cyberware.xml` (among others) and add a complete entry for Flashlight eyes inside. Custom Data can be spread across multiple Custom Data Files as long as they follow this file naming convention. GUIDs used for new items should be brand new and unique to each item; a web search will locate sites that create GUIDs online.

If a custom item with the same GUID or name already exists in the base data, it will not be added. This prevents duplicates.

Chummer comes with many examples of Custom files that you can consult if you are lost. Each can be found in one of the folders that come with Chummer when you download it, located inside Chummer's `customdata` subdirectory.

### Override Files

Override files are designed for when you wish to completely replace a piece of existing content. Their file names must start with `override_` and end with `_[datafile].xml`, where `[datafile]` corresponds to the type of data being overwritten by the given Override file. Chummer locates the content that you wish to override by matching your provided identifier with that of the original item. The matching process works as follows:

1. **Primary match by GUID**: If your override entry contains an `id` field, Chummer will match it to the original item's GUID.
2. **Fallback match by name**: If no `id` field is present, Chummer will attempt to match by the `name` field.
3. **Additional identifiers**: Child nodes marked with `isidnode="True"` can serve as additional identifier nodes, useful for items that use neither a name nor an ID.

For example, if you wanted to replace the Jack of All Trades quality with a different effect, you could create a new file called `override_qualities.xml`, `override_JoAT_qualities.xml`, or `override_coolstuff_qualities.xml` (among others) and add an entry for the new effect you want the quality to have inside, making sure to match your entry's `id` tag (or `name` tag if no ID is available) with the corresponding tag of the Jack of All Trades quality in the base Chummer data files. Override Data can be spread across multiple Override Data Files as long as they follow this file naming convention.

**Important**: Override files completely replace the matching item's content, so make sure to include all fields you want to preserve in your override entry.

### Amend Files

Amend files are designed for when you wish to modify a piece of existing content without completely replacing it. Their file names must start with `amend_` and end with `_[datafile].xml`, where `[datafile]` corresponds to the type of data being amended by the given Amend file. Chummer locates the content that you wish to amend by matching your provided identifier with that of the original item, or by using XPath filters to match nodes by other criteria. Amend Data can be spread across multiple Amend Data Files as long as they follow this file naming convention.

#### Matching Items for Amendment

Amend files can match items using:
- **GUID**: If your amend entry contains an `id` field, Chummer will match it to the original item's GUID.
- **Name**: If no `id` field is present, Chummer will attempt to match by the `name` field.
- **XPath filters**: Use the `xpathfilter` attribute to match nodes by other criteria. For example, `xpathfilter="name='Squatter'"` matches items by name, or `xpathfilter="category='Cyberware' and grade='Standard'"` matches by multiple criteria.

#### Amend Operations

Amend files use amend operations (specified via the `amendoperation` attribute) to specify how the data should be modified. The available amend operations are:

- `replace` - Replaces the entire node or attribute with the new content. If used with `addifnotfound="True"`, it will add the node if it doesn't exist (similar to custom files but with targeting).
- `addnode` - Adds a new node if it doesn't exist. If the node already exists, it will not be modified. This is useful for adding optional fields.
- `remove` - Removes the node or attribute entirely.
- `recurse` - Recursively processes child nodes. This is required when you need to modify nested elements within a matched item.
- `append` - Appends content to existing text nodes or adds new content to existing nodes.
- `regexreplace` - Performs a regular expression replacement on text content. Requires a `regexpattern` attribute specifying the regex pattern.

#### Additional Amend Attributes

- `addifnotfound` - When set to `"True"` on a `replace` operation, the node will be added if it doesn't exist (similar to custom files but with targeting).
- `xpathfilter` - Used to match items by XPath criteria instead of just GUID or name. Example: `xpathfilter="name='Squatter'"` or `xpathfilter="category='Quality' and karma='10'"`.

#### Amend Examples

The following examples demonstrate common amend operations. Note that the modern approach uses `amendoperation="recurse"` on parent nodes with `xpathfilter` attributes for matching, which provides more precise control.

**Example 1: Changing the cost of the Squatter lifestyle**

This example changes the cost of the Squatter lifestyle from its default value to 250 using the modern recurse approach:

```xml
<?xml version="1.0" encoding="utf-8"?>
<chummer amendoperation="recurse">
  <lifestyles amendoperation="recurse">
    <lifestyle xpathfilter="name='Squatter'" amendoperation="recurse">
      <cost amendoperation="replace">250</cost>
    </lifestyle>
  </lifestyles>
</chummer>
```

**Example 2: Hiding a quality**

The following example hides the Bad Rep quality from the list of qualities that characters can acquire:

```xml
<?xml version="1.0" encoding="utf-8"?>
<chummer amendoperation="recurse">
  <qualities amendoperation="recurse">
    <quality xpathfilter="name='Bad Rep'" amendoperation="recurse">
      <hide amendoperation="addnode" />
    </quality>
  </qualities>
</chummer>
```

**Example 3: Removing requirements**

The following example removes all extra requirements for learning the Mana Choke martial arts technique, which allows it to be learned by people without a Magic rating:

```xml
<?xml version="1.0" encoding="utf-8"?>
<chummer amendoperation="recurse">
  <techniques amendoperation="recurse">
    <technique xpathfilter="name='Mana Choke'" amendoperation="recurse">
      <required amendoperation="remove" />
    </technique>
  </techniques>
</chummer>
```

**Example 4: Modifying nested bonus nodes**

This example (from the actual codebase) modifies Adapsin bioware to change how it applies its bonus:

```xml
<?xml version="1.0" encoding="utf-8"?>
<chummer>
  <biowares>
    <bioware>
      <name>Adapsin</name>
      <bonus>
        <adapsin amendoperation="remove" />
        <cyberwaretotalessmultipliernonretroactive>90</cyberwaretotalessmultipliernonretroactive>
      </bonus>
    </bioware>
  </biowares>
</chummer>
```

**Note**: When using `amendoperation="recurse"`, you must apply it to each parent node in the hierarchy leading to the node you want to modify. The `xpathfilter` attribute allows you to target specific items without needing to know their GUID.

## Manifest Data

Since Nightly-v5.214.127, every custom data folder **should** have a `Manifest.xml` file. This Manifest can hold information about the author(s), version, content, dependencies, and incompatibilities. While the manifest is technically optional, it is strongly recommended as it helps users understand what the custom data does and how it relates to other custom data sets.

Every manifest needs to contain a unique GUID (you can generate one at www.guidgen.com or use any GUID generator). All other information is optional, but providing at least a description is highly recommended.

### Nodes

#### Version

Versions are given as one to four different numbers separated by periods. Examples:

```xml
<version>1.2</version>
```

```xml
<version>5.201.2</version>
```

#### Authors

Authors can contain one or more authors, with each entry needing a name. One author should be defined as the main author, who will be specially marked as such in Chummer. Example:

```xml
<authors>
   <author>
      <name>Francis</name>
      <main>True</main>
   </author>
   <author>
      <name>Peterson</name>
      <main>False</main>
   </author>
</authors>
```

#### Descriptions

Descriptions are language-specific and require a language code. Chummer will try to show the one that matches its current UI language and will otherwise default to US English if it is available.

```xml
<descriptions>
   <description>
      <text>This custom data folder will change all assault rifles to deal 5 more damage because they really needed a buff!</text>
      <lang>en-us</lang>
   </description>
   <description>
      <text>Dieser Ordner erhöht die Schaden von Sturmgewehren um 5. Geil, ne?</text>
      <lang>de-de</lang>
   </description>
</descriptions>
```

#### Dependencies

Dependencies are other custom data folders that need to be loaded for a given folder to work correctly. Chummer will not force a user to enable any datasets on which the given dataset is dependent, but it will warn users if any dependencies are not met. A custom data folder can have any number of dependencies. A dependency is defined by a `guid` and version range `(minversion,maxversion)`. This version range is inclusive, meaning that the given numbers will also be accepted. If no minversion or maxversion is given, the range is assumed to be open-ended. If no version information is given at all, only the `guid` is used to identify the requested custom dataset.

The name node is not used for identification, but is strictly required. Because tooltips will be useless to the user without a name to display.

The following example requires another custom data with the `guid a8d6147d-68e9-4286-80fd-8f9ee4eb6cbd` and the `version` has to be at least 2.

```xml
<dependencies>
  <dependency>
    <name>Needed example file</name>
    <guid>a8d6147d-68e9-4286-80fd-8f9ee4eb6cbd</guid>
    <minversion>2</minversion>
  </dependency>
</dependencies>
```

#### Incompatibilities

Incompatibilities are analogous to dependencies and work with the same syntax. Rather than requiring certain datasets to be enabled for the given dataset to work though, incompatibilities signal that the given custom dataset will not work (properly) if any of the specified custom datasets are enabled.