using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace VenusRootLoader.Patching.Aot;

/// <summary>
/// Extensions to <see cref="CilInstructionCollection"/>.
/// </summary>
public static class CilInstructionCollectionExtensions
{
    extension(CilInstructionCollection il)
    {
        /// <summary>
        /// Replaces a collection of CIL instructions at the provided index range. The labels of the first instruction
        /// are preserved, but its offset will be invalidated.
        /// </summary>
        /// <param name="startReplaceRange">The starting index to replace the instructions.</param>
        /// <param name="endReplaceRange">The ending index to replace the instructions.</param>
        /// <param name="instructions">The instructions to replace the range.</param>
        public void ReplaceRange(int startReplaceRange, int endReplaceRange, List<CilInstruction> instructions)
        {
            if (instructions.Count == 0)
                throw new ArgumentException("Cannot be empty", nameof(instructions));

            il[startReplaceRange].ReplaceWith(instructions[0].OpCode, instructions[0].Operand);
            for (int i = startReplaceRange + 1; i <= endReplaceRange; i++)
                il.RemoveAt(startReplaceRange + 1);

            for (int i = 1; i < instructions.Count; i++)
                il.Insert(startReplaceRange + i, instructions[i]);
        }

        /// <summary>
        /// Gets a clone of a range of instructions (only opcodes and operands are cloned, labels are not).
        /// </summary>
        /// <param name="start">The index to start cloning.</param>
        /// <param name="end">The index to stop cloning inclusive.</param>
        /// <returns>A list of instructions that are a different copy of the specified range.</returns>
        public List<CilInstruction> CloneRange(int start, int end)
        {
            List<CilInstruction> innerLogicCheckClone = [];
            for (int i = start; i <= end; i++)
                innerLogicCheckClone.Add(new(il[i].OpCode, il[i].Operand));
            return innerLogicCheckClone;
        }
    }
}