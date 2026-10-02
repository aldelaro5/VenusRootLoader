# GameLibs Directory
This directory contains publicized assemblies used both for building VenusRootLoader itself and for any buds that installs the NuGet package. The Assembly-CSharp.dll has been prepatched by the AOT patcher, publicized and stripped for copyright reasons. The rest are all UnityEngine assemblies which were simply publicized without stripping.

## Purpose of GameLibs
The purpose of having these files in the repository is to allow CI builds to function without having a copy of the game. It means that the CI builds can skip the AOT patching process and just refer to the assemblies directly since this is all that's needed to build the project.

## Local development flow
For local development, it's preferable to go through the entire process so changes in the AOT patcher are reflected immediately. The VenusRootLoader's building infrastructure is configured by default to require a real copy of Assembly-CSharp.dll so it can then be AOT patched, publicized, and stripped during the build process. It will still ultimately place the final assemblies in this directory which would need to be commited to reflect the changes caused by the AOT patcher.

## CI flow
For the CI, there's an opt-in MSBuild property that allows to skip the processing step and assume the Assembly-CSharp.dll commited in this directory is the correct one.

## Caution
NEVER commit an unstripped version of Assembly-CSharp.dll. The building infrastructure is configured to only place a stripped version of the assembly here.