using System;
using UnityEngine;

namespace VenusRootLoader.Unity.Runtime.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BranchAttribute : PropertyAttribute
    {
        public readonly Type BaseGameEnum;
        public readonly int BaseGameMinValue;
        public readonly int BaseGameMaxValue;
        public readonly string FieldNameHasValue;

        public BranchAttribute(Type baseGameEnum, string fieldNameHasValue = null)
        {
            BaseGameEnum = baseGameEnum;
            FieldNameHasValue = fieldNameHasValue;
        }

        public BranchAttribute(
            int baseGameMinValue = int.MinValue,
            int baseGameMaxValue = int.MaxValue,
            string fieldNameHasValue = null)
        {
            BaseGameMinValue = baseGameMinValue;
            BaseGameMaxValue = baseGameMaxValue;
            FieldNameHasValue = fieldNameHasValue;
        }
    }
}