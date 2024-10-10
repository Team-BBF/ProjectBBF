using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Singleton;
using UnityEngine;

[Singleton(ESingletonType.Global)]
public class MinigameController : MonoBehaviourSingleton<MinigameController>
{
    public override void PostInitialize()
    {
    }

    public override void PostRelease()
    {
    }

    public event Action<string> OnSignalMinigameStart;
    public event Action<string> OnSignalMinigameEnd;
    public string CurrentGameKey { get; set; }
    public HashSet<string> PlayOnceTable { get; private set; } = new();
    
    
    public void StartMinigame(string key)
    {
        OnSignalMinigameStart?.Invoke(key);
    }
    public void EndMinigame(string key)
    {
        OnSignalMinigameEnd?.Invoke(key);
    }
}
