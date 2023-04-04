// SPDX-FileCopyrightText: © 2023 YorVeX, https://github.com/YorVeX
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using ObsInterop;
namespace ObsCSharpExample;

public class Filter
{
  public struct Context
  {
    public uint FilterId;
  }

  #region Class fields
  static uint _filterCount = 0;
  static ConcurrentDictionary<uint, Filter> _filterList = new ConcurrentDictionary<uint, Filter>();
  #endregion Class fields

  #region Instance fields
  // OBS handling
  public unsafe Context* ContextPointer;
  public unsafe obs_source* Source;
  public unsafe obs_data* Settings;
  public unsafe uint FilterId
  {
    get => ((Context*)ContextPointer)->FilterId;
  }

  // frame handling
  uint _frameCycleCounter;

  #endregion Instance fields

  public unsafe Filter(obs_data* settings, obs_source* obsFilter, Context* contextPointer)
  {
    ContextPointer = contextPointer;
    Source = obsFilter;
    Settings = settings;
  }

  public unsafe void Dispose()
  {
    Marshal.FreeCoTaskMem((IntPtr)ContextPointer);
  }

  #region Instance methods

  public void ProcessFrame(ulong frameTimestamp, uint fps)
  {
    // ⚠️ this code is executed for every single frame, whatever is done here needs to be done fast, or it can cause rendering lag
    try
    {
      uint frameCycleCounter = Interlocked.Increment(ref _frameCycleCounter);
      if ((frameCycleCounter > 5) && (frameCycleCounter > fps)) // do this only roughly once per second
      {
        _frameCycleCounter = 0;
        Module.Log("ProcessFrame called, filter frame timestamp: " + frameTimestamp, ObsLogLevel.Debug);
      }
    }
    catch (Exception ex)
    {
      Module.Log(ex.GetType().Name + " in ProcessFrame(): " + ex.Message + "\n" + ex.StackTrace);
    }
  }
  #endregion Instance methods

  #region Helper methods
  public static unsafe void Register()
  {
    var sourceInfo = new obs_source_info();
    fixed (byte* id = Encoding.UTF8.GetBytes(Module.ModuleName + " Filter"))
    {
      sourceInfo.id = (sbyte*)id;
      sourceInfo.type = obs_source_type.OBS_SOURCE_TYPE_FILTER;
      sourceInfo.output_flags = ObsSource.OBS_SOURCE_ASYNC_VIDEO;
      sourceInfo.get_name = &filter_get_name;
      sourceInfo.create = &filter_create;
      sourceInfo.activate = &filter_activate;
      sourceInfo.deactivate = &filter_deactivate;
      sourceInfo.show = &filter_show;
      sourceInfo.hide = &filter_hide;
      sourceInfo.filter_remove = &filter_remove;
      sourceInfo.destroy = &filter_destroy;
      sourceInfo.get_defaults = &filter_get_defaults;
      sourceInfo.get_properties = &filter_get_properties;
      sourceInfo.update = &filter_update;
      sourceInfo.save = &filter_save;
      sourceInfo.filter_video = &filter_video;
      ObsSource.obs_register_source_s(&sourceInfo, (nuint)Marshal.SizeOf(sourceInfo));
    }
  }

  static unsafe private Filter getFilter(void* data)
  {
    var context = (Context*)data;
    return _filterList[(*context).FilterId];
  }
  #endregion Helper methods

  #region Filter API methods
  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe sbyte* filter_get_name(void* data)
  {
    Module.Log("filter_get_name called", ObsLogLevel.Debug);
    fixed (byte* filterName = "C# Example Filter"u8)
      return (sbyte*)filterName;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void* filter_create(obs_data* settings, obs_source* source)
  {
    Module.Log("filter_create called", ObsLogLevel.Debug);
    Context* contextPointer = (Context*)Marshal.AllocCoTaskMem(sizeof(Context)); ;
    contextPointer->FilterId = ++_filterCount;

    var filter = new Filter(settings, source, contextPointer);
    _filterList.TryAdd(contextPointer->FilterId, filter);

    return (void*)contextPointer;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void filter_activate(void* data)
  {
    Module.Log("filter_activate called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void filter_deactivate(void* data)
  {
    Module.Log("filter_deactivate called", ObsLogLevel.Debug);
  }


  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void filter_show(void* data)
  {
    Module.Log("filter_show called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void filter_hide(void* data)
  {
    Module.Log("filter_hide called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void filter_remove(void* data, obs_source* source)
  {
    Module.Log("filter_remove called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void filter_destroy(void* data)
  {
    Module.Log("filter_destroy called", ObsLogLevel.Debug);

    var filter = getFilter(data);
    _filterList.TryRemove(filter.FilterId, out _);

    filter.Dispose();
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe obs_properties* filter_get_properties(void* data)
  {
    Module.Log("filter_get_properties called");

    var properties = ObsProperties.obs_properties_create();
    fixed (byte*
      labelId = "label"u8,
      labelCaption = Module.ObsText("LabelCaption"),
      labelText = Module.ObsText("LabelText"),
      textboxId = "textbox"u8,
      textboxCaption = Module.ObsText("TextboxCaption"),
      textboxText = Module.ObsText("TextboxText"),
      buttonId = "button"u8,
      buttonCaption = Module.ObsText("ButtonCaption"),
      buttonText = Module.ObsText("ButtonText"),
      urlButtonId = "url_button"u8,
      urlButtonCaption = Module.ObsText("UrlButtonCaption"),
      urlButtonText = Module.ObsText("UrlButtonText"),
      urlButtonTarget = Module.ObsText("UrlButtonTarget"),
      checkboxId = "checkbox"u8,
      checkboxCaption = Module.ObsText("CheckboxCaption"),
      checkboxText = Module.ObsText("CheckboxText")
    )
    {
      var prop = ObsProperties.obs_properties_add_text(properties, (sbyte*)labelId, (sbyte*)labelCaption, obs_text_type.OBS_TEXT_INFO);
      ObsProperties.obs_property_set_long_description(prop, (sbyte*)labelText);

      prop = ObsProperties.obs_properties_add_text(properties, (sbyte*)textboxId, (sbyte*)textboxCaption, obs_text_type.OBS_TEXT_DEFAULT);
      ObsProperties.obs_property_set_long_description(prop, (sbyte*)textboxText);

      prop = ObsProperties.obs_properties_add_button(properties, (sbyte*)buttonId, (sbyte*)buttonCaption, &filter_properties_button_click);
      ObsProperties.obs_property_set_long_description(prop, (sbyte*)buttonText);

      prop = ObsProperties.obs_properties_add_button(properties, (sbyte*)urlButtonId, (sbyte*)urlButtonCaption, null);
      ObsProperties.obs_property_set_long_description(prop, (sbyte*)urlButtonText);
      ObsProperties.obs_property_button_set_type(prop, obs_button_type.OBS_BUTTON_URL);
      ObsProperties.obs_property_button_set_url(prop, (sbyte*)urlButtonTarget);

      prop = ObsProperties.obs_properties_add_bool(properties, (sbyte*)checkboxId, (sbyte*)checkboxCaption);
      ObsProperties.obs_property_set_long_description(prop, (sbyte*)checkboxText);
    }
    return properties;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void filter_get_defaults(obs_data* settings)
  {
    Module.Log("filter_get_defaults called");
    fixed (byte*
      textboxId = "textbox"u8,
      textboxDefaultText = Module.ObsText("TextboxDefaultText")
    )
    {
      ObsData.obs_data_set_default_string(settings, (sbyte*)textboxId, (sbyte*)textboxDefaultText);
    }
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe byte filter_properties_button_click(obs_properties* properties, obs_property* prop, void* data)
  {
    Module.Log("filter_properties_button_click called");

    return Convert.ToByte(true);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void filter_update(void* data, obs_data* settings)
  {
    Module.Log("filter_update called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void filter_save(void* data, obs_data* settings)
  {
    Module.Log("filter_save called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe obs_source_frame* filter_video(void* data, obs_source_frame* frame)
  {
    // ⚠️ this code is executed for every single frame, whatever is done here needs to be done fast, or it can cause rendering lag
    getFilter(data).ProcessFrame((*frame).timestamp, (uint)Obs.obs_get_active_fps());
    return frame;
  }
  #endregion Filter API methods


}