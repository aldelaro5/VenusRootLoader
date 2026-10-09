using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.LogicContainer;
using VenusRootLoader.Patching.Aot.LogicExtraction.SwitchArms;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.TopLevelPatching;

public sealed class MainManagerPatcher : ITopLevelTypePatcher
{
    public string TypeName => "MainManager";

    public void PatchType(GameModuleData gameModuleData, TypeDefinition type)
    {
        MethodDefinition setTextMethod = type.Methods
            .Where(x => x.Name == "SetText")
            .OrderByDescending(x => x.Signature!.ParameterTypes.Count)
            .First();
        StateMachine setTextStateMachine = new(gameModuleData, setTextMethod);
        ExtractSetText(gameModuleData, type, setTextStateMachine);

        MethodDefinition doItemEffectMethod = type.Methods
            .Single(x => x.Name == "DoItemEffect");
        MethodLogicContainer methodContainer = new(doItemEffectMethod);
        ExtractDoItemEffect(gameModuleData, methodContainer);
    }

    private static void ExtractSetText(
        GameModuleData gameModuleData,
        TypeDefinition type,
        StateMachine setTextStateMachine)
    {
        CilInstructionCollection setTextMoveNextIl =
            setTextStateMachine.MoveNextMethod.CilMethodBody!.Instructions;
        setTextMoveNextIl.CalculateOffsets();
        AsmResolverIlCursor ilCursor = new(setTextMoveNextIl);

        ilCursor.MatchNext(x => x.OpCode == Call && x.Operand == type.Methods.Single(y => y.Name == "SetTalk"));
        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction commandSwitch = ilCursor.Current();
        int indexCommandSwitch = ilCursor.Index;
        IList<ICilLabel> switchArmLabels = (IList<ICilLabel>)ilCursor.Current().Operand!;
        ilCursor.MatchPrevious(x => x.OpCode == Ldfld);
        CilInstruction beforeCommandSwitch = ilCursor.Current();

        FixWaitCnArmBoundaries(ilCursor, switchArmLabels, setTextMoveNextIl);
        setTextMoveNextIl.CalculateOffsets();
        ilCursor.Index = indexCommandSwitch;
        FixMissingSetTextGotoCases(ilCursor, switchArmLabels, setTextMoveNextIl);

        setTextStateMachine.AssignNewContext(
            "SetTextCommandContext",
            [
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("text", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("fonttype", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("linebreak", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("dialogue", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("tridimensional", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("position", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("cameraoffset", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("size", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("parent", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("caller", false),

                setTextStateMachine.AddContextFieldFromSpeakableFieldName("speed", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("currentoffset", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("currentline", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("maxlenght", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("bleeppitch", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("bleepvolume", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("langOffset", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("colorindex", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("sort", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("transferi", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("writen", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("ignorenext", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("layer", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("centralize", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("wavy", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("shaky", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("rainbow", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("glitchy", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("skipi", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("end", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("promptmenu", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("minibubble", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("tempoverf", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("questboardpromp", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("tempevent", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("fadeletter", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("fontlock", false),
                // TODO: Only affects something on the first 10 iterations of the char loop?
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("initialflip", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("ui3d", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("single", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("testing", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("locksize", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("superglitch", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("dropshadow", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("returnentitycol", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("ndd", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("ds", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("transfer", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("backbox", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("textholder", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("textbox", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("windowstyle", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("bubbles", true),
                // TODO: Probably should be read only, Blank writes a new blank list while it probably wanted to clear it.
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("buts", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("tokenbox", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("bleep", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("i", false),
                // TODO: Only Prompt writes to it, but it's suspicious, recheck if this could be read only.
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("command", false),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("temp", true),
                setTextStateMachine.AddContextFieldFromSpeakableFieldName("com", true)
            ]);

        SwitchArmsCoroutineExtractor setTextSwitchArmsExtractor = new(gameModuleData, setTextStateMachine);
        setTextSwitchArmsExtractor.ExtractSwitchArms(
            "SetTextCommand",
            commandSwitch,
            beforeCommandSwitch,
            null,
            null);
    }

    private static void FixWaitCnArmBoundaries(
        AsmResolverIlCursor ilCursor,
        IList<ICilLabel> switchArmLabels,
        CilInstructionCollection setTextMoveNextIl)
    {
        ilCursor.MatchNext(x => x.Offset == switchArmLabels[165].Offset);
        int indexSwitchArmStartFromLabel = ilCursor.Index;
        ilCursor.MatchPrevious(x => x.IsUnconditionalBranch());
        int indexRealSwitchArmStart = ilCursor.Index + 1;
        ilCursor.Index++;
        ilCursor.MatchNext(x => x.IsUnconditionalBranch());
        int indexSwitchArmEnd = ilCursor.Index;

        List<CilInstruction> instructionsToMove = setTextMoveNextIl
            .Skip(indexSwitchArmStartFromLabel)
            .Take(indexSwitchArmEnd - indexSwitchArmStartFromLabel + 1)
            .ToList();
        setTextMoveNextIl.RemoveRange(indexSwitchArmStartFromLabel, instructionsToMove.Count);
        setTextMoveNextIl.InsertRange(indexRealSwitchArmStart, instructionsToMove);
        setTextMoveNextIl.Insert(indexSwitchArmEnd + 1, new CilInstruction(Br, instructionsToMove[0].CreateLabel()));
    }

    private static void FixMissingSetTextGotoCases(
        AsmResolverIlCursor ilCursor,
        IList<ICilLabel> switchArmLabels,
        CilInstructionCollection setTextMoveNextIl)
    {
        ilCursor.MatchNext(x => x.Offset == switchArmLabels[179].Offset);
        ilCursor.MatchNext(x => x.OpCode == Stfld);
        ilCursor.Index++;
        setTextMoveNextIl.Insert(ilCursor.Index, new CilInstruction(Br, ilCursor.Current().CreateLabel()));
        ilCursor.MatchNext(x => x.Offset == switchArmLabels[209].Offset);
        ilCursor.MatchNext(x => x.OpCode == Stfld);
        ilCursor.Index++;
        setTextMoveNextIl.Insert(ilCursor.Index, new CilInstruction(Br, ilCursor.Current().CreateLabel()));
    }

    private static void ExtractDoItemEffect(GameModuleData gameModuleData, MethodLogicContainer doItemEffectContainer)
    {
        CilInstructionCollection methodIl = doItemEffectContainer.ReceivingMethod.CilMethodBody!.Instructions;
        AsmResolverIlCursor ilCursor = new(methodIl);

        ilCursor.MatchNext(x => x.OpCode == Switch);

        CilInstruction doItemEffectSwitch = ilCursor.Current();
        IList<ICilLabel> switchArmLabels = (IList<ICilLabel>)ilCursor.Current().Operand!;
        ilCursor.MatchPrevious(x => x.OpCode == Ldarg_0 || x.OpCode == Ldarg);
        CilInstruction beforeDoItemEffectSwitch = ilCursor.Current();

        FixMissingDoItemEffectGotoCases(ilCursor, switchArmLabels, methodIl);

        doItemEffectContainer.AssignNewContext(
            "DoItemEffectContext",
            [
                doItemEffectContainer.AddContextFieldFromArgumentIndexAndLocalIndex(0, 0, true),
                doItemEffectContainer.AddContextFieldFromArgumentIndex(1, false),
                doItemEffectContainer.AddContextFieldFromArgumentIndex(2, true),
            ]);

        SwitchArmsMethodsExtractor doItemEffectSwitchArmsExtractor = new(
            gameModuleData,
            doItemEffectContainer);
        doItemEffectSwitchArmsExtractor.ExtractSwitchArms(
            "DoItemEffect",
            doItemEffectSwitch,
            beforeDoItemEffectSwitch,
            null,
            null);
    }

    private static void FixMissingDoItemEffectGotoCases(
        AsmResolverIlCursor ilCursor,
        IList<ICilLabel> switchArmLabels,
        CilInstructionCollection doItemEffectIl)
    {
        ilCursor.MatchNext(x => x.Offset == switchArmLabels[8].Offset);
        ilCursor.MatchNext(x => x.OpCode == Call && ((IMethodDefOrRef)x.Operand!).Name == "BadgeIsEquipped");
        ilCursor.MatchNext(x => x.IsConditionalBranch());
        ilCursor.Index++;
        doItemEffectIl.Insert(ilCursor.Index, new CilInstruction(Br, ilCursor.Current().CreateLabel()));
    }
}