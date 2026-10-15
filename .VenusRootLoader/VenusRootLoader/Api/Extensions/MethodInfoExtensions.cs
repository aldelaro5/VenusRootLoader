using System.Reflection;
using System.Collections;
using VenusRootLoader.Utility;
using CommunityToolkit.Diagnostics;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api.Extensions;

/// <summary>
/// Extensions for <see cref="MethodInfo"/>
/// </summary>
public static class MethodInfoExtensions
{
    /// <summary>
    /// Assuming this is a coroutine method, returns its underlying <see cref="IEnumerator.MoveNext"/> MethodInfo.
    /// </summary>
    /// <param name="enumeratorMethod">A method that returns an <see cref="IEnumerator"/> and contains at least 1 yield return statement.</param>
    /// <returns>The <see cref="IEnumerator.MoveNext"/> MethodInfo that matches this coroutine.</returns>
    /// <exception cref="ArgumentException">Thrown when either <paramref name="enumeratorMethod"/> does not return an <see cref="IEnumerator"/>
    /// , it does not yield return, or it does not have a <see cref="MemberInfo.DeclaringType"/></exception>
    public static MethodInfo GetMoveNextForCoroutineMethod(this MethodInfo enumeratorMethod)
    {
        if (enumeratorMethod.ReturnType != typeof(IEnumerator))
            ThrowHelper.ThrowArgumentException(nameof(enumeratorMethod), "The method must return an IEnumerator.");
        if (enumeratorMethod.DeclaringType == null)
            ThrowHelper.ThrowArgumentException(nameof(enumeratorMethod), "The method must have a declaring type.");

        return EnumeratorUtils.FindMoveNextFromEnumeratorMethod(enumeratorMethod);
    }
}