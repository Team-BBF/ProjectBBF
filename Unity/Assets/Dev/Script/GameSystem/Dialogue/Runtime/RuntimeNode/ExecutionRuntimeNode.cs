using System;
using System.Collections.Generic;
using DS.Core;

namespace DS.Runtime
{
    public class ExecutionRuntimeNode : DialogueRuntimeNode
    {
        public override bool IsLeaf => NextNode is null;
        public readonly DialogueRuntimeNode NextNode;
        public readonly ParameterHandler Handler;
        public readonly object[] Arguments;

        public ExecutionRuntimeNode(DialogueRuntimeNode nextNode, ParameterHandler handler, object[] arguments)
        {
            NextNode = nextNode;
            Handler = handler;
            Arguments = arguments;
        }

        public override DialogueItem CreateItem()
            => new ExecutionItem(this, () => _ = Handler.Execute(Arguments));
    }
}