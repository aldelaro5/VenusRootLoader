using MonoDetour.HookGen;

[assembly: MonoDetourTargets(typeof(MainManager), GenerateControlFlowVariants = true)]
[assembly: MonoDetourTargets(typeof(BattleControl))]
[assembly: MonoDetourTargets(typeof(MapControl))]
[assembly: MonoDetourTargets(typeof(EventControl))]
[assembly: MonoDetourTargets(typeof(EntityControl))]
[assembly: MonoDetourTargets(typeof(NPCControl))]
[assembly: MonoDetourTargets(typeof(StartMenu))]
[assembly: MonoDetourTargets(typeof(PauseMenu))]
[assembly: MonoDetourTargets(typeof(InputIOManager.InputIO))]
[assembly: MonoDetourTargets(typeof(Hazards))]