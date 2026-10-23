using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Extensions;

/// <summary>
/// Extensions for <see cref="ILCursor"/>.
/// </summary>
public static class ILCursorExtensions
{
    private static readonly FieldInfo MainManagerInstanceField =
        typeof(MainManager).FieldByName(nameof(MainManager.instance))!;

    private static readonly MethodInfo MonoBehaviourStartCoroutineMethod =
        typeof(MonoBehaviour).MethodByName(nameof(MonoBehaviour.StartCoroutine), [typeof(IEnumerator)])!;

    extension(ILCursor cursor)
    {
        /// <summary>
        /// The current instruction.
        /// </summary>
        public Instruction Current => cursor.Instrs[cursor.Index];

        /// <summary>
        /// Set the current instruction's opcode and leave its operand to null.
        /// </summary>
        /// <param name="opCode">The new opcode to set.</param>
        public void Set(OpCode opCode)
        {
            cursor.Current.OpCode = opCode;
            cursor.Current.Operand = null;
        }

        /// <summary>
        /// Set the current instruction's opcode and operand.
        /// </summary>
        /// <param name="opCode">The new opcode.</param>
        /// <param name="operand">The new operand.</param>
        public void Set(OpCode opCode, object operand)
        {
            cursor.Current.OpCode = opCode;
            cursor.Current.Operand = operand;
        }

        /// <summary>
        /// Set the current instruction's opcode and leave its operand to null, then advance the cursor after the instruction.
        /// </summary>
        /// <param name="opCode"></param>
        public void SetAndAdvance(OpCode opCode)
        {
            cursor.Set(opCode);
            cursor.Index++;
        }

        /// <summary>
        /// Set the current instruction's opcode and operand, then advance the cursor after the instruction.
        /// </summary>
        /// <param name="opCode">The new opcode.</param>
        /// <param name="operand">The new operand.</param>
        public void SetAndAdvance(OpCode opCode, object operand)
        {
            cursor.Set(opCode, operand);
            cursor.Index++;
        }

        /// <summary>
        /// Set the current instruction to be a call to a static delegate.
        /// </summary>
        /// <param name="cb">The delegate to call.</param>
        /// <typeparam name="T">The type of delegate to call, this can be automatically determined.</typeparam>
        public void SetToStaticDelegateCall<T>(T cb) where T : Delegate
        {
            cursor.Set(OpCodes.Call, cb.Method);
        }

        /// <summary>
        /// Set the current instruction to be a call to a static delegate, then advance the cursor after the instruction.
        /// </summary>
        /// <param name="cb">The delegate to call.</param>
        /// <typeparam name="T">The type of delegate to call, this can be automatically determined.</typeparam>
        public void SetToStaticDelegateCallAndAdvance<T>(T cb) where T : Delegate
        {
            cursor.Set(OpCodes.Call, cb.Method);
            cursor.Index++;
        }

        /// <summary>
        /// Emit a list of instructions that will yield return a coroutine from a <see cref="IEnumerator.MoveNext"/>
        /// method and resume at a label once done. After the target coroutine's first <see cref="IEnumerator.MoveNext"/>
        /// call, if it has already finished, the instructions returned will instead branch directly to the label and skip
        /// the yield return which allows to resume without waiting for the next frame.
        /// </summary>
        /// <param name="sourceMoveNextMethod">The <see cref="IEnumerator.MoveNext"/> method that will perform the yield return.
        ///     From a transpiler, this can be obtained by declaring a parameter in the transpiler method of type <see cref="MethodBase"/>.</param>
        /// <param name="targetCoroutineDelegate">The coroutine to yield return to. This delegate must return an <see cref="IEnumerator"/>.</param>
        /// <param name="argumentsLoadAction">An action to insert additional instructions that will place the arguments of
        ///     <paramref name="targetCoroutineDelegate"/> on the stack. The arguments to place includes the <c>this</c> for instance coroutine.</param>
        /// <param name="labelToResumeTo">The label to resume to after the yield return is done and <paramref name="targetCoroutineDelegate"/>
        ///     is done executing. If the label refers to a state that already exists, it will be reused, otherwise, a new state
        ///     will be created, and it will be used.</param>
        /// <typeparam name="T">The <paramref name="targetCoroutineDelegate"/> delegate type. This will automatically be determined based
        /// on the value of <paramref name="targetCoroutineDelegate"/></typeparam>
        public void EmitYieldReturnCoroutine<T>(
            MethodBase sourceMoveNextMethod,
            T targetCoroutineDelegate,
            Action<ILCursor> argumentsLoadAction,
            ILLabel labelToResumeTo)
            where T : Delegate
        {
            cursor.EmitYieldReturnCoroutine(
                sourceMoveNextMethod,
                targetCoroutineDelegate.Method,
                argumentsLoadAction,
                labelToResumeTo);
        }

        /// <summary>
        /// Emit a list of instructions that will yield return a coroutine from a <see cref="IEnumerator.MoveNext"/>
        /// method and resume at a label once done. After the target coroutine's first <see cref="IEnumerator.MoveNext"/>
        /// call, if it has already finished, the instructions returned will instead branch directly to the label and skip
        /// the yield return which allows to resume without waiting for the next frame.
        /// </summary>
        /// <param name="sourceMoveNextMethod">The <see cref="IEnumerator.MoveNext"/> method that will perform the yield return.
        /// From a transpiler, this can be obtained by declaring a parameter in the transpiler method of type <see cref="MethodBase"/>.</param>
        /// <param name="targetEnumeratorMethod">The coroutine's enumerator method to yield return to. This method must return an <see cref="IEnumerator"/>.</param>
        /// <param name="argumentsLoadAction">An action to insert additional instructions that will place the arguments of
        ///     <paramref name="targetEnumeratorMethod"/> on the stack. The arguments to place includes the <c>this</c> for instance coroutine.</param>
        /// <param name="labelToResumeTo">The label to resume to after the yield return is done and <paramref name="targetEnumeratorMethod"/>
        /// is done executing. If the label refers to a state that already exists, it will be reused, otherwise, a new state
        /// will be created, and it will be used.</param>
        public void EmitYieldReturnCoroutine(
            MethodBase sourceMoveNextMethod,
            MethodInfo targetEnumeratorMethod,
            Action<ILCursor> argumentsLoadAction,
            ILLabel labelToResumeTo)
        {
            int oldCursorIndex = cursor.Index;

            cursor.Goto(0);
            // The first switch is always the state switch.
            ILLabel[] stateLabels = null!;
            cursor.GotoNext(x => x.MatchSwitch(out stateLabels!));
            Instruction stateSwitch = cursor.Current;

            // Labels targeting the same instruction won't be equal so we need to manually find an instruction that both
            // has the label to resume to AND that at least one of its label is in the state switch.
            cursor.GotoLabel(labelToResumeTo);
            int stateToSetBeforeReturn = stateLabels.Length;
            if (cursor.IncomingLabels.Any(stateLabels.Contains))
            {
                stateToSetBeforeReturn = Array.FindIndex(stateLabels, x => cursor.IncomingLabels.Contains(x));
            }
            else
            {
                List<ILLabel> newStateLabels = stateLabels.ToList();
                newStateLabels.Add(labelToResumeTo);
                stateSwitch.Operand = newStateLabels.ToArray();
            }

            cursor.Index = oldCursorIndex;

            FieldInfo currentField = sourceMoveNextMethod.DeclaringType!.FieldByName(EnumeratorUtils.CurrentFieldName)!;
            FieldInfo stateField = sourceMoveNextMethod.DeclaringType!.FieldByName(EnumeratorUtils.StateFieldName)!;
            // The this field will not exist for static coroutines.
            FieldInfo? thisField = sourceMoveNextMethod.DeclaringType!.FieldByName(EnumeratorUtils.ThisFieldName);

            cursor.EmitLdarg0();
            if (thisField != null)
            {
                cursor.EmitLdarg0();
                cursor.EmitLdfld(thisField);
            }
            else
            {
                cursor.EmitLdsfld(MainManagerInstanceField);
            }

            argumentsLoadAction(cursor);

            ILLabel labelYieldReturn = cursor.DefineLabel();

            cursor.EmitCall(targetEnumeratorMethod);
            cursor.EmitCall(MonoBehaviourStartCoroutineMethod);
            cursor.EmitDup();
            cursor.EmitBrtrue(labelYieldReturn);

            cursor.EmitPop();
            cursor.EmitPop();
            cursor.EmitBr(labelToResumeTo);

            cursor.EmitStfld(currentField);
            cursor.Index--;
            cursor.MarkLabel(labelYieldReturn);
            cursor.Index++;
            cursor.EmitLdarg0();
            cursor.EmitLdcI4(stateToSetBeforeReturn);
            cursor.EmitStfld(stateField);
            cursor.EmitLdcI4(1);
            cursor.EmitRet();
        }
    }
}