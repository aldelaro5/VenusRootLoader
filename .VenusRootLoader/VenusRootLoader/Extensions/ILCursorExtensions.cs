using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace VenusRootLoader.Extensions;

internal static class ILCursorExtensions
{
    extension(ILCursor cursor)
    {
        public Instruction Current => cursor.Instrs[cursor.Index];

        public void Set(OpCode opcode)
        {
            cursor.Current.OpCode = opcode;
            cursor.Current.Operand = null;
        }

        public void Set(OpCode opcode, object operand)
        {
            cursor.Current.OpCode = opcode;
            cursor.Current.Operand = operand;
        }

        public void SetAndAdvance(OpCode opCode)
        {
            cursor.Set(opCode);
            cursor.Index++;
        }

        public void SetAndAdvance(OpCode opCode, object operand)
        {
            cursor.Set(opCode, operand);
            cursor.Index++;
        }

        public void SetToStaticDelegateCall<T>(T cb) where T : Delegate
        {
            cursor.Set(OpCodes.Call, cb.Method);
        }

        public void SetToStaticDelegateCallAndAdvance<T>(T cb) where T : Delegate
        {
            cursor.Set(OpCodes.Call, cb.Method);
            cursor.Index++;
        }
    }
}