Structure
```xml
<chummer>
   <martialarts>
      <martialart />
   </martialarts>
   <maneuvers>
      <maneuver />
   </maneuvers>
</chummer>
```

[martialart](#martialart) nodes describe different Martial Arts.

[technique](#technique) nodes describes the different Techniques available to be taken by Martial Arts.

## martialart Node
```xml
<martialart>
   <id />
   <name />
   <isquality />
   <cost />
   <hide />
   <ignoresourcedisabled />
   <bonus />
   <alltechniques />
   <techniques />
   <forbidden />
   <required />
   <source />
   <page />
</martialart>
```

**id** (required): a unique identifier (GUID) for the Martial Art.

**name** (required): the name of the Martial Art.

**isquality** (optional): the martial art _only_ comes from the addmartialart improvement type. Used to hide the martial art from selection and prevent it from being removed independently of the object that provided it.

**cost** (optional): the Karma cost of the martial art.

**hide** (optional): Whether the martial art is hidden from user selection. Generally used to provide a martial art via the addmartialart improvement.

**ignoresourcedisabled** (optional): Whether the art should be available for the user to take separate from its parent sourcebook being enabled.

**bonus** (optional): a bonus node that describes any bonuses this entry grants. See [Improvement Manager](Improvement-Manager "Improvement Manager") for more information.

**alltechniques** (optional): Whether the art should have all techniques available to it. 

**techniques** (optional): List of techniques that are available to be added to the character when this martial art is present. See  [techniques](#techniques) Node for further details. 

**notes** (optional): Descriptive text that will be added to the Martial Art.

**source** (required): the code for the Sourcebook that this entry comes from. See [Books](Books).

**page** (required): the page number this item can be found on in the Sourcebook.

**forbidden** (optional): XML node containing a list of conditions that will prevent this Martial Art from being available to the character if they are met. See [Conditionals](Conditionals) for more detail.

**required** (optional): XML node containing a list of conditions that will prevent this Martial Art from being available to the character if they are NOT met. See [Conditionals](Conditionals) for more detail.

**notes** (optional): Descriptive text that will be added to the Martial Art.

## techniques Node
```xml
<technique>
   <name />
</technique>
```

**name** (required): the name of the Martial Art Technique that will be available to add to the character.

## technique Node
```xml
<technique>
   <id />
   <name />
   <ignoresourcedisabled />
   <bonus />
   <notes />
   <source />
   <page />
   <forbidden />
   <required />
   <notes />
</technique>
```
**id** (required): a unique identifier (GUID) for the Martial Art Technique.

**name** (required): the name of the Maneuver.

**ignoresourcedisabled** (optional): Whether the item should be available for the user to take separate from its parent sourcebook being enabled.

**bonus** (optional): a bonus node that describes any bonuses this entry grants. See [Improvement Manager](Improvement-Manager "Improvement Manager") for more information.

**notes** (optional): Descriptive text that will be added to the Martial Art.

**source** (required): the code for the Sourcebook that this entry comes from. See [Books](Books)

**page** (required): the page number this item can be found on in the Sourcebook.

**notes** (optional): Descriptive text that will be added to the Martial Art Technique.