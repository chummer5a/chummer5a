Life Modules Documentation
==========================

This document details how to format the lifemodules.xml file in chummer5a  
The nodes <[bonus]> and <[addquality]> only have a rudimentary explanation and are
better detailed in [bonus] and [addquality]

**NOTICE:** If you are unfamiliar with XML, please see this tutoral by [w3shools.com](http://www.w3schools.com/xml/)


Table of Contents
-----------------


File layout
-----------
Under the root node `chummer`, there are 3 nodes used by life modules. Those are `stages`, `modules` and `storybuilder`.

### stages  
Stages contain information about the order the different life modules are presented in.  
Once reaching the last stage, Chummer5a will allow you to continue adding more modules from that stage.

### modules  
Modules contain the acctual modules that can be added to a character, including what bonuses they give, their cost, requirements and possible versions.

### storybuilder
Storybuilder is a feature on life modules where each module can contain a small piece of backstory based on that module. 
Then, as multiple life modules are added, a small backstory is written for that character, that can then either be the backstory, provide inspiration or scraped in favour of another backstory


Stages
------
A stage module looks like this
```XML
<stage order="index">Stage</stage>
```
Stage is the name of the stage and index is the 1 based index of that stage.  
Each stage is placed under `chummer/stages` and looks like this
```XML
<stages>
  <stage order="1">Nationality</stage>
  <stage order="2">Formative Years</stage>
  <stage order="3">Teen Years</stage>
</stages>
```

It is possible to add the last stage multiple times, increasing the number of stages will lead to Chummer5a automaticaly detecting the increase and changing the last.  
If a gap is placed in the index, Chummer5a will offer no options for continuing, halting progress.

Modules
-------
### Basics
The most basic module looks like this
```XML
<module>
    <id>GUID HERE</id>
    <stage>The stage</stage>
    <category>LifeModule</category>
    <name>The name</name>
    <karma>a number</karma>
    <source>A 2 letter book acronym</source>
    <page>A page</page>
</module>
```
The `id` is an [GUID](https://en.wikipedia.org/wiki/Globally_unique_identifier)(Global Unique IDentifier), 
this value is used by Chummer5a internaly to recgonise different modules. 
If this field is not present, Chummer5a will be unable to load the life module.  
You can leave this field out and once you are finished use the tool ChummerTools.exe to generate all missing `id`s or 
use [this](https://www.guidgenerator.com/) tool to generate them. You can type them by hand, making them up, but this is disencuraged.


The `stage` is the stage this life module is placed in. This have to be equal to one of the Stages defined in `chummer/stages`

The `catagory` have to be "LifeModule". If `catagory` is not "LifeModule" or `catagory` is absent, Chummer5a will try loading the life module as a Positive Quality, leading to all sorts of bad.

The `name` is the name of the Life Module. This is the value displayed to the user.

The `karma` field is the price of the Life Module. If this value fails to evaluate to a number 
(or it evaluates to a number bigger than 2^31 -1) the Life Module will be free. 
Negative values are possible for Life Modules that give karma for having them.

The `source` field is the book this Life Module came from. In most cases this will be RF but it can be DT or CF too. For all possible books, see [books.xml](https://github.com/chummer5a/chummer5a/blob/master/Chummer/data/books.xml).
Leaving out this field will make the Life Module not show up.

The `page` field is the page this Life Module is on. Chummer5a have a feature to open your PDF reader onto that page, but said feature is currently broken.
Leaving out this field will result in users being unable to select said life module and add it to a character.

This is the bare minimum required to make a Life Module, but said Life Module will do nothing.
In the following selection we will explore how to make a Life Module add bonuses to a character, 
add requirements for it showing up, bundle qualities and automaticaly writing a backstory.

### The `bonus` node
This will only give a brief discription on how the [bonus] node works. See [bonus] for all possible options.

### The `versions` node

### The `addquality` node
This will only give a brief discription on how the [addquality] node works. See [addquality] for all possible options.

### The `story` node

### The `forbidden` and `required` nodes




Storybuilder
------------
### Basics

Storybuilder is a feature that dynamicaly creates a backstory based on selected life modules.

In the most simple mode of operation it just adds the `story` nodes to the backstory, 
but it have a macro feature that can replace text. 

the most simple macro is a node placed under `chummer/storybuilder`  
```XML
<anything>You can do anything if you try hard enough</anything>
```

You then invoke this macro by in a `story` node writing something like this  
```XML
<story>
Hell-0 have always had the mantra of saying " $ANYTHING " when he have problems. 
Now a lot of other people are also saying $ANYTHING
</story>
```

Now at runtime, this will be turned into:
>Hell-0 have always had the mantra of saying "You can do anything if you try hard enough" when he have problems. 
>Now a lot of other people are also saying You can do anything if you try hard enough

The one space on each side of a macro will vanish, when having multiple macros after eachother, only one space between each macro is required 
**NOTICE:** macros can be in both upper and lower case, in text but for ease of reading type them in UPPERCASE to make it easy to see if something is a macro.
In `chummer/storybuilder` they have to be lovercase

Macros can contain other macros, up to a depth of 5.

### Random
In a macro, instead of including text, you can instead define a child named `random` and then define several children   
```XML
<rmega>
    <random>
        <ares>Ares</ares>
        <aztech>Aztechnology</aztech
        <renraku>Renraku</renraku>
        <sk>Saeder-Krupp</sk>
    </random>
</rmega>
```
Then, when typing $RMEGA, it will evaluate to of the 4 Megas defined above.
Additionaly, you can limit the selection to only some possibilities by typing
`$RMEGA(ARES|SK)`
This will chose either the value under the `<ares>` tag or the `<sk>` tag.  
You can use more than one | to allow more than 2 values
`$RMEGA(ARES|AZTECH|RENRAKU|SK)` is a legal option.

Additionaly, you can also use ! to signify NOT  
`$RMEGA(!ARES)` evaluates to a Mega, except Ares

This can again be chained 
`$RMEGA(!ARES|SK)` is a Mega, except for SK or Ares  
Only one ! is needed or allowed

Macros inside parenthesis are not evaluated so it is not possible to do
$RMEGA(!$OTHERMEGA)

### Persistent
it is also possible to place a `persistent` node instead of a `random`
```XML
<body>
    <persistent>
        <head>side of the head</head>
        <shoulder>shoulder</shoulder>
        <arm>upper arm</arm>
        <hand>hand</hand>
    </persistent>
</body>
```
This works the same way as `random` except for one small change. It is only random once.  
`Hell-0 was once shot in the $BODY , now whenever he runs he hurts in the $BODY because of the bullet there`  
Here, each `$BODY` will evaluate to the same, meaning it can be reused. This is persistent over multiple story blocks. 

Limiters works the same as in random. Just be carefull as the excluded choice can have been chosen earlier. 

Additionaly, persistent have another feature. adding a _ after macro, you can have a different pools of persistent randoms.  
So while `$BODY` works as normal, you can add `$BODY_BUDDY` and have that be a different value. Later calls to `$BODY_BUDDY` will then evaluate to the same as the orginal `$BODY_BUDDY`

You can also use this feature to peek into the result of another persitent
if you have the macro
```XML
<hurt>
    <persistent>
        <head>headaches</head>
        <shoulder>pain moving</shoulder>
        <arm>pain moving</arm>
        <hand>unsteady hands</hand>
    </persistent>
</hurt>
```

and then calling `$HURT_BODY` the selected value will be based on the value used by $BODY. If $BODY have not yet been called, $HURT_BODY will change all sequent calls to `$BODY`

This feature can also be used to get a specific value from a random.  
By first calling `$HURT` and then calling `$RMEGA_HURT` it will try to get a Mega named with the key `head`, `shoulder`, `arm` or `hand`. (I didn't want to type out a working example )

It is possible to define a `<default>` key under both `<random>` and `<persistent>` that will be accessed if you try to read a missing value. This value will not be chosen in other cases.

If a node contains both a `<random>` and a `<persistent>` node, the `<random>` node will have precedence

### System Macros
Other than the macros defined in `storybuilder` there are a few macros defined in code, using other info from the character.

`$STREET`  
This is the characters street name, as defined on the front page of the character sheet.

`$REAL`  
This is the characters real name, as defined on the "info" tab on the character sheet.

`$YEAR`  
This is the year the character was born in, based on the "age" field on the "info" tab on the character sheet.
This assumes the campain is happening in 2075.

If the age field is empty, or the age field fails to evaluate to a number, it will write an error.

`$YEAR_XX`
This is the year, the character turned XX years old.

If XX fails to evaluate to a number, it will behave as `$YEAR`

`$METATYPE`  
This evaluates to the name of the chracters metatype.

`$METAVARIANT`
This evaluates to the name of the characters metavariant.  
Please note that no metavariant evaluates to human, "elf", "dwarf", "elf" or "troll", not "none"

Those can be used as for other macros as a pool, meaning you can define a macro with the keys `troll`, `ork`... and access them by `$MACRO_METATYPE`  
Those can also be used with the non default pool, to get a random metatype.

Please note that in the beta version of Life Modules for chummer5a, `$METAVARIANT` behaves as `$METATYPE` instead.

`$DOLLAR`  
This evaluates to the $ sign.





[bonus]: javascript:alert("broken")
[addquality]: javascript:alert("broken")
[XML]: http://www.w3schools.com/xml/