using System;
using System.Collections.Generic;
using DS.Core;
using UnityEngine;

namespace DS.Runtime
{
    public class ConditionRuntimeNode : DialogueRuntimeNode
    {
        public readonly DialogueRuntimeNode TrueNode;
        public readonly DialogueRuntimeNode FalseNode;
        public readonly ParameterHandler Handler;
        public readonly object[] Args;

        public override bool IsLeaf => TrueNode is null && FalseNode is null;

        public ConditionRuntimeNode(DialogueRuntimeNode trueNode, DialogueRuntimeNode falseNode, ParameterHandler handler, object[] args)
        {
            TrueNode = trueNode;
            FalseNode = falseNode;
            Handler = handler;
            Args = args;
        }

        public override DialogueItem CreateItem()
            => new ConditionItem(this, () =>
            {
                var rtv = Handler.Execute(Args);
                Debug.Assert(rtv is bool);

                return (bool)rtv ? TrueNode : FalseNode;
            });
    }
}