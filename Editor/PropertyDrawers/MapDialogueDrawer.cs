using UnityEditor;
using UnityEngine;
using VenusRootLoader.Unity.Runtime.Enums;
using VenusRootLoader.Unity.Runtime.ScriptableObjects;

namespace Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(DialogueBranch))]
    public class MapDialogueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect expanderPosition = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            bool expanded = EditorGUI.Foldout(expanderPosition, property.isExpanded, label, true);
            if (EditorGUI.EndChangeCheck())
                property.isExpanded = expanded;

            if (!expanded)
                return;

            EditorGUI.indentLevel++;
            Rect enumFieldPosition = new Rect(
                position.x,
                expanderPosition.y + expanderPosition.height,
                position.width - position.width * 0.40f,
                EditorGUIUtility.singleLineHeight);
            DialogueBranch dialogueBranch = (DialogueBranch)fieldInfo.GetValue(property.serializedObject.targetObject);
            EditorGUI.BeginChangeCheck();
            MapDialogueKind dialogueKind =
                (MapDialogueKind)EditorGUI.EnumPopup(enumFieldPosition, "Kind", dialogueBranch.DialogueKind);
            if (EditorGUI.EndChangeCheck())
                dialogueBranch.DialogueKind = dialogueKind;

            Rect creatorKindPosition = new Rect(
                position.x,
                expanderPosition.y + expanderPosition.height +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width - position.width * 0.40f,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            if (dialogueKind == MapDialogueKind.Common)
            {
                BranchCreatorKind creatorKind = (BranchCreatorKind)EditorGUI.EnumPopup(
                    creatorKindPosition,
                    "Creator Id",
                    dialogueBranch.Dialogue.CreatorKind);
                if (EditorGUI.EndChangeCheck())
                    dialogueBranch.Dialogue.CreatorKind = creatorKind;
            }
            else
            {
                int oldCreatorKind = (int)dialogueBranch.Dialogue.CreatorKind;
                if (oldCreatorKind == 0)
                    oldCreatorKind = 1;
                int enumValue = EditorGUI.IntPopup(
                    creatorKindPosition,
                    "Creator Id",
                    oldCreatorKind,
                    new[]
                    {
                        "Same As This Asset",
                        nameof(BranchCreatorKind.Custom)
                    },
                    new[]
                    {
                        1,
                        2
                    });
                if (EditorGUI.EndChangeCheck())
                    dialogueBranch.Dialogue.CreatorKind = (BranchCreatorKind)enumValue;
            }

            if (dialogueBranch.Dialogue.CreatorKind == BranchCreatorKind.Custom)
            {
                Rect creatorIdFieldPosition = new Rect(
                    position.x + creatorKindPosition.width,
                    expanderPosition.y + expanderPosition.height + EditorGUIUtility.singleLineHeight +
                    EditorGUIUtility.standardVerticalSpacing,
                    position.width - creatorKindPosition.width,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.BeginChangeCheck();
                string customCreatorId = EditorGUI.TextField(
                    creatorIdFieldPosition,
                    dialogueBranch.Dialogue.CustomCreatorId);
                if (EditorGUI.EndChangeCheck())
                    dialogueBranch.Dialogue.CustomCreatorId = customCreatorId;
            }

            Rect namedIdFieldPosition = new Rect(
                position.x,
                expanderPosition.y + expanderPosition.height +
                (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * 2f,
                position.width,
                EditorGUIUtility.singleLineHeight);

            if (dialogueBranch.Dialogue.CreatorKind == BranchCreatorKind.BaseGame)
            {
                int oldBaseGameNamedId = -1;
                int minimum = -203;
                if (int.TryParse(dialogueBranch.Dialogue.NamedId, out int parsedValue)
                    && parsedValue <= -1
                    && parsedValue >= minimum)
                {
                    oldBaseGameNamedId = parsedValue;
                }

                EditorGUI.BeginChangeCheck();
                int newBaseGameNamedId = EditorGUI.IntField(namedIdFieldPosition, "Named Id", oldBaseGameNamedId);
                if (EditorGUI.EndChangeCheck())
                    dialogueBranch.Dialogue.NamedId = newBaseGameNamedId.ToString();

                EditorGUI.indentLevel--;
                return;
            }

            EditorGUI.BeginChangeCheck();
            string namedId = EditorGUI.TextField(namedIdFieldPosition, "Named Id", dialogueBranch.Dialogue.NamedId);
            if (EditorGUI.EndChangeCheck())
                dialogueBranch.Dialogue.NamedId = namedId;
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
                return EditorGUIUtility.singleLineHeight * 4f + EditorGUIUtility.standardVerticalSpacing * 3f;
            return EditorGUIUtility.singleLineHeight;
        }
    }
}