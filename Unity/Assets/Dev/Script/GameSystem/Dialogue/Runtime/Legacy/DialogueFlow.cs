using System.Collections.Generic;
using System.Linq;
using DS.Runtime;

namespace DS.Core
{
    public class DialogueFlow
    {
        private DialogueContainer _container;
        private string _currentGuid;
        
        public DialogueFlow(DialogueContainer container)
        {
            _container = container;
            _currentGuid = container.NodeLinks[0].TargetNodeGuid;
        }
        
        public List<NodeLinkData> GetCurrentNodeLinks()
        { 
            var dialogueNodeLinks = _container.NodeLinks.Where(c => c.BaseNodeGuid == _currentGuid).ToList();
            return dialogueNodeLinks;
        }

        public DialogueNodeData GetCurrentNodeData()
        {
            var dialogueNodeData = _container.NodeData.FirstOrDefault(c => c.GUID == _currentGuid);
            return dialogueNodeData;
        }
        
        public void ChangeCurrentNodeData(string guid)
        {
            _currentGuid = guid;
        }
    }
}

