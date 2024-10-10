using System;
using System.Collections.Generic;
using System.Linq;
using DS.Core;
using UnityEngine;

namespace DS.Runtime
{
    [Serializable]
    public class TextNodeData : DialogueNodeData
    {
        public string DialogueText;
        public string PortraitKey;
        public string ActorKey;

        public override DialogueNodeData Clone()
        {
            var data = new TextNodeData();

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
            NodeLinkData result = links.FirstOrDefault(x => x.BaseNodeGuid == GUID);
            if (result is null) return new TextRuntimeNode(DialogueText, ActorKey, PortraitKey, null);

            DialogueNodeData nextData = datas.FirstOrDefault(x => x.GUID == result.TargetNodeGuid);
            if (nextData is null) return new TextRuntimeNode(DialogueText, ActorKey, PortraitKey, null);

            DialogueRuntimeNode nextNode = nextData.CreateRuntimeNode(datas, links);

            return new TextRuntimeNode(DialogueText, ActorKey, PortraitKey, nextNode);
        }

        public override bool IsEqual(DialogueNodeData other)
        {
            if (!base.IsEqual(other))
                return false;

            var otherNode = other as TextNodeData;
            
            if (DialogueText != otherNode.DialogueText)
                return false;
            if (PortraitKey != otherNode.PortraitKey)
                return false;
            if (ActorKey != otherNode.ActorKey)
                return false;
            
            return true;
        }
    }

}