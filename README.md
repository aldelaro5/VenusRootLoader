# VenusRootLoader

An experimental mod loader specifically targetting Bug Fables.

It can be cross compiled from Linux to Windows. Since the goal is only to support Bug Fables, only win-x64 platform is supported, but it should work under Wine/Proton. It is heavily inspired by [MelonLoader](https://github.com/LavaGang/MelonLoader) and [BepInEx](https://github.com/BepInEx/BepInEx).

## Setting up dependencies

There are some dependencies that needs to be installed before building this.

### Install LLVM

An installation of LLVM is required because you need a functional `clang-cl` added to your PATH environment for building. After performing the installation, you can confirm it works by opening a command prompt and perform the following command:

```
clang-cl --version
```

If it returns version information, the installation was successful.

#### Windows

You can install it by downloading the latest installer from the LLVM
project's [GitHub Releases page](https://github.com/llvm/llvm-project/releases). Pick the one whose
filename ends in `-win64.exe`.

> NOTE: When prompted, accept to add LLVM to your PATH environment.

#### Linux

Refer to your distribution documentation for how to install LLVM.

### Install the Windows SDK headers

The entrypoint side requires some headers from the Windows SDK that needs to be installed.

#### Windows

If you already have Visual Studio installed (with the C/C++ workload), you already have the required
headers and can proceed to build.

If you do not, you can install the SDK separately. The version doesn't matter, choose the latest
available. You will also need to install
the [Visual Studio build tools](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022).

#### Linux

While the project is already setup to support cross compilation from Linux, there's still some setup
required for the Windows SDK headers to be pulled.

Mainly, the xwin utility needs to be installed which allows the project to pull the required files.
You can install it by downloading a tarball from
the [xwin GitHub Releases page](https://github.com/Jake-Shadle/xwin/releases/tag/0.6.6-rc.2). This
is known to work with version 0.6.6-rc.2. Pick the tarbal named
`xwin-0.6.6-rc.2-x86_64-pc-windows-msvc.tar.gz`.

You will then need to add the xwin executable to your PATH environment. To verify the installation
was successful, open a terminal and perform the following command:

```
xwin --version
```

If it returns version information, the installation was successful.

### Install Wine (Linux only)
To run the integration tests on Linux, wine must be installed on your system. Refer to your distribution documentation on how to install it.

## Building the project

First, clone the repository as usual.

> NOTE: On Linux specifically, be prepared for a directory to be created on the parent directory of
> the repository since it is where the xwin cache files will end up. This is done to avoid an issue
> with Rider where it would attempt to index all the files downloaded if it was located anywhere
> within the repository which can degrade the IDE's performance.

Then, perform the following commands from the repository's directory to build the project:

```
git submodule update --init --recursive
dotnet build
```

The build artifacts will be located in the Output directory.

## Installation

To install the mod loader into Bug Fables, simply copy the winhttp.dll (and the pdb if you want to have debugging symbols) to the game's directory. From there, any launch of the game should cause the mod loader to kick in during the game's boot process.

If you performed the build process while you also happened to have the game installed in Steam's default install directory, the mod loader will be automatically installed there each time the project is built. Support for non default installation is planned in the future.

## Configuration
The built project includes a default config file in the config directory called `config.jsonc` which is a JSON config file with comments explaining what each options does. The mod loader also supports configuration via environment variables and command line arguments.

Here are all the options available in all configuration scheme:

|`config.jsonc` key|Environment|Arguement|Allowed value|Description|
|-----------------|-----------|---------|---------|-----------|
|`DisableVrl`|`VRL_GLOBAL_DISABLE`|`--global-disable`|`true` / `false`|Disables VenusRootLoader. The bootstrap will immediately exit upon seeing a value of `true`|
|`SkipUnitySplashScreen`|`VRL_SKIP_UNITY_SPLASHSCREEN`|`--skip-unity-splashscreen`|`true` / `false`|Causes the game's `data.unity3d`'s asset bundle to be redirected to a modified version which skips the Unity splash screen on boot. Once the modified bundle is generated once, further boots will reuse it to make this process faster|
|`LoggingSettings`.`IncludeUnityLogs`|`VRL_INCLUDE_UNITY_LOGS`|`--include-unity-logs`|`true` / `false`|Captures the native and managed logs of the Unity player in VenusRootLoader's logs. All those logs will appear under the `UNITY` category which are rendered in cyan if the console logs and its colors are enabled|
|`LoggingSettings`.`ConsoleLoggerSettings`.`Enable`|`VRL_ENABLE_CONSOLE_LOGS`|`--enable-console-logs`|`true` / `false`|Enables and show a console window when the game starts that contains the VenusRootLoader logs|
|`LoggingSettings`.`ConsoleLoggerSettings`.`LogWithColors`|`VRL_CONSOLE_COLORS`|`--console-colors`|`true` / `false`|Enables colors to be rendered in the console logs if console logs are enabled. The colors will be rendered using ANSI codes if supported, but if they aren't supported on the terminal used or if using wine (since `wineconsole` does not support ANSI codes), they will be rendered using the legacy colors scheme which only allows up to 16 colors. `UNITY` logs are rendered in cyan while VenusRootLoader's own logs are rendered in magenta|
|`LoggingSettings`.`DiskFileLoggerSettings`.`Enable`|`VRL_ENABLE_FILES_LOGS`|`--enable-files-logs`|`true` / `false`|Enables logs to be written to a file in the Logs directory. All logs of the current session will go to a file named `latest.log` while logs of past sessions will be named after their creation timestamp (they get renamed from the previous session's `latest.log` upon subsequent boot)|
|`LoggingSettings`.`DiskFileLoggerSettings`.`MaxFilesToKeep`|`VRL_MAX_FILES_LOGS`|`--max-files-logs`|Number being at least 1|Determines the amount of files to preserve in the Logs directory at all times if the disk files logs are enabled. If the amount of files present is reached when booting, the oldest logs file gets deleted to make place for the new one|
|`MonoDebuggerSettings`.`Enable`|`VRL_DEBUGGER_ENABLE`|`--debugger-enable`|`true` / `false`|Enables Mono's integrated debugging server which allows IDEs and the Unity editor to connect and remotely debug the game's managed code. It also features the ability for IDEs to automatically discover the player's connection which avoids the need to enter the information manually. If running on Linux using wine, this feature also includes fixes such that PDB files path are reported correctly to the IDE using their Linux path instead of their wine path|
|`MonoDebuggerSettings`.`IpAddress`|`VRL_DEBUGGER_IP_ADDRESS`|`--debugger-ip-address`|An IP address string in the format `X.X.X.X` where each `X` is between 0 and 255|If the Mono debugger is enabled, this is the IP address the server will bind to. Common values are 127.0.0.1 to bind only to the host machine and 0.0.0.0 to bind to every IP address which allows remote debugging from a different machine on the local network. The default is 127.0.0.1|
|`MonoDebuggerSettings`.`Port`|`VRL_DEBUGGER_PORT`|`--debugger-port`|A number between 0 and 65535|If the Mono debugger is enabled, this is the port the server will bind to. The default is 55555|
|`MonoDebuggerSettings`.`SuspendOnBoot`|`VRL_DEBUGGER_SUSPEND_BOOT`|`--debugger-suspend-boot`|`true` / `false`|If the Mono debugger is enabled, this tells if Mono should wait for a debugger to be attached when it initialises. VenusRootLoader logs will indicate when the debugger will be attached, but if console logs are disabled, enabling this feature will make it seem that the game has not launched while it has, and it's waiting for a debugger to be attached. This is primarily used to debug issues which occurs very early on boot as it allows to attach a debugger at the earliest possible moment|

### Logging filters configuration
While the above settings allows to configure which loggers are enabled and the logger's specific settings, they don't allow to configure their verbosity filtering.

This is done through a `config.jsonc` specific structure in the `Logging` key. It allows to configure the verbosity of all logs, but it also allows to configure it for each category which overrides the default set. A cateogry is typically a type's full name, but it can be user defined such as the special `UNITY` category. A log's verbosity level can be one of the following (each levels includes the ones above it except for `None`):

- `Critical`: Showstopper errors that impacts critical functionality
- `Error`: Errors that don't impact critical functionality
- `Warning`: Events that aren't erroneous, but might be abnormal
- `Information`: Typical events that happens under normal operation
- `Debug`: A higher verbosity level primarily used to log detailed information that might be useful for a developer, but not to a user
- `Trace`: The highest verbosity where specific data or processing is logged even if performance could be impacted
- `None`: A special value to indicate no logs should be produced

An exhaustive documentation of this logging configuration structure is documented here: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#configure-logging-without-code

For example, consider the following `Logging` settings:

```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "VenusRootLoader.Bootstrap": "Trace",
    "VenusRootLoader.Bootstrap.Unity.PlayerConnectionDiscovery": "Debug"
  }
}
```
This does the following:

- By default, all logs of level `Debug` and above are included
- All logs from the bootstrap component of the modloader will be included including the `Trace` logs. Notice how this override what was specificed above because it is a more specific filter and also notice how this works because all the bootstrap's logging category starts with `VenusRootLoader.Bootstrap` since all types that logs are from that namespace
- The logs specifically in the `VenusRootLoader.Bootstrap.Unity.PlayerConnectionDiscovery` categoruy will not include their `Trace` logs. This overrides all the above filters because it is the most specific filter where a specific category level is configured
