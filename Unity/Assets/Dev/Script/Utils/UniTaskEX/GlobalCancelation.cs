using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public static class GlobalCancelation
{
    private static CancellationTokenSource _playMode;

    public static CancellationToken PlayMode
    {
        get
        {
            if (_playMode == null)
            {
                _playMode = new CancellationTokenSource();
                return _playMode.Token;
            }

            return _playMode.Token;
        }
    }

    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        if (Application.isPlaying == false) return;
        
        var obj = new GameObject("GlobalCancelation");
        GameObject.DontDestroyOnLoad(obj);
        obj.AddComponent<GlobalCancelationObject>();
    }


    public class GlobalCancelationObject : MonoBehaviour
    {
        private void OnDestroy()
        {
            GlobalCancelation._playMode?.Cancel();
            _playMode = null;
        }
    }

}