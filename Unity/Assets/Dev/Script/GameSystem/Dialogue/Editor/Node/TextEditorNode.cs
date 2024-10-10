using System;
using System.Globalization;
using System.Linq;
using DS.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using DS.Core;

namespace DS.Editor
{
    [DialogueNodeType("Add Text", "Text")]
    public class TextEditorNode : DialogueEditorNode
    {
        private readonly Vector2 NodeSize = new Vector2(150, 200);
        private readonly DialogueGraphView _dialogueGraphView;

        #region Main Container

        private TextNodeData _data = new();

        private TextField _dialogueTextField;
        private TextField _portraitTextField;
        private TextField _actorKeyTextField;

        public string DialogueText
        {
            get => _data.DialogueText;
            set
            {
                _data.DialogueText = value;

                if (_dialogueTextField != null)
                    _dialogueTextField.SetValueWithoutNotify(value);
            }
        }

        public override string GUID
        {
            get => _data.GUID;
            set => _data.GUID = value;
        }

        public override string NodeTitle
        {
            get => _data.NodeTitle;
            set => _data.NodeTitle = value;
        }

        public override string TypeName
        {
            get => _data.TypeName;
            set => _data.TypeName = value;
        }

        public override Vector2 Position
        {
            get => _data.Position;
            set => _data.Position = value;
        }

        #endregion

        public TextEditorNode(DialogueGraphView dialogueGraphView, string name)
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

            var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Connection";
            inputContainer.Add(inputPort);

            var outputPort = GeneratePort(Direction.Output);
            outputPort.portName = "Next";
            outputContainer.Add(outputPort);

            LoadStyleSheet();

            AddTitleTextField();
            _dialogueTextField = AddTextField(
                evt => _data.DialogueText = evt.newValue,
                "DialogueText-textfield",
                "Dialogue Text"
            );
            _portraitTextField = AddTextField(str => _data.PortraitKey = str.newValue,
                "output-textfield", "Portrait Key");
            _actorKeyTextField = AddTextField(str => _data.ActorKey = str.newValue,
                "output-textfield", "ActorKey Key");

            RefreshExpandedState();
            RefreshPorts();

            _dialogueGraphView.AddElement(this);
            SetPosition(new Rect(position, NodeSize));
        }


        public override DialogueNodeData ToSerializedData()
        {
            Position = GetPosition().position;
            return _data;
        }
        
        public override void FromSerializedData(DialogueNodeData data, DialogueContainer containerCache)
        {
            if (data is not TextNodeData myData) return;
            
            _data = myData;

            _dialogueTextField.value = _data.DialogueText;
            _portraitTextField.value = myData.PortraitKey;
            _actorKeyTextField.value = myData.ActorKey;
            
            _dialogueGraphView.AddElement(this);
        }
    }
}