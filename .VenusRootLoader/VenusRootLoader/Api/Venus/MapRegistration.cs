// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Api.Unity.AssetLoading;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.Runtime.Enums;
using VenusRootLoader.Unity.Runtime.ScriptableObjects;

namespace VenusRootLoader.Api;

public partial class Venus
{
    public MapLeaf RegisterMap(
        string namedId,
        IAssetLoader<GameObject> prefabLoader,
        Branch<AreaLeaf> area,
        Branch<DialogueLeaf> spyDialogue,
        Branch<MusicLeaf>? defaultMapMusic)
    {
        MapLeaf mapLeaf = RegistryResolver.Resolve<MapLeaf>().RegisterNew(BudId, namedId);
        mapLeaf.PrefabLoader = prefabLoader;
        mapLeaf.Area = area;
        mapLeaf.SpyDialogue = spyDialogue;
        mapLeaf.AddMusicToMap(defaultMapMusic);
        mapLeaf.DialoguesRegistry = new AutoSequentialIdBasedRegistry<MapDialogueLeaf>(
            LoggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.DialoguesRegistry)}"),
            IdSequenceDirection.Increment);
        mapLeaf.EntitiesRegistry = new AutoSequentialIdBasedRegistry<MapEntityLeaf>(
            LoggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.EntitiesRegistry)}"),
            IdSequenceDirection.Increment);
        return mapLeaf;
    }

    public MapLeaf RegisterMapFromScriptableObjectInAssetBundle(
        string namedId,
        AssetBundle assetBundle,
        string pathToScriptableObjectInBundle)
    {
        MapLeafScriptableObject scriptableObject =
            assetBundle.LoadAsset<MapLeafScriptableObject>(pathToScriptableObjectInBundle);
        if (scriptableObject == null)
        {
            ThrowHelper.ThrowArgumentException(
                $"No such {nameof(MapLeafScriptableObject)} exists in this AssetBundle at path {pathToScriptableObjectInBundle}");
        }

        MapLeaf mapLeaf = RegistryResolver.Resolve<MapLeaf>().RegisterNew(BudId, namedId);
        mapLeaf.PrefabLoader = new AssetLoaderFromBundle<GameObject>(assetBundle, pathToScriptableObjectInBundle);
        mapLeaf.Area = RegistryResolver.Resolve<AreaLeaf>()
            .LeavesByEffectiveIds[GetEffectiveIdFromScriptableObjectBranch(scriptableObject.Area, BudId)];

        mapLeaf.DialoguesRegistry = new AutoSequentialIdBasedRegistry<MapDialogueLeaf>(
            LoggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.DialoguesRegistry)}"),
            IdSequenceDirection.Increment);
        mapLeaf.EntitiesRegistry = new AutoSequentialIdBasedRegistry<MapEntityLeaf>(
            LoggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.EntitiesRegistry)}"),
            IdSequenceDirection.Increment);
        return mapLeaf;
    }

    private static string GetEffectiveIdFromScriptableObjectBranch(Branch scriptableObjectBranch, string mapCreatorId)
    {
        return scriptableObjectBranch.CreatorKind switch
        {
            BranchCreatorKind.BaseGame => scriptableObjectBranch.NamedId,
            BranchCreatorKind.SameAsThisAsset =>
                $"{mapCreatorId}{Constants.LeafEffectiveIdSeparator}{scriptableObjectBranch.NamedId}",
            BranchCreatorKind.Custom =>
                $"{scriptableObjectBranch.CustomCreatorId}{Constants.LeafEffectiveIdSeparator}{scriptableObjectBranch.NamedId}",
            _ => ThrowHelper.ThrowArgumentException<string>(nameof(Branch.CreatorKind))
        };
    }
}