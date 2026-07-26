using System;
using UnityEngine;

namespace VenusRootLoader.Unity.Runtime.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TransformPathInPrefabAttribute : PropertyAttribute
    {
        public readonly string FieldNameTransform;
        public readonly string FieldNamePrefab;
        public readonly string FieldNamePrefabPath;

        public TransformPathInPrefabAttribute(
            string fieldNameTransform,
            string fieldNamePrefab,
            string fieldNamePrefabPath)
        {
            FieldNameTransform = fieldNameTransform;
            FieldNamePrefab = fieldNamePrefab;
            FieldNamePrefabPath = fieldNamePrefabPath;
        }
    }
}