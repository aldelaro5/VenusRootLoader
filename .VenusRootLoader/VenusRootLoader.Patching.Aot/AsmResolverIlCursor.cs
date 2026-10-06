using AsmResolver.PE.DotNet.Cil;
using System.Collections;

namespace VenusRootLoader.Patching.Aot;

/// <summary>
/// A utility class to easily navigates within a sequence of <see cref="CilInstruction"/>.
/// </summary>
public sealed class AsmResolverIlCursor
{
    private IList<CilInstruction> Il { get; }

    /// <summary>
    /// The current index of the cursor within the list of instructions. A value of -1 means the index is invalid.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Obtains the current instruction if the <see cref="Index"/> is valid.
    /// </summary>
    /// <returns>The current <see cref="CilInstruction"/> if the <see cref="Index"/> is valid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <see cref="Index"/> is invalid.</exception>
    public CilInstruction Current()
    {
        if (Index < 0 || Index >= Il.Count)
            throw new ArgumentOutOfRangeException(nameof(Index));
        return Il[Index];
    }

    /// <summary>
    /// Creates an <see cref="AsmResolverIlCursor"/> using a list of IL instructions.
    /// </summary>
    /// <param name="il">The list of IL instructions to browse with the cursor.</param>
    public AsmResolverIlCursor(IList<CilInstruction> il)
    {
        Il = il;
        Index = 0;
    }

    /// <summary>
    /// Finds the next instruction that satisfies a predicate. Once found, the <see cref="Index"/> is set to it. If not found,
    /// the <see cref="Index"/> is set to -1.
    /// </summary>
    /// <param name="predicate">The predicate to match.</param>
    public void MatchNext(Predicate<CilInstruction> predicate)
    {
        while (Index < Il.Count)
        {
            if (predicate(Il[Index]))
                return;
            Index++;
        }

        Index = -1;
    }

    /// <summary>
    /// Finds the previous instruction that satisfies a predicate. Once found, the <see cref="Index"/> is set to it. If not found,
    /// the <see cref="Index"/> is set to -1.
    /// </summary>
    /// <param name="predicate">The predicate to match.</param>
    public void MatchPrevious(Predicate<CilInstruction> predicate)
    {
        while (Index >= 0)
        {
            if (predicate(Il[Index]))
                return;
            Index--;
        }

        Index = -1;
    }

    /// <summary>
    /// Assuming the IL instructions are within the <see cref="IEnumerator.MoveNext"/> of a state machine, this obtains
    /// the current state number from the current index assuming all states are in sequential order.
    /// </summary>
    /// <returns>The state number that corresponds to the current instruction.</returns>
    public int ObtainCurrentStateMachineState()
    {
        int oldIndex = Index;
        MatchPrevious(x => x.OpCode == CilOpCodes.Ret);
        MatchPrevious(x => x.OpCode == CilOpCodes.Stfld);
        MatchPrevious(x => x.IsLdcI4());
        int switchStateNumber = Index < 0 ? 0 : Current().GetLdcI4Constant();
        Index = oldIndex;
        return switchStateNumber;
    }
}