using UnityEditor;
using UnityEngine;
using VenusRootLoader.Unity.Runtime.Attributes;

namespace Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(OptionalFieldAttribute))]
    public sealed class OptionalFieldAttributeDrawer : PropertyDrawer
    {
        private OptionalFieldAttribute OptionalFieldAttribute => (OptionalFieldAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Float
                && property.propertyType != SerializedPropertyType.Vector3)
            {
                EditorGUI.LabelField(position, label.text, "Use NullableFloatField with Float or Vector3.");
                return;
            }

            string propertyIsNulPath = OptionalFieldAttribute.FieldNameHasValue;
            int lastDot = property.propertyPath.LastIndexOf('.');
            if (lastDot > -1)
                propertyIsNulPath = $"{property.propertyPath.Substring(0, lastDot)}.{propertyIsNulPath}";

            if (!Utility.EnsurePropertyIsValidFromAbsolutePath(
                    property.serializedObject,
                    propertyIsNulPath,
                    SerializedPropertyType.Boolean,
                    position,
                    label,
                    out SerializedProperty propertyIsNull))
            {
                return;
            }

            Rect checkBoxPosition = new Rect(
                position.x,
                position.y,
                8f + EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            bool hasValue = EditorGUI.Toggle(checkBoxPosition, label, propertyIsNull.boolValue);
            if (EditorGUI.EndChangeCheck())
                propertyIsNull.boolValue = hasValue;
            if (!propertyIsNull.boolValue)
                return;

            Rect valueFieldPosition = new Rect(
                position.x + checkBoxPosition.width,
                position.y,
                position.width - checkBoxPosition.width,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginChangeCheck();
            if (property.propertyType == SerializedPropertyType.Vector3)
            {
                valueFieldPosition.x += 16f;
                valueFieldPosition.width -= 16f;
                Vector3 vector3Field = EditorGUI.Vector3Field(
                    valueFieldPosition,
                    GUIContent.none,
                    property.vector3Value);
                if (!EditorGUI.EndChangeCheck())
                    return;

                property.vector3Value = vector3Field;
            }
            else
            {
                float floatField = EditorGUI.FloatField(valueFieldPosition, GUIContent.none, property.floatValue);
                if (!EditorGUI.EndChangeCheck())
                    return;

                property.floatValue = floatField;
            }
        }
    }
}