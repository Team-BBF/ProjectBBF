using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DS.Core;
using UnityEngine;

namespace DS.Runtime
{
    [Serializable]
    public class ExecutionNodeData : ParameterNodeData
    {
        public override DialogueNodeData Clone()
        {
            var data = new ExecutionNodeData();

            data.GUID = GUID;
            data.NodeTitle = NodeTitle;
            data.TypeName = TypeName;
            data.Position = Position;

            data.Handler = Handler;
            data.Warps = Warps;

            return data;
        }

        public override DialogueRuntimeNode CreateRuntimeNode(List<DialogueNodeData> datas, List<NodeLinkData> links)
        {
            NodeLinkData result = links.FirstOrDefault(x => x.BaseNodeGuid == GUID);
            if (result is null) return new ExecutionRuntimeNode(null, Handler, GetArgs());

            DialogueNodeData nextData = datas.FirstOrDefault(x => x.GUID == result.TargetNodeGuid);
            if (nextData is null) return new ExecutionRuntimeNode(null, Handler, GetArgs());

            DialogueRuntimeNode nextNode = nextData.CreateRuntimeNode(datas, links);
            
            return new ExecutionRuntimeNode(nextNode, Handler, GetArgs());
        }
            
        public override bool IsEqual(DialogueNodeData other)
        {
            if (!base.IsEqual(other))
                return false;
            
            return true;
        }
    }
}