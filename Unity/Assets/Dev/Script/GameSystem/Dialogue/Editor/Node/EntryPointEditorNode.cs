using System;
using DS.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using DS.Core;

namespace DS.Editor
{
    public class EntryPointEditorNode : DialogueEditorNode
    {
        private readonly Vector2 NodeSize = new Vector2(100, 150);

        public override string GUID { get; set; }
        public override string NodeTitle { get; set; }
        public override string TypeName { get; set; }
        public override Vector2 Position { get; set; }

        public override DialogueNodeData ToSerializedData() => null;
        
        public EntryPointEditorNode(DialogueGraphView dialogueGraphView, string name)
        {
            title = name;
            NodeTitle = name;
            EntryPoint = true;
            GUID = Guid.NewGuid().ToString();
        }

        public override void Initialize(Vector2 position)
        {
            var outputPort = GeneratePort(Direction.Output);
            outputPort.portName = "Execute";
            outputContainer.Add(outputPort);

            capabilities &= ~Capabilities.Movable;
            capabilities &= ~Capabilities.Deletable;
            
            LoadStyleSheet();

            RefreshExpandedState(); 
            RefreshPorts();

            SetPosition(new Rect(new Vector2(100, 200),  NodeSize));
        }

        public override void FromSerializedData(DialogueNodeData data, DialogueContainer containerCache)
        {
            // do nothing
        }
    }
}