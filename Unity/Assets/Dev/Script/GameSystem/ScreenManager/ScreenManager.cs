using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectBBF.Persistence;
using ProjectBBF.Singleton;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public enum CursorType
{
    Default,
    CanClick,
    None,
}

[Singleton(ESingletonType.Global, 2)]
public class ScreenManager : MonoBehaviourSingleton<ScreenManager>
{
    private CursorTable _cursorTable;
    private GameSetting _setting;

    public override void PostInitialize()
    {
        var resList = new List<Vector2Int>();
        foreach (var resolution in Screen.resolutions)
        {
            resList.Add(new Vector2Int(resolution.width, resolution.height));
        }

        MaxFrameRate = Mathf.RoundToInt((float)Screen.resolutions.Max(x => x.refreshRateRatio.value));
        AllResolutions = resList.Distinct().ToList();
        PersistenceManager.Instance.TryLoadOrCreateUserData("GameSetting", out GameSetting setting);

        if (setting.IsCreatedScreen is false)
        {
            setting.IsCreatedScreen = true;
            setting.ScreenMode = (int)FullScreenMode.MaximizedWindow;
            setting.VsyncCount = QualitySettings.vSyncCount;
            setting.Resolution = AllResolutions[^1];
            setting.RefreshRate = MaxFrameRate;
            setting.RenderScale = 1f;
            RenderScale = 1f;
        }
        else
        {
            SetResolution(setting.Resolution.x, setting.Resolution.y);
            ScreenMode = (FullScreenMode)setting.ScreenMode;
            TargetFrameRate = setting.RefreshRate;
            VsyncCount = setting.VsyncCount;
            RenderScale = setting.RenderScale;
        }

        _setting = setting;


        _cursorTable = Resources.Load<CursorTable>("Data/Dat_CursorTable");
        Debug.Assert(_cursorTable, "CursorTable을 찾을 수 없습니다. ");
    }

    public override void PostRelease()
    {
    }

    public void SaveSetting()
    {
        _setting.ScreenMode = (int)ScreenMode;
        _setting.VsyncCount = VsyncCount;
        _setting.Resolution = CurrentResolution;
        _setting.RefreshRate = TargetFrameRate;
        _setting.RenderScale = RenderScale;

        PersistenceManager.Instance.SaveUserData();
    }

    public event Action<Vector2Int> OnChangedResolution;

    public IReadOnlyList<Vector2Int> AllResolutions { get; private set; }

    private FullScreenMode _screenMode;

    public FullScreenMode ScreenMode
    {
        get => _screenMode;
        set
        {
            Screen.fullScreenMode = value;
            _screenMode = value;
        }
    }

    public float RenderScale
    {
        get
        {
            var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
            return urpAsset.renderScale;
        }
        set
        {
            var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
            urpAsset.renderScale = Mathf.Clamp(value, 0.1f, MaxRenderScale);
        }
    }

    public float MaxRenderScale => 2f;

    private int _resolutionIndex;

    public Vector2Int CurrentResolution { get; private set; }

    private int _targetFrameRate;

    public int TargetFrameRate
    {
        get => _targetFrameRate;
        set
        {
            Application.targetFrameRate = value;
            _targetFrameRate = value;
        }
    }

    public int MaxFrameRate { get; private set; }

    public int VsyncCount
    {
        get => QualitySettings.vSyncCount;
        set => QualitySettings.vSyncCount = value;
    }

    public void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, ScreenMode);
        CurrentResolution = new Vector2Int(width, height);
        OnChangedResolution?.Invoke(CurrentResolution);
    }

    private CursorType _cursor = CursorType.None;

    public CursorType CurrentCursor
    {
        get => _cursor;
        set
        {
            if (_cursor == value) return;

            SetCursorForce(value);
        }
    }

    public void SetCursorForce(CursorType type)
    {
        _cursor = type;
            
        if (_cursorTable == false) return;

        if (_cursor == CursorType.None)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            return;
        }
            
            
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
            
        Texture2D cursorTexture = _cursorTable.List.FirstOrDefault(x => type == x.Type).Texture2D;
        if (_cursorTable == false) return;
            
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.ForceSoftware);
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