using System;

namespace DS.Runtime
{
    public class ExecutionItem : DialogueItemT<ExecutionRuntimeNode>
    {
        public readonly Action Execution;
        
        public ExecutionItem(ExecutionRuntimeNode node, Action execution) : base(node, "Default", "None")
        {
            Execution = execution;
        }
    }
}