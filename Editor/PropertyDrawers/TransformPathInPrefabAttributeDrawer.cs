using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using VenusRootLoader.Unity.Runtime.Attributes;

namespace Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(TransformPathInPrefabAttribute))]
    public sealed class TransformPathInPrefabAttributeDrawer : PropertyDrawer
    {
        private const string NoPrefabErrorMessage = "Assign the main prefab first.";
        private static readonly char[] DotSplitSeparator = { '.' };

        private TransformPathInPrefabAttribute TransformPathInPrefabAttribute =>
            (TransformPathInPrefabAttribute)attribute;

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use TransformPathInPrefab with String.");
                return;
            }

            Rect valueFieldPosition = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);
            string fieldNameTransform = TransformPathInPrefabAttribute.FieldNameTransform;
            // ReSharper disable once PossibleNullReferenceException
            FieldInfo transformField = fieldInfo.DeclaringType.GetField(fieldNameTransform);
            object containingInstance = ObtainInstanceContainingTransform(property);
            Transform oldTransform = null;
            if (containingInstance != null)
                oldTransform = (Transform)transformField.GetValue(containingInstance);

            string fieldNamePrefab = TransformPathInPrefabAttribute.FieldNamePrefab;
            // ReSharper disable once PossibleNullReferenceException
            FieldInfo prefabField = property.serializedObject.targetObject.GetType().GetField(fieldNamePrefab);
            GameObject prefab = (GameObject)prefabField.GetValue(property.serializedObject.targetObject);

            string fieldNamePrefabPath = TransformPathInPrefabAttribute.FieldNamePrefabPath;
            SerializedProperty prefabPathProperty = property.serializedObject.FindProperty(fieldNamePrefabPath);
            string prefabPath = prefabPathProperty.stringValue;

            // No prefab assigned
            if (string.IsNullOrEmpty(prefabPath))
            {
                EditorGUI.LabelField(position, label.text, NoPrefabErrorMessage);
                return;
            }

            // Attempt to recover the prefab using its path
            if (prefab == null)
            {
                prefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
                if (prefab == null)
                {
                    // The prefab no longer exists
                    prefabPathProperty.stringValue = null;
                    EditorGUI.LabelField(position, label.text, NoPrefabErrorMessage);
                    return;
                }

                prefabField.SetValue(property.serializedObject.targetObject, prefab);
            }

            // Attempt to recover the transform if it has a path
            if (containingInstance != null && oldTransform == null && !string.IsNullOrEmpty(property.stringValue))
            {
                oldTransform = prefab.transform.Find(property.stringValue);
                if (oldTransform != null)
                    transformField.SetValue(containingInstance, oldTransform);
            }

            // Attempt to recover the path if the transform changed
            if (containingInstance != null
                && !string.IsNullOrEmpty(property.stringValue)
                && prefab.transform.Find(property.stringValue) == null)
            {
                string transformPath = GetTransformPathFromRoot(
                    oldTransform,
                    new Stack<string>(),
                    out Transform root);
                if (transformPath == null
                    || prefab.transform.name != root.name
                    || prefab.transform.Find(transformPath) == null)
                {
                    // The transform no longer exists so we reset
                    property.stringValue = null;
                    transformField.SetValue(containingInstance, null);
                }
                else
                {
                    property.stringValue = transformPath;
                }
            }

            EditorGUI.BeginChangeCheck();
            Transform transform = (Transform)EditorGUI.ObjectField(
                valueFieldPosition,
                label,
                oldTransform,
                typeof(Transform),
                true);
            // We need to refuse assets which isn't possible to filter with the drag and drop,
            // but we can still deny it
            if (!EditorGUI.EndChangeCheck() || EditorUtility.IsPersistent(transform))
                return;

            string transformPathFromRoot = GetTransformPathFromRoot(
                transform,
                new Stack<string>(),
                out Transform rootTransform);
            // We can't accept transforms from another prefab than the one assigned
            if (transformPathFromRoot == null
                || prefab.transform.name != rootTransform.name
                || prefab.transform.Find(transformPathFromRoot) == null)
                return;

            property.stringValue = transformPathFromRoot;
            transformField.SetValue(containingInstance, transform);
        }

        private static object ObtainInstanceContainingTransform(SerializedProperty property)
        {
            if (!property.propertyPath.Contains("."))
                return property.serializedObject.targetObject;

            Type type = property.serializedObject.targetObject.GetType();
            object instance = property.serializedObject.targetObject;
            string[] pathParts = property.propertyPath.Split(DotSplitSeparator);
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                string pathPart = pathParts[i];
                if (pathPart == "Array" && pathParts[i + 1].Contains("["))
                {
                    i++;
                    pathPart = pathParts[i];
                    int index = int.Parse(pathPart.Substring(5, pathPart.Length - 6));
                    IList list = (IList)instance;
                    if (index >= list.Count)
                        return null;
                    instance = list[index];
                    type = instance.GetType();
                    continue;
                }

                FieldInfo field = type.GetField(pathPart);
                instance = field.GetValue(instance);
                type = field.FieldType;
            }

            return instance;
        }

        private static string GetTransformPathFromRoot(
            Transform transform,
            Stack<string> pathPartsStack,
            out Transform root)
        {
            if (transform == null)
            {
                root = null;
                return null;
            }

            if (transform.parent != null)
            {
                pathPartsStack.Push(transform.gameObject.name);
                return GetTransformPathFromRoot(transform.parent, pathPartsStack, out root);
            }

            StringBuilder sb = new StringBuilder();
            while (pathPartsStack.Count > 0)
            {
                if (sb.Length > 0)
                    sb.Append('/');
                sb.Append(pathPartsStack.Pop());
            }

            root = transform;
            return sb.ToString();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;
    }
}