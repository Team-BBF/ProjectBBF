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

namespace DS.Editor
{
    [DialogueNodeType("Add Execution", "Execution")]
    public class ExecutionEditorNode : ParameterEditorNode
    {
        private readonly Vector2 NodeSize = new Vector2(150, 200);
        private readonly DialogueGraphView _dialogueGraphView;


        #region Main Container
        private ExecutionNodeData _data = new();

        protected override ParameterNodeData ParameterData => _data;
        protected override Type HandlerType => typeof(ParameterHandler);

        #endregion

        public ExecutionEditorNode(DialogueGraphView dialogueGraphView, string name) : base(dialogueGraphView, name)
        {
        }

        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);
            

            AddTitleTextField();

            var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Connection";
            inputContainer.Add(inputPort);

            var outputPort = GeneratePort(Direction.Output);
            outputPort.portName = "Next";
            outputContainer.Add(outputPort);
        }


        public override DialogueNodeData ToSerializedData()
        {
            return base.ToSerializedData();;
        }
        
        public override void FromSerializedData(DialogueNodeData data, DialogueContainer containerCache)
        {
            base.FromSerializedData(data, containerCache);
            
            if (data is not ExecutionNodeData myData) return;
            _data = myData;
        }
    }
}