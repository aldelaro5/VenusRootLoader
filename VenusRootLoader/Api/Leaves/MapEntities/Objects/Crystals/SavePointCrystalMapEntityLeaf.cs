using UnityEngine;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Crystals;

public sealed class SavePointCrystalMapEntityLeaf : CrystalMapEntityLeaf
{
    internal SavePointCrystalMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public bool HealsPartyWhenHit { get => InternalData[2].Value == 0; set => InternalData[2].Value = value ? 0 : 1; }

    public Vector3 SpawnPositionSavedWhenSavingTheGame
    {
        get => InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value;
    }

    internal void InitializeFromNew(
        Vector3 startingPosition,
        Vector3 spawnPositionSavedWhenSavingTheGame,
        bool healsPartyWhenHit)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(1), new(0), new(1)]);
        SpawnPositionSavedWhenSavingTheGame = spawnPositionSavedWhenSavingTheGame;
        HealsPartyWhenHit = healsPartyWhenHit;
    }
}