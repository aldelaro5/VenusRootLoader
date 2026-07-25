# UnityJitMonoBcl
These files were pulled from a Unity 2018.4.12f1 installation. This is the same version distributed with all release versions of Bug Fables. They contain assemblies that implements the Base Class Library (aka BCL) of .NET made by the Mono development team, but modified by Unity to work with their version of Mono.

## Explanation of `unityjit` vs `unityaot`
Specifically, these assemblies were found in the `Editor/Data/MonoBleedingEdge/lib/mono/unityjit` directory. As such, the BCL assemblies found there are refered to as the `unityjit` BCL. The game comes with a set of assemblies located in the `Editor/Data/MonoBleedingEdge/lib/mono/unityaot` directory of the installation and as such, this BCL is refered to as the `unityaot` BCL.

A developer using this version of Unity is able to select the BCL they want even though it's not very detailed in Unity's documentation. The setting to do so is located in the editor in the Project Settings, Player tab on the left, under the Other Settings category with a label of "Api Compatibility Level". This only applies if the project uses the net472 compliant Mono runtime which Bug Fables does as of 1.1.2. There are 2 options available which maps to either of the aforementioned BCL set:

- .NET Standard 2.0 -> `unityaot`
- .NET 4.x -> `unityjit`

Notably, .NET Standard 2.0 is the default option for new projects and it is the one Unity recommends the most because it has "broader compatibility". Bug Fables is released with that setting set to .NET Standard 2.0 which explains why it ships with the `unityaot` BCL. Unity recommends to only set the compatibility level to .NET 4.x if the project has a specific need to access the broader APIs since it's "less compatible".

## What the Unity documentation does not mention
The truth is however much more nuanced. What the documentation doesn't explain is that choosing the `unityaot` BCL assumes compatibility through all platforms Unity supports, including mobile. This is relevant because it is believed to be the justification for the `unityaot` BCL to have its System.Reflection.Emit (aka SRE) stubbed out. Mobile platforms don't support it so it could make sense for developers targetting these platforms and others to not have SRE for any platforms if they don't use it. In contrast, `unityjit` is much closer to the BCL Mono provides meaning this profile offers the best possible API from the BCL that the given Mono version supported at the time. SRE support is of course included in that BCL.

## Negative impacts on PC modding
The problem is if the build of the game is only meant to be distributed on PC (which is the case for Bug Fables, console builds are handled separately), then there's no good reasons to not have access to SRE which is normally supported. This creates an annoying situation because mods might frequently need the APIs offered by SRE since it allows to have a standard way to define in memory assemblies and code. Reorg MonoMod for example will try to leverage SRE support when possible, but if it is stripped, it will falback to Cecil which has its own downsides.

Because of this, modding Bug Fables or many other known games where this issue also applies needs to involve some form of unstripping solutions to get SRE support. VenusRootLoader is no exceptions: it needs to support the ability to load the `unityjit` BCL instead of the `unityaot` one provided by the game at runtime.

## Method of unstripping the BCL
The most well known method of doing this is to set the Mono's assemblies path such that a path containing the needed `unityjit` assemblies is prepanded before the one Unity sets (typically the game's Data/Managed directory). Since VenusRootLoader specifically targets Bug Fables which always has the same Unity version with no signs of this version changing in the future, it turns out it's simpler to include the assemblies within the project and have them copied on the output directory on each build. Since the assemblies gets distributed alongside the loader, the user no longer needs to setup anything: everything is provided for them such that they won't need to worry about this.

## How the specific assemblies were chosen
As for why these 25 assemblies were selected specifically out of the many others present in the `unityjit` directory, 23 of them are the `unityjit` equivalent that comes with the game. This means they have a matching `unityaot` equivalent present with the game vanilla. There was testing done that concluded that the same 23 assemblies also appears in a blank project's build when the compatibility level is configured to use the `unityaot` BCL.

The other 2 assemblies (Mono.Posix and System.Security) do not ship with the game, but they would have had if the project was configured to be built with the `unityjit` BCL. This was tested both on building an export of the game's project and on building a blank project. No other assemblies other than these 2 have this characteristic.

The result is we only have the assemblies we know changed in the `unityjit` BCL in addition to the `unityjit` exclusive assemblies that would have been present if the game was configured to use it originally. This is enough to appear completely transparent from Mono and leads to no assembly resolution errors at runtime.
