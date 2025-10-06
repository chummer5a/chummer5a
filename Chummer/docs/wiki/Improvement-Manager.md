A significant amount of Chummer's power comes from the Improvement Manager. The Improvement Manager gathers all of the bonuses that can be applied to any aspect of a character into a single place. These bonuses can be both positive and negative. When the character looks at any aspect of itself, it considers its base value and all of its bonuses that are present in the Improvement Manager to come up with a final number. Most of the data files can affect the character via the Improvement Manager by including a `<bonus>` node.

Multiple bonuses can be specified in a single `<bonus>` node. For example, to improve a character's Impact Armor by 2 and their Infiltration Skill by 1, the following code would be used. 

```XML
<bonus>
  <armor>2</armor>
    <specificskill>
      <name>Infiltration</name>
      <bonus>1</bonus>
    </specificskill>
</bonus>
```

<!---
Contents
--------

[Improvements](#Improvements)

[adapsin](#adapsin)

-   [Attributes](#Attributes)
-   [Elements](#Elements)
-   [Limitations](#Limitations)
-   [Example](#Example)

[addattribute](#addattribute)

-   [Attributes](#Attributes_2)
-   [Elements](#Elements_2)
-   [Limitations](#Limitations_2)
-   [Example](#Example_2)

[addspell](#addspell)

-   [Attributes](#Attributes_2)
-   [Elements](#Elements_2)
-   [Limitations](#Limitations_2)
-   [Example](#Example_2)

[addware](#addware)

-   [Attributes](#Attributes_2)
-   [Elements](#Elements_2)
-   [Limitations](#Limitations_2)
-   [Example](#Example_2)

[adeptlinguistics](#adeptlinguistics)

-   [Attributes](#Attributes_3)
-   [Elements](#Elements_3)
-   [Limitations](#Limitations_3)
-   [Example](#Example_3)

[adeptpowerpoints](#adeptpowerpoints)

-   [Attributes](#Attributes_4)
-   [Elements](#Elements_4)
-   [Limitations](#Limitations_4)
-   [Example](#Example_4)

[armor](#armor)

-   [Attributes](#Attributes_5)
-   [Elements](#Elements_5)
-   [Limitations](#Limitations_5)
-   [Example](#Example_5)

[armorencumbrancepenalty](#armorencumbrancepenalty)

-   [Attributes](#Attributes_6)
-   [Elements](#Elements_6)
-   [Limitations](#Limitations_6)
-   [Example](#Example_6)

[basicbiowareessmultiplier](#basicbiowareessmultiplier)

-   [Attributes](#Attributes_7)
-   [Elements](#Elements_7)
-   [Limitations](#Limitations_7)
-   [Example](#Example_7)

[basiclifestylecost](#basiclifestylecost)

-   [Attributes](#Attributes_8)
-   [Elements](#Elements_8)
-   [Limitations](#Limitations_8)
-   [Example](#Example_8)

[biowareessmultiplier](#biowareessmultiplier)

-   [Attributes](#Attributes_9)
-   [Elements](#Elements_9)
-   [Limitations](#Limitations_9)
-   [Example](#Example_9)

[blackmarketdiscount](#blackmarketdiscount)

-   [Attributes](#Attributes_10)
-   [Elements](#Elements_10)
-   [Limitations](#Limitations_10)
-   [Example](#Example_10)

[complexformlimit](#complexformlimit)

-   [Attributes](#Attributes_11)
-   [Elements](#Elements_11)
-   [Limitations](#Limitations_11)
-   [Example](#Example_11)

[composure](#composure)

-   [Attributes](#Attributes_12)
-   [Elements](#Elements_12)
-   [Limitations](#Limitations_12)
-   [Example](#Example_12)

[concealability](#concealability)

-   [Attributes](#Attributes_13)
-   [Elements](#Elements_13)
-   [Limitations](#Limitations_13)
-   [Example](#Example_13)

[conditionmonitor](#conditionmonitor)

-   [Attributes](#Attributes_14)
-   [Elements](#Elements_14)
-   [Limitations](#Limitations_14)
-   [Example](#Example_14)

[cyberwareessmultiplier](#cyberwareessmultiplier)

-   [Attributes](#Attributes_15)
-   [Elements](#Elements_15)
-   [Limitations](#Limitations_15)
-   [Example](#Example_15)

[damageresistance](#damageresistance)

-   [Attributes](#Attributes_16)
-   [Elements](#Elements_16)
-   [Limitations](#Limitations_16)
-   [Example](#Example_16)

[drainresist](#drainresist)

-   [Attributes](#Attributes_17)
-   [Elements](#Elements_17)
-   [Limitations](#Limitations_17)
-   [Example](#Example_17)

[enabletab](#enabletab)

-   [Attributes](#Attributes_18)
-   [Elements](#Elements_18)
-   [Limitations](#Limitations_18)
-   [Example](#Example_18)

[fadingresist](#fadingresist)

-   [Attributes](#Attributes_19)
-   [Elements](#Elements_19)
-   [Limitations](#Limitations_19)
-   [Example](#Example_19)

[flypercent](#flypercent)

-   [Attributes](#Attributes_20)
-   [Elements](#Elements_20)
-   [Limitations](#Limitations_20)
-   [Example](#Example_20)

[flyspeed](#flyspeed)

-   [Attributes](#Attributes_21)
-   [Elements](#Elements_21)
-   [Limitations](#Limitations_21)
-   [Example](#Example_21)

[freenegativequalities](#freenegativequalities)

-   [Attributes](#Attributes_22)
-   [Elements](#Elements_22)
-   [Limitations](#Limitations_22)
-   [Example](#Example_22)

[freepositivequalities](#freepositivequalities)

-   [Attributes](#Attributes_23)
-   [Elements](#Elements_23)
-   [Limitations](#Limitations_23)
-   [Example](#Example_23)

[freespiritpowerpoints](#freespiritpowerpoints)

-   [Attributes](#Attributes_24)
-   [Elements](#Elements_24)
-   [Limitations](#Limitations_24)
-   [Example](#Example_24)

[genetechcostmultiplier](#genetechcostmultiplier)

-   [Attributes](#Attributes_25)
-   [Elements](#Elements_25)
-   [Limitations](#Limitations_25)
-   [Example](#Example_25)

[ignorecmpenaltyphysical](#ignorecmpenaltyphysical)

-   [Attributes](#Attributes_26)
-   [Elements](#Elements_26)
-   [Limitations](#Limitations_26)
-   [Example](#Example_26)

[ignorecmpenaltystun](#ignorecmpenaltystun)

-   [Attributes](#Attributes_27)
-   [Elements](#Elements_27)
-   [Limitations](#Limitations_27)
-   [Example](#Example_27)

[infirm](#infirm)

-   [Attributes](#Attributes_28)
-   [Elements](#Elements_28)
-   [Limitations](#Limitations_28)
-   [Example](#Example_28)

[initiation](#initiation)

-   [Attributes](#Attributes_29)
-   [Elements](#Elements_29)
-   [Limitations](#Limitations_29)
-   [Example](#Example_29)

[initiative](#initiative)

-   [Attributes](#Attributes_30)
-   [Elements](#Elements_30)
-   [Limitations](#Limitations_30)
-   [Example](#Example_30)

[initiativepass](#initiativepass)

-   [Attributes](#Attributes_31)
-   [Elements](#Elements_31)
-   [Limitations](#Limitations_31)
-   [Example](#Example_31)

[initiativepassadd](#initiativepassadd)

-   [Attributes](#Attributes_32)
-   [Elements](#Elements_32)
-   [Limitations](#Limitations_32)
-   [Example](#Example_32)

[judgeintentions](#judgeintentions)

-   [Attributes](#Attributes_33)
-   [Elements](#Elements_33)
-   [Limitations](#Limitations_33)
-   [Example](#Example_33)

[lifestylecost](#lifestylecost)

-   [Attributes](#Attributes_34)
-   [Elements](#Elements_34)
-   [Limitations](#Limitations_34)
-   [Example](#Example_34)

[liftandcarry](#liftandcarry)

-   [Attributes](#Attributes_35)
-   [Elements](#Elements_35)
-   [Limitations](#Limitations_35)
-   [Example](#Example_35)

[livingpersona](#livingpersona)

-   [Attributes](#Attributes_36)
-   [Elements](#Elements_36)
-   [Limitations](#Limitations_36)
-   [Example](#Example_36)

[matrixinitiative](#matrixinitiative)

-   [Attributes](#Attributes_37)
-   [Elements](#Elements_37)
-   [Limitations](#Limitations_37)
-   [Example](#Example_37)

[matrixinitiativepass](#matrixinitiativepass)

-   [Attributes](#Attributes_38)
-   [Elements](#Elements_38)
-   [Limitations](#Limitations_38)
-   [Example](#Example_38)

[matrixinitiativepassadd](#matrixinitiativepassadd)

-   [Attributes](#Attributes_39)
-   [Elements](#Elements_39)
-   [Limitations](#Limitations_39)
-   [Example](#Example_39)

[memory](#memory)

-   [Attributes](#Attributes_40)
-   [Elements](#Elements_40)
-   [Limitations](#Limitations_40)
-   [Example](#Example_40)

[movementpercent](#movementpercent)

-   [Attributes](#Attributes_41)
-   [Elements](#Elements_41)
-   [Limitations](#Limitations_41)
-   [Example](#Example_41)

[notoriety](#notoriety)

-   [Attributes](#Attributes_42)
-   [Elements](#Elements_42)
-   [Limitations](#Limitations_42)
-   [Example](#Example_42)

[nuyenamt](#nuyenamt)

-   [Attributes](#Attributes_43)
-   [Elements](#Elements_43)
-   [Limitations](#Limitations_43)
-   [Example](#Example_43)

[nuyenmaxbp](#nuyenmaxbp)

-   [Attributes](#Attributes_44)
-   [Elements](#Elements_44)
-   [Limitations](#Limitations_44)
-   [Example](#Example_44)

[quickeningmetamagic](#quickeningmetamagic)

-   [Attributes](#Attributes_45)
-   [Elements](#Elements_45)
-   [Limitations](#Limitations_45)
-   [Example](#Example_45)

[reach](#reach)

-   [Attributes](#Attributes_46)
-   [Elements](#Elements_46)
-   [Limitations](#Limitations_46)
-   [Example](#Example_46)

[restricteditemcount](#restricteditemcount)

-   [Attributes](#Attributes_47)
-   [Elements](#Elements_47)
-   [Limitations](#Limitations_47)
-   [Example](#Example_47)

[selectattribute](#selectattribute)

-   [Attributes](#Attributes_48)
-   [Elements](#Elements_48)
-   [Limitations](#Limitations_48)
-   [Example](#Example_48)

[selectmentorspirit](#selectmentorspirit)

-   [Attributes](#Attributes_49)
-   [Elements](#Elements_49)
-   [Limitations](#Limitations_49)
-   [Example](#Example_49)

[selectparagon](#selectparagon)

-   [Attributes](#Attributes_50)
-   [Elements](#Elements_50)
-   [Limitations](#Limitations_50)
-   [Example](#Example_50)

[selectrestricted](#selectrestricted)

-   [Attributes](#Attributes_51)
-   [Elements](#Elements_51)
-   [Limitations](#Limitations_51)
-   [Example](#Example_51)

[selectside](#selectside)

-   [Attributes](#Attributes_52)
-   [Elements](#Elements_52)
-   [Limitations](#Limitations_52)
-   [Example](#Example_52)

[selectskill](#selectskill)

-   [Attributes](#Attributes_53)
-   [Elements](#Elements_53)
-   [Limitations](#Limitations_53)
-   [Example](#Example_53)

[selectskillgroup](#selectskillgroup)

-   [Attributes](#Attributes_54)
-   [Elements](#Elements_54)
-   [Limitations](#Limitations_54)
-   [Example](#Example_54)

[selectspell](#selectspell)

-   [Attributes](#Attributes_55)
-   [Elements](#Elements_55)
-   [Limitations](#Limitations_55)
-   [Example](#Example_55)

[selectsprite](#selectsprite)

-   [Attributes](#Attributes_56)
-   [Elements](#Elements_56)
-   [Limitations](#Limitations_56)
-   [Example](#Example_56)

[selecttext](#selecttext)

-   [Attributes](#Attributes_57)
-   [Elements](#Elements_57)
-   [Limitations](#Limitations_57)
-   [Example](#Example_57)

[selectweapon](#selectweapon)

-   [Attributes](#Attributes_58)
-   [Elements](#Elements_58)
-   [Limitations](#Limitations_58)
-   [Example](#Example_58)

[sensitivesystem](#sensitivesystem)

-   [Attributes](#Attributes_59)
-   [Elements](#Elements_59)
-   [Limitations](#Limitations_59)
-   [Example](#Example_59)

[skillarticulation](#skillarticulation)

-   [Attributes](#Attributes_60)
-   [Elements](#Elements_60)
-   [Limitations](#Limitations_60)
-   [Example](#Example_60)

[skillattribute](#skillattribute)

-   [Attributes](#Attributes_61)
-   [Elements](#Elements_61)
-   [Limitations](#Limitations_61)
-   [Example](#Example_61)

[skillcategory](#skillcategory)

-   [Attributes](#Attributes_62)
-   [Elements](#Elements_62)
-   [Limitations](#Limitations_62)
-   [Example](#Example_62)

[skillgroup](#skillgroup)

-   [Attributes](#Attributes_63)
-   [Elements](#Elements_63)
-   [Limitations](#Limitations_63)
-   [Example](#Example_63)

[skillsoftaccess](#skillsoftaccess)

-   [Attributes](#Attributes_64)
-   [Elements](#Elements_64)
-   [Limitations](#Limitations_64)
-   [Example](#Example_64)

[skillwire](#skillwire)

-   [Attributes](#Attributes_65)
-   [Elements](#Elements_65)
-   [Limitations](#Limitations_65)
-   [Example](#Example_65)

[smartlink](#smartlink)

-   [Attributes](#Attributes_66)
-   [Elements](#Elements_66)
-   [Limitations](#Limitations_66)
-   [Example](#Example_66)

[softweave](#softweave)

-   [Attributes](#Attributes_67)
-   [Elements](#Elements_67)
-   [Limitations](#Limitations_67)
-   [Example](#Example_67)

[specificattribute](#specificattribute)

-   [Attributes](#Attributes_68)
-   [Elements](#Elements_68)
-   [Limitations](#Limitations_68)
-   [Example](#Example_68)

[specificskill](#specificskill)

-   [Attributes](#Attributes_69)
-   [Elements](#Elements_69)
-   [Limitations](#Limitations_69)
-   [Example](#Example_69)

[spellcategory](#spellcategory)

-   [Attributes](#Attributes_70)
-   [Elements](#Elements_70)
-   [Limitations](#Limitations_70)
-   [Example](#Example_70)

[spelllimit](#spelllimit)

-   [Attributes](#Attributes_71)
-   [Elements](#Elements_71)
-   [Limitations](#Limitations_71)
-   [Example](#Example_71)

[submersion](#submersion)

-   [Attributes](#Attributes_72)
-   [Elements](#Elements_72)
-   [Limitations](#Limitations_72)
-   [Example](#Example_72)

[swapskillattribute](#swapskillattribute)

-   [Attributes](#Attributes_73)
-   [Elements](#Elements_73)
-   [Limitations](#Limitations_73)
-   [Example](#Example_73)

[swimpercent](#swimpercent)

-   [Attributes](#Attributes_74)
-   [Elements](#Elements_74)
-   [Limitations](#Limitations_74)
-   [Example](#Example_74)

[throwrange](#throwrange)

-   [Attributes](#Attributes_75)
-   [Elements](#Elements_75)
-   [Limitations](#Limitations_75)
-   [Example](#Example_75)

[throwstr](#throwstr)

-   [Attributes](#Attributes_76)
-   [Elements](#Elements_76)
-   [Limitations](#Limitations_76)
-   [Example](#Example_76)

[transgenicsgenetechcost](#transgenicsgenetechcost)

-   [Attributes](#Attributes_77)
-   [Elements](#Elements_77)
-   [Limitations](#Limitations_77)
-   [Example](#Example_77)

[unarmedap](#unarmedap)

-   [Attributes](#Attributes_78)
-   [Elements](#Elements_78)
-   [Limitations](#Limitations_78)
-   [Example](#Example_78)

[unarmeddv](#unarmeddv)

-   [Attributes](#Attributes_79)
-   [Elements](#Elements_79)
-   [Limitations](#Limitations_79)
-   [Example](#Example_79)

[unarmeddvphysical](#unarmeddvphysical)

-   [Attributes](#Attributes_80)
-   [Elements](#Elements_80)
-   [Limitations](#Limitations_80)
-   [Example](#Example_80)

[uncouth](#uncouth)

-   [Attributes](#Attributes_81)
-   [Elements](#Elements_81)
-   [Limitations](#Limitations_81)
-   [Example](#Example_81)

[uneducated](#uneducated)

-   [Attributes](#Attributes_82)
-   [Elements](#Elements_82)
-   [Limitations](#Limitations_82)
-   [Example](#Example_82)

[weaponcategorydv](#weaponcategorydv)

-   [Attributes](#Attributes_83)
-   [Elements](#Elements_83)
-   [Limitations](#Limitations_83)
-   [Example](#Example_83)

[Example Interaction](#interaction)

* * * * *

--->

Improvements
------------

The following is a list of all of the Improvements supported by the Improvement Manager. Each defines their use, possible attributes, element names, limitations, and examples. When the item creating an Improvement has a Rating (such as Cyberware and Bioware), numeric values can be replaced with "Rating". When this is used, the Improvement Manager replaces "Rating" with the source item's current Rating value.

### adapsin

Adds the Adapsin special flag to the character which discounts the Essence cost of Cyberware by 10%.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example adds the Adapsin special flag to the character. 

```XML
<bonus> 
  <adapsin /> 
</bonus>
```

### addattribute

Adds a Special Attribute to the character.

##### Attributes

None.

##### Elements

**name** Name of the Special Attribute to add. Acceptable Values: **MAG**, **RES**.

**min** Minimum value allowed for the Special Attribute.

**max** Maximum value allowed for the Special Attribute.

**aug** Maximum Augmented value allowed for the Special Attribute.

**val** Starting value for the Special Attribute.

##### Limitations

None.

##### Example

The following example adds the Magic Special Attribute to a character with a minimum and start value of 1, maximum value and maximum augmented value of 6. 

```XML
<bonus>
  <addaddtribute>
    <name>MAG</name>
    <min>1</min>
    <max>6</max>
    <aug>6</aug>
    <val>1</val>
  </addattribute>
</bonus>
```

### addspell

Adds a Spell to the character. The character will still need to be magic-enabled in some way to allow access to the Spells and Spirits tab.

##### Attributes

**alchemical** Boolean. If present, sets the spell's Alchemical state. 
**extended** Boolean. If present, sets the spell's Extended state.
**limited** Boolean. If present, sets the spell's Limited state. 
**select** String. If present, appends the "select" value to the spell. Typically used to indicate a specific target or element for spells. 
**usesunarmed** Boolean. If present, sets the spell's UsesUnarmed property. This causes the spell's active skill to be calculated using the Unarmed Combat skill of the character instead of the Spellcasting skill. 

##### Elements

None. The innertext of the node should be the name of the spell.

##### Limitations

None.

##### Example

The following example adds the Alter Ballistics spell to a character with the alchemical flag set to true, which will set it as an alchemical preparation. 

```XML
<bonus>
  <addspell alchemical="True">Alter Ballistics</addspell>
</bonus>
```

### addware

Adds Cyberware (or Bioware) to the character.

##### Attributes

None.

##### Elements

**type** String. Optional. If present and set to 'bioware', the ware will be of type Bioware, searching bioware.xml for the relevant object.
**rating** Integer. If present, sets the ware's Rating.
**grade** String. Sets the Grade of the ware to the provided value. Note that this does not respect effects that alter or limit available grades. Whatever grade is provided is what will be selected. 

##### Limitations

None.

##### Example

The following example adds the Busted Ware cyberware with the None cyberware grade.  

```XML
<bonus>
  <addware>
    <name>Busted Ware</name>
    <grade>None</grade>
    <type>Cyberware</type>
  </addware>
</bonus>
```

### adeptlinguistics

Marks a character as having the Linguistics Adept Power (or equivalent) which negates the cost of improving a Language Knowledge Skill to Rating 1.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example marks the character as having the Linguistics Adept Power. 
```XML
<bonus>
  <adeptlinguistics />
</bonus>
```

### adeptpowerpoints

Adjust the number of Power Points an Adept/Mystic Adept has to purchase Adept Powers.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example grants an Adept/Mystic Adept 1 additional Power Point. 

```XML
<bonus>
  <adeptpowerpoints>1</adeptpowerpoints>
</bonus>
```

### armor

Adjusts the character's natural Armor Ratings. These adjustments are applied to all pieces of Armor. A good example of this is the Troll's natural Armor.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example adjusts a character's Ballistic and Impact Armor Ratings by 1. 

```XML
<bonus>
<armor>1</armor>
</bonus>
```

### armorencumbrancepenalty

Adjust the Armor Encumbrance penalty applied to AGI and REA by the specified value.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example reduces the Armor Encumbrance penalty to AGI and REA by 2. 
```XML
<bonus>
<armorencumbrancepenalty>-2</armorencumbrancepenalty>
</bonus>
```

### basicbiowareessmultiplier

Adjust the Essence cost of Basic Bioware by multiplying them by the specified value.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example reduces the Essence cost of Basic Bioware for the character to 90% of its original cost. 

```XML
<bonus>
<basicbiowarewareessmultiplier>90</basicbiowarewareessmultiplier>
</bonus>
```

### basiclifestylecost

Adjusts the cost of all of the character's basic Lifestyles by a percentage. A positive value increase the cost of all basic Lifestyles while a negative decreases it. Basic Lifestyles are only those found in the SR4 book and do not include Advanced Lifestyles, Safehouses, or Bolt Holes.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example decreases a character's basic Lifestyle costs by 10%. 

```XML
<bonus>
<basiclifestylecost>-10</basiclifestylecost>
</bonus>
```

### biowareessmultiplier

Adjust the Essence cost of Bioware by multiplying them by the specified value.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example reduces the Essence cost of Bioware for the character to 90% of its original cost. 
```XML
<bonus>
<biowareessmultiplier>90</biowareessmultiplier>
</bonus>
```

### blackmarketdiscount

Lets the character mark items as receiving a discount from the Black Market Positive Quality.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example lets the character discount items from their Black Market Positive Quality. 

```XML
<bonus>
<blackmarketdiscount />
</bonus>
```

### complexformlimit

Adjusts the number of Complex Forms a character can have in Create Mode by the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example lets a character have one additional Complex Form in Create Mode. 

```XML
<bonus>
<complexformlimit>1</complexformlimit>
</bonus>
```

### composure

Adjusts the character's Composure Special Attribute Test by the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's Composure by 1. 

```XML
<bonus>
<composure>1</composure>
</bonus>
```

### concealability

Adjusts the character's ability to conceal Weapons by the amount specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example improves a character's Concealability by -2. 

```XML
<bonus>
<concealability>-2</concealability>
</bonus>
```

### conditionmonitor

Adjusts the properties of the character's Condition Monitors.

##### Attributes

None.

##### Elements

**physical** Adjusts the number of boxes in the character's Physical Condition Monitor by the value specified.

**stun** Adjusts the number of boxes in the character's Stun Condition Monitor by the value specified.

**threshold** Adjusts the Condition Monitor Threshold by the value specified, meaning the penalties for taking damage shifts by the number of boxes specified.

**thresholdoffset** Adjusts the number of Condition Monitor boxes that appear before the first Condition Monitor penalty.

##### Limitations

None.

##### Example

The following example improves a character's Physical and Stun Condition Monitor by 2 and improves their Threshold by 1 (meaning the penalties appear every 4 boxes instead of 3). 

```XML
<bonus>
<conditionmonitor>
<physical>2</physical>
<stun>2</stun>
<threshold>1</threshold>
</conditionmonitor>
</bonus>
```

### cyberwareessmultiplier

Adjust the Essence cost of Cyberware by multiplying them by the specified percentage.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example reduces the Essence cost of Cyberware for the character to 90% of its original cost. 

```XML
<bonus>
<cyberwareessmultiplier>90</cyberwareessmultiplier>
</bonus>
```

### damageresistance

Grants the character additional dice on their Damage Resistance Tests. This is typically found in Bone Lacing where the character receives additional BOD dice only for Damage Resistance Tests.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example grants a character 2 addition dice on their Damage Resistance Tests. 

```
XML
bonus>
<damageresistance>2</damageresistance>
</bonus>
```

### drainresist

Adjusts the size of the character's Drain Resistance pool by the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example adds 2 dice to the character's Drain Resistance pool. 

```
XML
bonus>
<drainresist>2</drainresist>
</bonus>
```

### enabletab

Gives the character access to one of the special ability tabs: Adept Powers (Adept), Spells and Spirits (Magician), and Sprites and Complex Forms (Technomancers). This can also grant characters built with BP access to the Initiation tab while in Create Mode.

##### Attributes

None.

##### Elements

**name** The name of the special tab to give the character access to. Acceptable values: **adept**, **critter**, **initiation**, **magician**, **technomancer**. This element can be specified multiple times to grant access to multiple special tabs as once.

##### Limitations

This item can only be used once per bonus element. **initiation** should be used only if absolutely necessary. Initiation is automatically enabled when creating a character using Karma or while in Career Mode for characters that should have access to it.

##### Example

The following example grants the character access to the Adept and Magician tabs. 

```XML
<bonus>
<enabletab>
<name>adept</name>
<name>magician</name>
</enabletab>
</bonus>
```

### fadingresist

Adjusts the size of the character's Fading Resistance pool by the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example adds 2 dice to the character's Fading Resistance pool. 

```XML
<bonus>
<fadingresist>2</fadingresist>
</bonus>
```

### flypercent

Adjusts a character's Fly Rate by the specified percent.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example improves a character's Fly Rate by 50%. 

```XML
<bonus>
<flypercent>50</flypercent>
</bonus>
```

### flyspeed

Grants a character a Fly Rate if they do not already have one.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example gives a character a Fly Rate of 100 meters. 

```XML
<bonus>
<flyspeed>100</flyspeed>
</bonus>
```

### freenegativequalities

Adjust the free points worth of Negative Qualities the character may select. This should be a positive value as it is effectively reducing the current value of Negative Qualities the character has selected.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example gives the character 10 BP worth of free Negative Qualities. 
```XML
<bonus>
<freenegativequalities>10</freenegativequalities>
</bonus>
```

### freepositivequalities

Adjust the free points worth of Positive Qualities the character may select. This should be a negative value as it is effectively reducing the current value of Positive Qualities the character has selected.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example gives the character 10 BP worth of free Positive Qualities. 
```
XML
bonus>
<freepositivequalities>-10</freepositivequalities>
</bonus>
```

### freespiritpowerpoints

Adjust the number of Power Points a Free Spirit has to purchase Critter Powers.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example grants a Free Spirit 1 additional Power Point. 
```XML
<bonus>
<freespiritpowerpoints>1</freespiritpowerpoints>
</bonus>
```

### genetechcostmultiplier

Adjust the Nuyen cost of Genetech by multiplying them by the specified value.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example reduces the Nuyen cost of Genetech for the character to 50% of its original cost. 

```XML
<bonus>
<genetechcostmultiplier>50</genetechcostmultiplier>
</bonus>
```

### ignorecmpenaltyphysical

Removes all of the Physical Condition Monitor penalties from the character.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example removes all Physical Condition Monitor penalties from the character. 

```XML
<bonus>
<ignorecmpenaltyphysical />
</bonus>
```

### ignorecmpenaltystun

Removes all of the Stun Condition Monitor penalties from the character.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example removes all Stun Condition Monitor penalties from the character. 
```XML
<bonus>
<ignorecmpenaltystun />
</bonus>
```

### infirm

Makes the character Infirm, meaning they must spend additional BP to take Physical Active Skills.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example makes a character Infirm. 

```XML
<bonus>
<infirm />
</bonus>
```

### initiation

Adds a number of Initiate Grades to the character equal to the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example grants a character 1 Initiate Grade. 

```XML
<bonus>
<initiation>1</initiation>
</bonus>
```

### initiative

Adjusts a character's Initiative by the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's Initiative by 2. 
```XML
<bonus>
<initiative>2</initiative>
</bonus>
```

### initiativepass

Adjusts the number of Initiative Dice a character receives by the number specified. Only the highest Initiative Dice modifier is applied to the character.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's number of Initiative Dice by 1. 
```XML
<bonus>
<initiativepass>1</initiativepass>
</bonus>
```

### initiativepassadd

Adjusts the number of Initiative Dice a character receives by the number specified. This bonus is stacked on top of the highest Initiative Dice modifier the character has from *initiativepass*.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's number of Initiative Dice by 1. 

```XML
<bonus>
<initiativepassadd>1</initiativepassadd>
</bonus>
```

### judgeintentions

Adjusts the character's Judge Intentions Special Attribute Test by the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's Judge Intentions by 1. 

```XML
<bonus>
<judgeintentions>1</judgeintentions>
</bonus>
```

### lifestylecost

Adjusts the cost of all of the character's Lifestyles by a percentage. A positive value increase the cost of all Lifestyles while a negative decreases it.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example decreases a character's Lifestyle costs by 10%. 

```XML
<bonus>
<lifestylecost>-10</lifestylecost>
</bonus>
```

### liftandcarry

Adjusts the character's Lift and Carry Special Attribute Test by the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's Lift and Carry by 1. 

```XML
<bonus>
<liftandcarry>1</leftandcarry>
</bonus>
```

### livingpersona

Adjusts the character's Living Persona Attributes.

##### Attributes

None.

##### Elements

**biofeedback** Adjusts the Living Persona's Biofeedback Filter by the value specified.

**firewall** Adjusts the Living Persona's Firewall by the value specified.

**response** Adjusts the Living Persona's Response by the value specified.

**signal** Adjust the Living Persona's Signal by the value specified.

**system** Adjust the Living Persona's System by the value specified.

##### Limitations

None.

##### Example

The following example improves a Living Persona's Firewall and Response by 1 each. 

```XML
<bonus>
<livingpersona>
<firewall>1</firewall>
<response>1</response>
</livingpersona>
</bonus>
```

### matrixinitiative

Adjusts a character's Matrix Initiative by the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's Matrix Initiative by 4. 

```XML
<bonus>
<matrixinitiative>4</matrixinitiative>
</bonus>
```

### matrixinitiativepass

Adjusts the number of Matrix Initiative Dice a character receives by the number specified. Only the highest Matrix Initiative Pass modifier is applied to the character.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's number of Matrix Initiative Dice by 2. 

```XML
<bonus>
<matrixinitiativepass>2</matrixinitiativepass>
</bonus>
```

### matrixinitiativepassadd

Adjusts the number of Matrix Initiative Dice a character receives by the number specified. This bonus is stacked on top of the highest Matrix Initiative Pass modifier the character has from *matrixinitiativepass*.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's number of Matrix Initiative Dice by 1. 

```XML
<bonus>
<matrixinitiativepassadd>1</matrixinitiativepassadd>
</bonus>
```

### memory

Adjusts the character's Memory Special Attribute Test by the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's Memory by 1. 

```XML
<bonus>
<memory>1</memory>
</bonus>
```

### movementpercent

Adjusts a character's Movement Rate by the specified percent.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example improves a character's Movement Rate by 50%. 

```XML
<bonus>
<movementpercent>50</movementpercent>
</bonus>
```


### movementpercent

Adjusts a character's Movement Rate by the specified percent.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example improves a character's Movement Rate by 50%. 

```XML
<bonus>
<movementpercent>50</movementpercent>
</bonus>
```


### naturalweapon

Adds a 'natural' weapon to the character, as defined by the data in the improvement rather than weapons.xml. 

##### Attributes

None.

##### Elements

**name** Name of the weapon. If not specified, will use the name of the Improvement instead.
**reach** Integer value of the weapons Reach stat. Required.
**damage** String value of the weapon's Damage stat. If not provided, will be empty. 
**ap** String value of the weapons AP stat. If not provided, will be empty. 
**useskill** String value of the Skill that the weapon should use. If not specified will default to empty, which may cause it to use an unexpected skill such as Pistols.
**accuracy** String value of the Accuracy of the weapon. If not provided, will be empty. 
**source** String value of the Sourcebook. Defaults to SR5 if not provided. Note that this will ignore enabled-book status, as the weapon is provided directly rather than by querying the XML data. 
**page** String value of the Page on which the weapon can be found in a sourcebook. Will default to 0. 
##### Limitations

None.

##### Example

The following example creates a natural weapon for a Falconine Shapeshifter.

```XML
<bonus>
  <naturalweapon>
    <name>Bite (Falconine Form)</name>
    <reach>0</reach>
    <damage>(STR+2)P</damage>
    <ap>-1</ap>
    <useskill>Unarmed Combat</useskill>
    <accuracy>Physical</accuracy>
    <source>RF</source>
    <page>105</page>
  </naturalweapon>
</bonus>
```

### newspellkarmacost

Adjust the cost of spells purchased with karma by the selected amount. 

##### Attributes

**type** If present, restricts the types of spells that are discounted. Currently permitted entries are Rituals, Enchantments, Preparations and Spells. 

##### Elements

None.

##### Limitations

None.

##### Example

The following example reduces the cost for Rituals by 1. 

```XML
<bonus>
<newspellkarmacost type="Rituals">-1</newspellkarmacost>
</bonus>
```

### notoriety

Adjusts a character's Notoriety by the specified amount.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example increases a character's Notoriety by 3. 

```XML
<bonus>
<notoriety>3</notoriety>
</bonus>
```

### nuyenamt

Adjusts the amount of Nuyen a character has by the amount specified. This can put characters over the normally allowed amounts during character creation.

##### Attributes

**condition** Specifies a condition under which the Nuyen is used. Only "Stolen" is supported mechanically, see example. Other strings will not be displayed or have any effect.  

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's Nuyen amount by 15000. 

```XML
<bonus>
<nuyenamt>15000</nuyenamt>
</bonus>
```

The following example increases a character's Nuyen amount by 15000, specifying that the money may only be spent on goods flagged as Stolen. 

```XML
<bonus>
<nuyenamt condition="Stolen">15000</nuyenamt>
</bonus>
```
### nuyenmaxbp

Adjusts the maximum number of Build Points that a character can spend on Nuyen during character creation.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element and only applies to the character creation process.

##### Example

The following example increases the number of Build Points a character can spend on Nuyen by 10. 

```XML
<bonus>
<nuyenmaxbp>10</nuyenmaxbp>
</bonus>
```

### qualitylevel

Adds a quality level to the character for a specific quality group. When the character is finalized, the highest level quality from each group will be applied.

##### Attributes

**group** String. Required. The name of the quality group this level belongs to. Must match a quality group defined in qualitylevels.xml.

##### Elements

None. The innertext of the node should be the numeric level value.

##### Limitations

- Multiple quality levels from the same group will be resolved to the highest level only
- The quality group must be defined in qualitylevels.xml
- The level value must correspond to a valid level in the quality group

##### Example

The following example adds level 2 of the SINner quality group to a character.

```xml
<bonus>
  <qualitylevel group="SINner">2</qualitylevel>
</bonus>
```

This would grant the "SINner (Criminal)" quality if the qualitylevels.xml defines:
```xml
<qualitygroup>
  <name>SINner</name>
  <levels>
    <level value="1">SINner (National)</level>
    <level value="2">SINner (Criminal)</level>
    <level value="3">SINner (Corporate Limited)</level>
    <level value="4">SINner (Corporate)</level>
  </levels>
</qualitygroup>
```

##### Usage in Life Modules

Quality levels are commonly used in Life Modules to grant different levels of the same quality type based on the character's background:

```xml
<!-- Life Module: Corporate SIN -->
<bonus>
  <qualitylevel group="SINner">4</qualitylevel>
</bonus>

<!-- Life Module: Criminal Background -->
<bonus>
  <qualitylevel group="SINner">2</qualitylevel>
</bonus>
```

### quickeningmetamagic

Grants the character access to the Quickening Metamagic and allows them to spend Karma on Quickening Spells.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example grants a character access to Quickening. 

```XML
<bonus>
<quickeningmetamagic />
</bonus>
```

### reach

Adjusts the character's Reach by the amount specified. This modifier is applied to all Melee Weapons.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example extends a character's Reach by 1. 

```XML
<bonus>
<reach>1</reach>
</bonus>
```

### restricteditemcount

Adjust the number of items over the starting Maximum Availability that a newly-create character is allowed to carry by the specified value.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example lets a newly-created character carry one item that is over the starting Maximum Availability. 

```XML
<bonus>
<restricteditemcount>1</restricteditemcount>
</bonus>
```

### selectattribute

Adjusts the properties of an Attribute of the user's choice.

##### Attributes

None.

##### Elements

**attribute** When included, limits the Attributes that a user can select. This can appear multiple times, once for each attribute to limit the list to. Acceptable values: **BOD**, **AGI**, **REA**, **STR**, **CHA**, **INT**, **LOG**, **WIL**, **EDG**, **MAG**, **RES**.

**excludeattribute** When included, the standard list of Attributes is shown but excludes the ones specified. This is the opposite of **attribute**.

**aug** Improves the Augmented Maximum value allowed for the Attribute by the specified amount.

**max** Improves the Maximum value allowed for the Attribute by the specified amount.

**min** Improves the Minimum value allowed for the Attribute by the specified amount.

**val** Improves the value of the Attribute by the specified amount.

**affectbase** Whether or not the Improvement should affect the Attribute's base value. The application treats the character's actual Attribute as being a number of points higher which increases the Karma cost of improving the Attribute and the cost of Adept Powers.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example prompts the user to select either the BOD or AGI Attribute, then adjusts its current and Maximum values by 1 and Augmented Maximum by 2. 

```XML
<bonus>
<selectattribute>
<attribute>BOD</attribute>
<attribute>AGI</attribute>
<aug>2</aug>
<max>1</max>
<val>1</val>
</selectattribute>
</bonus>
```

### selectmentorspirit

Prompts the user to select a Mentor Spirit. If the selected Mentor Spirit applies bonuses of its own, it invokes the Improvement Manager with its own bonus node.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example asks the user to select a Mentor Spirit then applies its bonuses. 

```XML
<bonus>
<selectmentorspirit />
</bonus>
```

### selectparagon

Prompts the user to select a Paragon. If the selected Paragon applies bonuses of its own, it invokes the Improvement Manager with its own bonus node.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example asks the user to select a Paragon then applies its bonuses. 

```XML
<bonus>
<selectparagon />
</bonus>
```

### selectpower

Asks the user to select a power.

##### Attributes

**limittopowers** Limit the list of powers to only the ones specified.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example asks the user to select a power.

```XML
<bonus>
<selectpower limittopowers="Critical Strike,Adept Spell" />
</bonus>
```

### selectrestricted

Prompts the user to either select a Restricted item from their current list of equipment or enter a text value. The value entered is displayed next to the item that created the Improvement in parenthesis. This is used by Fake License where characters need to identify what the license if for; either a piece of gear they're carrying or for any other need.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example asks the user to select one of their pieces of Restricted equipment or enter their own text value. 

```XML
<bonus>
<selectrestricted />
</bonus>
```

### selectside

Asks the character to select which side of the body a piece of Cyberware is installed in.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example asks the character to select a side of the body. 

```XML
<bonus>
<selectside />
</bonus>
```

### selectskill

Applies a bonus to an Active Skill selected by the user.

##### Attributes

**excludecategory** Excludes a Category of Active Skills from the list (example: Combat Active).

**limittoskill** Limit the list of Active Skills to only those listed (example: Con, Swimming). This is a comma-separate list.

**skillcategory** Limit the list of Active Skills to only those that belong to the specified Category (example: Combat Active).

**skillgroup** Limit the list of Active Skills to only those that belong to the specified Skill Group (example: Influence).

##### Elements

**max** Increases the maximum Skill Rating by the number specified.

**val** Improves the Skill Rating by the number specified.

**applytorating** The bonus is applied as a modifier to the Skill Rating instead of the Dice Pool. Bonuses applied to the Skill Rating are affected by the Maximum Modified Skill Rating cap.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example improves an Active Skill belonging to the Influence Skill Group of the user's choice by 1. 

```XML
<bonus>
<selectskill skillgroup="Influence">
<val>1</val>
</selectskill>
</bonus>
``` 

The following example improves an Active Skill of the user's choice by 1. 

```XML
<bonus>
<selectskill>
<val>1</val>
</selectskill>
</bonus>
```

### selectskillgroup

Applies a bonus to all Active Skills that belong to a Skill Group selected by the user.

##### Attributes

**excludecategory** Excludes a Category of Active Skills from the receiving bonuses, even if they belong to the Skill Group (example: Combat Active).

##### Elements

**bonus** Improves all of the Active Skills in the selected Skill Group by the number specified.

**applytorating** The bonus is applied as a modifier to the Skill Rating instead of the Dice Pool. Bonuses applied to the Skill Rating are affected by the Maximum Modified Skill Rating cap.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following improves all of the Active Skills belonging to a Skill Group of the user's choice by 1. 

```XML
<bonus>
<selectskillgroup>
<bonus>1</bonus>
</selectskillgroup>
</bonus>
```

### selectspell

Asks the user to select a Spell.

##### Attributes

**category** Limit the list of Spells to only the Category specified.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example asks the user to select a Spell from the Health Category.

```XML
<bonus>
<selectspell category="Health" />
</bonus>
```

### selectsprite

Prompts the user to select a Sprite.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example asks the user to select a Sprite.

```XML
<bonus>
<selectsprite />
</bonus>
```

### selecttext

Prompts the user to enter a text value. The value entered is displayed next to the item that created the Improvement in parenthesis. This is used in cases where users need to select a custom identifier for spells and other objects. For example, the Detect [Object] Spell requires the user specify a physical object that the Spell will affect when they choose to learn it. The Spell would then appear as "Detect [Object] (My Choice)".

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example asks the user to enter a value.

```XML
<bonus>
<selecttext />
</bonus>
```

### selectweapon

Prompts the user to select a Weapon that they currently have.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example asks the user to select a Weapon.

```XML
<bonus>
<selectweapon />
</bonus>
```

### sensitivesystem

Adds the Sensitive System special flag to the character which doubles the Essence cost of all Cyberware implants.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example adds the Sensitive System special flag to a character.

```XML
<bonus>
<sensitivesystem />
</bonus>
```

### skillarticulation

Adjusts the value of all Active Skills that belong to the Physical Active Category and are linked to a Physical Attribute. This is reserved for the Enhanced Articulation piece of Bioware.

##### Attributes

None.

##### Elements

**bonus** Improves the value of the Active Skills by the specified amount.

##### Limitations

None.

##### Example

The following example adjusts the value of all Physical Active Active Skills that are linked to a Physical Attribute by 1.
```XML
<bonus>
<skillarticulation>
<bonus>1</bonus>
</skillarticulation>
</bonus>
```

### skillattribute

Adjusts the value of all Active Skills that are linked to the specified Attribute.

##### Attributes

None.

##### Elements

**name** The Attribute that Active Skills are linked to.

**bonus** Improves the value of the Active Skills by the specified amount.

**exclude** Exclude a specific Skill from receiving the bonus. This it can only be used once per skillattribute element.

**applytorating** The bonus is applied as a modifier to the Skill Rating instead of the Dice Pool. Bonuses applied to the Skill Rating are affected by the Maximum Modified Skill Rating cap.

##### Limitations

None.

##### Example

The following example adjusts the value of all Active Skills that are linked to Strength by 1.

```XML
<bonus>
<skillattribute>
<name>STR</name>
<bonus>1</bonus>
</skillattribute>
</bonus>
```

### skillcategory

Adjusts the value of all Skills that belong to the specified Category. This applies to both Active Skills and Knowledge Skills.

##### Attributes

None.

##### Elements

**name** The name of the Skill Category that Skills are linked to.

**bonus** Improves the value of the Skills by the specified amount.

**exclude** Exclude a specific Skill from receiving the bonus. This it can only be used once per skillcategory element.

**applytorating** The bonus is applied as a modifier to the Skill Rating instead of the Dice Pool. Bonuses applied to the Skill Rating are affected by the Maximum Modified Skill Rating cap.

##### Limitations

None.

##### Example

The following example adjusts all Academic Knowledge Skills by 1 and all Social Active Active Skills by 2, excluding the Intimidation Active Skill.

```XML
<bonus>
<skillcategory>
<name>Academic</name>
<bonus>1</bonus>
</skillcategory>
<skillcategory>
<name>Social Active</name>
<exclude>Intimidation</exclude>
<bonus>2</bonus>
</skillcategory>
</bonus>
```

### skillgroup

Adjusts the value of all Skills that belong to the specified Skill Group.

##### Attributes

None.

##### Elements

**name** The name of the Skill Group that Active Skills are linked to.

**exclude** Exclude a specific Skill from receiving the bonus. This it can only be used once per skillgroup element.

**bonus** Improves the value of the Skills by the specified amount.

**applytorating** The bonus is applied as a modifier to the Skill Rating instead of the Dice Pool. Bonuses applied to the Skill Rating are affected by the Maximum Modified Skill Rating cap.

##### Limitations

None.

##### Example

The following example adjusts all Active Skills in the Electronics Skill Group by 1, excluding the Hardware Active Skill.

```XML
bonus>
<skillgroup>
<name>Electronics</name>
<exclude>Hardware</exclude>
<bonus>1</bonus>
</skillgroup>
</bonus>
```

### skillsoftaccess

Marks the character's as being able to use Knowsoft and Linguasoft Skillsofts.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example marks the character as being able to use Knowsoft and Linguasoft Skillsofts.

```XML
<bonus>
<skillsoftaccess />
</bonus>
```

### skillwire

Sets the character's Skillwires Rating which limits the effective Rating of all Skillsofts the character takes.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example sets the character's Skillwire Rating to 4.

```XML
<bonus>
<skillwire>4</skillwire>
</bonus>
```

### smartlink

Flags the character as having a Smartlink. If a user has the option enabled to include Smartlink bonuses in their firearms Active Skills, this will trigger the Skill bonus.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example flags a character as having a Smartlink.

```XML
<bonus>
<smartlink />
</bonus>
```

### softweave

Adds the Soft Weave special flag to the character which reduces the highest Ballistic and Impact Armor Ratings by the character's STR for purposes of determining Armor Encumbrance.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example adds the Soft Weave special flag to a character.

```XML
<bonus>
<softweave />
</bonus>
```

### specificattribute

Adjusts the properties of a specific Attribute.

##### Attributes

None.

##### Elements

**name** The Attribute that will be affected. Acceptable values: **BOD**, **AGI**, **REA**, **STR**, **CHA**, **INT**, **LOG**, **WIL**, **EDG**, **MAG**, **RES**, **ESS**.

**aug** Improves the Augmented Maximum value allowed for the Attribute by the specified amount. Because the Augmented Maximum value naturally increases if the Maximum increases (calculated as Maximum x 1.5), this bonus is in addition to the naturally increased amount.

**max** Improves the Maximum value allowed for the Attribute by the specified amount.

**min** Improves the Minimum value allowed for the Attribute by the specified amount.

**val** Improves the value of the Attribute by the specified amount.

**affectbase** Whether or not the Improvement should affect the Attribute's base value. The application treats the character's actual Attribute as being a number of points higher which increases the Karma cost of improving the Attribute and the cost of Adept Powers.

##### Limitations

None.

##### Example

The following example adjusts a character's Strength by 2 and Agility by the Rating of the item creating this (such as a piece of Bioware).

```XML
<bonus>
<specificattribute>
<name>STR</name>
<val>2</val>
</specificattribute>
<specificattribute>
<name>AGI</name>
<val>Rating</val>
</specificattribute>
</bonus>
```

### specificskill

Adjusts the value of a specific Active Skill.

##### Attributes

None.

##### Elements

**name** The Active Skill that will be affected.

**bonus** Improves the value of the Active Skill by the specified amount.

**applytorating** The bonus is applied as a modifier to the Skill Rating instead of the Dice Pool. Bonuses applied to the Skill Rating are affected by the Maximum Modified Skill Rating cap.

**max** Increases the maximum Skill Rating by the number specified.

##### Limitations

None.

##### Example

The following example adjusts a character's Dodge Active Skill by 3.

```XML
<bonus>
<specificskill>
<name>Dodge</name>
<bonus>3</bonus>
</specificskill>
</bonus>
```

### spellcategory

Adjusts the size the character's dice pool for casting Spells that belong to the specified Category.

##### Attributes

None.

##### Elements

**name** The name of the Spell Category.

**value** Improves the size of the character's dice pool for Spells belonging to the category by the specified amount.

##### Limitations

None.

##### Example

The following example adjusts the character's dice pool for Combat Spells by 2.

```XML
<bonus>
<spellcategory>
<name>Combat</name>
<bonus>2</bonus>
</spellcategory>
</bonus>
```

### spelllimit

Adjusts the number of Spells a character can have in Create Mode by the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example lets a character have one additional Spell in Create Mode.

```XML
<bonus>
<spelllimit>1</spelllimit>
</bonus>
```

### submersion

Adds a number of Submersion Grades to the character equal to the number specified.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example grants a character 1 Submersion Grade.

```XML
<bonus>
<submersion>1</submersion>
</bonus>
```

### swapskillattribute

Swaps the Physical Attribute for all Active Skills with the matching Mental Attribute. This is primarily used by the Mind Over Matter Adept Power.

##### Attributes

None.

##### Elements

**attribute** Further limits the list of Physical Attributes the character can select from.

##### Limitations

None.

##### Example

The following example lets the character select either STR or BOD.

```XML
<bonus>
<swapskillattribute>
<attribute>BOD</attribute>
<attribute>STR</attribute>
</swapskillattribute>
</bonus>
``` 

The following example lets the character select any Physical Attribute.

```XML
<bonus>
<swapskillattribute />
</bonus>
```

### swimpercent

Adjusts a character's Swim Rate by the specified percent.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example improves a character's Swim Rate by 50%.

```XML
<bonus>
<swimpercent>50</swimpercent>
</bonus>
```

### throwrange

Adjusts a character's effective Strength for the purpose of determining the range of Throwing Weapons.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example adds +2 to the character's STR for the purpose of determining the range of Throwing Weapons.

```XML
<bonus>
<throwrange>2</throwrange>
</bonus>
```

### throwstr

Adjusts a character's effective Strength for the purpose of determining the DV of Throwing Weapons.

##### Attributes

None.

##### Elements

None.

##### Limitations

None.

##### Example

The following example adds +2 to the character's STR for the purpose of determining the DV of Throwing Weapons.

```XML
<bonus>
<throwstr>2</throwstr>
</bonus>
```

### transgenicsgenetechcost

Adjust the Nuyen cost of Transgenics Genetech by multiplying them by the specified value.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example reduces the Nuyen cost of Transgenics Genetech for the character to 50% of its original cost.

```XML
<bonus>
<transgenicsgenetechcost>50</transgenicsgenetechcost>
</bonus>
```

### unarmedap

Adjusts the character's unarmed Armor Penetration by the amount specified. This applies only to a character's natural, unarmed attacks. It does not affect the unarmed attack damage for Bone Lacing or Weapons in the Unarmed Category.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's unarmed AP by -2.

```XML
<bonus>
<unarmedap>-2</unarmedap>
</bonus>
```

### unarmeddv

Adjusts the character's unarmed Damage Value by the amount specified. This applies only to a character's natural, unarmed attacks. It does not affect the unarmed attack damage for Bone Lacing or Weapons in the Unarmed Category.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example increases a character's unarmed DV by 2.

```XML
<bonus>
<unarmeddv>2</unarmeddv>
</bonus>
```

### unarmeddvphysical

Marks the character's natural Unarmed attack as doing Physical instead of Stun damage.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example changes a character's Unarmed attack to Physical damage.

```XML
<bonus>
<unarmeddvphysical />
</bonus>
```

### uncouth

Makes the character Uncouth, meaning they must spend additional BP to take Social Active Skills.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example makes a character Uncouth.

```XML
<bonus>
<uncouth />
</bonus>
```

### uneducated

Makes the character Uneducated, meaning they must spend additional BP to take Technical Active Skills, Academic Knowledge Skills, and Professional Knowledge Skills.

##### Attributes

None.

##### Elements

None.

##### Limitations

This item can only be used once per bonus element.

##### Example

The following example makes a character Uneducated.

```XML
<bonus>
<uneducated />
</bonus>
```

### weaponcategorydv

Adjusts the Damage of all weapons in a Category by the specified amount.

##### Attributes

None.

##### Elements

**name** The Weapon Category that will be affected.

**bonus** Improves the Damage of all Weapons in the Category by the specified amount.

##### Limitations

None.

##### Example

The following example adjusts the Damage a character does with Clubs by 1. 

```XML
<bonus>
<weaponcategorydv>
<name>Clubs</name>
<bonus>1</bonus>
</weaponcategorydv>
</bonus>
```



### Interactions
#### Weapon accessory giving adept power
##### custom_weapons
```XML
    <accessory>
      <id>[id]</id>
      <name>[Name of accessory]</name>
      <mount></mount>
      <avail>0</avail>
      <cost>0</cost>
      <source>[some book]</source>
      <page>0</page>
      <accuracy>1</accuracy>
      <conceal>0</conceal>
      <dicepool>0</dicepool>
      <rating>0</rating>
      <required>
        <weapondetails>
          <type>Melee</type>
        </weapondetails>
      </required>
	  <gears>
        <usegear>
          <name>[Name of gear]</name>
          <category>[category of gear]</category>
	  <addimprovements>True</addimprovements>
        </usegear>
      </gears>
    </accessory>
```
##### custom_gear
```XML
    <gear>
      <id>[id]</id>
      <name>[Name of gear]</name>
      <category>[category of gear]</category>
      <avail>0</avail>
      <cost>0</cost>
      <source>[source]</source>
      <page>0</page>
      <bonus>
        <specificpower>
          <name>Elemental Weapon</name>
        </specificpower>
      </bonus>
    </gear>
```