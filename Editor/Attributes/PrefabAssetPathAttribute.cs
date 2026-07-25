using System;
using UnityEngine;

namespace Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PrefabAssetPathAttribute : PropertyAttribute
    {
        public readonly string FieldNamePrefab;

        public PrefabAssetPathAttribute(string fieldNamePrefab)
        {
            FieldNamePrefab = fieldNamePrefab;
        }
    }
}