using System;

namespace DS.Runtime
{
    public class ExecutionItem : DialogueItemT<ExecutionRuntimeNode>
    {
        public readonly Action<ProcessorData> Execution;
        
        public ExecutionItem(ExecutionRuntimeNode node, Action<ProcessorData> execution) : base(node, "Default", "None")
        {
            Execution = execution;
        }
    }
}