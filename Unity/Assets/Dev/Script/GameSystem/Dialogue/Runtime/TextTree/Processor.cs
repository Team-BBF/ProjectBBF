using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Runtime
{
    public class ProcessorData
    {
        public readonly Dictionary<string, string> BindingTable;

        public static readonly ProcessorData Default = new ProcessorData
        (
            new Dictionary<string, string>()
        );

        public ProcessorData(Dictionary<string, string> bindingTable)
        {
            BindingTable = bindingTable;
        }
    }

    public static class Processor
    {
        private static Func<string, ProcessorData, string>[] _table;

        static Processor()
        {
            _table = new Func<string, ProcessorData, string>[(int)Tokenizer.TokenType.Length];

            _table[(int)Tokenizer.TokenType.Text] = Text;
            _table[(int)Tokenizer.TokenType.RichText] = RichText;
            _table[(int)Tokenizer.TokenType.BindingText] = BindingText;
            _table[(int)Tokenizer.TokenType.InverseSlash] = InverseSlash;
            _table[(int)Tokenizer.TokenType.Space] = Space;
        }

        private static string Text(string token, ProcessorData data)
        {
            return token;
        }

        private static string RichText(string token, ProcessorData data)
        {
            return token;
        }

        private static string BindingText(string token, ProcessorData data)
        {
            Debug.Assert(token[0] == '{' && token[^1] == '}');

            if (data.BindingTable.TryGetValue(token.Substring(1, token.Length - 2), out string bindings) is false)
            {
                bindings = token;
            }

            return bindings;
        }

        private static string InverseSlash(string token, ProcessorData data)
        {
            Debug.Assert(string.IsNullOrEmpty(token) is false);
            Debug.Assert(token[0] == '\\');

            if (token.Length == 1)
            {
                return "";
            }

            return token[1].ToString();
        }

        private static string Space(string token, ProcessorData data)
        {
            return token;
        }

        public static void Run(ref List<(string token, Tokenizer.TokenType type)> tokens, ProcessorData data)
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
                    tokens[i] = (processor.Invoke(token.token, data), token.type);
                }
            }
        }
    }
}