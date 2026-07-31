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

            float y = enumFieldPosition.y + enumFieldPosition.height + EditorGUIUtility.standardVerticalSpacing;
            Rect creatorKindPosition = new Rect(
                position.x,
                y,
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
                y += creatorKindPosition.height + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                dialogueBranch.Dialogue.CreatorKind = BranchCreatorKind.SameAsThisAsset;
            }

            if (dialogueBranch.Dialogue.CreatorKind == BranchCreatorKind.Custom)
            {
                Rect creatorIdFieldPosition = new Rect(
                    position.x + creatorKindPosition.width,
                    creatorKindPosition.y,
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
                y,
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
            {
                DialogueBranch dialogueBranch =
                    (DialogueBranch)fieldInfo.GetValue(property.serializedObject.targetObject);
                int numberLines = dialogueBranch.DialogueKind == MapDialogueKind.Common ? 4 : 3;
                return EditorGUIUtility.singleLineHeight * numberLines +
                       EditorGUIUtility.standardVerticalSpacing * (numberLines - 1);
            }

            return EditorGUIUtility.singleLineHeight;
        }
    }
}