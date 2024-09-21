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
        SetResolution(Screen.width, Screen.height);
    }

    public override void PostRelease()
    {
    }


    public event Action<Vector2Int> OnChangedResolution;

    public Resolution[] AllResolutions => Screen.resolutions;

    public FullScreenMode ScreenMode => Screen.fullScreenMode;
    private int _resolutionIndex;

    public Vector2Int CurrentResolution { get; private set; }

    public void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, ScreenMode);
        CurrentResolution = new Vector2Int(width, height);
        OnChangedResolution?.Invoke(CurrentResolution);
    }

    private void Update()
    {
        if (CurrentResolution.x == Screen.width &&
            CurrentResolution.y == Screen.height
           ) return;

        SetResolution(Screen.width, Screen.height);
    }
}