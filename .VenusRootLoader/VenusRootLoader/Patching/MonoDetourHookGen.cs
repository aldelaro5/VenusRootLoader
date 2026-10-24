using MonoDetour.HookGen;
using UnityEngine;

[assembly: MonoDetourTargets(typeof(MainManager), GenerateControlFlowVariants = true)]
[assembly: MonoDetourTargets(typeof(BattleControl))]
[assembly: MonoDetourTargets(typeof(MapControl), GenerateControlFlowVariants = true)]
[assembly: MonoDetourTargets(typeof(EventControl))]
[assembly: MonoDetourTargets(typeof(EntityControl))]
[assembly: MonoDetourTargets(typeof(NPCControl))]
[assembly: MonoDetourTargets(typeof(StartMenu))]
[assembly: MonoDetourTargets(typeof(PauseMenu))]
[assembly: MonoDetourTargets(typeof(InputIOManager.InputIO), GenerateControlFlowVariants = true)]
[assembly: MonoDetourTargets(typeof(Hazards))]
[assembly: MonoDetourTargets(typeof(AttributeHelperEngine), GenerateControlFlowVariants = true)]