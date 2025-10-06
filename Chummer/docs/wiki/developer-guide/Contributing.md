# How to Contribute

Contributions take many forms from submitting issues, writing docs, to making
code changes - we welcome it all!

## Getting Started

If you don't have a GitHub account, you can [sign up](https://github.com/signup/free)
as it will help you to participate with the project.

If you are looking to contribute to the codebase and don't already have a preferred IDE, 
we recommend you have Visual Studio 2015 installed - you can download the Community edition from
[here](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx)

If you are running GitHub Desktop, you can clone this repository locally from
GitHub using the "Clone in Desktop" button from the chummer5a project page,
or run this command in your Git-enabled shell:

`git clone https://github.com/chummer5a/chummer5a.git Chummer5a`

## Reporting Bugs

Even if you do not have the time and/or skills to contribute to Chummer5a in any other way, you can always help out by reporting bugs and crashes. After all, we cannot fix a bug that we do not know about.

You can submit bugs and crashes to [Chummer5a's GitHub issues page](https://github.com/chummer5a/chummer5a/issues), which can also be quickly visited from within the Chummer5a program through `Help > Issue Tracker`. To make sure bugs and crashes are fixed quickly, here are additional steps you can take:

* Make sure that you write specifically about the bug you experienced, not about any guesswork you did afterwards. Jumping to conclusions or following faulty assumptions can very easily lead us devs astray, so unless you can specifically point to a section of the source code that is at fault, only report what you can see. For example, if a value is not what it should be, that is what you should report as a bug, not any guesses or assumptions of why you think that value would not be what it should be.
* Make sure that you are using the latest version of Chummer5a, either on [the stable branch](https://github.com/chummer5a/chummer5a/releases/latest) or [the nightly branch](https://github.com/chummer5a/chummer5a/releases). It's possible that the bug/crash you encountered has already been fixed, you just haven't been using the fixed version of the program.
* If you encounter the bug/crash in the latest stable version of Chummer5a, make sure that it is still present when using [the latest nightly version](https://github.com/chummer5a/chummer5a/releases). It's possible that the bug/crash you encountered has already been fixed, but the fix has not been pushed to a stable release.
* Make sure you describe the exact steps you took to encounter the bug/crash, and that the bug/crash can be reliably replicated just by following those steps.
* If you only encounter the bug/crash with a particular character file, make sure you upload the character's .chum5 save.
* If your operating system's language is not US English, please make note of this. Some bugs/crashes are related to mishandling of number separators (thousands separators, decimal symbol), so this information could help clue us in.

## Getting Started: Git, Pull Requests, and EditorConfig

If you wish to contribute to Chummer5a beyond just submitting issues for bugs and crashes, you will need to use Git. Git is how all projects on GitHub, including Chummer5a, manage and synchronize changes between multiple contributors.

If you are not already familiar with Git, GitHub has [a user-friendly tool for interacting with GitHub projects](https://desktop.github.com/), paired with [a decent guide to get you started with it](https://help.github.com/desktop/guides/). You can also find all kinds of tutorials online if you're still having trouble using GitHub Desktop.

In most cases, you will not have permission to directly push changes to the main Chummer5a repository. What you can do though is create your own, personal copy of the main Chummer5a repository (called a "branch" or a "fork"), make changes to that copy, and when you're done, submit your changes in a large package called a [Pull Request](https://help.github.com/articles/about-pull-requests/). Maintainers of the main Chummer5a repository will then be able to see all your changes in the Pull Request, and after some review, will most likely merge them into the main repository. Once that happens, your changes will show up for everyone working with Chummer5a's code, and your name will automatically start showing up in the list of Chummer5a contributors.

Chummer5a uses an [EditorConfig](http://editorconfig.org/) file to maintain text standards of its filetypes. It is recommended that whatever editor you use to modify Chummer5a files, you install support for EditorConfig if it is not already present. If you cannot do this, then try to actively follow the standards set in the `[repository]\.editorconfig` file: use utf-8 encoding, use 2 spaces for indents in .xml, .xsl, .xsd, and .xslt files and 4 spaces for indents in all other files, do not insert spaces at the end of lines, insert a new line at the end of each file, and use Windows-style line endings (`CR LF`) instead of Linux-style (`LF`) or Macintosh-style (`CR`).

## Adding and Correcting Base Data Content

Additional Tools Required:
* An XML Editor of your choice. A bare-bones text editor like Notepad can do the trick, but you will probably want to use something more geared towards editing XML files, e.g. [Notepad++](https://notepad-plus-plus.org/), to help you out.

Data in Chummer5a are stored as XML files that are loaded and parsed when the program is run. If something has an incorrect cost, statblock, or effect, then chances are high that this can be fixed just by modifying data files.

All of Chummer5a's base data files are stored in the `[repository]\Chummer\data` directory as .xml files, each named for the type of data they contain. You should be able to familiarize yourself with how each type of data is constructed by looking at existing entries, but if you are having trouble, this wiki should contain explanations for each data type.

If you are adding new entries to a data file, not just changing existing entries, then you should make sure of two things:
1. There are some XML nodes that must be specified for each data type. Exploring the .xsd XML schema file corresponding to a given data type should tell you what nodes are absolutely necessary (they will have the `minOccurs` attribute set to something other than `0`); .xsd files can usually be opened and explored with the same text editor used to modify .xml files. For the most part though, if you start with a copy of an existing entry and don't delete any nodes, you should be fine. While .xsd files can be out-of-date in certain cases, they can be relied upon 100% of the time if you are just checking which nodes are mandatory and which are optional.
2. If the data type has an `id` node, then you must fill it with a GUID that is unique from all other entries'. GUIDs can be automatically generated using programs that are tooled to generate them, e.g. Visual Studio, or using websites built for generating GUIDs, like [this one](https://guidgenerator.com/).

## Correcting Character Sheets

Additional Tools Required:
* An XSL/XSLT Editor of your choice. A bare-bones text editor like Notepad can do the trick, but you will probably want to use something more geared towards editing XSL/XSLT files, e.g. [Notepad++](https://notepad-plus-plus.org/), to help you out.

Character sheets in Chummer5a are CSS- and Javascript-enriched HTML files built on-the-fly using XSL and XSL templates. If a character sheet isn't displaying some information correctly, or there is some undesirable element of a character sheet, then chances are high that this can be fixed by modifying the .xsl and .xslt files responsible for constructing character sheets.

All of Chummer5a's character sheet files are stored in the `[repository]\Chummer\sheets` directory as .xsl and xslt files. The character sheets themselves are .xsl files, but they rely on whatever .xslt files are imported at the beginning of the file, and those .xslt files can import additional .xslt files. The core template section in control of building the bulk of the character sheet's HTML is always wrapped within `<xsl:template match="/characters/character">`.

The character sheet preview window in Chummer5a is currently a WinForms webBrowser object, meaning it uses the version of Internet Explorer that is installed on the current user's system as its HTML renderer. The Save to PDF option uses the `wkhtmltopdf.exe` tool, which uses a version of WebKit as its HTML renderer.

If you want a complete list of all the variables printed with a character, please consult the `Character::PrintToStream()` method within `[repository]\Chummer\Classes\clsCharacter.cs`.

## Correcting and Enhancing Existing Graphics

Additional Tools Required:
* An image manipulation tool that is able to export into .png and/or .ico file formats.
* Optional: a tool for converting images into Base64 strings.

Chummer5a does not currently use a lot of custom graphical assets. All current assets are located in the `[repository]\Chummer\icons` and the `[repository]\Chummer\Resources` directories as .png files, and the `[repository]\Chummer\chummer.ico` file is used as the program's icon. Chummer5a uses nothing but built-in font types.

If you want to add images to a character sheet, the image must first be converted into a Base64 string. Then, an `<img />` HTML tag must be added to the desired place in the .xsl/.xslt file responsible for the given character sheet, and the HTML tag must be set up to use the Base64 string as its source.

## Correcting Translations/Localizations

Additional Tools Required:
* An XSL/XSLT and XML Editor of your choice. A bare-bones text editor like Notepad can do the trick, but you will probably want to use something more geared towards editing XSL/XSLT and XML files, e.g. [Notepad++](https://notepad-plus-plus.org/), to help you out.

Localizations in Chummer5a are split into two components: translations for the program itself and translations for character sheets.

Translations for the program itself are controlled by .xml files within `[repository]\Chummer\lang`. Changing the data there follows similar rules as adding or correcting base data content. However, you may also use a tool included in the Chummer5a download called `Translator.exe` as another way to modify translations for the main program.

Translations for the character sheets are split between .xsl/.xslt files located in `[repository]\Chummer\sheets\[language]` and the `[repository]\Chummer\data\sheets.xml` file. The former controls localized content within the character sheets themselves, while the latter dictates the character sheet names and filenames that show up within the main Chummer5a program. Modifying the former follows the same rules as correcting character sheets, while modifying the latter follows the same rules as adding or correcting base data content.

Note that adding new character sheets to sheets.xml is not enough to actually add new character sheets for others to download. See the instructions for adding new files to see what extra steps need to be taken for new sheets to show up in the Chummer5a download.

## Correcting Chummer5a Code

Additional Tools Required:
* A C# Editor of your choice. A bare-bones text editor like Notepad can do the trick, as can something richer like [Notepad++](https://notepad-plus-plus.org/), but it is highly recommended that you use some version of Visual Studio instead, as that will let you graphically interact with Chummer5a's forms and make sure any new code compiles correctly. [The latest version of Visual Studio Community](https://www.visualstudio.com/vs/community/) can be always be downloaded for free and used on a Windows-based system, but if you have licenses for Visual Studio Professional and/or Enterprise, those will work as well.

The bulk of Chummer5a is written in C# using libraries included in .NET v4.5 and later. The current UI is built on top of the older WinForms platform. A conversion to the newer WPF platform is being talked about (current ETA: the current year in Shadowrun 5e). Conversions to more multi-platform bases like Xamarin may be possible, but they are even less likely to happen than a conversion to WPF. 

The codebase for Chummer5a has grown rather large over the decades of development time that have gone into it and its 4e predecessor, so your best bet to familiarize yourself with its code is just to spend a lot of time reading it and jumping around between references and function calls. If you do make changes to Chummer5a's C# code, try to use the same form of Hungarian notation that is mostly present in the rest of the codebase, and try to maintain good programming practices. Try to write code that is as legible and easy-to-understand as possible, because you aren't going to be the only one having to deal with and possibly fix bugs in it.

Do not forget to run all T4 macros before compiling Chummer5a. If using Visual Studio, this can be done through the `Build > Transform All T4 Templates` option. Currently, there are two T4 templates within Chummer5a: the first creates a .cs file that references all contributors to the Chummer5a repository, and the second creates a .cs file containing a dictionary to all the methods in `[repository]\Chummer\Classes\AddImprovementCollection.cs` for use when processing `<bonus>` tags in data content.

## Adding New Files

Additional Tools Required:
* A tool that can edit .csproj files. A bare-bones text editor like Notepad can do the trick, as can something richer like [Notepad++](https://notepad-plus-plus.org/), but it is highly recommended that you use some version of Visual Studio instead. [The latest version of Visual Studio Community](https://www.visualstudio.com/vs/community/) can be always be downloaded for free and used on a Windows-based system, but if you have licenses for Visual Studio Professional and/or Enterprise, those will work as well.

If you wish to add new files to Chummer5a, whether they be new data files, new localizations, new character sheets, or new graphical assets, you will need to also add references to those files in Chummer5a's Visual Studio project file and make sure they are copied when the Chummer5a project is built.

You may do this graphically by opening up the `[repository]\Chummer.sln` file within Visual Studio. Right-click on the Chummer project or a folder therein within the Solution Explorer, and use the `Add > Existing Item` and `Add > New Folder` options within the right-click menu to populate the project with entries for your new files. Finally, select each newly added file, view their Properties, and select `Copy always` or `Copy if newer` as the value for the `Copy to Output Directory` property.

Alternatively, you may directly modify the `[repository]\Chummer\Chummer.csproj` file with a text editor and add `<Content>` entries for your new files to the appropriate `<ItemGroup>` and with the appropriate `<CopyToOutputDirectory>` child element.

You will need to follow similar steps for new .cs files, but those files should be set as source code to compile with the build, rather than content to be copied.
