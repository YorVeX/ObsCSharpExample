# ObsCSharpExample

Example for an OBS plugin written in C# containing various standard items like output, filter, source or a settings dialog in the OBS Tools menu. Meant to be used both to learn writing OBS plugins and C# and as a template for creating new plugins or plugin content like a source or an output.

![image](https://user-images.githubusercontent.com/528974/218354411-384a533b-1f14-43a5-b6d8-b78fa18a4b1d.png)

OBS Classic still had a [CLR Host Plugin](https://obsproject.com/forum/resources/clr-host-plugin.21/), but with OBS Studio writing plugins in C# wasn't possible anymore. This has changed as of recently. With the release of .NET 7 that includes NativeAOT it is now possible to build native code libraries that can be loaded by OBS Studio. This repository is here to show you how.


## Prerequisites
- OBS 29+ 64 bit
- Currently only working on Windows (tested only on Windows 10, but Windows 11 should also work)

## FAQ
- **Q**: Why is the binary plugin file so big compared to other plugins for the little bit it does, will this cause issues?
  - **A**: Unlike other plugins it's not written directly in C++ but in C# using .NET 7 and NativeAOT (for more details read on in the section for developers). This produces some overhead in the actual plugin file, however, the code that matters for functionality of this plugin should be just as efficient and fast as code directly written in C++ so there's no reason to worry about performance on your system.

- **Q**: Will there be a version for other operating systems, e.g. Linux?
  - **A**: NativeAOT only supports compiling for Windows targets when running on Windows and Linux targets when running on Linux, see [here](https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/docs/compiling.md#cross-architecture-compilation). I only use Windows myself so in order to be able to compile for Linux I'd need to set up a Linux VM first. I will probably do that at some point in the future but it doesn't have the highest priority. Feel free to try it yourself, will happily integrate contributions (e.g. information, pull requests and binaries) in this direction.

- **Q**: Will there be a 32 bit version of this plugin?
  - **A**: No. Feel free to try and compile it for x86 targets yourself, last time I checked it wasn't fully supported in NativeAOT.

## Content
This plugin code demonstrates different concepts about using unmanaged vs. managed code within a C# OBS plugin for the different items and source code files it includes. What exactly each file demonstrates is explained in the next sections.

### Module.cs
_TBD_

### SettingsDialog.cs
_TBD_

### Filter.cs
_TBD_

### Output.cs
_TBD_

### locale/en-US.ini
_TBD_

## Building
This plugin depends on the [NetObsBindings](https://github.com/kostya9/NetObsBindings) for building, you don't need to build this from source though, the project file provided in this repo already includes this as a [NuGet package](https://www.nuget.org/packages/NetObsBindings).
Generally the included build.cmd file is executing the necessary command to create the build, but some prerequisites need to be installed in the system first.

### Preparing the build environment
- Download and install the [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- If you want to use Visual Studio Code (recommended)
  - _This repository contains a tasks file for VS Code that can be used to easily create builds with just a simple hotkey_
  - Download and install VS Code, either from [here](https://code.visualstudio.com/download) or run this in a command prompt: `winget install Microsoft.VisualStudioCode`
  - Install the C# extension in VS Code, see [here](https://code.visualstudio.com/docs/languages/csharp) for instructions
  - Install the [NuGet Package Manager GUI extension](https://marketplace.visualstudio.com/items?itemName=aliasadidev.nugetpackagemanagergui) in VS Code
  - A restart of VS Code might be necessary for the extensions to work
  - Press <Ctrl + Shift + P> and type "NuGet", select "NuGet Package Manager GUI" from the list
  - Click the "Load Package Versions" button
  - You can try selecting the latest version of NetObsBindings or to be on the safe side select 0.0.1.29-alpha (which is what this example was tested with)
  - Press the "Update" button for NetObsBindings
  - Restart VS Code, it should now ask to do a Restore, confirm this
- If not using Visual Studio Code
  - Open a CMD or PowerShell window in the repository root folder and execute this command: `dotnet restore`
    - _This will download the referenced NetObsBindings NuGet package_

### Build without VS Code
- Run build.cmd
- Your build files should now have been created in the "publish" directory
- Optionally you can run the release.cmd script which creates a plugin folder structure that is ready for distribution.

### Build and working with VS Code
- When the project is open press <Ctrl + Alt + T>, then select "publish" from the list
- Your build files should now have been created in the "publish" directory
- Alternatively you can select the "publish and release" item, this will build and then execute the release.cmd script which creates a plugin folder structure that is ready for distribution.
- You can also put code into test.local.cmd (not included in the repo) that will copy your plugin from the "publish" folder to the plugin folder of an OBS instance you are testing with and run this OBS instance, execute this with the "test" or "publish and test" profiles from VS Code

## Credits
Many thanks to [kostya9](https://github.com/kostya9) for laying the groundwork of C# OBS Studio plugin creation, without him this plugin (and hopefully many more C# plugins following in the future) wouldn't exist. Read about his ventures into this area in his blog posts [here](https://sharovarskyi.com/blog/posts/dotnet-obs-plugin-with-nativeaot/) and [here](https://sharovarskyi.com/blog/posts/clangsharp-dotnet-interop-bindings/). 
