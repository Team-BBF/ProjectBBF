using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public static class TextUtil
{
    public static UniTask DoTextUniTask(this TMP_Text text, string endValue, float duration, bool stepDuration,
        CancellationToken? token = null)
    {
        return _DoText(str => text.text = str, endValue, duration, stepDuration, token);
    }
    public static UniTask DoTextUniTask(Action<string> stringInput, string endValue, float duration,bool stepDuration,
        CancellationToken? token = null)
    {
        return _DoText(stringInput, endValue, duration, stepDuration, token);
    }

    private static async UniTask _DoText(Action<string> stringInput, string endValue, float duration, bool stepDuration,
        CancellationToken? token = null)
    {
        string tempString = "";
        CancellationToken t = token == null
                ? GlobalCancelation.PlayMode
                : CancellationTokenSource.CreateLinkedTokenSource(GlobalCancelation.PlayMode, token.Value).Token
            ;


        for (int i = 0; i < endValue.Length; i++)
        {
            if (TryGetTag(ref endValue, i, out string beginTag, out string endTag, out string midText, out int nextIndex))
            {
                i = nextIndex;
                if (i >= endValue.Length) return;

                string temp = "";
                foreach (var tt in midText)
                {
                    temp += tt;
                    stringInput(tempString + beginTag + temp + endTag);

                    if (stepDuration)
                    {
                        await UniTask.Delay((int)(duration * 1000f), DelayType.DeltaTime, PlayerLoopTiming.Update,
                            t);
                    }
                    else
                    {
                        await UniTask.Delay((int)(duration / endValue.Length * 1000f), DelayType.DeltaTime, PlayerLoopTiming.Update,
                            t);
                    }
                }

                tempString += beginTag + temp + endTag;
            }
            else
            {
                tempString += endValue[i];
                stringInput(tempString);
                

                if (stepDuration)
                {
                    await UniTask.Delay((int)(duration * 1000f), DelayType.DeltaTime, PlayerLoopTiming.Update,
                        t);
                }
                else
                {
                    await UniTask.Delay((int)(duration / endValue.Length * 1000f), DelayType.DeltaTime, PlayerLoopTiming.Update,
                        t);
                }
            }
            

        }
    }

    private static bool TryGetTag(ref string str, int start, out string beginStr, out string endStr, out string text, out int nextIndex)
    {
        beginStr = endStr = string.Empty;
        text = string.Empty;
        nextIndex = start;
        
        if (str.Length <= start || start < 0) return false;
        if (str[start] != '<') return false;

        while (str[nextIndex] != '<')
        {
            nextIndex++;
            if (nextIndex >= str.Length) return false;
        }
        
        while (str[nextIndex] != '>')
        {
            nextIndex++;
            if (nextIndex >= str.Length) return false;
        }

        beginStr = str.Substring(start, nextIndex - start + 1);

        start = ++nextIndex;

        
        while (str[nextIndex] != '<')
        {
            nextIndex++;
            if (nextIndex >= str.Length) return false;
        }
        
        text = str.Substring(start, nextIndex - start);
        start = nextIndex;
        
        while (str[nextIndex] != '>')
        {
            nextIndex++;
            if (nextIndex >= str.Length) return false;
        }
        
        endStr = str.Substring(start, nextIndex - start + 1);

        return true;
    }
}