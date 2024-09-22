using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DS.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using DS.Core;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DS.Editor
{
    public abstract class ParameterEditorNode : DialogueEditorNode
    {
        private readonly Vector2 NodeSize = new Vector2(150, 200);
        private readonly DialogueGraphView _dialogueGraphView;


        #region Main Container

        private ObjectField _objField;
        private List<VisualElement> _argumentElements = new(2);

        protected abstract ParameterNodeData ParameterData { get; }
        protected abstract Type HandlerType { get; }
        

        public override string GUID
        {
            get => ParameterData.GUID;
            set => ParameterData.GUID = value;
        }

        public override string NodeTitle
        {
            get => ParameterData.NodeTitle;
            set => ParameterData.NodeTitle = value;
        }

        public override string TypeName
        {
            get => ParameterData.TypeName;
            set => ParameterData.TypeName = value;
        }

        public override Vector2 Position
        {
            get => ParameterData.Position;
            set => ParameterData.Position = value;
        }
        
        #endregion


        public ParameterEditorNode(DialogueGraphView dialogueGraphView, string name)
        {
            title = name;
            NodeTitle = name;
            TypeName = name;
            GUID = Guid.NewGuid().ToString();

            _dialogueGraphView = dialogueGraphView;
        }
        
        public override void Initialize(Vector2 position)
        {
            Position = position;
            

            LoadStyleSheet();
            ObjectFieldInit();
            
            _dialogueGraphView.AddElement(this);
            SetPosition(new Rect(position, NodeSize));

            RefreshExpandedState();
            RefreshPorts();

        }

        private void ObjectFieldInit()
        {
            _objField = AddObjectField(obj =>
            {
                ParameterData.Handler = obj.newValue as ParameterHandler;
                UpdateObjectParam();
                ArgsUpdate();
            }, HandlerType, "output-objectField");
        }

        private void UpdateObjectParam()
        {
            foreach (VisualElement element in _argumentElements)
            {
                mainContainer.Remove(element);
            }

            RefreshExpandedState();
            _argumentElements.Clear();

            if (ParameterData.Handler is null)
            {
                return;
            }

            var types = ParameterData.Handler.GetArgumentTypes();


            foreach (var type in types)
            {
                if (type == typeof(int))
                {
                    var t = AddIntField(_ => { }, "", "int");
                    _argumentElements.Add(t);
                }
                else if (type == typeof(float))
                {
                    var t = AddFloatField(_ => { }, "", "float");
                    _argumentElements.Add(t);
                }
                else if (type == typeof(bool))
                {
                    var t = AddTextField(_ => { }, "", "bool");
                    _argumentElements.Add(t);
                }
                else if (type == typeof(string))
                {
                    var t = AddTextField(_ => { }, "", "text");
                    _argumentElements.Add(t);
                }
                else if (type.IsSubclassOf(typeof(Object)))
                {
                    var t = AddObjectField(_ => { }, type, "","Object");
                    _argumentElements.Add(t);
                }
            }
        }


        public override DialogueNodeData ToSerializedData()
        {
            Position = GetPosition().position;

            ParameterData.Warps = _argumentElements.Select(x =>
            {
                switch (x)
                {
                    case IntegerField intField:
                        return new ParameterNodeData.Warp()
                        {
                            Target = "Int",
                            IntValue = intField.value
                        };
                    case FloatField floatField:
                        return new ParameterNodeData.Warp()
                        {
                            Target = "Float",
                            FloatValue = floatField.value
                        };
                    case TextField stringField:
                        return new ParameterNodeData.Warp()
                        {
                            Target = "String",
                            StringValue = stringField.value
                        };
                    case ObjectField objectField:
                        return new ParameterNodeData.Warp()
                        {
                            Target = "Object",
                            ObjectValue = objectField.value
                        };
                    default:
                        return null;
                }
            }).ToArray();

            return ParameterData;
        }

        private void ArgsUpdate()
        {
            if (ParameterData is null) return;
            if (ParameterData.Warps is null) return;
            var enumerator = ParameterData.Warps.GetEnumerator();

            foreach (VisualElement argumentElement in _argumentElements)
            {
                if (enumerator.MoveNext() == false) break;
                if (enumerator.Current is not ParameterNodeData.Warp warp) return;

                switch (argumentElement)
                {
                    case IntegerField intField:
                        intField.value = warp.IntValue;
                        break;
                    case FloatField floatField:
                        floatField.value = warp.FloatValue;
                        break;
                    case TextField stringField:
                        stringField.value = warp.StringValue;
                        break;
                    case ObjectField objectField:
                        objectField.value = warp.ObjectValue;
                        break;
                }
            }
        }

        public override void FromSerializedData(DialogueNodeData data, DialogueContainer containerCache)
        {
            if (data is not ParameterNodeData myData) return;

            _objField.value = myData.Handler;

            UpdateObjectParam();
            ArgsUpdate();

            _dialogueGraphView.AddElement(this);
            RefreshExpandedState();
            RefreshPorts();
        }
    }
}