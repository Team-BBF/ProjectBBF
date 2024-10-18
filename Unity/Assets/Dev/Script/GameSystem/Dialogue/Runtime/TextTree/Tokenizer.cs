using System.Collections;
using System.Collections.Generic;

namespace DS.Runtime
{
    public static class Tokenizer
    {
        public enum TokenType : int
        {
            Text = 0,
            RichText,
            BindingText,
            InverseSlash,
            Space,
            
            Length = Space + 1,
        }

        public static void GetToken(ref string str, ref List<(string token, TokenType type)> tokens)
        {
            tokens.Clear();
            
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ' ')
                {
                    (int endIndex, int length) tuple = JumpSpaceEnd(ref str, i);

                    tokens.Add((str.Substring(i, tuple.length), TokenType.Space));
                    i = tuple.endIndex;
                    continue;
                }

                if (str[i] == '<')
                {
                    (int endIndex, int length) tuple = JumpTagEnd(ref str, i);

                    tokens.Add((str.Substring(i, tuple.length), TokenType.RichText));
                    i = tuple.endIndex;
                    continue;
                }

                if (str[i] == '{')
                {
                    (int endIndex, int length) tuple = JumpBindEnd(ref str, i);

                    tokens.Add((str.Substring(i, tuple.length), TokenType.BindingText));
                    i = tuple.endIndex;
                    continue;
                }

                if (str[i] == '\\')
                {
                    (int endIndex, int length) tuple = JumpInverseSlashEnd(ref str, i);

                    tokens.Add((str.Substring(i, tuple.length), TokenType.InverseSlash));
                    i = tuple.endIndex;
                    continue;
                }

                {
                    (int endIndex, int length) tuple = JumpWordEnd(ref str, i);

                    tokens.Add((str.Substring(i, tuple.length), TokenType.Text));
                    i = tuple.endIndex;
                    continue;
                }
            }
        }

        private static (int endIndex, int length) JumpTagEnd(ref string str, int startIndex)
        {
            for (int i = startIndex + 1; i < str.Length; i++)
            {
                if (str[i] == '>')
                {
                    return (i, GetLength(startIndex, i));
                }
            }

            return (str.Length - 1, GetLength(startIndex, str.Length - 1));
        }

        private static (int endIndex, int length) JumpSpaceEnd(ref string str, int startIndex)
        {
            for (int i = startIndex + 1; i < str.Length; i++)
            {
                if (str[i] != ' ')
                {
                    return (i - 1, GetLength(startIndex, i - 1));
                }
            }

            return (str.Length - 1, GetLength(startIndex, str.Length - 1));
        }

        private static (int endIndex, int length) JumpWordEnd(ref string str, int startIndex)
        {
            for (int i = startIndex + 1; i < str.Length; i++)
            {
                if (IsNotChar(str[i]))
                {
                    return (i - 1, GetLength(startIndex, i - 1));
                }
            }

            return (str.Length - 1, GetLength(startIndex, str.Length - 1));
        }

        private static (int endIndex, int length) JumpBindEnd(ref string str, int startIndex)
        {
            for (int i = startIndex + 1; i < str.Length; i++)
            {
                if (str[i] == '}')
                {
                    return (i, GetLength(startIndex, i));
                }
            }

            return (str.Length - 1, GetLength(startIndex, str.Length - 1));
        }

        private static (int endIndex, int length) JumpInverseSlashEnd(ref string str, int startIndex)
        {
            if (str.Length <= startIndex + 1)
            {
                return (startIndex + 1, 1);
            }

            return (startIndex + 1, 2);
        }

        private static bool IsNotChar(char c)
        {
            return c is '<' or '>' or '\\' or ' ' or '{' or '}';
        }

        private static int GetLength(int startIndex, int endIndex)
        {
            return endIndex - startIndex + 1;
        }
    }
}