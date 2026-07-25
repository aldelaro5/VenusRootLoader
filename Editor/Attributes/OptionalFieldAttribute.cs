using System;
using UnityEngine;

namespace Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OptionalFieldAttribute : PropertyAttribute
    {
        public readonly string FieldNameHasValue;

        public OptionalFieldAttribute(string fieldNameHasValue)
        {
            FieldNameHasValue = fieldNameHasValue;
        }
    }
}