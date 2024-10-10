namespace DS.Runtime
{
    public class BranchItem : DialogueItemT<BranchRuntimeNode>
    {
        public readonly string[] BranchTexts;
        public readonly string Text;

        public BranchItem(BranchRuntimeNode node, string actorKey, string portraitKey, string text, string[] branchTexts) : base(node, actorKey, portraitKey)
        {
            this.BranchTexts = branchTexts;
            this.Text = text;
        }
    }
}