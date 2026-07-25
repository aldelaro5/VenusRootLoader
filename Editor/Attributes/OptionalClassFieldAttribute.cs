using System;
using UnityEngine;

namespace Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OptionalClassFieldAttribute : PropertyAttribute
    {
        public readonly string FieldNameHasValue;

        public OptionalClassFieldAttribute(string fieldNameHasValue)
        {
            FieldNameHasValue = fieldNameHasValue;
        }
    }
}