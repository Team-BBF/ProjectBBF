using System;
using System.Collections.Generic;
using System.Linq;
using DS.Core;
using UnityEngine;

namespace DS.Runtime
{
    [Serializable]
    public class BranchNodeData : DialogueNodeData
    {
        public string DialogueText;
        public string PortraitKey;
        public string ActorKey;
        
        public override DialogueNodeData Clone()
        {
            var data = new BranchNodeData();

            data.GUID = GUID;
            data.NodeTitle = NodeTitle;
            data.TypeName = TypeName;
            data.Position = Position;

            data.DialogueText = DialogueText;
            data.PortraitKey = PortraitKey;
            data.ActorKey = ActorKey;
            
            return data;
        }

        public override DialogueRuntimeNode CreateRuntimeNode(List<DialogueNodeData> datas, List<NodeLinkData> links)
        {
            var nextNodes = links
                .Where(x => x.BaseNodeGuid == GUID)
                .Select(x =>
                {
                    DialogueNodeData data = datas.FirstOrDefault(y => y.GUID == x.TargetNodeGuid);
                    Debug.Assert(data is not null);

                    DialogueRuntimeNode nextNode = data.CreateRuntimeNode(datas, links);
                    return new BranchRuntimeNode.BranchItem(nextNode, x.PortName);
                })
                .ToList();

            return new BranchRuntimeNode(DialogueText, ActorKey, PortraitKey, nextNodes);
        }

        public override bool IsEqual(DialogueNodeData other)
        {
            if (!base.IsEqual(other))
                return false;

            if (other is not BranchNodeData otherNode) return false;

            if (DialogueText != otherNode.DialogueText) return false;
            if (PortraitKey != otherNode.PortraitKey) return false;
            if (ActorKey != otherNode.ActorKey) return false;
            
            return true;
        }
    }
}