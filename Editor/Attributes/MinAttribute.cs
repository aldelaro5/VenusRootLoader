using System;
using UnityEngine;

namespace Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MinAttribute : PropertyAttribute
    {
        /// <summary>
        ///   <para>The minimum allowed value.</para>
        /// </summary>
        public readonly float Min;

        /// <summary>
        ///   <para>Attribute used to make a float or int variable in a script be restricted to a specific minimum value.</para>
        /// </summary>
        /// <param name="min">The minimum allowed value.</param>
        public MinAttribute(float min) => Min = min;
    }
}