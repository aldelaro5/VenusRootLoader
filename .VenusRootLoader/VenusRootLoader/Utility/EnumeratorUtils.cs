using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Reflection;

namespace VenusRootLoader.Utility;

internal static class EnumeratorUtils
{
    internal const string StateFieldName = "<>1__state";
    internal const string CurrentFieldName = "<>2__current";
    internal const string ThisFieldName = "<>4__this";

    internal sealed class DummyEnumerator : IEnumerator
    {
        public bool MoveNext() => false;
        public void Reset() => ThrowHelper.ThrowNotSupportedException();
        public object? Current => null;
    }

    internal static readonly DummyEnumerator EmptyEnumerator = new();

    internal static MethodInfo FindMoveNextFromEnumeratorMethod(MethodInfo enumeratorMethod)
    {
        Type? stateMachine = enumeratorMethod.DeclaringType!
            .GetNestedTypes(BindingFlags.NonPublic)
            .Where(x => typeof(IEnumerator).IsAssignableFrom(x))
            .SingleOrDefault(x => x.Name.StartsWith($"<{enumeratorMethod.Name}>d__")
                                  && StateMachineMatchesEnumeratorMethod(x, enumeratorMethod));

        if (stateMachine == null)
        {
            ThrowHelper.ThrowArgumentException(
                nameof(enumeratorMethod),
                "The state machine type backing this coroutine could not be found.");
        }

        MethodInfo? moveNextMethod = stateMachine.GetMethod(
            nameof(IEnumerator.MoveNext),
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (moveNextMethod == null)
        {
            ThrowHelper.ThrowArgumentException(
                nameof(enumeratorMethod),
                "The state machine type backing this coroutine does not have a MoveNext method.");
        }

        return moveNextMethod;
    }

    private static bool StateMachineMatchesEnumeratorMethod(Type stateMachine, MethodInfo enumeratorMethod)
    {
        IEnumerable<Type> parameterTypes = enumeratorMethod.GetParameters()
            .Select(y => y.ParameterType)
            .OrderBy(x => x.Name);
        IEnumerable<Type> stateMachineSpeakableFields = stateMachine
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(y => y.Name != ThisFieldName)
            .Select(y => y.FieldType)
            .OrderBy(x => x.Name);
        return parameterTypes.SequenceEqual(stateMachineSpeakableFields);
    }

    public static FieldInfo GetStateMachineFieldFromUniqueType(Type stateMachineType, Type fieldType)
    {
        return stateMachineType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(x => x.FieldType == fieldType);
    }

    public static FieldInfo GetStateMachineThisField(Type stateMachineType)
    {
        return stateMachineType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(x => x.Name == ThisFieldName);
    }
}