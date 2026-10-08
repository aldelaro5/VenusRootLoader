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

        setTextStateMachine.AssignNewContext(
            "SetTextCommandContext",
            [
                setTextStateMachine.AddContextFieldFromSpeakableName("text", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("fonttype", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("linebreak", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("dialogue", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("tridimensional", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("position", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("cameraoffset", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("size", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("parent", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("caller", false),

                setTextStateMachine.AddContextFieldFromSpeakableName("speed", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("currentoffset", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("currentline", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("maxlenght", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("bleeppitch", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("bleepvolume", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("langOffset", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("colorindex", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("sort", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("transferi", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("writen", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("ignorenext", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("layer", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("centralize", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("wavy", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("shaky", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("rainbow", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("glitchy", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("skipi", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("end", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("promptmenu", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("minibubble", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("tempoverf", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("questboardpromp", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("tempevent", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("fadeletter", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("fontlock", false),
                // TODO: Only affects something on the first 10 iterations of the char loop?
                setTextStateMachine.AddContextFieldFromSpeakableName("initialflip", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("ui3d", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("single", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("testing", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("locksize", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("superglitch", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("dropshadow", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("returnentitycol", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("ndd", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("ds", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("transfer", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("backbox", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("textholder", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("textbox", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("windowstyle", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("bubbles", true),
                // TODO: Probably should be read only, Blank writes a new blank list while it probably wanted to clear it.
                setTextStateMachine.AddContextFieldFromSpeakableName("buts", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("tokenbox", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("bleep", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("i", false),
                // TODO: Only Prompt writes to it, but it's suspicious, recheck if this could be read only.
                setTextStateMachine.AddContextFieldFromSpeakableName("command", false),
                setTextStateMachine.AddContextFieldFromSpeakableName("temp", true),
                setTextStateMachine.AddContextFieldFromSpeakableName("com", true)
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