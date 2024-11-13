using System;
using System.Collections.Generic;
using UnityEngine;


public class SettingGraphicModel
{
    public event Action<(string groupKey, object value)> OnChangedGraphic;

    private Dictionary<string, (Func<int> GetIndexCount, Func<int> GetIndex, Func<int, string> GetItemName, Action<int> SetIndex)> _dropdownCallbackDict;
    private Dictionary<string, (Func<(float normalizedValue, float? originMaxValue)> GetValue, Action<float> SetValue)> _sliderCallbackDict;
    
    public SettingGraphicModel()
    {
        if (ScreenManager.Instance == false) return;

        ScreenManager.Instance.OnChangedResolution += OnChangedResolution;
        _dropdownCallbackDict = new();
        _dropdownCallbackDict["ScreenMode"] = (GetIndexCount_ScreenMode, GetIndex_ScreenMode, GetItemName_ScreenMode, SetIndex_ScreenMode);
        _dropdownCallbackDict["Resolution"] = (GetIndexCount_Resolution, GetIndex_Resolution, GetItemName_Resolution, SetIndex_Resolution);
        _dropdownCallbackDict["Vsync"] =      (GetIndexCount_Vsync, GetIndex_Vsync, GetItemName_Vsync, SetIndex_Vsync);

        _sliderCallbackDict = new();
        _sliderCallbackDict["Frame"] = (GetFrame, SetFrame);
        _sliderCallbackDict["RenderScale"] = (GetRenderScale, SetRenderScale);
    }

    #region Resolution Callback

    private int GetIndexCount_Resolution()
    {
        if (ScreenManager.Instance)
            return ScreenManager.Instance.AllResolutions.Count;
        
        return -1;
    }
    private int GetIndex_Resolution()
    {
        if (ScreenManager.Instance)
            return GetResolutionIndex(ScreenManager.Instance.CurrentResolution);
        
        return -1;
    }
    private string GetItemName_Resolution(int index)
    {
        if (ScreenManager.Instance == false) return "ERROR";
        if (ScreenManager.Instance.AllResolutions.Count <= index || index < 0) return "ERROR";
        
        Vector2Int res = ScreenManager.Instance.AllResolutions[index];
        return $"{res.x} x {res.y}";
    }
    private void SetIndex_Resolution(int index)
    {
        if (ScreenManager.Instance == false) return;
        if (ScreenManager.Instance.AllResolutions.Count <= index || index < 0) return;

        Vector2Int res = ScreenManager.Instance.AllResolutions[index];

        ScreenManager.Instance.SetResolution(res.x, res.y);
    }

    private void OnChangedResolution(Vector2Int resolution)
    {
        OnChangedGraphic?.Invoke(("Resolution", GetResolutionIndex(resolution)));
    }
    
    #endregion

    #region ScreenMode Callback

    private int GetIndexCount_ScreenMode()
    {
        if (ScreenManager.Instance)
            return 3;
        
        return -1;
    }
    private int GetIndex_ScreenMode()
    {
        if (ScreenManager.Instance)
        {
            switch (ScreenManager.Instance.ScreenMode)
            {
                case FullScreenMode.ExclusiveFullScreen:
                    return 0;
                case FullScreenMode.FullScreenWindow:
                    return 0;
                case FullScreenMode.MaximizedWindow:
                    return 1;
                case FullScreenMode.Windowed:
                    return 2;
                default:
                    return 0;
            }
        }
        
        return -1;
    }
    private string GetItemName_ScreenMode(int index)
    {
        if (ScreenManager.Instance)
        {
            switch (index)
            {
                case 0:
                    return "전체 화면";
                case 1:
                    return "전체 창 모드";
                case 2:
                    return "창 모드";
                default:
                    return "전체 화면";
            }
        }
        
        return "ERROR";
    }
    private void SetIndex_ScreenMode(int index)
    {
        if (ScreenManager.Instance)
        {
            switch (index)
            {
                case 0:
                    ScreenManager.Instance.ScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case 1:
                    ScreenManager.Instance.ScreenMode = FullScreenMode.MaximizedWindow;
                    break;
                case 2:
                    ScreenManager.Instance.ScreenMode = FullScreenMode.Windowed;
                    break;
            }
        }
    }
    #endregion
    
    #region Vsync Callback
    private int GetIndexCount_Vsync()
    {
        if (ScreenManager.Instance)
            return 2;
        
        return -1;
    }
    private int GetIndex_Vsync()
    {
        if (ScreenManager.Instance)
        {
            return ScreenManager.Instance.VsyncCount == 0 ? 0 : 1;
        }
        
        return -1;
    }
    private string GetItemName_Vsync(int index)
    {
        if (ScreenManager.Instance)
        {
            switch (index)
            {
                case 0:
                    return "비활성화";
                case 1:
                    return "활성화";
                default:
                    return "ERROR";
            }
        }
        
        return "ERROR";
    }
    private void SetIndex_Vsync(int index)
    {
        if (ScreenManager.Instance)
        {
            ScreenManager.Instance.VsyncCount = index == 0 ? 0 : 1;
        }
    }
    #endregion

    #region Frame Callback

    private (float normalizedValue, float? originMaxValue) GetFrame()
    {
        var inst = ScreenManager.Instance;
        if (inst)
        {
            if (inst.TargetFrameRate == 0)
            {
                return (1f, inst.MaxFrameRate);
            }
            
            float normalizedValue = (float)inst.TargetFrameRate / Mathf.Max(1, inst.MaxFrameRate);
            return (normalizedValue, inst.MaxFrameRate);
        }

        return (0f, null);
    }
    private void SetFrame(float normalizedValue)
    {
        var inst = ScreenManager.Instance;
        if (inst)
        {
            float value = normalizedValue * inst.MaxFrameRate;
            inst.TargetFrameRate = Mathf.RoundToInt(value);
        }
    }
    #endregion
    

    #region RenderScale Callback

    private (float normalizedValue, float? originMaxValue) GetRenderScale()
    {
        var inst = ScreenManager.Instance;
        if (inst)
        {
            return (inst.RenderScale, inst.MaxRenderScale);
        }

        return (0f, null);
    }
    private void SetRenderScale(float normalizedValue)
    {
        var inst = ScreenManager.Instance;
        if (inst)
        {
            float v = Mathf.Clamp(normalizedValue, 0.1f, inst.MaxRenderScale);
            inst.RenderScale = Mathf.Min(v, inst.MaxRenderScale);
        }
    }
    #endregion
    public void Release()
    {
        if (ScreenManager.Instance == false) return;
        
        ScreenManager.Instance.OnChangedResolution -= OnChangedResolution;
    }

    public void ApplySetting()
    {
        if (ScreenManager.Instance == false) return;
        
        ScreenManager.Instance.SaveSetting();
    }

    public (float normalizedValue, float? originMaxValue) GetSliderValue(string groupKey)
    {
        if (_sliderCallbackDict.TryGetValue(groupKey, out var tuple))
        {
            return tuple.GetValue();
        }

        return (0f, null);
    }
    public void SetSliderValue(string groupKey, float normalizedValue)
    {
        if (_sliderCallbackDict.TryGetValue(groupKey, out var tuple))
        {
            tuple.SetValue(normalizedValue);
        }
    }
    public int GetIndexCount(string groupKey)
    {
        if (_dropdownCallbackDict.TryGetValue(groupKey, out var tuple))
        {
            return tuple.GetIndexCount();
        }

        return 0;
    }
    public int GetIndex(string groupKey)
    {
        if (_dropdownCallbackDict.TryGetValue(groupKey, out var tuple))
        {
            return tuple.GetIndex();
        }

        return -1;
    }

    public string GetItemName(string groupKey, int index)
    {
        if (_dropdownCallbackDict.TryGetValue(groupKey, out var tuple))
        {
            return tuple.GetItemName(index);
        }

        return "ERROR";
    }
    
    public void SetIndex(string groupKey, int index)
    {
        if (_dropdownCallbackDict.TryGetValue(groupKey, out var tuple))
        {
            tuple.SetIndex(index);
        }
    }
    
    private int GetResolutionIndex(Vector2Int targetResolution)
    {
        int resIndex = -1;
        for(int i =0; i<  ScreenManager.Instance.AllResolutions.Count; i++)
        {
            var res = ScreenManager.Instance.AllResolutions[i];
            if (res == targetResolution)
            {
                resIndex = i;
                break;
            }
            
        }

        if (resIndex == -1)
        {
            resIndex = ScreenManager.Instance.AllResolutions.Count - 1;
        }

        return resIndex;
    }

}