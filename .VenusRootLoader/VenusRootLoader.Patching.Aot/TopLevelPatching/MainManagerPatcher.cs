using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using VenusRootLoader.Patching.Aot.StateMachineUtils;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

namespace VenusRootLoader.Patching.Aot.TopLevelPatching;

public sealed class MainManagerPatcher : ITopLevelTypePatcher
{
    public string TypeName => "MainManager";

    public void PatchType(LocalNetStandardReferenceImporter referenceImporter, TypeDefinition type)
    {
        MethodDefinition setTextMethod = type.Methods
            .Where(x => x.Name == "SetText")
            .OrderByDescending(x => x.Signature!.ParameterTypes.Count)
            .First();
        StateMachine setTextStateMachine = StateMachine.CreateFromEnumeratorMethod(setTextMethod);

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
        FixMissingGotoCases(ilCursor, switchArmLabels, setTextMoveNextIl);

        List<StateMachineContextField> setTextContextFields =
        [
            setTextStateMachine.GetContextFieldFromSpeakableName("text"),
            setTextStateMachine.GetContextFieldFromSpeakableName("fonttype"),
            setTextStateMachine.GetContextFieldFromSpeakableName("linebreak"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("dialogue"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("tridimensional"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("position"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("cameraoffset"),
            setTextStateMachine.GetContextFieldFromSpeakableName("size"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("parent"),
            setTextStateMachine.GetContextFieldFromSpeakableName("caller"),

            setTextStateMachine.GetContextFieldFromSpeakableName("speed"),
            setTextStateMachine.GetContextFieldFromSpeakableName("currentoffset"),
            setTextStateMachine.GetContextFieldFromSpeakableName("currentline"),
            setTextStateMachine.GetContextFieldFromSpeakableName("maxlenght"),
            setTextStateMachine.GetContextFieldFromSpeakableName("bleeppitch"),
            setTextStateMachine.GetContextFieldFromSpeakableName("bleepvolume"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("langOffset"),
            setTextStateMachine.GetContextFieldFromSpeakableName("colorindex"),
            setTextStateMachine.GetContextFieldFromSpeakableName("sort"),
            setTextStateMachine.GetContextFieldFromSpeakableName("transferi"),
            setTextStateMachine.GetContextFieldFromSpeakableName("writen"),
            setTextStateMachine.GetContextFieldFromSpeakableName("ignorenext"),
            setTextStateMachine.GetContextFieldFromSpeakableName("layer"),
            setTextStateMachine.GetContextFieldFromSpeakableName("centralize"),
            setTextStateMachine.GetContextFieldFromSpeakableName("wavy"),
            setTextStateMachine.GetContextFieldFromSpeakableName("shaky"),
            setTextStateMachine.GetContextFieldFromSpeakableName("rainbow"),
            setTextStateMachine.GetContextFieldFromSpeakableName("glitchy"),
            setTextStateMachine.GetContextFieldFromSpeakableName("skipi"),
            setTextStateMachine.GetContextFieldFromSpeakableName("end"),
            setTextStateMachine.GetContextFieldFromSpeakableName("promptmenu"),
            setTextStateMachine.GetContextFieldFromSpeakableName("minibubble"),
            setTextStateMachine.GetContextFieldFromSpeakableName("tempoverf"),
            setTextStateMachine.GetContextFieldFromSpeakableName("questboardpromp"),
            setTextStateMachine.GetContextFieldFromSpeakableName("tempevent"),
            setTextStateMachine.GetContextFieldFromSpeakableName("fadeletter"),
            setTextStateMachine.GetContextFieldFromSpeakableName("fontlock"),
            // TODO: Only affects something on the first 10 iterations of the char loop?
            setTextStateMachine.GetContextFieldFromSpeakableName("initialflip"),
            setTextStateMachine.GetContextFieldFromSpeakableName("ui3d"),
            setTextStateMachine.GetContextFieldFromSpeakableName("single"),
            setTextStateMachine.GetContextFieldFromSpeakableName("testing"),
            setTextStateMachine.GetContextFieldFromSpeakableName("locksize"),
            setTextStateMachine.GetContextFieldFromSpeakableName("superglitch"),
            setTextStateMachine.GetContextFieldFromSpeakableName("dropshadow"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("returnentitycol"),
            setTextStateMachine.GetContextFieldFromSpeakableName("ndd"),
            setTextStateMachine.GetContextFieldFromSpeakableName("ds"),
            setTextStateMachine.GetContextFieldFromSpeakableName("transfer"),
            setTextStateMachine.GetContextFieldFromSpeakableName("backbox"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("textholder"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("textbox"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("windowstyle"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("bubbles"),
            // TODO: Probably should be read only, Blank writes a new blank list while it probably wanted to clear it.
            setTextStateMachine.GetContextFieldFromSpeakableName("buts"),
            setTextStateMachine.GetContextFieldFromSpeakableName("tokenbox"),
            setTextStateMachine.GetContextFieldFromSpeakableName("bleep"),
            setTextStateMachine.GetContextFieldFromSpeakableName("i"),
            // TODO: Only Prompt writes to it, but it's suspicious, recheck if this could be read only.
            setTextStateMachine.GetContextFieldFromSpeakableName("command"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("temp"),
            setTextStateMachine.GetReadOnlyContextFieldFromSpeakableName("com")
        ];

        StateMachineContextInfo stateMachineContextInfo = new()
        {
            TypeName = "SetTextCommandContext",
            Fields = setTextContextFields,
        };

        SwitchArmsCoroutineExtractor.ExtractSwitchArmsToStateMachines(
            referenceImporter,
            setTextStateMachine,
            "SetTextCommand",
            stateMachineContextInfo,
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

    private static void FixMissingGotoCases(
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
}