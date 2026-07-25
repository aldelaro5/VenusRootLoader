using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Attributes
{
    [CustomPropertyDrawer(typeof(BranchAttribute))]
    public sealed class BranchAttributeDrawer : PropertyDrawer
    {
        private BranchAttribute BranchAttribute => (BranchAttribute)attribute;

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            Type baseGameEnumType = BranchAttribute.BaseGameEnum;
            int baseGameMinValue = BranchAttribute.BaseGameMinValue;
            int baseGameMaxValue = BranchAttribute.BaseGameMaxValue;
            if (property.type != nameof(Branch))
            {
                EditorGUI.LabelField(position, label.text, $"Use Branch attribute with {nameof(Branch)} type.");
                return;
            }

            if (baseGameEnumType != null && !baseGameEnumType.IsEnum)
            {
                EditorGUI.LabelField(
                    position,
                    label.text,
                    $"{nameof(BranchAttribute.BaseGameEnum)} must be an enum type.");
                return;
            }

            SerializedProperty propertyHasValue = null;
            if (!string.IsNullOrWhiteSpace(BranchAttribute.FieldNameHasValue))
            {
                string propertyHasValuePath = BranchAttribute.FieldNameHasValue;
                int lastDot = property.propertyPath.LastIndexOf('.');
                if (lastDot > -1)
                    propertyHasValuePath =
                        $"{property.propertyPath.Substring(0, lastDot)}.{propertyHasValuePath}";

                if (!Utility.EnsurePropertyIsValidFromAbsolutePath(
                        property.serializedObject,
                        propertyHasValuePath,
                        SerializedPropertyType.Boolean,
                        position,
                        label,
                        out propertyHasValue))
                {
                    return;
                }
            }

            if (!Utility.EnsurePropertyIsValidFromRelativePath(
                    property,
                    nameof(Branch.CreatorKind),
                    SerializedPropertyType.Enum,
                    position,
                    label,
                    out SerializedProperty propertyCreatorKind))
            {
                return;
            }

            if (!Utility.EnsurePropertyIsValidFromRelativePath(
                    property,
                    nameof(Branch.CustomCreatorId),
                    SerializedPropertyType.String,
                    position,
                    label,
                    out SerializedProperty propertyCreatorId))
            {
                return;
            }

            if (!Utility.EnsurePropertyIsValidFromRelativePath(
                    property,
                    nameof(Branch.NamedId),
                    SerializedPropertyType.String,
                    position,
                    label,
                    out SerializedProperty propertyNamedId))
            {
                return;
            }

            Rect expanderPosition = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            if (propertyHasValue == null)
            {
                bool expanded = EditorGUI.Foldout(expanderPosition, property.isExpanded, label, true);
                if (EditorGUI.EndChangeCheck())
                    property.isExpanded = expanded;

                if (!expanded)
                    return;
            }
            else
            {
                bool hasValue = EditorGUI.Toggle(expanderPosition, label, propertyHasValue.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    propertyHasValue.boolValue = hasValue;
                    property.isExpanded = propertyHasValue.boolValue;
                }

                if (!propertyHasValue.boolValue)
                    return;
            }

            EditorGUI.indentLevel++;
            Rect enumFieldPosition = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width - position.width * 0.40f,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            BranchCreatorKind creatorKind = (BranchCreatorKind)EditorGUI.EnumPopup(
                enumFieldPosition,
                "Creator Id",
                (BranchCreatorKind)propertyCreatorKind.intValue);
            if (EditorGUI.EndChangeCheck())
                propertyCreatorKind.intValue = (int)creatorKind;

            if (creatorKind == BranchCreatorKind.Custom)
            {
                Rect creatorIdFieldPosition = new Rect(
                    position.x + enumFieldPosition.width,
                    position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                    position.width - enumFieldPosition.width,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.BeginChangeCheck();
                string customCreatorId = EditorGUI.TextField(creatorIdFieldPosition, propertyCreatorId.stringValue);
                if (EditorGUI.EndChangeCheck())
                    propertyCreatorId.stringValue = customCreatorId;
            }

            Rect namedIdFieldPosition = new Rect(
                position.x,
                position.y + (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * 2f,
                position.width,
                EditorGUIUtility.singleLineHeight);

            if (creatorKind == BranchCreatorKind.BaseGame)
            {
                if (baseGameEnumType != null)
                {
                    string[] baseGameEnumNames = Enum.GetNames(baseGameEnumType);
                    Enum oldBaseGameNamedId = (Enum)Enum.Parse(baseGameEnumType, baseGameEnumNames[0]);
                    if (baseGameEnumNames.Contains(propertyNamedId.stringValue))
                        oldBaseGameNamedId = (Enum)Enum.Parse(baseGameEnumType, propertyNamedId.stringValue);

                    EditorGUI.BeginChangeCheck();
                    Enum newBaseGameNamedId = EditorGUI.EnumPopup(namedIdFieldPosition, "Named Id", oldBaseGameNamedId);
                    if (EditorGUI.EndChangeCheck())
                        propertyNamedId.stringValue = newBaseGameNamedId.ToString();
                }
                else
                {
                    int oldBaseGameNamedId = 0;
                    if (int.TryParse(propertyNamedId.stringValue, out int parsedValue)
                        && parsedValue <= baseGameMaxValue
                        && parsedValue >= baseGameMinValue)
                    {
                        oldBaseGameNamedId = parsedValue;
                    }

                    EditorGUI.BeginChangeCheck();
                    int newBaseGameNamedId = EditorGUI.IntField(namedIdFieldPosition, "Named Id", oldBaseGameNamedId);
                    if (EditorGUI.EndChangeCheck())
                        propertyNamedId.stringValue = newBaseGameNamedId.ToString();
                }

                EditorGUI.indentLevel--;
                return;
            }

            EditorGUI.BeginChangeCheck();
            string namedId = EditorGUI.TextField(namedIdFieldPosition, "Named Id", propertyNamedId.stringValue);
            if (EditorGUI.EndChangeCheck())
                propertyNamedId.stringValue = namedId;
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            if (property.isExpanded)
                return EditorGUIUtility.singleLineHeight * 3f + EditorGUIUtility.standardVerticalSpacing * 2f;
            return EditorGUIUtility.singleLineHeight;
        }
    }
}