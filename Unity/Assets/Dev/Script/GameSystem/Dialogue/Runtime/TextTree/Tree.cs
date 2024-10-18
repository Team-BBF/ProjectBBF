using System.Collections.Generic;
using System.Text;

namespace DS.Runtime
{
    public class TextTree
    {
        public readonly List<(string token, Tokenizer.TokenType type)> Tokens;
        public readonly int TextLength;

        private string _completedString;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_completedString))
            {
                StringBuilder builder = new();

                foreach ((string token, Tokenizer.TokenType type) token in Tokens)
                {
                    builder.Append(token.token);
                }
                
                _completedString = builder.ToString();
            }
            
            return _completedString;
        }

        public TextTree(int textLength, List<(string token, Tokenizer.TokenType type)> tokens)
        {
            TextLength = textLength;
            Tokens = tokens;
        }
    }
}