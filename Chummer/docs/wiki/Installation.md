# Installation Guide

Complete installation instructions for Chummer5a on all supported platforms.

## Windows Installation

1. **Download the latest release** from our [releases page](https://github.com/chummer5a/chummer5a/releases)
2. **Extract the files** to a folder of your choice
3. **Run Chummer5.exe** - The application should start immediately

### Prerequisites
- **.NET Framework 4.8** - Download from [Microsoft's website](https://support.microsoft.com/en-us/topic/microsoft-net-framework-4-8-offline-installer-for-windows-9d23f658-3b97-68ab-d013-aa3c3e7495e0) if not already installed
- **Windows 7 or later** - The application requires a modern Windows version

### Security Warnings
Windows SmartScreen and antivirus software may warn you that running the program is unsafe. This is because:
- The auto-updater downloads files from the internet
- The auto-updater can replace and delete files
- The auto-updater can run downloaded executables

**The program is completely safe** - we have taken extra precautions to prevent unintentional file deletion. You can audit the auto-updater code [here](https://github.com/chummer5a/chummer5a/blob/7bcde977da74f4ec1bb0721210cf2f7bba80cff1/Chummer/Forms/Utility%20Forms/ChummerUpdater.cs).

## Linux Installation (Using Wine)

### Prerequisites
Install Wine and required tools:

```bash
# On Fedora (example)
sudo dnf install wine-devel-3.5-1.fc28.i686
sudo dnf install wine-common-3.5-1.fc28.noarch
sudo dnf install wine-mono-4.7.1-2.fc28.noarch
sudo dnf install winetricks-20180603-1.fc28.noarch
```

### Setup Wine Environment

1. **Create wine prefix:**
```bash
WINEARCH=win32 winecfg
```

2. **Install dependencies:**
```bash
winetricks -q dotnet48 allfonts msls31 ie8
```

3. **Run Chummer:**
```bash
wine Chummer.exe
```

4. **Configure settings:**
   - Go to global settings
   - Toggle 'Apply Linux printing fix'
   - Change 'Preview uses Internet Explorer version' to 8

### Performance Note
Chummer runs significantly slower through Wine than natively on Windows.

## macOS Installation (Using Wine)

### Prerequisites

1. **Install Homebrew:**
```bash
/usr/bin/ruby -e "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
```

2. **Install XQuartz:**
```bash
brew cask install xquartz
```

3. **Update XCode:**
```bash
sudo xcodebuild -license
```

4. **Install Wine:**
```bash
brew install wine
```

### Setup Wine Environment

1. **Create wine prefix:**
```bash
WINEARCH=win32 winecfg
```

2. **Install Winetricks:**
```bash
brew install Winetricks
```

3. **Install dependencies:**
```bash
winetricks -q dotnet48 allfonts
```

4. **Run Chummer:**
```bash
wine Chummer.exe
```

### Known Issues
- **Infotips don't work** with Wine on macOS and will make the app crash
- Performance is significantly slower than native Windows

## Mono Support (Not Recommended)

**Note:** Mono support in Chummer5a has been mostly abandoned. Do not expect Chummer5a to function properly under Mono.

If you still want to try:
1. Install Mono from [mono-project.com](http://www.mono-project.com/download/)
2. Run: `mono Chummer5.exe`
3. Not all elements are supported on non-Windows platforms
4. Translator doesn't seem to currently run

## Troubleshooting

### Common Issues

1. **".NET Framework not installed"**
   - Download and install .NET Framework 4.8 from Microsoft

2. **"Application won't start"**
   - Check that you have the latest version
   - Try running as administrator
   - Check Windows compatibility mode

3. **"Antivirus blocking the application"**
   - Add Chummer5a folder to antivirus exclusions
   - The application is safe despite the warnings

4. **"Character sheet preview not working"**
   - Ensure Internet Explorer is installed and updated
   - On Linux/macOS, install IE8 via winetricks

### Getting Help

- Check our [GitHub Issues](https://github.com/chummer5a/chummer5a/issues)
- Read the [Contributing Guide](Contributing.md) for development help
- Browse the [Game Content Documentation](game-content/README.md) for game-specific information
