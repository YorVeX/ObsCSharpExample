using System.Runtime.InteropServices;
using System.Text;
using ObsInterop;
namespace ObsCSharpExample;

public static class Output
{
  unsafe struct Context
  {
    public obs_data* Settings;
    public obs_output* Output;
  }

  static Context _outputData;
  static IntPtr _outputDataPointer;
  static ulong _frameCycleCounter = 0;

  #region Helper methods
  public static unsafe void Register()
  {
    var outputInfo = new obs_output_info();
    fixed (byte* id = Encoding.UTF8.GetBytes(Module.ModuleName + " Output"))
    {
      outputInfo.id = (sbyte*)id;
      outputInfo.flags = ObsOutput.OBS_OUTPUT_AV;
      outputInfo.get_name = &output_get_name;
      outputInfo.create = &output_create;
      outputInfo.destroy = &output_destroy;
      outputInfo.start = &output_start;
      outputInfo.stop = &output_stop;
      outputInfo.raw_video = &output_raw_video;
      outputInfo.raw_audio = &output_raw_audio;
      ObsOutput.obs_register_output_s(&outputInfo, (nuint)Marshal.SizeOf(outputInfo));
    }
  }
  public static unsafe void Create()
  {
    fixed (byte* id = Encoding.UTF8.GetBytes(Module.ModuleName + " Output"))
      Obs.obs_output_create((sbyte*)id, (sbyte*)id, null, null);
  }

  public static unsafe void Start()
  {
    if (!Convert.ToBoolean(Obs.obs_output_active(_outputData.Output)))
    {
      Module.Log("Starting output...");
      Obs.obs_output_start(_outputData.Output);
      Module.Log("Output started.");
    }
    else
      Module.Log("Output not started, already running.");
  }
  public static unsafe void Stop()
  {
    if (Convert.ToBoolean(Obs.obs_output_active(_outputData.Output)))
    {
      Module.Log("Stopping output...");
      Obs.obs_output_stop(_outputData.Output);
      Module.Log("Output stopped.");
    }
    else
      Module.Log("Output not stopped, wasn't running.");
  }
  public static unsafe void Dispose()
  {
    Obs.obs_output_release(_outputData.Output);
  }
  #endregion Helper methods

  #region Output API methods
  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe sbyte* output_get_name(void* data)
  {
    Module.Log("output_get_name called", ObsLogLevel.Debug);
    var asciiBytes = Encoding.UTF8.GetBytes("C# Example Output");
    fixed (byte* logMessagePtr = asciiBytes)
      return (sbyte*)logMessagePtr;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void* output_create(obs_data* settings, obs_output* output)
  {
    Module.Log("output_create called", ObsLogLevel.Debug);

    var obsFilterData = new Context();
    IntPtr mem = Marshal.AllocCoTaskMem(Marshal.SizeOf(obsFilterData));
    Context* obsOutputDataPointer = (Context*)mem;
    obsOutputDataPointer->Settings = settings;
    obsOutputDataPointer->Output = output;
    _outputDataPointer = mem;
    _outputData = *obsOutputDataPointer;
    return (void*)mem;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void output_destroy(void* data)
  {
    Module.Log("output_destroy called", ObsLogLevel.Debug);
    Marshal.FreeCoTaskMem(_outputDataPointer);
    _outputData.Output = null;
    ObsData.obs_data_release(_outputData.Settings);
    _outputData.Settings = null;
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe byte output_start(void* data)
  {
    Module.Log("output_start called", ObsLogLevel.Debug);

    if (Convert.ToBoolean(Obs.obs_output_can_begin_data_capture(_outputData.Output, ObsOutput.OBS_OUTPUT_VIDEO)))
      Obs.obs_output_begin_data_capture(_outputData.Output, ObsOutput.OBS_OUTPUT_VIDEO);

    return Convert.ToByte(true);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void output_stop(void* data, ulong ts)
  {
    Module.Log("output_stop called", ObsLogLevel.Debug);
    Obs.obs_output_end_data_capture(_outputData.Output);
  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void output_raw_video(void* data, video_data* frame)
  {
    _frameCycleCounter++;
    if ((_frameCycleCounter > 5) && (_frameCycleCounter > Obs.obs_get_active_fps())) // do this only roughly once per second
    {
      _frameCycleCounter = 1;
      Module.Log("output_raw_video called, frame timestamp: " + (*frame).timestamp, ObsLogLevel.Debug);
    }

  }

  [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
  public static unsafe void output_raw_audio(void* data, audio_data* frames)
  {
    Module.Log("output_raw_audio will never be called, since obs_output_can_begin_data_capture() was only done for ObsOutput.OBS_OUTPUT_VIDEO, but it still needs to exist.", ObsLogLevel.Debug);
  }
  #endregion Output API methods


}