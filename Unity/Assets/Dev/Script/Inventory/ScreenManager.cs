using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Singleton;
using UnityEngine;
using UnityEngine.UI;

[Singleton(ESingletonType.Global)]
public class ScreenManager : MonoBehaviourSingleton<ScreenManager>
{
    public override void PostInitialize()
    {
        var res = AllResolutions[^1];
        SetResolution(res.width, res.height);
    }

    public override void PostRelease()
    {
    }


    public event Action<Vector2Int> OnChangedResolution;

    public Resolution[] AllResolutions => Screen.resolutions;

    public FullScreenMode ScreenMode
    {
        get => Screen.fullScreenMode;
        set => Screen.fullScreenMode = value;
    }
    private int _resolutionIndex;

    public Vector2Int CurrentResolution { get; private set; }

    public void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, ScreenMode);
        CurrentResolution = new Vector2Int(width, height);
        OnChangedResolution?.Invoke(CurrentResolution);
    }

    #if UNITY_EDITOR
    private void Update()
    {
        if (CurrentResolution.x == Screen.width &&
            CurrentResolution.y == Screen.height
           ) return;

        SetResolution(Screen.width, Screen.height);
    }
    #endif
}