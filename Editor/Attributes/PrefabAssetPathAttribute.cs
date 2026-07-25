using System;
using UnityEngine;

namespace Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PrefabAssetPathAttribute : PropertyAttribute
    {
        public readonly string FieldNamePrefab;

        public PrefabAssetPathAttribute(string fieldNamePrefab)
        {
            FieldNamePrefab = fieldNamePrefab;
        }
    }
}