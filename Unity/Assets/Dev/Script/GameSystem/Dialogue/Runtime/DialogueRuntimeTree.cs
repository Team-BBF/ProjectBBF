using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DS.Core;
using DS.Runtime;
using JetBrains.Annotations;
using UnityEngine;

namespace DS.Core
{
    public class DialogueRuntimeTree
    {
        public static DialogueRuntimeTree Build(DialogueContainer container)
        {
            List<DialogueNodeData> datas = container.NodeData;
            List<NodeLinkData> links = container.NodeLinks;

            var firstNodeData = datas.First(x => x.GUID == links.First().TargetNodeGuid);
            
            var currentRuntimeNode = firstNodeData.CreateRuntimeNode(datas, links);

            var tree = new DialogueRuntimeTree();
            tree.EntryPoint = currentRuntimeNode;
            
            Debug.Assert(tree.EntryPoint is not null);

            return tree;
        }
        

        public DialogueRuntimeNode EntryPoint { get; private set; }
    }
}