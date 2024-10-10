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
    [DialogueNodeType("Add Condition", "Condition")]
    public class ConditionEditorNode : ParameterEditorNode
    {
        private readonly Vector2 NodeSize = new Vector2(150, 200);
        private readonly DialogueGraphView _dialogueGraphView;

        #region Main Container

        private ConditionNodeData _data= new();

        protected override ParameterNodeData ParameterData => _data;
        protected override Type HandlerType => typeof(ParameterHandler);

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

        public ConditionEditorNode(DialogueGraphView dialogueGraphView, string name)
            : base(dialogueGraphView, name)
        {
        }

        //TODO: withoutOutput 어떤 기능인지 파악 후 처리하기
        public override void Initialize(Vector2 position/**, bool withoutOutput = false*/)
        {
            base.Initialize(position);
            
            var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Connection";
            inputContainer.Add(inputPort);
            
            if (true)
            {
                var outputTruePort = GeneratePort(Direction.Output);
                outputTruePort.portName = "True";
                outputContainer.Add(outputTruePort);
                
                var outputFalsePort = GeneratePort(Direction.Output);
                outputFalsePort.portName = "False";
                outputContainer.Add(outputFalsePort);
            }
            
            AddTitleTextField();

            RefreshExpandedState();
            RefreshPorts();
        }

        public override DialogueNodeData ToSerializedData()
        {
            return base.ToSerializedData();
        }

        public override void FromSerializedData(DialogueNodeData data, DialogueContainer containerCache)
        {
            base.FromSerializedData(data, containerCache);
            if (data is not ConditionNodeData myData) return;
            
            _data = myData;
            RefreshExpandedState();
            RefreshPorts();
            
            //var nodePorts = containerCache.NodeLinks.Where(x => x.BaseNodeGuid == data.GUID).ToList();
            //nodePorts.ForEach(x => this.AddConditionPort(x.PortName));
        }
        
        public void AddConditionPort(string portName)
        {
            var generatedPort = GeneratePort(Direction.Output);

            generatedPort.portName = portName;
            outputContainer.Add(generatedPort);
            
            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
