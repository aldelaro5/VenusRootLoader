using HarmonyLib;
using Microsoft.Extensions.Logging;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class SkillsCollector : IBaseGameCollector
{
    private readonly string[] _skillsData = RootCollector.ReadTextAssetLines(ResourcesPaths.DataSkillsPath);

    private readonly Dictionary<int, string[]> _skillsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(ResourcesPaths.DataLocalizedSkillsPathSuffix);

    private readonly string[] _skillNamedIds = Enum.GetNames(typeof(MainManager.Skills)).ToArray();

    private readonly ILogger<SkillsCollector> _logger;
    private readonly ITextAssetParser<SkillLeaf> _skillTextAssetParser;
    private readonly ILocalizedTextAssetParser<SkillLeaf> _skillLocalizedTextAssetParser;
    private readonly IOrderedLeavesRegistry<SkillLeaf> _orderedSkillsRegistry;
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public SkillsCollector(
        ILogger<SkillsCollector> logger,
        ITextAssetParser<SkillLeaf> skillTextAssetParser,
        ILocalizedTextAssetParser<SkillLeaf> skillLocalizedTextAssetParser,
        IOrderedLeavesRegistry<SkillLeaf> orderedSkillsRegistry,
        ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _logger = logger;
        _skillTextAssetParser = skillTextAssetParser;
        _skillLocalizedTextAssetParser = skillLocalizedTextAssetParser;
        _orderedSkillsRegistry = orderedSkillsRegistry;
        _languageRegistry = languageRegistry;
    }

    public void CollectBaseGameData()
    {
        Dictionary<string, MethodInfo> refreshSkillPredicates = RootCollector.GetLeavesMethods(
            typeof(MainManager),
            nameof(MainManager.RefreshSkills),
            "Predicate");

        int skillsAmount = _skillNamedIds.Length;
        for (int i = 0; i < skillsAmount; i++)
        {
            SkillLeaf skillLeaf = _orderedSkillsRegistry.RegisterExistingWithOrdering(i, _skillNamedIds[i]);
            _skillTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataSkillsPath,
                _skillsData[i],
                skillLeaf);

            // The overworld skills all have incorrect usable by fields so we need to fix them.
            skillLeaf.UsableBy = skillLeaf.GameId switch
            {
                (int)MainManager.Skills.FieldBeemerang => SkillLeaf.SkillUsability.Bee,
                (int)MainManager.Skills.FieldHalt => SkillLeaf.SkillUsability.Bee,
                (int)MainManager.Skills.FieldFly => SkillLeaf.SkillUsability.Bee,
                (int)MainManager.Skills.FieldHorn => SkillLeaf.SkillUsability.Beetle,
                (int)MainManager.Skills.FirstDash => SkillLeaf.SkillUsability.Beetle,
                (int)MainManager.Skills.FieldDig => SkillLeaf.SkillUsability.Beetle,
                (int)MainManager.Skills.FieldDash => SkillLeaf.SkillUsability.Beetle,
                (int)MainManager.Skills.FieldFreeze => SkillLeaf.SkillUsability.Moth,
                (int)MainManager.Skills.FieldShield => SkillLeaf.SkillUsability.Moth,
                (int)MainManager.Skills.FieldIcecle => SkillLeaf.SkillUsability.Moth,
                _ => skillLeaf.UsableBy
            };

            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                skillLeaf.LocalizedData[_languageRegistry.GetByGameId(j)] = new();
                _skillLocalizedTextAssetParser.FromTextAssetSerializedString(
                    ResourcesPaths.DataLocalizedSkillsPathSuffix,
                    j,
                    _skillsLanguageData[j][i],
                    skillLeaf);
            }

            if (refreshSkillPredicates.TryGetValue(skillLeaf.NamedId, out MethodInfo method))
            {
                skillLeaf.UsabilityPredicate =
                    RootCollector.CreateOpenDelegateFromMethod<SkillLeaf.SkillUsabilityPredicate>(method);
            }
        }

        int[] skillsBaseGameOrdering = ComputerSkillsBaseGameOrdering();
        _orderedSkillsRegistry.SetBaseGameOrdering(skillsBaseGameOrdering);

        RootCollector.LogCollectedAmount(_logger, _orderedSkillsRegistry.Registry, skillsAmount);
    }

    private static int[] ComputerSkillsBaseGameOrdering()
    {
        // This ordering always fit with the base game no matter the party member.
        HashSet<int> teamSkills =
        [
            (int)MainManager.Skills.BeeFly,
            (int)MainManager.Skills.IceDrill,
            (int)MainManager.Skills.IceBeemerang,
            (int)MainManager.Skills.IceSphere
        ];

        // Every member have these in this exact order.
        HashSet<int> sharedMedalSkills =
        [
            (int)MainManager.Skills.Sturdy,
            (int)MainManager.Skills.HardCharge,
            (int)MainManager.Skills.RoyalDecree
        ];

        List<List<int>> skillsBeforeTeamSkills = [];
        List<List<int>> skillsAfterTeamSkills = [];

        MethodInfo refreshSkillsMethod =
            AccessTools.DeclaredMethod(typeof(MainManager), nameof(MainManager.RefreshSkills))!;
        using DynamicMethodDefinition dmd = new(refreshSkillsMethod);
        ILContext context = new(dmd.Definition);
        context.Invoke(ctx =>
        {
            ILCursor cursor = new(ctx);
            ILLabel[] switchLabels = [];
            cursor.GotoNext(x => x.MatchSwitch(out switchLabels!));
            for (int i = 0; i < switchLabels.Length; i++)
            {
                ILLabel label = switchLabels[i];
                cursor.GotoLabel(label);
                int indexStart = cursor.Index;
                int indexEnd = cursor.Instrs.Count - 1;
                if (cursor.TryGotoNext(x => x.MatchBr(out _)))
                    indexEnd = cursor.Index;

                skillsBeforeTeamSkills.Add([]);
                skillsAfterTeamSkills.Add([]);

                bool pastTeamSkills = false;
                cursor.Index = indexStart;
                cursor.GotoNext(x => x.MatchCallOrCallvirt<List<int>>(nameof(List<>.Add)));
                while (cursor.Index < indexEnd)
                {
                    cursor.Instrs[cursor.Index - 1].MatchLdcI4(out int skillGameId);
                    if (sharedMedalSkills.Contains(skillGameId))
                        break;

                    if (!teamSkills.Contains(skillGameId))
                    {
                        List<int> skillsList = pastTeamSkills
                            ? skillsAfterTeamSkills[i]
                            : skillsBeforeTeamSkills[i];
                        skillsList.Add(skillGameId);
                    }
                    else
                    {
                        pastTeamSkills = true;
                    }

                    cursor.Index++;
                    if (!cursor.TryGotoNext(x => x.MatchCallOrCallvirt<List<int>>(nameof(List<>.Add))))
                        break;
                }
            }
        });

        // This is an unused skill, but was originally meant to be a revival skill for Bee before Beetle got PepTalk.
        // Since PepTalk is right before the shared medals one, we can just assume Bee's would be ordered the same way.
        skillsAfterTeamSkills[0].Add((int)MainManager.Skills.RevivalMassage);

        int[] skillsBaseGameOrdering =
        [
            // These are unused, and we have no traces of them or what their purpose would have been, but they're still
            // technically present so we'll just make them appear before everything.
            (int)MainManager.Skills.RESERVED,
            (int)MainManager.Skills.RESERVED2,
            .. skillsBeforeTeamSkills.SelectMany(x => x),
            .. teamSkills,
            .. skillsAfterTeamSkills.SelectMany(x => x),
            .. sharedMedalSkills
        ];
        return skillsBaseGameOrdering;
    }
}