using System;

namespace DS.Core
{
    [Serializable]
    public class NodeLinkData
    {
        public string BaseNodeGuid;
        public string PortName;
        public string TargetNodeGuid;
        
        public bool IsEqual(NodeLinkData other)
        {
            if (BaseNodeGuid != other.BaseNodeGuid)
                return false;
            
            if (PortName != other.PortName)
                return false;
            
            if (TargetNodeGuid != other.TargetNodeGuid)
                return false;
            
            return true;
        }
    }
}
