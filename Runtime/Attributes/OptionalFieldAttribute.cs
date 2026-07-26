using System;
using UnityEngine;

namespace VenusRootLoader.Unity.Runtime.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OptionalFieldAttribute : PropertyAttribute
    {
        public readonly string FieldNameHasValue;

        public OptionalFieldAttribute(string fieldNameHasValue)
        {
            FieldNameHasValue = fieldNameHasValue;
        }
    }
}