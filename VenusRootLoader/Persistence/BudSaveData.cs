namespace VenusRootLoader.Persistence;

internal sealed class BudSaveData
{
    public required BaseGameExtensions BaseGameExtensions { get; init; }
    public required CustomBudSaveData CustomBudSaveData { get; init; }
}

internal sealed class BaseGameExtensions
{
    public required Dictionary<string, MedalShopLeafSaveData> MedalShops { get; init; } = new();
    public required Dictionary<string, bool> DiscoveryUnlocks { get; init; } = new();
    public required Dictionary<string, EnemySaveData> Enemies { get; init; } = new();
    public required Dictionary<string, bool> RecipeLibraryEntryUnlocks { get; init; } = new();
    public required Dictionary<string, bool> RecordUnlocks { get; init; } = new();
    public required Dictionary<string, bool> AreaUnlocks { get; init; } = new();
    public required Dictionary<string, bool> Flags { get; init; } = new();
    public required Dictionary<string, string> Flagstrings { get; init; } = new();
    public required Dictionary<string, int> Flagvars { get; init; } = new();
    public required Dictionary<string, bool> CrystalBerries { get; init; } = new();
}

internal sealed class MedalShopLeafSaveData
{
    public required List<string> AvailablePool { get; init; } = new();
    public required List<string> ShopStock { get; init; } = new();
}

internal sealed class EnemySaveData
{
    public required bool IsBestiaryEntryUnlocked { get; init; }
    public required int AmountSeen { get; init; }
    public required int AmountDefeated { get; init; }
}

internal sealed class CustomBudSaveData;