using System.Collections.Generic;
using System.Linq;
using DS.Core;

namespace DS.Runtime
{
    public class BranchRuntimeNode : DialogueRuntimeNode
    {
        public class BranchItem
        {
            private DialogueRuntimeNode _nextNode;
            private string _text;

            public DialogueRuntimeNode NextNode => _nextNode;
            public string Text => _text;

            public BranchItem(DialogueRuntimeNode nextNode, string text)
            {
                _nextNode = nextNode;
                _text = text;
            }
        }

        public override bool IsLeaf => NextNodes.Count == 0;
        public IReadOnlyList<BranchItem> NextNodes { get; private set; }
        public readonly string Text;
        public readonly string PortraitKey;
        public readonly string Actorkey;

        
        public BranchRuntimeNode(string text, string actorKey, string portraitKey, IReadOnlyList<BranchItem> nextNodes)
        {
            NextNodes = nextNodes;
            Text = text;
            PortraitKey = portraitKey;
            Actorkey = actorKey;
        }
        
        public override DialogueItem CreateItem()
            => new Runtime.BranchItem(this, Actorkey, PortraitKey, Text, NextNodes.Select(x=>x.Text).ToArray());

        public DialogueRuntimeNode GetNext(int index)
            => NextNodes[index].NextNode;
    }
}