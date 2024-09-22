using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using DS.Core;
using DS.Runtime;
using UnityEditor.UIElements;

namespace DS.Editor
{
    public abstract class DialogueEditorNode : Node
    {
        public abstract string GUID { get; set; }
        public abstract string NodeTitle{ get; set; }
        public abstract string TypeName{ get; set; }
        public abstract Vector2 Position { get; set; }
        
        public bool EntryPoint = false;
        
        protected void LoadStyleSheet()
        {
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(DialogueEditorResourcesPath.DIALOGUE_NODE_STYLESHEET_PATH);
            
            styleSheets.Add(styleSheet);
        }

        protected static void RemoveLabel(VisualElement content, string labelName)
        {
            var oldLabel = content.Q<Label>(labelName);
            if (oldLabel != null)
            {
                content.Remove(oldLabel);
            }
        }

        protected Port GeneratePort(Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }

        public virtual TextField AddTitleTextField()
        {
            RemoveLabel(titleContainer, "title-label");
            
            var textField = new TextField(string.Empty);
            textField.RegisterValueChangedCallback(evt =>
            {
                title = evt.newValue;
                NodeTitle = evt.newValue;
            });
            textField.SetValueWithoutNotify(title);
            titleContainer.Insert(0, textField);

            return textField;
        }
        public virtual TextField AddTextField(EventCallback<ChangeEvent<string>> changeCallback, string styleClass, string fieldName, string defaultValue="")
        {
            RemoveLabel(titleContainer, "title-label");
            
            var textField = new TextField(fieldName);
            textField.RegisterValueChangedCallback(changeCallback);
            textField.SetValueWithoutNotify(defaultValue);
            textField.AddToClassList(styleClass);
            mainContainer.Add(textField);
            
            return textField;
        }
        public virtual ObjectField AddObjectField(EventCallback<ChangeEvent<UnityEngine.Object>> changeCallback, Type objectType, string styleClass, string fieldName = "")
        {
            RemoveLabel(titleContainer, "title-label");
            
            var propertyField = new ObjectField(fieldName);
            propertyField.RegisterCallback(changeCallback);
            propertyField.AddToClassList(styleClass);
            propertyField.objectType = objectType;
            mainContainer.Add(propertyField);
            
            return propertyField;
        }
        
        public FloatField AddFloatField(EventCallback<ChangeEvent<float>> changeCallback, string styleClass, string fieldName, float defaultValue=default)
        {
            var field = new FloatField(fieldName);
            field.RegisterValueChangedCallback(changeCallback);
            field.SetValueWithoutNotify(defaultValue);
            field.AddToClassList(styleClass);
            mainContainer.Add(field);

            return field;
        }


        public IntegerField AddIntField(EventCallback<ChangeEvent<int>> changeCallback, string styleClass, string fieldName, int defaultValue=default)
        {
            var field = new IntegerField(fieldName);
            field = new IntegerField(fieldName);
            field.RegisterValueChangedCallback(changeCallback);
            field.SetValueWithoutNotify(defaultValue);
            field.AddToClassList(styleClass);
            mainContainer.Add(field);

            return field;
        }
        
        public abstract void Initialize(Vector2 position);
        public abstract void FromSerializedData(DialogueNodeData data, DialogueContainer containerCache);
        public abstract DialogueNodeData ToSerializedData();
    }
}