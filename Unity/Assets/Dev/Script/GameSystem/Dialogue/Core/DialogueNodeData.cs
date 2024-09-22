using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Core
{
    
    [Serializable]
    public abstract class DialogueNodeData
    {
        public string GUID;
        public string NodeTitle;
        public string TypeName;
        public Vector2 Position;

        public abstract DialogueNodeData Clone();

        public abstract DialogueRuntimeNode CreateRuntimeNode(List<DialogueNodeData> datas, List<NodeLinkData> links);
        
        public virtual bool IsEqual(DialogueNodeData other)
        {
            if (GUID != other.GUID)
                return false;

            if (NodeTitle != other.NodeTitle)
                return false;

            if (TypeName != other.TypeName)
                return false;

            if (!(Mathf.Approximately(Position.x, other.Position.x)
                  && Mathf.Approximately(Position.y, other.Position.y)))
                return false;

            return true;
        }
    }
}