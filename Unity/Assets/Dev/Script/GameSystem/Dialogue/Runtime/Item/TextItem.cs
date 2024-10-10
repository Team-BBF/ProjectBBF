namespace DS.Runtime
{
    public class TextItem : DialogueItemT<TextRuntimeNode>
    {
        public string Text { get; private set; }
        
        public TextItem(TextRuntimeNode node, string text, string characterDisplayName, string portraitKey) : base(node, characterDisplayName, portraitKey)
        {
            Text = text;
        }
    }
}