using System;
using System.Collections.Generic;
using System.Linq;
using DS.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;
using DS.Core;

namespace DS.Editor
{
    public class GraphSaveUtility
    {
        private DialogueGraphView _targetGraphView;
        private DialogueContainer _containerCache;

        private List<Edge> Edges => _targetGraphView.edges.ToList();
        private List<DialogueEditorNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueEditorNode>().ToList();

        public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
        {
            return new GraphSaveUtility
            {
                _targetGraphView = targetGraphView
            };
        }

        public bool CheckAnyChange(string fullPath)
        {
            var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();

            SaveNodes(dialogueContainer);
            SaveLinks(dialogueContainer);

            string relativePath = ConvertFullToRelativePath(fullPath);
            _containerCache = AssetDatabase.LoadAssetAtPath<DialogueContainer>(relativePath);

            if (_containerCache == null)
                return true;

            return !dialogueContainer.IsEqual(_containerCache);
        }

        public void SaveGraph(string fullPath)
        {
            string relativePath = ConvertFullToRelativePath(fullPath);
            
            if (string.IsNullOrEmpty(relativePath))
                return;

            DialogueContainer dialogueContainer = AssetDatabase.LoadAssetAtPath<DialogueContainer>(relativePath);
            
            if (dialogueContainer == null)
            {
                dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
                AssetDatabase.CreateAsset(dialogueContainer, relativePath);
            }

            ResetContainer(dialogueContainer);
            SaveNodes(dialogueContainer);
            SaveLinks(dialogueContainer);

            EditorUtility.SetDirty(dialogueContainer);
            AssetDatabase.SaveAssets();
        }

        private void ResetContainer(DialogueContainer container)
        {
            container.NodeData = new List<DialogueNodeData>();
            container.NodeLinks = new List<NodeLinkData>();
        }

        private void SaveLinks(DialogueContainer container)
        {
            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();

            var entryNode = Nodes.FirstOrDefault(node => node.EntryPoint);
            container.NodeLinks.Add(new NodeLinkData
            {
                BaseNodeGuid = entryNode.GUID
            });
              
            foreach (var port in connectedPorts)
            {
                var outputNode = port.output.node as DialogueEditorNode;
                var inputNode = port.input.node as DialogueEditorNode;

                if (outputNode.GUID == entryNode.GUID)
                {
                    container.NodeLinks[0].PortName = port.output.portName;
                    container.NodeLinks[0].TargetNodeGuid = inputNode.GUID;
                    continue;
                }

                container.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGuid = outputNode.GUID,
                    PortName = port.output.portName,
                    TargetNodeGuid = inputNode.GUID
                });
                
                // 종종 DialogueGraphView.Edges의 내용의 순서가 일정하지 않는 경우가 있음.
                // 순서를 일정하게 유지하기 정렬
                container.NodeLinks.Sort((x, y) =>
                {
                    if (x.BaseNodeGuid == entryNode.GUID) return -1;
                    if (y.BaseNodeGuid == entryNode.GUID) return 1;
                    
                    return String.Compare(x.BaseNodeGuid, y.BaseNodeGuid, StringComparison.Ordinal);
                });
            }
        }

        private void SaveNodes(DialogueContainer container)
        {
            foreach (var dialogueNode in Nodes.Where(node => !node.EntryPoint))
            {
                container.NodeData.Add(dialogueNode.ToSerializedData());
            }
            
            var entryNode = Nodes.FirstOrDefault(node => node.EntryPoint);
            
            
            // 종종 DialogueGraphView.Edges의 내용의 순서가 일정하지 않는 경우가 있음.
            // NodeData의 경우 해당 케이스를 확인하지는 못했지만, 무결성을 유지하기 위해 정렬
            container.NodeData.Sort((x, y) =>
            {
                if (x.GUID == entryNode.GUID) return -1;
                if (y.GUID == entryNode.GUID) return 1;
                    
                return String.Compare(x.GUID, y.GUID, StringComparison.Ordinal);
            });
        }

        public void LoadGraph(string fullPath)
        {
            string relativePath = ConvertFullToRelativePath(fullPath);
            _containerCache = AssetDatabase.LoadAssetAtPath<DialogueContainer>(relativePath);

            if (_containerCache == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exist!\n\nPath: " + relativePath, "OK");
                return;
            }
            
            ClearGraph();
            CreateNodes();
            ConnectNodes();
        }
        
        private string ConvertFullToRelativePath(string fullPath)
        {
            string projectPath = Application.dataPath;
            if (fullPath.StartsWith(projectPath))
            {
                string relativePath = "Assets" + fullPath.Substring(projectPath.Length);
                return relativePath;
            }

            EditorUtility.DisplayDialog("Outside of the Project", "File path is outside of the project: " + fullPath, "OK");
            return "";
        }

        public void ClearGraph()
        {
            foreach (var node in Nodes)
            {
                if (node.EntryPoint)
                    continue;
                
                Edges.Where(x => x.input.node == node).ToList()
                    .ForEach(edge => _targetGraphView.RemoveElement(edge));
                
                _targetGraphView.RemoveElement(node);
            }
        }

        private void CreateNodes()
        {
            if (_containerCache.NodeLinks.Count > 0)
            {
                Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGuid;
            }
            
            foreach (var nodeData in _containerCache.NodeData)
            {
                var node = DialogueNodeFactory.CreateFromData(nodeData, _targetGraphView, _containerCache);

                if (node == null)
                {
                    Debug.LogError($"노드 생성 실패. type: {nodeData.TypeName} guid: {nodeData.GUID}");
                }
            }
        }
        private void ConnectNodes()
        {
            foreach (var node in Nodes)
            {
                var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == node.GUID).ToList();
                for (int j = 0; j < connections.Count; j++)
                {
                    var targetNodeGuid = connections[j].TargetNodeGuid;
                    
                    var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
                    LinkNodes(node.outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);
                    
                    targetNode.SetPosition(new Rect(
                        _containerCache.NodeData.First(x => x.GUID == targetNodeGuid).Position,
                        _targetGraphView.DefaultNodeSize
                    ));
                }
            }
        }

        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            
            _targetGraphView.Add(tempEdge);
        }
    }
}
