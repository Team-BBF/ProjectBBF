using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DS.Runtime
{

    public static class TextUtil
    {
        public static UniTask DoTextUniTask(this TMP_Text text, string endValue, float duration, bool stepDuration,
            CancellationToken? token = null)
        {
            return _DoText(str => text.text = str, endValue, duration, stepDuration, token);
        }

        public static UniTask DoTextUniTask(Action<string> stringInput, string endValue, float duration,
            bool stepDuration,
            CancellationToken? token = null)
        {
            return _DoText(stringInput, endValue, duration, stepDuration, token);
        }

        private static async UniTask _DoText(Action<string> stringInput, string endValue, float duration,
            bool stepDuration,
            CancellationToken? token = null)
        {
            try
            {
                CancellationToken t = token == null
                        ? GlobalCancelation.PlayMode
                        : CancellationTokenSource.CreateLinkedTokenSource(GlobalCancelation.PlayMode, token.Value).Token
                    ;

                var table = new Dictionary<string, string>();
                table["player"] = "플레이어";
                var require = new Processor.ProcessorRequire(table);
                
                List<(string token, Tokenizer.TokenType type)> tokens = new(5);
                Tokenizer.GetToken(ref endValue, ref tokens);
                Processor.Run(ref tokens, require);

                StringBuilder builder = new();
                foreach (var tt in tokens)
                {
                    Debug.Log(tt);

                    int length = tt.token.Length;
                    string str = tt.token;

                    if (tt.type is Tokenizer.TokenType.Text or Tokenizer.TokenType.BindingText)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            builder.Append(str[i]);
                            stringInput?.Invoke(builder.ToString());

                            await UniTask.Delay(100, cancellationToken: t);
                        }
                    }
                    else
                    {
                        builder.Append(str);
                        stringInput?.Invoke(builder.ToString());
                    }
                }

            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                Debug.LogException(e);
            }
        }

    }
}