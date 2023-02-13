using System.Runtime.InteropServices;
using System.Text;
using ObsInterop;
namespace ObsCSharpExample;

public static class SettingsDialog
{

  static unsafe obs_data* _settings;
  static unsafe obs_source* _source;

  public static unsafe void Register()
  {
    var sourceInfo = new obs_source_info();
    fixed (byte* id = Encoding.UTF8.GetBytes(Module.ModuleName + " output settings"))
    {
      sourceInfo.id = (sbyte*)id;
      sourceInfo.type = obs_source_type.OBS_SOURCE_TYPE_FILTER;
      sourceInfo.output_flags = ObsSource.OBS_SOURCE_CAP_DISABLED;
      sourceInfo.get_name = &settings_get_name;
      sourceInfo.create = &settings_create;
      sourceInfo.destroy = &settings_destroy;
      sourceInfo.get_defaults = &settings_get_defaults;
      sourceInfo.get_properties = &settings_get_properties;
      ObsSource.obs_register_source_s(&sourceInfo, (nuint)Marshal.SizeOf(sourceInfo));
      var source = Obs.obs_source_create((sbyte*)id, (sbyte*)id, null, null);
      string configPath = Module.GetString(Obs.obs_module_get_config_path(Module.ObsModule, null));
      Directory.CreateDirectory(configPath); // ensure this directory exists
      fixed (byte* configFile = Encoding.UTF8.GetBytes(Path.Combine(configPath, Module.ModuleName + ".json")))
      {
        var settings = ObsData.obs_data_create_from_json_file((sbyte*)configFile);
        Obs.obs_source_update(_source, settings);
        ObsData.obs_data_release(settings);
      }
    }
  }

  public static unsafe void Show()
  {
    ObsFrontendApi.obs_frontend_open_source_properties(_source);
  }

  public static unsafe void Save()
  {
    fixed (byte* fileName = Encoding.UTF8.GetBytes(Module.ModuleName + ".json"))
    {
      var configPathObs = Obs.obs_module_get_config_path(Module.ObsModule, (sbyte*)fileName);
      ObsData.obs_data_save_json(_settings, configPathObs);
      ObsBmem.bfree(configPathObs);
    }
  }

  public static unsafe void Dispose()
  {
    Obs.obs_source_release(_source);
    ObsData.obs_data_release(_settings);
  }


  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe sbyte* settings_get_name(void* data)
  {
    return null;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void* settings_create(obs_data* settings, obs_source* source)
  {
    Module.Log("settings_create called", ObsLogLevel.Debug);
    _settings = settings;
    _source = source;
    return settings;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void settings_destroy(void* data)
  {
    Module.Log("settings_destroy called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe obs_properties* settings_get_properties(void* data)
  {
    var properties = ObsProperties.obs_properties_create();
    fixed (byte*
      labelId = Encoding.UTF8.GetBytes("label"),
      labelCaption = Module.ObsText("LabelCaption"),
      labelText = Module.ObsText("LabelText"),
      textboxId = Encoding.UTF8.GetBytes("textbox"),
      textboxCaption = Module.ObsText("TextboxCaption"),
      textboxText = Module.ObsText("TextboxText"),
      outputStartButtonId = Encoding.UTF8.GetBytes("output_start_button"),
      outputStartButtonCaption = Module.ObsText("OutputStartButtonCaption"),
      outputStartButtonText = Module.ObsText("OutputStartButtonText"),
      outputStopButtonId = Encoding.UTF8.GetBytes("output_stop_button"),
      outputStopButtonCaption = Module.ObsText("OutputStopButtonCaption"),
      outputStopButtonText = Module.ObsText("OutputStopButtonText"),
      urlButtonId = Encoding.UTF8.GetBytes("url_button"),
      urlButtonCaption = Module.ObsText("UrlButtonCaption"),
      urlButtonText = Module.ObsText("UrlButtonText"),
      urlButtonTarget = Module.ObsText("UrlButtonTarget"),
      checkboxId = Encoding.UTF8.GetBytes("checkbox"),
      checkboxCaption = Module.ObsText("CheckboxCaption"),
      checkboxText = Module.ObsText("CheckboxText")
    )
    {
      var prop = ObsProperties.obs_properties_add_text(properties, (sbyte*)labelId, (sbyte*)labelCaption, obs_text_type.OBS_TEXT_INFO);
      ObsProperties.obs_property_set_long_description(prop, (sbyte*)labelText);

      prop = ObsProperties.obs_properties_add_text(properties, (sbyte*)textboxId, (sbyte*)textboxCaption, obs_text_type.OBS_TEXT_DEFAULT);
      ObsProperties.obs_property_set_long_description(prop, (sbyte*)textboxText);

      prop = ObsProperties.obs_properties_add_button(properties, (sbyte*)outputStartButtonId, (sbyte*)outputStartButtonCaption, &settings_start_button_click);
      ObsProperties.obs_property_set_long_description(prop, (sbyte*)outputStartButtonText);
      prop = ObsProperties.obs_properties_add_button(properties, (sbyte*)outputStopButtonId, (sbyte*)outputStopButtonCaption, &settings_stop_button_click);
      ObsProperties.obs_property_set_long_description(prop, (sbyte*)outputStopButtonText);

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
  public static unsafe void settings_get_defaults(obs_data* settings)
  {
    Module.Log("settings_get_defaults called", ObsLogLevel.Debug);
    fixed (byte*
      textboxId = Encoding.UTF8.GetBytes("textbox"),
      textboxDefaultText = Module.ObsText("TextboxDefaultText")
    )
    {
      ObsData.obs_data_set_default_string(settings, (sbyte*)textboxId, (sbyte*)textboxDefaultText);
    }
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe byte settings_start_button_click(obs_properties* properties, obs_property* prop, void* data)
  {
    Output.Start();
    return Convert.ToByte(true);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe byte settings_stop_button_click(obs_properties* properties, obs_property* prop, void* data)
  {
    Output.Stop();
    return Convert.ToByte(true);
  }

}