using DS.Core;

namespace DS.Runtime
{
    public class TextRuntimeNode : DialogueRuntimeNode
    {
        public readonly string Text;
        public readonly string ActorKey;
        public readonly string PortraitKey;
        public DialogueRuntimeNode NextNode { get; private set; }
        public override bool IsLeaf => NextNode is null;

        public TextRuntimeNode(string text, string actorKey, string portraitKey, DialogueRuntimeNode nextNode)
        {
            Text = text;
            NextNode = nextNode;
            ActorKey = actorKey;
            PortraitKey = portraitKey;
        }

        public override DialogueItem CreateItem()
        {
            return new TextItem(this, Text, ActorKey, PortraitKey);
        }

        public DialogueRuntimeNode GetNext()
            => NextNode;
    }
}