# Developer Guide

This section contains technical documentation for developers contributing to Chummer5a.

## 📖 Available Documentation

- **[Contributing](Contributing)** - Complete guide to contributing to the project
  - Bug reporting guidelines
  - Git workflow and pull requests
  - Code standards and EditorConfig
  - Adding and correcting data content
  - Character sheet modifications
  - Graphics and translations
  - C# code contributions

- **[Custom Data Files](Custom-Data-Files)** - Working with custom data files
- **[Creating Custom Metatypes](Creating-Custom-Metatypes)** - Creating custom metatypes

## 🛠️ Development Setup

1. **Clone the repository** - `git clone https://github.com/chummer5a/chummer5a.git`
2. **Install Visual Studio** - Community edition is free
3. **Open the solution** - `Chummer.sln`
4. **Build the project** - Follow the build instructions
5. **Read the contributing guide** - [Contributing](Contributing)

## 🔧 Development Tools

- **Visual Studio** - Recommended IDE
- **XML Editor** - For data file editing (Notepad++ works well)
- **XSL/XSLT Editor** - For character sheet modifications
- **Image Editor** - For graphics assets
- **Git** - For version control

## 📋 Development Workflow

1. **Create a branch** - `git checkout -b feature/your-feature-name`
2. **Make your changes** - Follow the coding standards
3. **Test your changes** - Ensure everything compiles and works
4. **Commit your changes** - Use descriptive commit messages
5. **Push your branch** - `git push origin feature/your-feature-name`
6. **Create a pull request** - Submit for review

## 🎯 Key Areas

- **Data Files** - XML files in `Chummer/data/`
- **Character Sheets** - XSL/XSLT files in `Chummer/sheets/`
- **Translations** - XML files in `Chummer/lang/`
- **Code** - C# files throughout the project
- **Graphics** - PNG files in `Chummer/Resources/`

## 📚 Additional Resources

- [Game Content Documentation](Game-Content-Documentation) - Game-specific information
- [User Guide](User-Guide) - End-user documentation
- [GitHub Issues](https://github.com/chummer5a/chummer5a/issues) - Bug reports and feature requests
