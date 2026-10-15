using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using VenusRootLoader.Utility;
using static System.Reflection.Emit.OpCodes;

namespace VenusRootLoader.Api.Extensions;

/// <summary>
/// Extensions for <see cref="Transpilers"/>
/// </summary>
public static class TranspilersExtensions
{
    private static readonly FieldInfo MainManagerInstanceField = AccessTools.Field(
        typeof(MainManager),
        nameof(MainManager.instance));

    private static readonly MethodInfo MonoBehaviourStartCoroutineMethod = AccessTools.Method(
        typeof(MonoBehaviour),
        nameof(MonoBehaviour.StartCoroutine),
        [typeof(IEnumerator)]);

    extension(Transpilers)
    {
        /// <summary>
        /// Returns a list of instructions that will yield return a coroutine from a <see cref="IEnumerator.MoveNext"/>
        /// method and resume at a label once done. After the target coroutine's first <see cref="IEnumerator.MoveNext"/>
        /// call, if it has already finished, the instructions returned will instead branch directly to the label and skip
        /// the yield return which allows to resume without waiting for the next frame.
        /// </summary>
        /// <param name="sourceMoveNextMethod">The <see cref="IEnumerator.MoveNext"/> method that will perform the yield return.
        ///     From a transpiler, this can be obtained by declaring a parameter in the transpiler method of type <see cref="MethodBase"/>.</param>
        /// <param name="targetCoroutineDelegate">The coroutine to yield return to. This delegate must return an <see cref="IEnumerator"/>.</param>
        /// <param name="matcher">A code matcher to use to determine the states and their associated labels. This method will
        ///     operate on a clone of it so its state won't be changed by the method. It MUST have been created with an
        ///     <see cref="ILGenerator"/> for this method to function.</param>
        /// <param name="argumentsLoadInstructions">Additional instructions to insert that will place the arguments of
        ///     <paramref name="targetCoroutineDelegate"/> on the stack. The arguments to place includes the <c>this</c> for instance coroutine.</param>
        /// <param name="labelToResumeTo">The label to resume to after the yield return is done and <paramref name="targetCoroutineDelegate"/>
        ///     is done executing. If the label refers to a state that already exists, it will be reused, otherwise, a new state
        ///     will be created, and it will be used.</param>
        /// <typeparam name="T">The <paramref name="targetCoroutineDelegate"/> delegate type. This will automatically be determined based
        /// on the value of <paramref name="targetCoroutineDelegate"/></typeparam>
        /// <returns>A series of instructions that will have <paramref name="sourceMoveNextMethod"/> yield return to <paramref name="targetCoroutineDelegate"/>
        /// using the arguments obtained by <paramref name="argumentsLoadInstructions"/> and then resume the method at
        /// <paramref name="labelToResumeTo"/></returns>
        public static IEnumerable<CodeInstruction> EmitYieldReturnCoroutine<T>(
            MethodBase sourceMoveNextMethod,
            T targetCoroutineDelegate,
            CodeMatcher matcher,
            IEnumerable<CodeInstruction> argumentsLoadInstructions,
            Label labelToResumeTo)
            where T : Delegate
        {
            return Transpilers.EmitYieldReturnCoroutine(
                sourceMoveNextMethod,
                targetCoroutineDelegate.Method,
                matcher,
                argumentsLoadInstructions,
                labelToResumeTo);
        }

        /// <summary>
        /// Returns a list of instructions that will yield return a coroutine from a <see cref="IEnumerator.MoveNext"/>
        /// method and resume at a label once done. After the target coroutine's first <see cref="IEnumerator.MoveNext"/>
        /// call, if it has already finished, the instructions returned will instead branch directly to the label and skip
        /// the yield return which allows to resume without waiting for the next frame.
        /// </summary>
        /// <param name="sourceMoveNextMethod">The <see cref="IEnumerator.MoveNext"/> method that will perform the yield return.
        /// From a transpiler, this can be obtained by declaring a parameter in the transpiler method of type <see cref="MethodBase"/>.</param>
        /// <param name="matcher">A code matcher to use to determine the states and their associated labels. This method will
        /// operate on a clone of it so its state won't be changed by the method. It MUST have been created with an
        /// <see cref="ILGenerator"/> for this method to function.</param>
        /// <param name="targetEnumeratorMethod">The coroutine's enumerator method to yield return to. This method must return an <see cref="IEnumerator"/>.</param>
        /// <param name="argumentsLoadInstructions">Additional instructions to insert that will place the arguments of
        /// <paramref name="targetEnumeratorMethod"/> on the stack. The arguments to place includes the <c>this</c> for instance coroutine.</param>
        /// <param name="labelToResumeTo">The label to resume to after the yield return is done and <paramref name="targetEnumeratorMethod"/>
        /// is done executing. If the label refers to a state that already exists, it will be reused, otherwise, a new state
        /// will be created, and it will be used.</param>
        /// <returns>A series of instructions that will have <paramref name="sourceMoveNextMethod"/> yield return to <paramref name="targetEnumeratorMethod"/>
        /// using the arguments obtained by <paramref name="argumentsLoadInstructions"/> and then resume the method at
        /// <paramref name="labelToResumeTo"/></returns>
        public static IEnumerable<CodeInstruction> EmitYieldReturnCoroutine(
            MethodBase sourceMoveNextMethod,
            MethodInfo targetEnumeratorMethod,
            CodeMatcher matcher,
            IEnumerable<CodeInstruction> argumentsLoadInstructions,
            Label labelToResumeTo)
        {
            CodeMatcher cloneMatcher = matcher.Clone();

            cloneMatcher.Start();
            // The first switch is always the state switch.
            cloneMatcher.MatchStartForward(Code.Switch);
            CodeInstruction stateSwitch = cloneMatcher.Instruction;
            List<Label> stateLabels = ((Label[])stateSwitch.operand).ToList();
            // Labels targeting the same instruction won't be equal so we need to manually find an instruction that both
            // has the label to resume to AND that at least one of its label is in the state switch.
            cloneMatcher.MatchStartForward(
                new CodeMatch(x => x.labels.Contains(labelToResumeTo)
                                   && x.labels.Any(stateLabels.Contains)));

            int stateToSetBeforeReturn = stateLabels.Count;
            if (cloneMatcher.IsValid)
            {
                stateToSetBeforeReturn = stateLabels.FindIndex(x => cloneMatcher.Instruction.labels.Contains(x));
            }
            else
            {
                stateLabels.Add(labelToResumeTo);
                stateSwitch.operand = stateLabels.ToArray();
            }

            FieldInfo currentField = AccessTools.Field(
                sourceMoveNextMethod.DeclaringType,
                EnumeratorUtils.CurrentFieldName);
            FieldInfo stateField = AccessTools.Field(
                sourceMoveNextMethod.DeclaringType,
                EnumeratorUtils.StateFieldName);
            // The this field will not exist for static coroutines.
            FieldInfo? thisField = AccessTools.Field(
                sourceMoveNextMethod.DeclaringType,
                EnumeratorUtils.ThisFieldName);

            List<CodeInstruction> il = [new(Ldarg_0)];
            if (thisField != null)
            {
                il.Add(new(Ldarg_0));
                il.Add(new(Ldfld, thisField));
            }
            else
            {
                il.Add(new(Ldsfld, MainManagerInstanceField));
            }

            il.AddRange(argumentsLoadInstructions);

            cloneMatcher.DefineLabel(out Label labelYieldReturn);

            il.Add(new(Call, targetEnumeratorMethod));
            il.Add(new(Call, MonoBehaviourStartCoroutineMethod));
            il.Add(new(Dup));
            il.Add(new(Brtrue, labelYieldReturn));

            il.Add(new(Pop));
            il.Add(new(Pop));
            il.Add(new(Br, labelToResumeTo));

            il.Add(new(Stfld, currentField) { labels = [labelYieldReturn] });
            il.Add(new(Ldarg_0));
            il.Add(new(Ldc_I4, stateToSetBeforeReturn));
            il.Add(new(Stfld, stateField));
            il.Add(new(Ldc_I4_1));
            il.Add(new(Ret));

            return il;
        }
    }
}