using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Objects.Enums;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.CuttableGrasses;

public sealed class CuttableGrassWithCrystalBerryDropMapEntityLeaf : CuttableGrassMapEntityLeaf
{
    internal CuttableGrassWithCrystalBerryDropMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public Branch<CrystalBerryLeaf> CrystalBerryDroppedWhenCut
    {
        get;
        set
        {
            InternalData[1].Value = value.Resolve().GameId;
            field = value;
        }
    } = null!;

    public ObjectDetectorBehavior DetectorBehavior
    {
        get
        {
            if (Modifiers.HasFlag(MapEntityModifiers.NDTCT))
                return ObjectDetectorBehavior.NeverDetects;
            return Modifiers.HasFlag(MapEntityModifiers.DDIST)
                ? ObjectDetectorBehavior.MustBe20UnitsOrLessToDetect
                : ObjectDetectorBehavior.AlwaysDetects;
        }
        set
        {
            switch (value)
            {
                case ObjectDetectorBehavior.AlwaysDetects:
                    Modifiers &= ~MapEntityModifiers.NDTCT;
                    Modifiers &= ~MapEntityModifiers.DDIST;
                    break;
                case ObjectDetectorBehavior.MustBe20UnitsOrLessToDetect:
                    Modifiers &= ~MapEntityModifiers.NDTCT;
                    Modifiers |= MapEntityModifiers.DDIST;
                    break;
                case ObjectDetectorBehavior.NeverDetects:
                    Modifiers |= MapEntityModifiers.NDTCT;
                    Modifiers &= ~MapEntityModifiers.DDIST;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(DetectorBehavior));
                    break;
            }
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition, Branch<CrystalBerryLeaf> crystalBerryDroppedWhenCut)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(0), new(0)]);
        CrystalBerryDroppedWhenCut = crystalBerryDroppedWhenCut;
    }

    internal override void InitializeFromExisting()
    {
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry = RegistryResolver.Resolve<CrystalBerryLeaf>();
        CrystalBerryDroppedWhenCut = new(crystalBerriesRegistry.GetByGameId(InternalData[1].Value));
    }
}