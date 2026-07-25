using UnityEditor;
using UnityEngine;

namespace Editor.Attributes
{
    public static class Utility
    {
        public static bool EnsurePropertyIsValidFromRelativePath(
            SerializedProperty baseProperty,
            string relativePropertyPath,
            SerializedPropertyType validType,
            Rect basePosition,
            GUIContent baseLabel,
            out SerializedProperty property)
        {
            property = baseProperty.FindPropertyRelative(relativePropertyPath);
            if (property == null)
            {
                EditorGUI.LabelField(
                    basePosition,
                    baseLabel.text,
                    $"The path {relativePropertyPath} must lead to a property that exists.");
                property = null;
                return false;
            }

            if (property.propertyType != validType)
            {
                EditorGUI.LabelField(
                    basePosition,
                    baseLabel.text,
                    $"The path {relativePropertyPath} must lead to a property of type {validType}.");
                property = null;
                return false;
            }

            return true;
        }

        public static bool EnsurePropertyIsValidFromAbsolutePath(
            SerializedObject baseObject,
            string propertyPath,
            SerializedPropertyType validType,
            Rect basePosition,
            GUIContent baseLabel,
            out SerializedProperty property)
        {
            property = baseObject.FindProperty(propertyPath);
            if (property == null)
            {
                EditorGUI.LabelField(
                    basePosition,
                    baseLabel.text,
                    $"The path {propertyPath} must lead to a property that exists.");
                property = null;
                return false;
            }

            if (property.propertyType != validType)
            {
                EditorGUI.LabelField(
                    basePosition,
                    baseLabel.text,
                    $"The path {propertyPath} must lead to a property of type {validType}.");
                property = null;
                return false;
            }

            return true;
        }
    }
}