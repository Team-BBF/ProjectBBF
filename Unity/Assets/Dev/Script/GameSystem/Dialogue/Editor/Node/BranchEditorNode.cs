using System;
using System.Linq;
using DS.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using DS.Core;

namespace DS.Editor
{
    [DialogueNodeType("Add Branch", "Branch")]
    public class BranchEditorNode : DialogueEditorNode
    {
        private readonly Vector2 NodeSize = new Vector2(150, 200);
        private readonly DialogueGraphView _dialogueGraphView;

        #region Main Container

        private BranchNodeData _data = new();

        public override DialogueNodeData ToSerializedData()
        {
            Position = GetPosition().position;
            return _data;
        }

        private TextField _dialogueTextField;
        private TextField _portraitTextField;
        private TextField _actorKeyTextField;

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

        public BranchEditorNode(DialogueGraphView dialogueGraphView, string name)
        {
            title = name;
            NodeTitle = name;
            TypeName = name;   
            GUID = Guid.NewGuid().ToString();
            
            _dialogueGraphView = dialogueGraphView;
        }

        public override void Initialize(Vector2 position)
        {
            var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Connection";
            inputContainer.Add(inputPort);
            
            LoadStyleSheet();

            AddTitleTextField();
            AddNewChoiceButton();
            _dialogueTextField = AddTextField(str => _data.DialogueText = str.newValue,
                "output-textfield", "Dialogue Text");
            _portraitTextField = AddTextField(str => _data.PortraitKey = str.newValue,
                "output-textfield", "Portrait Key");
            _actorKeyTextField = AddTextField(str => _data.ActorKey = str.newValue,
                "output-textfield", "ActorKey Key");
            
            RefreshExpandedState();
            RefreshPorts();
            
            Position = position;
            _dialogueGraphView.AddElement(this);
            SetPosition(new Rect(position, NodeSize));
        }

        public override void FromSerializedData(DialogueNodeData data, DialogueContainer containerCache)
        {
            if (data is not BranchNodeData myData) return;

            _data = myData;

            _dialogueTextField.value = myData.DialogueText;
            _portraitTextField.value = myData.PortraitKey;
            _actorKeyTextField.value = myData.ActorKey;
            
            _dialogueGraphView.AddElement(this);
            var nodePorts = containerCache.NodeLinks.Where(x => x.BaseNodeGuid == data.GUID).ToList();
            nodePorts.ForEach(x => this.AddChoicePort(x.PortName));
        }
        
        private void AddNewChoiceButton()
        {
            var button = new Button(() => { AddChoicePort(); }) { text = "+" };
            button.AddToClassList("choice-button");
            titleContainer.Add(button);
        }
        
        public void AddChoicePort(string overriddenPortName = "")
        {
            var generatedPort = GeneratePort(Direction.Output);
            RemoveLabel(generatedPort.contentContainer, "type");

            var choicePortName = GenerateChoicePortName(overriddenPortName);
            ConfigureTextFieldForPort(generatedPort, choicePortName);
            AddDeleteButtonToPort(generatedPort);

            generatedPort.portName = choicePortName;
            outputContainer.Add(generatedPort);
            
            RefreshExpandedState();
            RefreshPorts();
        }
        
        private string GenerateChoicePortName(string overriddenPortName)
        {
            var outputPortCount = outputContainer.Query("connector").ToList().Count;
            return string.IsNullOrEmpty(overriddenPortName)
                ? $"Choice {outputPortCount + 1}"
                : overriddenPortName;
        }

        private static void ConfigureTextFieldForPort(Port generatedPort, string portName)
        {
            var textField = new TextField { name = string.Empty, value = portName };
            textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            textField.AddToClassList("output-textfield");
            generatedPort.contentContainer.Add(new Label("    "));
            generatedPort.contentContainer.Add(textField);
        }

        private void AddDeleteButtonToPort(Port generatedPort)
        {
            var deleteButton = new Button(() => RemovePort(generatedPort)) { text = "X" };
            deleteButton.AddToClassList("delete-choice-button");
            generatedPort.contentContainer.Add(deleteButton);
        }

        private void RemovePort(Port generatedPort)
        {
            var targetEdge = _dialogueGraphView.edges.ToList().FirstOrDefault(x =>
                x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

            if (targetEdge != null)
            {
                targetEdge.input.Disconnect(targetEdge);
                _dialogueGraphView.RemoveElement(targetEdge);
            }

            outputContainer.Remove(generatedPort);
            RefreshPorts();
            RefreshExpandedState();
        }
    }
}
