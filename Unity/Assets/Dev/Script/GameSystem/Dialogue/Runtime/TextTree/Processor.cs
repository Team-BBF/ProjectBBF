


using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Runtime
{

    public static class Processor
    {
        public class ProcessorRequire
        {
            public readonly Dictionary<string, string> BindingTable;

            public ProcessorRequire(Dictionary<string, string> bindingTable)
            {
                BindingTable = bindingTable;
            }
        }
        
        private static Func<string, ProcessorRequire, string>[] _table;
        
        static Processor()
        {
            _table = new Func<string, ProcessorRequire, string>[(int)Tokenizer.TokenType.Length];

            _table[(int)Tokenizer.TokenType.Text] = Text;
            _table[(int)Tokenizer.TokenType.RichText] = RichText;
            _table[(int)Tokenizer.TokenType.BindingText] = BindingText;
            _table[(int)Tokenizer.TokenType.InverseSlash] = InverseSlash;
            _table[(int)Tokenizer.TokenType.Space] = Space;
        }

        private static string Text(string token, ProcessorRequire require)
        {
            return token;
        }
        private static string RichText(string token, ProcessorRequire require)
        {
            return token;
        }
        private static string BindingText(string token, ProcessorRequire require)
        {
            Debug.Assert(token[0] == '{' && token[^1] == '}');
            
            if (require.BindingTable.TryGetValue(token.Substring(1, token.Length - 2), out string bindings) is false)
            {
                bindings = token;
            }
            
            return bindings;
        }
        private static string InverseSlash(string token, ProcessorRequire require)
        {
            Debug.Assert(string.IsNullOrEmpty(token) is false);
            Debug.Assert(token[0] == '\\');

            if (token.Length == 1)
            {
                return "";
            }

            return token[1].ToString();
        }
        private static string Space(string token, ProcessorRequire require)
        {
            return token;
        }
        
        public static void Run(ref List<(string token, Tokenizer.TokenType type)> tokens, ProcessorRequire require)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                
                var processor = _table[(int)token.type];
                if (processor is null)
                {
                    Debug.LogError($"token({token})의 Type에 해당하는 Processor가 없습니다.");
                }
                else
                {
                    tokens[i] = (processor.Invoke(token.token, require), token.type);
                }
                
            }
        }
    }
}