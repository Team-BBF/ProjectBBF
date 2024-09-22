using System;
using System.Collections;
using System.Collections.Generic;
using DS.Core;
using JetBrains.Annotations;
using UnityEngine;


namespace DS.Runtime
{
    public class EntryPointRuntimeNode : DialogueRuntimeNode
    {
        public DialogueRuntimeNode NextNode { get; private set; }
        public override bool IsLeaf => NextNode is null;

        public EntryPointRuntimeNode(DialogueRuntimeNode nextNode)
        {
            NextNode = nextNode;
        }

        public override DialogueItem CreateItem()
            => throw new NotImplementedException();

        public DialogueRuntimeNode GetNext()
            => NextNode;
    }
}