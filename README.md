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

Disclaimer: The main focus of this project is to give a general idea of how an OBS plugin can be implemented with C#. I am new to OBS plugin programming myself, while I try my best to show how each of the items is properly implemented I might have made mistakes there that an experienced OBS plugin programmer wouldn't have made. Feel free to file GitHub issues for mistakes that you find either in the documentation text here or in the code and I will correct them.

### ObsCSharpExample.csproj
This is the project file that defines the project to include NetObsBindings assembly and be compiled as a NativeAOT application. Feel free to adapt this to your needs. There is only this and no solution file (.sln) since Visual Studio wouldn't add anything to this except bloat ;-)

### Module.cs
This is the central file for the module. As you can see from this example a plugin module can include multiple objects like sources, filters and outputs but they need to be registered and managed from a central instance, which is this module.

A secondary job this class has is providing global utility functionality to other classes in the module, e.g. a log function or an implementation for the OBS locale system (which is also on module level, meaning that all objects living in a module share the same locale files) with the ObsText(String) methods. 

There is also a GetString() method which automatically frees memory from unmanaged strings from OBS before returning their managed string representation. Note that whether this should be used depends on the function the string is retrieved from. E.g. ObsData.obs_data_get_string() returns a string from a settings object that will continue to live after that method call so you need to leave the memory for it allocated, whereas Obs.obs_module_get_config_path() returns a string for your temporary use that should be "bfreed" afterwards, so use GetString() on that one for convenience.

Last but not least the Module class also takes care of talking to the frontend API to register an item for the Tools menu.

### locale/en-US.ini
This file contains the locale strings used for this project. If you need new text items just add them here.

### SettingsDialog.cs
This is not exactly the example of a standard procedure to register a dialog that can be called from the Tools main menu in OBS, more like a small hack. Instead of bringing our own Qt implementation to add a GUI object (the dialog window for our output settings) we create a dummy source that has properties attached to it and then show these properties using the ObsFrontendApi.obs_frontend_open_source_properties() method when clicking the Tools menu entry.

This smart idea was blatantly stolen from [fzwoch](https://github.com/fzwoch), who used this method to register the settings dialog for his very nice [obs-teleport plugin](https://github.com/fzwoch/obs-teleport). I liked it so much that I thought it should definitely be part of an example project. But beware, as with all hacks it might break because of future changes to it that don't accomodate for this unintended way of using things. On the other hand it has the advantage that it will keep on working regardless of Qt (or in general UI) library changes in OBS and always respect theme settings correctly.

In addition to example settings (called "properties" in OBS) there is also buttons to start and stop the example output that is implemented in the Output class in Output.cs. Note that as soon as an output is active certain settings in OBS are locked, e.g. the resolution, so that would also be a way to test whether the output was really started after you clicked the Start button.

### Output.cs
This class registers an [output](https://obsproject.com/docs/reference-outputs.html) in OBS. When it's active it receives all the frame data from OBS. This specific output doesn't register for audio data, though it still shows the function signature necessary for the raw_audio callback, which needs to be provided even when the output is only registering itself for video data. The video data is itself is also not really processed, for the sake of this example the plugin is merely logging the frame timestamps as OBS debug log messages.

### Filter.cs
_TBD_

## Source.cs
_TBD_

# Building
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
