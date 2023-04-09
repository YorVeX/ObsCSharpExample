# ObsCSharpExample

Example for an OBS plugin written in C# containing various standard items like output, filter, source or a settings dialog in the OBS Tools menu. Meant to be used both to learn writing OBS plugins and C# and as a template for creating new plugins or plugin content like a source or an output.

![image](https://user-images.githubusercontent.com/528974/218354411-384a533b-1f14-43a5-b6d8-b78fa18a4b1d.png)

OBS Classic still had a [CLR Host Plugin](https://obsproject.com/forum/resources/clr-host-plugin.21/), but with OBS Studio writing plugins in C# wasn't possible anymore. This has changed as of recently. With the release of .NET 7 that includes NativeAOT it is now possible to build native code libraries that can be loaded by OBS Studio. This repository is here to show you how.


## Prerequisites
- OBS 29+ 64 bit
- Windows
  - tested only on Windows 10, but Windows 11 should also work
- Linux
  - not tested
  - binary build created on Ubuntu 20.04 WSL environment, therefore linked against glibc 2.31

## FAQ
- **Q**: Why is the binary plugin file so big compared to other plugins for the little bit it does, will this cause issues?
  - **A**: Unlike other plugins it's not written directly in C++ but in C# using .NET 7 and NativeAOT (for more details read on in the section for developers). This produces some overhead in the actual plugin file, however, the code that matters for functionality of this plugin should be just as efficient and fast as code directly written in C++ so there's no reason to worry about performance on your system.

- **Q**: Will there be a version for MacOS?
  - **A**: NativeAOT [doesn't support cross-compiling](https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/docs/compiling.md#cross-architecture-compilation) and I don't have a Mac, so I currently can't compile it, let alone test it. You can try to compile it yourself, but note that MacOS [is currently only supported by the next preview version of .NET 8](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/#platformarchitecture-restrictions), although people [do already successfully create builds](https://github.com/dotnet/runtime/issues/79253) with it.

- **Q**: Will there be a 32 bit version of this plugin?
  - **A**: No. Feel free to try and compile it for x86 targets yourself, last time I checked it wasn't fully supported in NativeAOT.

## C# programming for OBS with NetObsBindings

### Type differences
- Boolean values from OBS are treated as type "byte" in NetObsBindings. This applies to both directions. One could simply work with using 0 or 1 when calling OBS functions that return or expect bool parameters, but for better readability it is recommended to use something like Convert.ToByte(true) or Convert.ToBoolean(byteParamFromObs).
- Strings are very different between OBS/C++ and C#. NetObsBindings represent strings from OBS as a pointer to an sbyte, which is simply a pointer to the first character of the string. To work with such strings in C# they need to be converted to a managed string first. Fortunately there is the .NET function Marshal.PtrToStringUTF8() which does all the hard work. After this function was called the managed string is not tied to the original unmanaged string, so even if the latter is removed from memory the managed string can still be accessed, it is an independent copy of the original string.
- The other direction is a bit more tedious. A managed string might have its location in memory changed at any time, but if you pass this to unmanaged code this code needs to be able to rely on finding the given string at the pointer location in memory at all times. Therefore the [fixed](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/fixed) statement is used to pin a byte array to a fixed memory location for the duration of OBS function calls. This byte array is then filled with string data that is either converted to byte data by using the .NET base function Encoding.UTF8.GetBytes() for dynamic strings or by using UTF-8 string constants with the "u8" notation, since OBS works with UTF-8 strings. As OBS functions through NetObsBindings expect an sbyte pointer the byte pointer created this way is simply cast to an sbyte pointer when handing over the parameter to the function (from pointer/memory perspective an sbyte pointer is the same as a byte pointer so a simple cast does the job here). If unmanaged code is supposed to work with dynamic strings handed over to them at a later time use GC.AllocateArray() instead of the "fixed" statement to make memory pinning permament.
- Be careful when using unsafe structs. Unlike managed structs fields in them are not initialized but contain random values.

### Managed vs. unmanaged, instance vs. static
For various objects different approaches were used so that all of these variants are demonstrated. For each of these objects there will be a description which approach was used. What fits best for you will depend on your project.

Basically there is two ways how to handle callbacks from OBS for objects like outputs or sources:
1. Store everything in the unmanaged object that is passed to and from OBS. --> Do this when only unmanaged fields need to be stored and managed code doesn't need to be applied to any of the data, e.g. when a filter does its job solely based on calling OBS functions. See [here](https://github.com/YorVeX/xObsBrowserAutoRefresh/blob/main/BrowserFilter.cs) for another example outside of this repo for an example how this can be done and how the fields are accessed from the callbacks.
2. Store everything in a managed object, which cannot be passed to and from OBS, so the OBS data is only used to identify/index the right managed data from a list. Then either the static callback directly works with the data from that list or invokes instance functions on an object from the list, which then can simply work with their own instance variables. --> Do this when you want/need to use managed code, e.g. to provide a small HTTP server or use an HTTP client from .NET base functionality. This introduces some extra complexity so should only be done when needed.

Mixtures of both variants are also possible.

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

**Managed vs. unmanaged, instance vs. static**

The assumption is that there will be only one output, so everything is based on static fields and methods. A "Context" struct is used to show the basic concept behind this for the OBS related objects, this is the struct that will be passed to OBS and passed back to the plugin by OBS when callbacks are invoked, although it is not really used. Other things are simply stored in global variables (they are named with preceding underscores), since everything is static and only one output uses this it won't be a problem.

## Source.cs
This class registers a [source](https://obsproject.com/docs/reference-sources.html) in OBS. To give you a template for the callbacks there are very many implemented here, even when they don't do anything but logging that they were called so that you can test when which callback is invoked by OBS. On module level it also prepares an image downloaded from the internet that is used as a texture, so it also has some managed code when using the HttpClient class for this.

It also shows the basic concept of surrounding graphics functions with Obs.obs_enter_graphics() and Obs.obs_leave_graphics() calls and image_source_video_render() shows how a texture can be drawn.

Example properties of various types are added to show how a source could be made configurable.

**Managed vs. unmanaged, instance vs. static**

There can be more than one source of this type, however, the callback code doesn't do anything source specific. Instead the texture to be drawn is simply shared between all sources so much like for the Output class this is simply stored in global variables.

### Filter.cs
This class registers a filter in OBS, which internally is also just a source with some specific flags and a few different callbacks. A getFilter() helper function makes the transition from a static callback context to the instance context easy, based on the object that OBS hands over for each callback.

Example properties of various types are added to show how a source could be made configurable.

**Managed vs. unmanaged, instance vs. static**

Potentially an infinite number of filters could be added to various sources or even the same source, hence data needs to be stored per filter. However, the callbacks are still static so unlike for the Output class we really need the data passed to us by OBS to identify the filter the code is called for. For the sake of this example let's pretend managed code is needed, therefore in this case the Context structure is only used to identify the filter by an ID number from a list and store everything else in instance variables. Then we can invoke instance functions and these can work within the context of their instance. This is demonstrated here with the ProcessFrame() function which is a fully managed function working with its instance variables.

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
