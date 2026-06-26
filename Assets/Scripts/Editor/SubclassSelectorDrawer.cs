using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SelectSubclassAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        if (property.propertyType == SerializedPropertyType.ManagedReference) {
            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label);

            // Create a dropdown menu of all classes that inherit from CardAction
            if (GUI.Button(new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                property.managedReferenceFullTypename.Split(' ').LastOrDefault() ?? "Null (Click to Select)")) {

                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => fieldInfo.FieldType.GetGenericArguments()[0].IsAssignableFrom(p) && !p.IsAbstract && p.IsClass);

                GenericMenu menu = new GenericMenu();
                foreach (var type in types) {
                    menu.AddItem(new GUIContent(type.Name), false, () => {
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }
            EditorGUI.PropertyField(position, property, label, true);
        }
        else {
            EditorGUI.LabelField(position, "Use [SelectSubclass] only on [SerializeReference] fields!");
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, true);
    }
}