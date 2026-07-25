using System.Reflection;
using Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(PrefabAssetPathAttribute))]
    public class PrefabAssetPathAttributeDrawer : PropertyDrawer
    {
        private PrefabAssetPathAttribute PrefabAssetPathAttribute =>
            (PrefabAssetPathAttribute)attribute;

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use PrefabAssetPath with String.");
                return;
            }

            Rect valueFieldPosition = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);
            string fieldNamePrefab = PrefabAssetPathAttribute.FieldNamePrefab;
            // ReSharper disable once PossibleNullReferenceException
            FieldInfo prefabField = fieldInfo.DeclaringType.GetField(fieldNamePrefab);
            GameObject prefab = (GameObject)prefabField.GetValue(property.serializedObject.targetObject);
            if (prefab == null && !string.IsNullOrEmpty(property.stringValue))
            {
                prefab = (GameObject)AssetDatabase.LoadAssetAtPath(property.stringValue, typeof(GameObject));
                if (prefab != null)
                {
                    prefabField.SetValue(property.serializedObject.targetObject, prefab);
                }
                else
                {
                    property.stringValue = null;
                }
            }

            string currentAssetPath = AssetDatabase.GetAssetPath(prefab);
            if (currentAssetPath != property.stringValue)
                property.stringValue = currentAssetPath;

            EditorGUI.BeginChangeCheck();
            GameObject gameObject = (GameObject)EditorGUI.ObjectField(
                valueFieldPosition,
                label,
                prefab,
                typeof(GameObject),
                false);
            if (!EditorGUI.EndChangeCheck())
                return;

            string newAssetPath = AssetDatabase.GetAssetPath(gameObject);
            property.stringValue = newAssetPath;
            prefabField.SetValue(property.serializedObject.targetObject, gameObject);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;
    }
}