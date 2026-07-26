using System;
using UnityEngine;

namespace VenusRootLoader.Unity.Runtime.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OptionalClassFieldAttribute : PropertyAttribute
    {
        public readonly string FieldNameHasValue;

        public OptionalClassFieldAttribute(string fieldNameHasValue)
        {
            FieldNameHasValue = fieldNameHasValue;
        }
    }
}