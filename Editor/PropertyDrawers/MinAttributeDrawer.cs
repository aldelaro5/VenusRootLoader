using UnityEditor;
using UnityEngine;

namespace Editor.Attributes
{
    [CustomPropertyDrawer(typeof(MinAttribute))]
    public sealed class MinAttributeDrawer : PropertyDrawer
    {
        private static readonly string InvalidTypeMessage = L10n.Tr("Use Min with float, int or Vector.");

        private MinAttribute MinAttribute => (MinAttribute)attribute;

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);
            if (!EditorGUI.EndChangeCheck())
                return;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    property.floatValue = Mathf.Max(MinAttribute.Min, property.floatValue);
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = Mathf.Max((int)MinAttribute.Min, property.intValue);
                    break;
                case SerializedPropertyType.Vector2:
                    {
                        property.vector2Value = new Vector2(
                            Mathf.Max(MinAttribute.Min, property.vector2Value.x),
                            Mathf.Max(MinAttribute.Min, property.vector2Value.y));
                        break;
                    }
                case SerializedPropertyType.Vector2Int:
                    {
                        property.vector2IntValue = new Vector2Int(
                            Mathf.Max((int)MinAttribute.Min, property.vector2IntValue.x),
                            Mathf.Max((int)MinAttribute.Min, property.vector2IntValue.y));
                        break;
                    }
                case SerializedPropertyType.Vector3:
                    {
                        property.vector3Value = new Vector3(
                            Mathf.Max(MinAttribute.Min, property.vector3Value.x),
                            Mathf.Max(MinAttribute.Min, property.vector3Value.y),
                            Mathf.Max(MinAttribute.Min, property.vector3Value.z));
                        break;
                    }
                case SerializedPropertyType.Vector3Int:
                    {
                        property.vector3IntValue = new Vector3Int(
                            Mathf.Max((int)MinAttribute.Min, property.vector3IntValue.x),
                            Mathf.Max((int)MinAttribute.Min, property.vector3IntValue.y),
                            Mathf.Max((int)MinAttribute.Min, property.vector3IntValue.z));
                        break;
                    }
                case SerializedPropertyType.Vector4:
                    {
                        property.vector4Value = new Vector4(
                            Mathf.Max(MinAttribute.Min, property.vector4Value.x),
                            Mathf.Max(MinAttribute.Min, property.vector4Value.y),
                            Mathf.Max(MinAttribute.Min, property.vector4Value.z),
                            Mathf.Max(MinAttribute.Min, property.vector4Value.w));
                        break;
                    }
                default:
                    EditorGUI.LabelField(position, label.text, InvalidTypeMessage);
                    break;
            }
        }
    }
}