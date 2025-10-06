Chummer uses a common Requirements system for almost all objects stored in the data XMLs. This system uses a combination of [**forbidden**](#forbidden) and [**required**](#required) XML elements that compare properties of the current character or parent object against a desired state (for example, is the current character a Troll, or do they have the Adept quality). Values are returned as a boolean, with Required nodes expecting a true result and Forbidden expecting false to either allow or prohibit the object from being added. 

When designing custom data files it is critical to note that items that are exclusive of each other MUST have a required/forbidden node referencing each other. If this is not done and one object forbids another, if that object is later added no error will be generated. For example, the Astral Beacon and Astral Chameleon qualities each have a forbidden node that references each other; if one of these were not present it would be possible to purchase the quality with the forbidden entry, then the quality that does not.

## <a id="forbidden"></a>forbidden Node
    <forbidden>
       <allof />
       <oneof />
       <geardetails />
       <vehicledetails />
       <weapondetails />
       <weaponmountdetails />
    </forbidden>
**allof** (optional): all of the requirements listed in this node must be met in order for the object to NOT be available for selection. If at least one condition is not met, the object will be made available. See [**allof** Node](#requiredspecific "allof Node") for more information.

**oneof** (optional): If at least one of the requirements listed in this node is met the object will NOT be available for selection. See [**conditions** Node](#conditions "conditions Node") for more information.

**geardetails** (optional): If at least one of the requirements listed in this node is met the object will NOT be available for selection. See [**geardetails** Node](#geardetails "geardetails Node") for more information.

**vehicledetails** (optional): If at least one of the requirements listed in this node is met the object will NOT be available for selection. See [**vehicledetails** Node](#vehicledetails "vehicledetails Node") for more information.

**weapondetails** (optional): If at least one of the requirements listed in this node is met the object will NOT be available for selection. See [**weapondetails** Node](#weapondetails "weapondetails Node") for more information.

**weaponmountdetails** (optional): If at least one of the requirements listed in this node is met the object will NOT be available for selection. See [**weaponmountdetails** Node](#weaponmountdetails "weaponmountdetails Node") for more information.

## <a id="required"></a>required Node
    <required>
       <allof />
       <oneof />
       <geardetails />
       <vehicledetails />
       <weapondetails />
       <weaponmountdetails />
    </required>
**allof** (optional): all of the requirements listed in this node must be met in order for the object to be available for selection. See [**conditions** Node](#conditions "conditions Node") for more information.

**oneof** (optional): at least one of the requirements listed in this node must be met in order for the object to be available for selection. See [**conditions** Node](#conditions "conditions Node") for more information.

**geardetails** (optional): at least one of the requirements listed in this node must be met in order for the object to be available for selection. See [**geardetails** Node](#geardetails "geardetails Node") for more information.

**vehicledetails** (optional): at least one of the requirements listed in this node must be met in order for the object to be available for selection. See [**vehicledetails** Node](#vehicledetails "vehicledetails Node") for more information.

**weapondetails** (optional): at least one of the requirements listed in this node must be met in order for the object to be available for selection. See [**weapondetails** Node](#weapondetails "weapondetails Node") for more information.

**weaponmountdetails** (optional): at least one of the requirements listed in this node must be met in order for the object to be available for selection. See [**weaponmountdetails** Node](#weaponmountdetails "weaponmountdetails Node") for more information.

## <a id="conditions"></a> conditions Nodes
### accessory

Checks for the presence of an accessory on the intended parent.

#### Attributes
None.

#### Elements
None.

#### Limitations

Only valid on objects that can be attached to a Weapon. 

#### Example

```XML
<accessory>Smartgun System, Internal</accessory>
```

### armormod

Checks for the presence of an armormod on any of the character's Armor.

#### Attributes
**sameparent** Boolean. If present, requires that the armormod be attached to the same piece of armor as the intended parent. If specified and there is no parent will return false.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<armormod sameparent="True">Stillsuit Filter</armormod>
```

### art

Checks for the presence of a High Art (Street Grimoire). Typically used for metamagics. If the setting "Ignore Art Requirements" is enabled, this will always return true. 

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<art>Psychometry</art>
```

### attribute

Check a named character attribute (ie BOD) for values. Can be used to check for enabled Special attributes by using (MAG/MAGAdept/RES/DEP) and a total value of 1.

#### Attributes
None.

#### Elements

**name** Shortform name of the attribute to investigate, typically first three letters in English uppercase.

**total** The attributes Total Value (including augmentations & improvements) must match or exceed this value. 

**natural**  If present, the attribute's natural value (Metatype base + Karma + Priority points, if any) must match or exceed this value. Takes priority over Total.

#### Limitations

None.

#### Example
Checks that the character has a total of 5 STR, including bonuses from augmentation bonuses such as cyberware.
```XML
<attribute><name>STR</name><total>5</total></attribute>
```
Checks that the character has 5 STR before any bonuses are applied.
```XML
<attribute><name>MAG</name><total>5</total><natural /></attribute>
```

### attributetotal

Checks multiple character attributes for a combined total value.

#### Attributes
None.

#### Elements

**attributes** String representation of an XPath operation. Character attributes should be wrapped in braces to be replaced with their value, which then forms a mathematical operation. Valid values for replacement are {ATT}. {ATTBase},{ATTUnaug}, where {ATT} returns the  TotalValue (metatype + karma + improvement values), {AttUnaug} returns Value (metatype + karma) and {ATTBase} returns  the peiority points spent on the attribute. 

**val**  The return value of the Xpath operation must match or exceed this value.

#### Limitations

None.

#### Example

```XML
<attributetotal><attributes>{BOD}+{REA}-1</attributes><val>3</val></attributetotal>
```

### careerkarma

Checks that the character's total earned karma (including spent) meets or exceeds the requested value.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<careerkarma>10</careerkarma>
```

### critterpower

Checks that the character possesses the requested Critter Power.

#### Attributes
None.

#### Elements
None.

#### Limitations

Returns False is the character does not have access to the Critter Powers tab, even if the character has somehow been given a critter power.

#### Example

```XML
<critterpower>Accident</critterpower>
```

### bioware

Checks for the presence of one or more bioware items with a name that exactly matches the provided name on the current character.

#### Attributes

**count** If specified, checks that the character meets or exceeds the total number of bioware items with this name.

**select** If specified, checks the 'Extra' value of the Bioware for a specified string; bioware without a matching name and Extra will not count towards the total.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<bioware count="2" select="Foot">Striking Callus</bioware>
```

### biowarecontains

Checks for the presence of one or more bioware items on the current character that have a partial match for the provided name.

#### Attributes

**count** If specified, checks that the character meets or exceeds the total number of bioware items with this name.

**select** If specified, checks the 'Extra' value of the Bioware for a specified string; bioware without a matching name and Extra will not count towards the total.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<biowarecontains count="2" select="Foot">Striking</biowarecontains>
```

### cyberware

Checks for the presence of one or more cyberware items with a name that exactly matches the provided name on the current character.

#### Attributes

**count** If specified, checks that the character meets or exceeds the total number of cyberware items with this name.

**select** If specified, checks the 'Extra' value of the cyberware for a specified string; cyberware without a matching name and Extra will not count towards the total.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<cyberware count="2" select="Foot">Striking Callus</cyberware>
```

### cyberwarecontains

Checks for the presence of one or more cyberware items on the current character that have a partial match for the provided name.

#### Attributes

**count** If specified, checks that the character meets or exceeds the total number of cyberware items with this name.

**select** If specified, checks the 'Extra' value of the cyberware for a specified string; cyberware without a matching name and Extra will not count towards the total.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<cyberwarecontains count="2" select="Foot">Striking</cyberwarecontains>
```

### damageresistance

Checks that the character's damageresistance pool (BOD + DamageResistance improvements) meets or exceeds the requested value.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<damageresistance>2</damageresistance>
```

### depenabled

Checks that the character has the Depth attribute enabled.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<depenabled>True</depenabled>
```

### ess

Checks that the character an Essence value that meets or exceeds the requested value. If the value is prefixed with a "-" then the value must be below the listed value instead.

#### Attributes
**grade** Comma-separated list of Grade names. If specified, the Essence loss must have come from specific Grades of cyberware/bioware.

#### Elements
None.

#### Limitations

Note that there is no separation between bioware and cyberware for this check. 

#### Example

```XML
<ess grade="Used">0.0001</ess>
```

### gameplayoption

Checks for the presence of a given gameplay option, such as Prime Runner.

#### Attributes
None.

#### Elements
None.

#### Limitations

This checks for an exact match against the name of the Settings file, so custom data changes that want to use existing qualities will need to use the amend system to alter this setting for base data. 

#### Example

```XML
<gameplayoption>Prime Runner</gameplayoption>
```

### gear

Checks for the presence of a given piece of named Gear on the current character. 

#### Attributes

**minrating** The gear must have a rating that meets or exceeds this value

**rating** The gear must have a rating that exactly matches this value

**maxrating** The gear must have a rating that is less than or equal to this value

#### Elements
None.

#### Limitations

Only one attribute can be used

#### Example

```XML
<gear minrating="4">Fake SIN</gear>
```

### group

Acts in a similar fashion to allof; any other element can be added to this node as a child. When evaluating this node, it will return true if ALL elements below it are true, and false if at least one element fails. This allows for having a 'one of' selector that actually requires one of two or more sets of options

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<group><quality>Mystic Adept</quality><tradition>Buddhism</tradition></group>
```

### grouponeof

Acts in a similar fashion to oneof; any other element can be added to this node as a child. When evaluating this node, it will return true if ANY elements below it are true, and false if all elements fail. This allows for having an 'all of' selector that actually requires multple sets with different optional choices. 

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<grouponeof><quality>Mystic Adept</quality><tradition>Buddhism</tradition></grouponeof>
```

### initiategrade

Checks that the character's Initiate Grade is equal to or higher than the provided value.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<initiategrade>2</initiategrade>
```

### magenabled

Checks that the character has the Magic attribute enabled.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<magenabled>True</magenabled>
```

### martialart

Checks that the character has Martial Art with a name that matches the provided text. 

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<martialart>Capoeira</martialart>
```

### martialtechnique

Checks that the character has any Martial Art Technique with a name that matches the provided text. 

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<martialtechnique>The Cowboy Way</martialtechnique>
```

### metamagic

Checks for the presence of any metamagic with a name that matches the provided text. 

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<metamagic>Centering</metamagic>
```

### metamagicart

Checks for the presence of a Metamagic or Art that matches the provided text.  If the setting Ignore Art Requirements is NOT enabled, it will first check for an Art with this name, then a metamagic with that name, returning false is neither is true.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<metamagicart>Blood Magic</metamagicart>
```

### metatype

Checks whether the character's Metatype is a match for the provided text.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<metatype>Troll</metatype>
```

### metatypecategory

Checks whether the character's Metatype Category (ie metasapient) is a match for the provided text.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<metatypecategory>Metahuman</metatypecategory>
```

### metavariant

Checks whether the character's Metavariant is a match for the provided text. None can be used to check whether the character is a metavariant or not.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<metavariant>Dryad</metavariant>
```

### nuyen

Checks whether the character's current Nuyen total is equal to or greather than the provided text.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<nuyen>5000</nuyen>
```

### onlyprioritygiven

Checks whether the character's build method uses a priority table or point buy.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<onlyprioritygiven />
```

### power

Checks for the presence of an Adept Power with the provided text as its Name.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<power>Killing Hands</power>
```

### program

Checks for the presence of an AI Program with the provided text as its Name.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<program>Browse</program>
```

### quality

Checks for the presence of a Quality with the provided text as its Name.

#### Attributes
**extra** If present, the quality must also possess the same 'Extra' value.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<quality extra="Horse">Mentor Spirit</quality>
```

### resenabled

Checks that the character has the Resonance attribute enabled.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<resenabled>True</resenabled>
```

### skill

Checks whether the character has a given Skill, usually with a given Rating or Specialisation.

#### Attributes
None.

#### Elements

**name** Name of the skill to look for.

**spec** If present, the skill must have the provided Specialisation name.

**type** If present, checks for a Knowledge Skill of the provided category.

**val** If present, the skill's value must meet or exceed this value.

#### Limitations

None.

#### Example

```XML
<skill><name>Spellcasting</name><spec>Combat</spec><type>Academic</type><val>8</val></skill>
```

### skillgrouptotal

Checks if the total combined Ratings of Skill Groups add up to a particular total.

#### Attributes
None.

#### Elements

**skillgroups** Named skill groups, separated by '+' character.

**val** Combined rating that needs to be met or exceeded.

#### Limitations

None.

#### Example

```XML
<skillgrouptotal><skillgroups>Athletics+Close Combat</skillgroups><val>5</val></skillgrouptotal>
```

### skilltotal

Checks if the total combined Ratings of Skills add up to a particular total.

#### Attributes
None.

#### Elements

**skills** Named skills, separated by '+' character.

**type** If present, ALL skills are checked as Knowledge Skills.

**val** Combined rating that needs to be met or exceeded.

#### Limitations

None.

#### Example

```XML
<skilltotal><skills>Athletics+Close Combat</skills><type /><val>5</val></skilltotal>
```

### specialmodificationlimit

Checks for weapon accessories and underbarrel weapons on the current character's weapons that count towards the 'Special Modification' limit.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<specialmodificationlimit>4</specialmodificationlimit>
```

### spell

Checks for the presence of a Spell with the provided text as its Name.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<spell>Agony</spell>
```

### spellcategory

Checks for a specified number of Spells from the listed Category.

#### Attributes
None.

#### Elements

**name** Name of the Spell Category to check for.

**count** Number of Spells with Category that must match for the character

#### Limitations

None.

#### Example

```XML
<spellcategory><name>Detection</name><val>2</val></spellcategory>
```

### spelldescriptor

Checks for a specified number of Spells with the listed Descriptor.

#### Attributes
None.

#### Elements

**name** Name of the Descriptor to check for.

**count** Number of Spells with Descriptor that must match for the character

#### Limitations

None.

#### Example

```XML
<spelldescriptor><name>Elemental</name><count>2</count></spelldescriptor>
```

### streetcredvsnotoriety

Checks whether the character's Street Cred is higher than their Notoriety.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<streetcredvsnotoriety />
```

### submersiongrade

Checks that the character's Submersion Grade is equal to or higher than the provided value.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<submersiongrade>2</submersiongrade>
```

### tradition

Checks whether the character's Tradition Name matches the requested text.

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<tradition>Buddhism</tradition>
```

### traditionspiritform

Checks whether the form that the character's Tradition (if any) provides the requested spirit form (Possession, Materialization, etc.)

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<traditionspiritform>Possession</traditionspiritform>
```

### weapon

Checks whether the character has a specific named Weapon. 

#### Attributes
None.

#### Elements
None.

#### Limitations

None.

#### Example

```XML
<weapon>Crossbow</weapon>
```

## <a id="geardetails"></a>geardetails Node

## <a id="vehicledetails"></a>vehicledetails Node

## <a id="weapondetails"></a>weapondetails Node

## <a id="weaponmountdetails"></a>weaponmountdetails Node