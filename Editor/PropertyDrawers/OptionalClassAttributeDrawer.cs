using UnityEditor;
using UnityEngine;

namespace Editor.Attributes
{
    [CustomPropertyDrawer(typeof(OptionalClassFieldAttribute))]
    public sealed class OptionalClassAttributeDrawer : PropertyDrawer
    {
        private OptionalClassFieldAttribute OptionalClassFieldAttribute =>
            (OptionalClassFieldAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string propertyHasValuePath = OptionalClassFieldAttribute.FieldNameHasValue;
            int lastDot = property.propertyPath.LastIndexOf('.');
            if (lastDot > -1)
                propertyHasValuePath = $"{property.propertyPath.Substring(0, lastDot)}.{propertyHasValuePath}";

            if (!Utility.EnsurePropertyIsValidFromAbsolutePath(
                    property.serializedObject,
                    propertyHasValuePath,
                    SerializedPropertyType.Boolean,
                    position,
                    label,
                    out SerializedProperty propertyHasValue))
            {
                return;
            }

            Rect checkBoxPosition = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            bool hasValue = EditorGUI.Toggle(checkBoxPosition, label, propertyHasValue.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                propertyHasValue.boolValue = hasValue;
                property.isExpanded = propertyHasValue.boolValue;
            }

            if (!propertyHasValue.boolValue)
                return;

            EditorGUI.indentLevel++;
            float totalHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            int targetDepth = property.depth + 1;
            foreach (SerializedProperty childProperty in property)
            {
                if (childProperty.depth != targetDepth)
                    continue;
                float propertyHeight = EditorGUI.GetPropertyHeight(childProperty);
                Rect floatFieldPosition = new Rect(
                    position.x,
                    position.y + totalHeight,
                    position.width,
                    propertyHeight);
                EditorGUI.PropertyField(floatFieldPosition, property, label);
                totalHeight += propertyHeight;
                totalHeight += EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float totalHeight = EditorGUIUtility.singleLineHeight;
            int targetDepth = property.depth + 1;
            foreach (SerializedProperty childProperty in property)
            {
                if (childProperty.depth != targetDepth)
                    continue;
                totalHeight += EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(childProperty);
            }

            return totalHeight;
        }
    }
}