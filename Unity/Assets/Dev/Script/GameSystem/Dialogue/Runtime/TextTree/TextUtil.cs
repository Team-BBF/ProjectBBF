using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace DS.Runtime
{
    public static class TextUtil
    {
        public static TextTree CreateTextTree(string text, ProcessorData processorData)
        {
            List<(string token, Tokenizer.TokenType type)> tokens = new(5);
            Tokenizer.GetToken(ref text, ref tokens);
            Processor.Run(ref tokens, processorData);

            return new TextTree(text.Length, tokens);
        }
        public static UniTask DoTextUniTask(this TMP_Text text, TextTree textTree, float duration,
            bool normalizedTime,
            CancellationToken? token = null)
        {
            return _DoText(str => text.text = str, textTree, duration, normalizedTime, token);
        }

        public static UniTask DoTextUniTask(Action<string> stringInput, TextTree textTree, float duration,
            bool normalizedTime, ProcessorData processorData,
            CancellationToken? token = null)
        {
            return _DoText(stringInput, textTree, duration, normalizedTime, token);
        }

        private static async UniTask _DoText(Action<string> stringInput, TextTree textTree, float duration,
            bool normalizedTime,
            CancellationToken? token = null)
        {
            try
            {
                CancellationToken t = token == null
                        ? GlobalCancelation.PlayMode
                        : CancellationTokenSource.CreateLinkedTokenSource(GlobalCancelation.PlayMode, token.Value).Token
                    ;

                List<(string token, Tokenizer.TokenType type)> tokens = textTree.Tokens;
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

                            if (normalizedTime)
                            {
                                await UniTask.Delay((int)(duration / textTree.TextLength * 1000f), DelayType.DeltaTime,
                                    PlayerLoopTiming.Update,
                                    t);
                            }
                            else
                            {
                                await UniTask.Delay((int)(duration * 1000f), DelayType.DeltaTime,
                                    PlayerLoopTiming.Update,
                                    t);
                            }
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