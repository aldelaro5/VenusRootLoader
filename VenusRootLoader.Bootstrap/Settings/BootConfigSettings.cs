namespace VenusRootLoader.Bootstrap.Settings;

public class BootConfigSettings
{
    public bool? GfxEnableNativeGfxJobs { get; set; }
    public bool? WaitForNativeDebugger { get; set; }
    public string? ScriptingRuntimeVersion { get; set; }
    public bool? VrEnabled { get; set; }
    public bool? HdrDisplayEnabled { get; set; }
    public string? MonoCodeGen { get; set; }
    public int? MaxNumLoopsNoJobBeforeGoingIdle { get; set; }
    public bool? WaitForManagedDebugger { get; set; }
    public int? PreloadManagerThreadStackSize { get; set; }
    public bool? GfxDisableMtRendering { get; set; }
    public bool? GfxEnableGfxJobs { get; set; }
    public bool? ForceGfxDirect { get; set; }
    public bool? ForceGfxSt { get; set; }
    public bool? ForceGfxMt { get; set; }
    public bool? ForceGfxJobs { get; set; }
    public bool? GfxJobsSync { get; set; }
    public string? HttpFilesystemApiKey { get; set; }
    public bool? HttpFilesystemEnable { get; set; }
    public string? HttpFilesystemPrefix { get; set; }
    public string? HttpFilesystemPubKey { get; set; }
    public bool? Headless { get; set; }
    public bool? SingleInstance { get; set; }
    public string? PlayerConnectionIp { get; set; }
    public bool? PlayerConnectionDebug { get; set; }
    public string? PlayerConnectionMode { get; set; }
    public int? PlayerConnectionGuid { get; set; }
    public string? PlayerConnectionListenAddress { get; set; }
    public int? PlayerConnectionWaitTimeout { get; set; }
    public int? ProfilerMaxPoolMemory { get; set; }
    public int? ProfilerMaxUsedMemory { get; set; }
    public bool? ProfilerEnableOnStartup { get; set; }
}
