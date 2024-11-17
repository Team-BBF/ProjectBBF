using System;
using DS.Core;

namespace DS.Runtime
{
    public class ConditionItem : DialogueItemT<ConditionRuntimeNode>
    {
        public readonly Func<ProcessorData, DialogueRuntimeNode> GetNextNode;
        
        public ConditionItem(ConditionRuntimeNode node, Func<ProcessorData,DialogueRuntimeNode> getNextNode) : base(node, "Default", "None")
        {
            GetNextNode = getNextNode;
        }
    }
}