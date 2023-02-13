using System.Runtime.InteropServices;
using System.Text;
using ObsInterop;
namespace ObsCSharpExample;

public class Source
{
  public unsafe struct Context
  {
    public obs_data* Settings;
    public obs_source* Source;

  }

  #region Global variables shared by all sources
  static unsafe gs_texture* _texture = null;
  static uint _textureWidth = 0;
  static uint _textureHeight = 0;
  #endregion Global variables shared by all sources

  #region Helper methods
  public static unsafe void Register()
  {
    new Thread(() => prepareImage("https://obsproject.com/assets/images/new_icon_small.png")).Start();

    var sourceInfo = new obs_source_info();
    fixed (byte* id = Encoding.UTF8.GetBytes(Module.ModuleName + " Source"))
    {
      sourceInfo.id = (sbyte*)id;
      sourceInfo.type = obs_source_type.OBS_SOURCE_TYPE_INPUT;
      sourceInfo.icon_type = obs_icon_type.OBS_ICON_TYPE_IMAGE;
      sourceInfo.output_flags = ObsSource.OBS_SOURCE_VIDEO;
      sourceInfo.get_name = &image_source_get_name;
      sourceInfo.create = &image_source_create;
      sourceInfo.get_width = &image_source_get_width;
      sourceInfo.get_height = &image_source_get_height;
      sourceInfo.activate = &image_source_activate;
      sourceInfo.deactivate = &image_source_deactivate;
      sourceInfo.show = &image_source_show;
      sourceInfo.hide = &image_source_hide;
      sourceInfo.destroy = &image_source_destroy;
      sourceInfo.get_defaults = &image_source_get_defaults;
      sourceInfo.get_properties = &image_source_get_properties;
      sourceInfo.update = &image_source_update;
      sourceInfo.save = &image_source_save;
      sourceInfo.video_render = &image_source_video_render;
      sourceInfo.missing_files = &image_source_missing_files;
      ObsSource.obs_register_source_s(&sourceInfo, (nuint)Marshal.SizeOf(sourceInfo));
    }
  }

  public static unsafe void prepareImage(string url)
  {
    string imageFileName = Path.GetTempFileName();
    // download the example image from the OBS website
    try
    {
      using (var fileStream = new FileStream(imageFileName, FileMode.OpenOrCreate, FileAccess.Write))
      using (var httpClient = new HttpClient())
        httpClient.GetStreamAsync(url).Result.CopyTo(fileStream);
    }
    catch (Exception ex)
    {
      Module.Log("Downloading example image for image source failed with " + ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace);
    }

    // load the image in OBS
    Obs.obs_enter_graphics();
    fixed (byte* imageFileNameObs = Encoding.UTF8.GetBytes(imageFileName))
      _texture = ObsGraphics.gs_texture_create_from_file((sbyte*)imageFileNameObs);
    _textureWidth = ObsGraphics.gs_texture_get_width(_texture);
    _textureHeight = ObsGraphics.gs_texture_get_height(_texture);
    Obs.obs_leave_graphics();
    Module.Log("Loaded image with size " + _textureWidth + "x" + _textureHeight + " from file: " + imageFileName, ObsLogLevel.Debug);
  }

  #endregion Helper methods

  #region Source API methods
  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe sbyte* image_source_get_name(void* data)
  {
    Module.Log("image_source_get_name called", ObsLogLevel.Debug);
    fixed (byte* logMessagePtr = Encoding.UTF8.GetBytes("C# Example Image Source"))
      return (sbyte*)logMessagePtr;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void* image_source_create(obs_data* settings, obs_source* source)
  {
    Module.Log("image_source_create called", ObsLogLevel.Debug);
    Context* context = (Context*)Marshal.AllocCoTaskMem(sizeof(Context)); ;
    context->Settings = settings;
    context->Source = source;
    return (void*)context;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void image_source_destroy(void* data)
  {
    Module.Log("image_source_destroy called", ObsLogLevel.Debug);
    if (_texture != null)
    {
      Obs.obs_enter_graphics();
      ObsGraphics.gs_texture_destroy(_texture);
      Obs.obs_leave_graphics();
    }
    Marshal.FreeCoTaskMem((IntPtr)data);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void image_source_activate(void* data)
  {
    Module.Log("image_source_activate called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void image_source_deactivate(void* data)
  {
    Module.Log("image_source_deactivate called", ObsLogLevel.Debug);
  }


  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void image_source_show(void* data)
  {
    Module.Log("image_source_show called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void image_source_hide(void* data)
  {
    Module.Log("image_source_hide called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe obs_properties* image_source_get_properties(void* data)
  {
    Module.Log("image_source_get_properties called", ObsLogLevel.Debug);

    var properties = ObsProperties.obs_properties_create();
    fixed (byte*
      labelId = Encoding.UTF8.GetBytes("label"),
      labelCaption = Module.ObsText("LabelCaption"),
      labelText = Module.ObsText("LabelText"),
      textboxId = Encoding.UTF8.GetBytes("textbox"),
      textboxCaption = Module.ObsText("TextboxCaption"),
      textboxText = Module.ObsText("TextboxText"),
      buttonId = Encoding.UTF8.GetBytes("button"),
      buttonCaption = Module.ObsText("ButtonCaption"),
      buttonText = Module.ObsText("ButtonText"),
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

      prop = ObsProperties.obs_properties_add_button(properties, (sbyte*)buttonId, (sbyte*)buttonCaption, &image_source_properties_button_click);
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
  public static unsafe void image_source_get_defaults(obs_data* settings)
  {
    Module.Log("image_source_get_defaults called", ObsLogLevel.Debug);
    fixed (byte*
      textboxId = Encoding.UTF8.GetBytes("textbox"),
      textboxDefaultText = Module.ObsText("TextboxDefaultText")
    )
    {
      ObsData.obs_data_set_default_string(settings, (sbyte*)textboxId, (sbyte*)textboxDefaultText);
    }
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe byte image_source_properties_button_click(obs_properties* properties, obs_property* prop, void* data)
  {
    Module.Log("image_source_properties_button_click called");
    return Convert.ToByte(true);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void image_source_update(void* data, obs_data* settings)
  {
    Module.Log("image_source_update called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void image_source_save(void* data, obs_data* settings)
  {
    Module.Log("image_source_save called", ObsLogLevel.Debug);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe uint image_source_get_width(void* data)
  {
    return _textureWidth;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe uint image_source_get_height(void* data)
  {
    return _textureHeight;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void image_source_video_render(void* data, gs_effect* effect)
  {
    if (_texture == null)
      return;

    ObsGraphics.gs_blend_state_push();
    ObsGraphics.gs_blend_function(gs_blend_type.GS_BLEND_ONE, gs_blend_type.GS_BLEND_INVSRCALPHA);
    fixed (byte* imageParam = Encoding.UTF8.GetBytes("image"))
      ObsGraphics.gs_effect_set_texture(ObsGraphics.gs_effect_get_param_by_name(effect, (sbyte*)imageParam), _texture);
    ObsGraphics.gs_draw_sprite(_texture, 0, _textureWidth, _textureHeight);
    ObsGraphics.gs_blend_state_pop();
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe obs_missing_files* image_source_missing_files(void* data)
  {
    Module.Log("image_source_missing_files called", ObsLogLevel.Debug);
    var missingFiles = ObsMissingFiles.obs_missing_files_create();
    // ObsMissingFiles.obs_missing_files_destroy

    // if a file is missing call ObsMissingFiles.obs_missing_file_create() for that file here and add it using ObsMissingFiles.obs_missing_files_add_file(missingFiles, theFile)

    return missingFiles;

  }

  #endregion Source API methods


}