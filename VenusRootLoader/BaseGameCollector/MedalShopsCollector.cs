using HarmonyLib;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class MedalShopsCollector : IBaseGameCollector
{
    private readonly string[] _baseGameMedalShopNamedIds = ["Merab", "Shades"];

    private readonly ILogger<MedalShopsCollector> _logger;
    private readonly ILeavesRegistry<MedalShopLeaf> _medalShopsRegistry;
    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;
    private readonly ILeavesRegistry<MedalLeaf> _medalsRegistry;
    private readonly IAssemblyCSharpDataCollector _assemblyCSharpDataCollector;

    public MedalShopsCollector(
        ILogger<MedalShopsCollector> logger,
        ILeavesRegistry<FlagLeaf> flagsRegistry,
        ILeavesRegistry<MedalLeaf> medalsRegistry,
        ILeavesRegistry<MedalShopLeaf> medalShopsRegistry,
        IAssemblyCSharpDataCollector assemblyCSharpDataCollector)
    {
        _logger = logger;
        _flagsRegistry = flagsRegistry;
        _medalsRegistry = medalsRegistry;
        _medalShopsRegistry = medalShopsRegistry;
        _assemblyCSharpDataCollector = assemblyCSharpDataCollector;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int medalShopsAmount = CollectMedalShopsAmount();
        List<List<Branch<MedalLeaf>>> medalShopsStartingStock = CollectMedalShopsStartingStock(medalShopsAmount);
        List<Branch<FlagLeaf>> medalShopsBoughtAllStockFlags = CollectMedalShopsBoughtAllStockFlags(medalShopsAmount);

        for (int i = 0; i < medalShopsAmount; i++)
        {
            MedalShopLeaf medalShopLeaf =
                _medalShopsRegistry.RegisterExisting(i, _baseGameMedalShopNamedIds[i], baseGameId);
            medalShopLeaf.StartingMedalsStock.AddRange(medalShopsStartingStock[i]);
            medalShopLeaf.BoughtAllStockFlag = medalShopsBoughtAllStockFlags[i];
        }

        _logger.LogInformation(
            "Collected and registered {MedalShopsAmount} base game medal shops",
            medalShopsAmount);
    }

    private static int CollectMedalShopsAmount()
    {
        int medalShopsAmount = 0;

        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(typeof(MainManager), nameof(MainManager.SetVariables))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
        ILContext context = new(dmd.Definition);
        context.Invoke(ilc =>
        {
            ILCursor cursor = new(ilc);
            cursor.GotoNext(i => i.MatchStfld<MainManager>(nameof(MainManager.badgeshops)));
            cursor.GotoPrev(i => i.MatchLdcI4(out medalShopsAmount));
        });

        return medalShopsAmount;
    }

    private List<List<Branch<MedalLeaf>>> CollectMedalShopsStartingStock(int medalShopsAmount)
    {
        List<List<Branch<MedalLeaf>>> medalShopsStartingStock = new();

        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(typeof(MainManager), nameof(MainManager.SetUpBadges))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
        ILContext context = new(dmd.Definition);
        context.Invoke(ilc =>
        {
            ILCursor cursor = new(ilc);
            for (int i = 0; i < medalShopsAmount; i++)
            {
                IMetadataTokenProvider token = null!;
                cursor.GotoNext(instruction => instruction.MatchLdtoken(out token!));
                FieldInfo startingStockField = ((FieldReference)token).ResolveReflection();
                int[] startingStockMedalIds =
                    _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(startingStockField);
                List<Branch<MedalLeaf>> medals = new();
                foreach (int medalId in startingStockMedalIds)
                    medals.Add(new(_medalsRegistry.LeavesByGameIds[medalId]));
                medalShopsStartingStock.Add(medals);
            }
        });

        return medalShopsStartingStock;
    }

    private List<Branch<FlagLeaf>> CollectMedalShopsBoughtAllStockFlags(int medalShopsAmount)
    {
        List<Branch<FlagLeaf>> medalShopsStartingStock = new();

        MethodInfo setVariableMethod =
            AccessTools.DeclaredMethod(typeof(MainManager), nameof(MainManager.EndOfMessage))!;
        using DynamicMethodDefinition dmd = new(setVariableMethod);
        ILContext context = new(dmd.Definition);
        context.Invoke(ilc =>
        {
            ILCursor cursor = new(ilc);
            int firstMedalShopBoughtAllFlagGameId = 0;
            cursor.GotoNext(i => i.MatchLdfld<MainManager>(nameof(MainManager.badgeshops)));
            cursor.GotoNext(i => i.MatchLdfld<MainManager>(nameof(MainManager.flags)));
            cursor.GotoNext(i => i.MatchLdcI4(out firstMedalShopBoughtAllFlagGameId));

            for (int i = 0; i < medalShopsAmount; i++)
                medalShopsStartingStock.Add(new(_flagsRegistry.LeavesByGameIds[firstMedalShopBoughtAllFlagGameId + i]));
        });

        return medalShopsStartingStock;
    }
}