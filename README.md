# VenusRootLoader

A proof of concept for a mod loader idea I had for Bug Fables. It doesn't do anything practical for
now, and it's only for experimentation.

It can be cross compiled from Linux to Windows. Since the goal is only to support Bug Fables, only
win-x64 platform is supported, but it should work under Wine/Proton. It is heavily inspired
by [MelonLoader](https://github.com/LavaGang/MelonLoader)
and [BepInEx](https://github.com/BepInEx/BepInEx).

## Setting up dependencies

There are some dependencies that needs to be installed before building this.

### Install LLVM

An installation of LLVM is required because you need a functional `clang-cl` added to your PATH
environment for building. After performing the installation, you can confirm it works by opening a
command prompt and perform the following command:

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
___
Once the above is done, you are now ready to build the project.

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

To install the mod loader into Bug Fables, simply copy the winhttp.dll (and the pdb if you want to
have debugging symbols) to the game's directory. From there, any launch of the game should cause the
mod loader to kick in during the game's boot process.
